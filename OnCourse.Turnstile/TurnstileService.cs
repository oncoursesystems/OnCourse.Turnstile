using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace OnCourse.Turnstile;

public class TurnstileService : ITurnstileService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TurnstileSettings _settings;

    public TurnstileService(IOptions<TurnstileSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<TurnstileVerifyResult?> VerifyAsync(TurnstileVerifyRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        var result = await client.PostAsJsonAsync($"{_settings.BaseUrl}/siteverify", request, cancellationToken);
        result.EnsureSuccessStatusCode();

        return await result.Content.ReadFromJsonAsync<TurnstileVerifyResult>(cancellationToken: cancellationToken);
    }
}
