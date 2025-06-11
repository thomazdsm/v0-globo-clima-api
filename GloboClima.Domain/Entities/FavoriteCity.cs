
namespace GloboClima.Domain.Entities
{
    public class FavoriteCity
    {
        public string UserId { get; private set; }
        public string LocationId { get; private set; }
        public string CountryCode { get; private set; }
        public string CityName { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Constructor para criação
        public FavoriteCity(string userId, string countryCode, string cityName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ArgumentException("CountryCode cannot be empty", nameof(countryCode));

            if (string.IsNullOrWhiteSpace(cityName))
                throw new ArgumentException("CityName cannot be empty", nameof(cityName));

            UserId = userId;
            CountryCode = countryCode.ToUpper();
            CityName = cityName.Trim();
            LocationId = GenerateLocationId(countryCode, cityName);
            CreatedAt = DateTime.UtcNow;
        }

        // Constructor para hidratação (quando vem do banco)
        private FavoriteCity() { }

        public static FavoriteCity FromRepository(string userId, string locationId,
            string countryCode, string cityName, DateTime createdAt)
        {
            return new FavoriteCity
            {
                UserId = userId,
                LocationId = locationId,
                CountryCode = countryCode,
                CityName = cityName,
                CreatedAt = createdAt
            };
        }

        private static string GenerateLocationId(string countryCode, string cityName)
        {
            return $"{countryCode.ToUpper()}-{cityName.Replace(" ", "").Replace("-", "")}";
        }

        public bool IsSameLocation(string countryCode, string cityName)
        {
            return CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase) &&
                   CityName.Equals(cityName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
