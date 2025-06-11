using GloboClima.Application.DTOs.Request.Weather;
using GloboClima.Application.DTOs.Response.Weather;
using GloboClima.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GloboClima.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Clima")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        /// <summary>
        /// Obtém clima por cidade
        /// </summary>
        [HttpGet("city/{cityName}")]
        [ProducesResponseType(typeof(WeatherResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WeatherResponseDto>> GetWeatherByCity(string cityName, [FromQuery] string? countryCode = null)
        {
            var weather = await _weatherService.GetWeatherByCityAsync(cityName, countryCode);

            if (weather == null)
                return NotFound(new { message = "Dados climáticos não encontrados para esta cidade" });

            return Ok(weather);
        }

        /// <summary>
        /// Obtém clima por coordenadas
        /// </summary>
        [HttpGet("coordinates")]
        [ProducesResponseType(typeof(WeatherResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WeatherResponseDto>> GetWeatherByCoordinates([FromQuery] double latitude, [FromQuery] double longitude)
        {
            var weather = await _weatherService.GetWeatherByCoordinatesAsync(latitude, longitude);

            if (weather == null)
                return NotFound(new { message = "Dados climáticos não encontrados para estas coordenadas" });

            return Ok(weather);
        }

        /// <summary>
        /// Obtém clima por dados de request
        /// </summary>
        [HttpPost("city")]
        [ProducesResponseType(typeof(WeatherResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WeatherResponseDto>> GetWeatherByCity([FromBody] WeatherByCityRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var weather = await _weatherService.GetWeatherByCityAsync(request.CityName, request.CountryCode);

            if (weather == null)
                return NotFound(new { message = "Dados climáticos não encontrados" });

            return Ok(weather);
        }

        /// <summary>
        /// Obtém clima por coordenadas via POST
        /// </summary>
        [HttpPost("coordinates")]
        [ProducesResponseType(typeof(WeatherResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WeatherResponseDto>> GetWeatherByCoordinates([FromBody] WeatherByCoordinatesRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var weather = await _weatherService.GetWeatherByCoordinatesAsync(request.Latitude, request.Longitude);

            if (weather == null)
                return NotFound(new { message = "Dados climáticos não encontrados" });

            return Ok(weather);
        }
    }
}
