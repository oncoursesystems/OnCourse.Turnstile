namespace OnCourse.Turnstile;

public class TurnstileSettings
{
    public required string SecretKey { get; set; }
    public required string SiteKey { get; set; }
    public string BaseUrl { get; set; } = "https://challenges.cloudflare.com/turnstile/v0";
    public bool Enabled { get; set; } = true;
}
