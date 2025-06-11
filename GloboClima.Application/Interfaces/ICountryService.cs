using GloboClima.Application.DTOs.Response.Country;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboClima.Application.Interfaces
{
    public interface ICountryService
    {
        Task<List<CountryResponseDto>> GetAllCountriesAsync();
        Task<CountryResponseDto?> GetCountryByNameAsync(string countryName);
        Task<List<CountryResponseDto>> SearchCountriesByNameAsync(string searchTerm);
    }
}
