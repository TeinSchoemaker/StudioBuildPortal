using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class JenkinsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public JenkinsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    private void AddAuthHeader()
    {
        var user = _configuration["Jenkins:Username"];
        var token = _configuration["Jenkins:ApiToken"];

        var authBytes = Encoding.ASCII.GetBytes($"{user}:{token}");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
    }

    public async Task TriggerBuildAsync(string jobName, string branch)
    {
        AddAuthHeader();

        var baseUrl = _configuration["Jenkins:BaseUrl"];
        var url = $"{baseUrl}/job/{jobName}/buildWithParameters?BRANCH={branch}";

        var response = await _httpClient.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<(string status, int? buildNumber)> GetLastBuildAsync(string jobName)
    {
        AddAuthHeader();

        var baseUrl = _configuration["Jenkins:BaseUrl"];
        var url = $"{baseUrl}/job/{jobName}/lastBuild/api/json";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return ("Unknown", null);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        var number = root.TryGetProperty("number", out var numberProp)
            ? numberProp.GetInt32()
            : (int?)null;

        string status = "Running";

        if (root.TryGetProperty("result", out var resultProp) && resultProp.ValueKind != JsonValueKind.Null)
        {
            status = resultProp.GetString() ?? "Unknown";
        }

        return (status, number);
    }
}