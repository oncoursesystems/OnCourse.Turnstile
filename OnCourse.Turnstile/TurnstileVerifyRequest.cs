using System.Text.Json.Serialization;

namespace OnCourse.Turnstile;

public class TurnstileVerifyRequest
{
    [JsonPropertyName("secret")]
    public required string SecretKey { get; set; }
    [JsonPropertyName("response")]
    public required string Token { get; set; }
    [JsonPropertyName("remoteip")]
    public string? UserIpAddress { get; set; }
    [JsonPropertyName("idempotency_key")]
    public string? IdempotencyKey { get; set; }
}
