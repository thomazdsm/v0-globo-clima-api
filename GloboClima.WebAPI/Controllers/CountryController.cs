using GloboClima.Application.DTOs.Response.Country;
using GloboClima.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GloboClima.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Países")]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        /// <summary>
        /// Obtém todos os países
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<CountryResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CountryResponseDto>>> GetAllCountries()
        {
            var countries = await _countryService.GetAllCountriesAsync();
            return Ok(countries);
        }

        /// <summary>
        /// Busca país por nome
        /// </summary>
        [HttpGet("{countryName}")]
        [ProducesResponseType(typeof(CountryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CountryResponseDto>> GetCountryByName(string countryName)
        {
            var country = await _countryService.GetCountryByNameAsync(countryName);

            if (country == null)
                return NotFound(new { message = "País não encontrado" });

            return Ok(country);
        }

        /// <summary>
        /// Pesquisa países por termo
        /// </summary>
        [HttpGet("search/{searchTerm}")]
        [ProducesResponseType(typeof(List<CountryResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CountryResponseDto>>> SearchCountries(string searchTerm)
        {
            var countries = await _countryService.SearchCountriesByNameAsync(searchTerm);
            return Ok(countries);
        }
    }
}
