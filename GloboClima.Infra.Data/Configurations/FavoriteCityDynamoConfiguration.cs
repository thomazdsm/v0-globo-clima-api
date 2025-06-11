using Amazon.DynamoDBv2.DataModel;
using GloboClima.Domain.Entities;

namespace GloboClima.Infra.Data.Configurations
{
    [DynamoDBTable("globo-clima-app-user-favorites")]
    public class FavoriteCityDynamoModel
    { 
        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public string LocationId { get; set; }

        [DynamoDBProperty]
        public string CountryCode { get; set; }

        [DynamoDBProperty]
        public string CityName { get; set; }

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; }

        // Conversão para entidade de domínio
        public FavoriteCity ToEntity()
        {
            return FavoriteCity.FromRepository(UserId, LocationId, CountryCode, CityName, CreatedAt);
        }

        // Conversão da entidade de domínio
        public static FavoriteCityDynamoModel FromEntity(FavoriteCity entity)
        {
            return new FavoriteCityDynamoModel
            {
                UserId = entity.UserId,
                LocationId = entity.LocationId,
                CountryCode = entity.CountryCode,
                CityName = entity.CityName,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
