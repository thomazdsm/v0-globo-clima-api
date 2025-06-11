using GloboClima.Application.DTOs.Response.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboClima.Application.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherResponseDto?> GetWeatherByCityAsync(string cityName, string? countryCode = null);
        Task<WeatherResponseDto?> GetWeatherByCoordinatesAsync(double latitude, double longitude);
    }
}
