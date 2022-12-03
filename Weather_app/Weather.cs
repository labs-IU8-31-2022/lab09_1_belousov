using System;
using System.Data;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeatherBot;

internal struct Weather
{
    private static readonly string? ApiKey = "d37806bbb02cecdb04dabf01fdfa2975";
    public string? Country { get; }
    public string? Name { get; }
    public decimal Temp { get; }
    public string Description { get; }

    private Weather(string country, string name, decimal temp, string description) =>
        (Country, Name, Temp, Description) = (country, name, temp, description);

    public static async Task<Weather> GetAsync(decimal lat, decimal lon)
    {
        if (lat < -90 && lat > 90 && lon < -180 && lon > 180)
        {
            throw new Exception("Check range of coordinates");
        }

        HttpClient client = new();
        client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/weather");
        var response =
            await client.GetAsync($"?lat={lat}&lon={lon}&appid={ApiKey}&units=metric").ConfigureAwait(false);
        //Console.WriteLine(response.EnsureSuccessStatusCode());
        var (weather, code) = Convert.ToInt32(response.StatusCode) == 200
            ? FromJsonDeserializer(await response.Content.ReadAsStringAsync())
            : (new Weather(), Convert.ToInt32(response.StatusCode));

        if (code == 200 && weather.Country is not null && weather.Name is not null)
        {
            return weather;
        }

        if (weather.Country is null || weather.Name is null)
        {
            throw new DataException("Null country or city");
        }

        throw new Exception("Странная поломка, да?)");
    }

    private static (Weather w, int code) FromJsonDeserializer(string json)
    {
        var dynamic = JsonConvert.DeserializeObject<dynamic>(json);

        var country = Convert.ToString(dynamic?.sys.country);
        var name = Convert.ToString(dynamic?.name);
        var temp = Convert.ToDecimal(dynamic?.main.temp);
        var description = Convert.ToString(dynamic?.weather[0].description);
        return (new Weather(country, name, temp, description), dynamic?.cod);
    }
}