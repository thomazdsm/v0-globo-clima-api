using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboClima.Application.DTOs.Response.Weather
{
    // DTO para a resposta da API original
    public class WeatherApiResponse
    {
        public WeatherCoord? Coord { get; set; }
        public WeatherInfo[]? Weather { get; set; }
        public WeatherMain? Main { get; set; }
        public WeatherWind? Wind { get; set; }
        public WeatherSys? Sys { get; set; }
        public string? Name { get; set; }
        public long Dt { get; set; }
    }

    public class WeatherCoord
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
    }

    public class WeatherInfo
    {
        public string? Main { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }

    public class WeatherMain
    {
        public double Temp { get; set; }
        public double Feels_like { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
    }

    public class WeatherWind
    {
        public double Speed { get; set; }
    }

    public class WeatherSys
    {
        public string? Country { get; set; }
    }
}
