using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OnCourse.Turnstile;

public static class TurnstileExtensions
{
    public static IServiceCollection AddTurnstile(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TurnstileSettings>(configuration);
        services.AddHttpClient();
        services.AddTransient<ITurnstileService, TurnstileService>();

        return services;
    }

    public static IHtmlContent Turnstile(this IHtmlHelper helper, string? cssClass = null, string? appearance = null, string? size = null, string? theme = null, string? language = null, string? action = null, string? cData = null, string? execution = null,
        string? callback = null, string? errorCallback = null, string? expiredCallback = null, string? beforeInteractiveCallback = null, string? afterInteractiveCallback = null,
        string? unsupportedCallback = null, string? timeoutCallback = null, string? retry = null, int? retryInterval = null, string? refreshExpired = null, string? refreshTimeout = null,
        bool? feedbackEnabled = null)
    {
        var settings = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<TurnstileSettings>>().Value;

        if (!settings.Enabled)
        {
            return HtmlString.Empty;
        }

        return TurnstileWidget(settings.SiteKey, cssClass, appearance, size, theme, language, action, cData, execution,
            callback, errorCallback, expiredCallback, beforeInteractiveCallback, afterInteractiveCallback,
            unsupportedCallback, timeoutCallback, retry, retryInterval, refreshExpired, refreshTimeout,
            feedbackEnabled);
    }

    internal static IHtmlContent TurnstileWidget(string siteKey, string? cssClass = null, string? appearance = null, string? size = null, string? theme = null, string? language = null, string? action = null, string? cData = null, string? execution = null,
        string? callback = null, string? errorCallback = null, string? expiredCallback = null, string? beforeInteractiveCallback = null, string? afterInteractiveCallback = null,
        string? unsupportedCallback = null, string? timeoutCallback = null, string? retry = null, int? retryInterval = null, string? refreshExpired = null, string? refreshTimeout = null,
        bool? feedbackEnabled = null)
    {
        if (execution != null && execution != "render" && execution != "execute")
        {
            throw new ArgumentException("Execution attribute must be either 'render' or 'execute'", nameof(execution));
        }

        if (theme != null && theme != "light" && theme != "dark" && theme != "auto")
        {
            throw new ArgumentException("Theme attribute must be either 'light', 'dark', or 'auto'", nameof(theme));
        }

        if (size != null && size != "normal" && size != "compact" && size != "flexible")
        {
            throw new ArgumentException("Size attribute must be either 'normal', 'compact', or 'flexible'", nameof(size));
        }

        if (retry != null && retry != "auto" && retry != "never")
        {
            throw new ArgumentException("Retry attribute must be either 'auto' or 'never'", nameof(retry));
        }

        if (refreshExpired != null && refreshExpired != "auto" && refreshExpired != "manual" && refreshExpired != "never")
        {
            throw new ArgumentException("RefreshExpired attribute must be either 'auto', 'manual', or 'never'", nameof(refreshExpired));
        }

        if (refreshTimeout != null && refreshTimeout != "auto" && refreshTimeout != "manual" && refreshTimeout != "never")
        {
            throw new ArgumentException("RefreshTimeout attribute must be either 'auto', 'manual', or 'never'", nameof(refreshTimeout));
        }

        if (appearance != null && appearance != "always" && appearance != "execute" && appearance != "interaction-only")
        {
            throw new ArgumentException("Appearance attribute must be either 'always', 'execute', or 'interaction-only'", nameof(appearance));
        }

        var content = new HtmlContentBuilder();
        content.AppendHtml("<script src=\"https://challenges.cloudflare.com/turnstile/v0/api.js?onload=onloadTurnstileCallback\" async defer></script>");

        content.AppendHtml($"<div data-sitekey=\"{siteKey}\" class=\"cf-turnstile {cssClass}\"");

        if (appearance != null)
            content.AppendHtml($" data-appearance=\"{appearance}\"");
        if (size != null)
            content.AppendHtml($" data-size=\"{size}\"");
        if (theme != null)
            content.AppendHtml($" data-theme=\"{theme}\"");
        if (language != null)
            content.AppendHtml($" data-language=\"{language}\"");
        if (action != null)
            content.AppendHtml($" data-action=\"{action}\"");
        if (cData != null)
            content.AppendHtml($" data-cdata=\"{cData}\"");
        if (execution != null)
            content.AppendHtml($" data-execution=\"{execution}\"");
        if (callback != null)
            content.AppendHtml($" data-callback=\"{callback}\"");
        if (errorCallback != null)
            content.AppendHtml($" data-error-callback=\"{errorCallback}\"");
        if (expiredCallback != null)
            content.AppendHtml($" data-expired-callback=\"{expiredCallback}\"");
        if (beforeInteractiveCallback != null)
            content.AppendHtml($" data-before-interactive-callback=\"{beforeInteractiveCallback}\"");
        if (afterInteractiveCallback != null)
            content.AppendHtml($" data-after-interactive-callback=\"{afterInteractiveCallback}\"");
        if (unsupportedCallback != null)
            content.AppendHtml($" data-unsupported-callback=\"{unsupportedCallback}\"");
        if (timeoutCallback != null)
            content.AppendHtml($" data-timeout-callback=\"{timeoutCallback}\"");
        if (retry != null)
            content.AppendHtml($" data-retry=\"{retry}\"");
        if (retryInterval.HasValue)
            content.AppendHtml($" data-retry-interval=\"{retryInterval}\"");
        if (refreshExpired != null)
            content.AppendHtml($" data-refresh-expired=\"{refreshExpired}\"");
        if (refreshTimeout != null)
            content.AppendHtml($" data-refresh-timeout=\"{refreshTimeout}\"");
        if (feedbackEnabled.HasValue)
            content.AppendHtml($" data-feedback-enabled=\"{feedbackEnabled.Value.ToString().ToLowerInvariant()}\"");

        content.AppendHtml("></div>");
        return content;
    }
}
