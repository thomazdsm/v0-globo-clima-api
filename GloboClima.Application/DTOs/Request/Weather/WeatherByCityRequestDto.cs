using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboClima.Application.DTOs.Request.Weather
{
    public class WeatherByCityRequestDto
    {
        [Required]
        public string CityName { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
    }
}
