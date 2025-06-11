using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GloboClima.Application.DTOs;
using System.Security.Claims;
using GloboClima.Application.Interfaces;

namespace GloboClima.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteCitiesController : ControllerBase
    {
        private readonly IFavoriteCityService _favoriteCityService;
        private readonly ILogger<FavoriteCitiesController> _logger;

        public FavoriteCitiesController(
            IFavoriteCityService favoriteCityService,
            ILogger<FavoriteCitiesController> logger)
        {
            _favoriteCityService = favoriteCityService;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona uma cidade aos favoritos do usuário
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FavoriteCityResponse>> AddFavoriteCity([FromBody] CreateFavoriteCityRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var result = await _favoriteCityService.AddFavoriteCityAsync(userId, request);
                return CreatedAtAction(nameof(GetUserFavorites), result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding favorite city for user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtém todas as cidades favoritas do usuário
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteCityResponse>>> GetUserFavorites()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var favorites = await _favoriteCityService.GetUserFavoriteCitiesAsync(userId);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user favorites");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Remove uma cidade dos favoritos do usuário
        /// </summary>
        [HttpDelete("{locationId}")]
        public async Task<IActionResult> RemoveFavoriteCity(string locationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var success = await _favoriteCityService.RemoveFavoriteCityAsync(userId, locationId);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing favorite city");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Verifica se uma cidade é favorita do usuário
        /// </summary>
        [HttpGet("check/{locationId}")]
        public async Task<ActionResult<bool>> IsCityFavorite(string locationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var isFavorite = await _favoriteCityService.IsCityFavoriteAsync(userId, locationId);
                return Ok(isFavorite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if city is favorite");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtém cidades favoritas por país (endpoint público ou admin)
        /// </summary>
        [HttpGet("country/{countryCode}")]
        [AllowAnonymous] // Ou use [Authorize(Policy = "AdminOnly")] se quiser restringir
        public async Task<ActionResult<IEnumerable<FavoriteCityResponse>>> GetFavoriteCitiesByCountry(string countryCode)
        {
            try
            {
                var favorites = await _favoriteCityService.GetFavoriteCitiesByCountryAsync(countryCode);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites by country");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private string GetCurrentUserId()
        {
            // Para AWS Cognito, o user ID geralmente está no claim 'sub'
            return User.FindFirst("sub")?.Value ??
                   User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   User.FindFirst("email")?.Value; // fallback para email se necessário
        }
    }
}
