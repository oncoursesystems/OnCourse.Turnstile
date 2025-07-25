namespace OnCourse.Turnstile;

public interface ITurnstileService
{
    Task<TurnstileVerifyResult?> VerifyAsync(TurnstileVerifyRequest request, CancellationToken cancellationToken = default);
}
