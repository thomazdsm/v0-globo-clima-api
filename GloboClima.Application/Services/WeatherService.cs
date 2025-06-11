using GloboClima.Application.DTOs.Response.Weather;
using GloboClima.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace GloboClima.Application.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5";

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<WeatherResponseDto?> GetWeatherByCityAsync(string cityName, string? countryCode = null)
        {
            try
            {
                var apiKey = _configuration["OpenWeatherMap:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("OpenWeatherMap API Key não configurada");
                }

                var query = string.IsNullOrEmpty(countryCode) ? cityName : $"{cityName},{countryCode}";
                var url = $"{_baseUrl}/weather?q={query}&appid={apiKey}&units=metric&lang=pt_br";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var weather = JsonSerializer.Deserialize<WeatherApiResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return weather != null ? MapToWeatherResponse(weather) : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar clima para {cityName}: {ex.Message}");
                return null;
            }
        }

        public async Task<WeatherResponseDto?> GetWeatherByCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var apiKey = _configuration["OpenWeatherMap:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("OpenWeatherMap API Key não configurada");
                }

                var url = $"{_baseUrl}/weather?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric&lang=pt_br";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var weather = JsonSerializer.Deserialize<WeatherApiResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return weather != null ? MapToWeatherResponse(weather) : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar clima para coordenadas {latitude},{longitude}: {ex.Message}");
                return null;
            }
        }

        #region Métodos Auxiliares

        private static WeatherResponseDto MapToWeatherResponse(WeatherApiResponse weather)
        {
            return new WeatherResponseDto
            {
                CityName = weather.Name ?? "",
                Country = weather.Sys?.Country ?? "",
                Temperature = Math.Round(weather.Main?.Temp ?? 0, 1),
                FeelsLike = Math.Round(weather.Main?.Feels_like ?? 0, 1),
                Humidity = weather.Main?.Humidity ?? 0,
                Description = weather.Weather?.FirstOrDefault()?.Description ?? "",
                Icon = weather.Weather?.FirstOrDefault()?.Icon ?? "",
                WindSpeed = Math.Round(weather.Wind?.Speed ?? 0, 1),
                Pressure = weather.Main?.Pressure ?? 0,
                DateTime = DateTimeOffset.FromUnixTimeSeconds(weather.Dt).DateTime
            };
        }

        #endregion
    }
}
