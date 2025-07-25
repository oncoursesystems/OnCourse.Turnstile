using System.Text.Json.Serialization;

namespace OnCourse.Turnstile;

public class TurnstileVerifyResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("error-codes")]
    public string[] ErrorCodes { get; set; } = [];
    [JsonPropertyName("challenge_ts")]
    public DateTimeOffset? On { get; set; }
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }
    [JsonPropertyName("action")]
    public string? Action { get; set; }
    [JsonPropertyName("cdata")]
    public string? CData { get; set; }
}
