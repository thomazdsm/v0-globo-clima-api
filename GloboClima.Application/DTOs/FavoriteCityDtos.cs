using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloboClima.Application.DTOs
{
    public record CreateFavoriteCityRequest(string CountryCode, string CityName);

    public record FavoriteCityResponse(
        string LocationId,
        string CountryCode,
        string CityName,
        DateTime CreatedAt
    );
}
