using GloboClima.Application.DTOs;
using GloboClima.Application.Interfaces;
using GloboClima.Domain.Entities;
using GloboClima.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboClima.Application.Services
{
    public class FavoriteCityService : IFavoriteCityService
    {
        private readonly IFavoriteCityRepository _repository;

        public FavoriteCityService(IFavoriteCityRepository repository)
        {
            _repository = repository;
        }

        public async Task<FavoriteCityResponse> AddFavoriteCityAsync(string userId, CreateFavoriteCityRequest request)
        {
            var favoriteCity = new FavoriteCity(userId, request.CountryCode, request.CityName);

            var exists = await _repository.ExistsAsync(userId, favoriteCity.LocationId);
            if (exists)
                throw new InvalidOperationException("City is already in favorites");

            await _repository.AddAsync(favoriteCity);

            return MapToResponse(favoriteCity);
        }

        public async Task<IEnumerable<FavoriteCityResponse>> GetUserFavoriteCitiesAsync(string userId)
        {
            var cities = await _repository.GetByUserIdAsync(userId);
            return cities.Select(MapToResponse).OrderByDescending(c => c.CreatedAt);
        }

        public async Task<IEnumerable<FavoriteCityResponse>> GetFavoriteCitiesByCountryAsync(string countryCode)
        {
            var cities = await _repository.GetByCountryCodeAsync(countryCode);
            return cities.Select(MapToResponse).OrderByDescending(c => c.CreatedAt);
        }

        public async Task<bool> RemoveFavoriteCityAsync(string userId, string locationId)
        {
            var exists = await _repository.ExistsAsync(userId, locationId);
            if (!exists) return false;

            await _repository.RemoveAsync(userId, locationId);
            return true;
        }

        public async Task<bool> IsCityFavoriteAsync(string userId, string locationId)
        {
            return await _repository.ExistsAsync(userId, locationId);
        }

        private static FavoriteCityResponse MapToResponse(FavoriteCity entity)
        {
            return new FavoriteCityResponse(
                entity.LocationId,
                entity.CountryCode,
                entity.CityName,
                entity.CreatedAt
            );
        }
    }
}
