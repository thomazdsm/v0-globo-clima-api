using GloboClima.Application.DTOs.Response.Country;
using GloboClima.Application.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GloboClima.Application.Services
{
    public class CountryService : ICountryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CountryService> _logger;
        private readonly string _baseUrl = "https://restcountries.com/v3.1";

        public CountryService(HttpClient httpClient, ILogger<CountryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CountryResponseDto>> GetAllCountriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/all?fields=name,cca2,capital,region,flags,capitalInfo");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var countries = JsonSerializer.Deserialize<CountryApiResponse[]>(json, options);

                    return countries?.Select(MapToCountryResponse).ToList() ?? new List<CountryResponseDto>();
                }

                _logger.LogWarning("Falha ao buscar países. Status: {StatusCode}", response.StatusCode);
                return new List<CountryResponseDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro de desserialização ao buscar países");
                return new List<CountryResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar países");
                return new List<CountryResponseDto>();
            }
        }

        public async Task<CountryResponseDto?> GetCountryByNameAsync(string countryName)
        {
            try
            {
                var encodedName = Uri.EscapeDataString(countryName);
                var response = await _httpClient.GetAsync($"{_baseUrl}/name/{encodedName}?fields=name,cca2,capital,region,flags,capitalInfo");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var countries = JsonSerializer.Deserialize<CountryApiResponse[]>(json, options);

                    return countries?.FirstOrDefault() != null ? MapToCountryResponse(countries.First()) : null;
                }

                _logger.LogWarning("País não encontrado: {CountryName}. Status: {StatusCode}", countryName, response.StatusCode);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro de desserialização ao buscar país {CountryName}", countryName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar país {CountryName}", countryName);
                return null;
            }
        }

        public async Task<List<CountryResponseDto>> SearchCountriesByNameAsync(string searchTerm)
        {
            try
            {
                var encodedTerm = Uri.EscapeDataString(searchTerm);
                var response = await _httpClient.GetAsync($"{_baseUrl}/name/{encodedTerm}?fields=name,cca2,capital,region,flags,capitalInfo");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var countries = JsonSerializer.Deserialize<CountryApiResponse[]>(json, options);

                    return countries?.Select(MapToCountryResponse).ToList() ?? new List<CountryResponseDto>();
                }

                _logger.LogWarning("Nenhum país encontrado para o termo: {SearchTerm}. Status: {StatusCode}", searchTerm, response.StatusCode);
                return new List<CountryResponseDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro de desserialização ao pesquisar países com termo {SearchTerm}", searchTerm);
                return new List<CountryResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pesquisar países com termo {SearchTerm}", searchTerm);
                return new List<CountryResponseDto>();
            }
        }

        #region Métodos Auxiliares

        private static CountryResponseDto MapToCountryResponse(CountryApiResponse country)
        {
            return new CountryResponseDto
            {
                Name = country.Name?.Common ?? "",
                Code = country.Cca2 ?? "", // Mudança: removido FirstOrDefault() pois agora é string
                Capital = country.Capital?.FirstOrDefault() ?? "",
                Region = country.Region ?? "",
                Flag = country.Flags?.Png ?? country.Flags?.Svg ?? "",
                Coordinates = country.CapitalInfo?.Latlng
            };
        }

        #endregion
    }
}