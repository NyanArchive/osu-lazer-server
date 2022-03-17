using System.Text.Json;

namespace OsuLazerServer.Utils;

public class IPUtils
{
    public static async Task<string> GetCountry(string country)
    {
        var response = await (new HttpClient()).GetAsync($"http://ip-api.com/json/{country}");

        if (!response.IsSuccessStatusCode)
        {
            return "US";
        }

        var body = (dynamic) JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync())!;

        return body["countryCode"];
    }
}