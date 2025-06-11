using GloboClima.Application.DTOs;

namespace GloboClima.Application.Interfaces
{
    public interface IFavoriteCityService
    {
        Task<FavoriteCityResponse> AddFavoriteCityAsync(string userId, CreateFavoriteCityRequest request);
        Task<IEnumerable<FavoriteCityResponse>> GetUserFavoriteCitiesAsync(string userId);
        Task<IEnumerable<FavoriteCityResponse>> GetFavoriteCitiesByCountryAsync(string countryCode);
        Task<bool> RemoveFavoriteCityAsync(string userId, string locationId);
        Task<bool> IsCityFavoriteAsync(string userId, string locationId);
    }
}
