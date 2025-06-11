using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboClima.Application.DTOs.Response.Country
{
    public class CountryResponseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Capital { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;
        public double[]? Coordinates { get; set; }
    }
}
