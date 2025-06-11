using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GloboClima.Application.DTOs.Response.Country
{
    // DTO para a resposta da API original
    public class CountryApiResponse
    {
        [JsonPropertyName("name")]
        public CountryName? Name { get; set; }

        [JsonPropertyName("cca2")]
        public string? Cca2 { get; set; }  // Mudança: string ao invés de array

        [JsonPropertyName("capital")]
        public string[]? Capital { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("flags")]
        public CountryFlags? Flags { get; set; }

        [JsonPropertyName("capitalInfo")]
        public CountryCapitalInfo? CapitalInfo { get; set; }
    }

    public class CountryName
    {
        [JsonPropertyName("common")]
        public string? Common { get; set; }

        [JsonPropertyName("official")]
        public string? Official { get; set; }
    }

    public class CountryFlags
    {
        [JsonPropertyName("png")]
        public string? Png { get; set; }

        [JsonPropertyName("svg")]
        public string? Svg { get; set; }

        [JsonPropertyName("alt")]
        public string? Alt { get; set; }
    }

    public class CountryCapitalInfo
    {
        [JsonPropertyName("latlng")]
        public double[]? Latlng { get; set; }
    }
}
