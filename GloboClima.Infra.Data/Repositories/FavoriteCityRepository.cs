using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces;
using GloboClima.Infra.Data.Configurations;
using Microsoft.Extensions.Configuration;

namespace GloboClima.Infra.Data.Repositories
{
    public class FavoriteCityRepository : IFavoriteCityRepository
    {
        private readonly IDynamoDBContext _dynamoContext;
        private readonly IAmazonDynamoDB _dynamoClient;
        private readonly string _tableName;

        public FavoriteCityRepository(
            IDynamoDBContext dynamoContext,
            IAmazonDynamoDB dynamoClient,
            IConfiguration config)
        {
            _dynamoContext = dynamoContext;
            _dynamoClient = dynamoClient;
            _tableName = config["DynamoDB:FavoriteCitiesTableName"] ?? "globoclima-user-favorites-dev";
        }

        public async Task<FavoriteCity?> GetByLocationAsync(string userId, string locationId)
        {
            var model = await _dynamoContext.LoadAsync<FavoriteCityDynamoModel>(userId, locationId);
            return model?.ToEntity();
        }

        public async Task<IEnumerable<FavoriteCity>> GetByUserIdAsync(string userId)
        {
            var search = _dynamoContext.QueryAsync<FavoriteCityDynamoModel>(userId);
            var models = await search.GetRemainingAsync();
            return models.Select(m => m.ToEntity());
        }

        public async Task<IEnumerable<FavoriteCity>> GetByCountryCodeAsync(string countryCode)
        {
            // Para esta query funcionar, você precisa criar um GSI (Global Secondary Index)
            // Chave: CountryCode, Sort Key: CreatedAt
            var request = new QueryRequest
            {
                TableName = _tableName,
                IndexName = "CountryCodeIndex", // Nome do GSI que você precisa criar
                KeyConditionExpression = "CountryCode = :countryCode",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":countryCode", new AttributeValue { S = countryCode.ToUpper() } }
                }
            };

            try
            {
                var response = await _dynamoClient.QueryAsync(request);

                return response.Items.Select(item => FavoriteCity.FromRepository(
                    item["UserId"].S,
                    item["LocationId"].S,
                    item["CountryCode"].S,
                    item["CityName"].S,
                    DateTime.Parse(item["CreatedAt"].S)
                ));
            }
            catch (Exception ex)
            {
                // Log do erro e retorna lista vazia se GSI não existir
                Console.WriteLine($"Error querying by country code: {ex.Message}");
                return new List<FavoriteCity>();
            }
        }

        public async Task<bool> ExistsAsync(string userId, string locationId)
        {
            var model = await _dynamoContext.LoadAsync<FavoriteCityDynamoModel>(userId, locationId);
            return model != null;
        }

        public async Task AddAsync(FavoriteCity favoriteCity)
        {
            var model = FavoriteCityDynamoModel.FromEntity(favoriteCity);
            await _dynamoContext.SaveAsync(model);
        }

        public async Task RemoveAsync(string userId, string locationId)
        {
            await _dynamoContext.DeleteAsync<FavoriteCityDynamoModel>(userId, locationId);
        }
    }
}