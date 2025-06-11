using GloboClima.Domain.Entities;

namespace GloboClima.Domain.Interfaces
{
    public interface IFavoriteCityRepository
    {
        Task<FavoriteCity?> GetByLocationAsync(string userId, string locationId);
        Task<IEnumerable<FavoriteCity>> GetByUserIdAsync(string userId);
        Task<IEnumerable<FavoriteCity>> GetByCountryCodeAsync(string countryCode);
        Task<bool> ExistsAsync(string userId, string locationId);
        Task AddAsync(FavoriteCity favoriteCity);
        Task RemoveAsync(string userId, string locationId);
    }
}
