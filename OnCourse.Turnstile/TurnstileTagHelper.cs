using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OnCourse.Turnstile;

public class TurnstileTagHelper : TagHelper
{
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }
    [HtmlAttributeName("appearance")]
    public string? Appearance { get; set; }
    [HtmlAttributeName("size")]
    public string? Size { get; set; }
    [HtmlAttributeName("theme")]
    public string? Theme { get; set; }
    [HtmlAttributeName("language")]
    public string? Language { get; set; }
    [HtmlAttributeName("action")]
    public string? Action { get; set; }
    [HtmlAttributeName("cdata")]
    public string? CData { get; set; }
    [HtmlAttributeName("execution")]
    public string? Execution { get; set; }
    [HtmlAttributeName("callback")]
    public string? Callback { get; set; }
    [HtmlAttributeName("error-callback")]
    public string? ErrorCallback { get; set; }
    [HtmlAttributeName("expired-callback")]
    public string? ExpiredCallback { get; set; }
    [HtmlAttributeName("before-interactive-callback")]
    public string? BeforeInteractiveCallback { get; set; }
    [HtmlAttributeName("after-interactive-callback")]
    public string? AfterInteractiveCallback { get; set; }
    [HtmlAttributeName("unsupported-callback")]
    public string? UnsupportedCallback { get; set; }
    [HtmlAttributeName("timeout-callback")]
    public string? TimeoutCallback { get; set; }
    [HtmlAttributeName("retry")]
    public string? Retry { get; set; }
    [HtmlAttributeName("retry-interval")]
    public int? RetryInterval { get; set; }
    [HtmlAttributeName("refresh-expired")]
    public string? RefreshExpired { get; set; }
    [HtmlAttributeName("refresh-timeout")]
    public string? RefreshTimeout { get; set; }
    [HtmlAttributeName("feedback-enabled")]
    public bool? FeedbackEnabled { get; set; }

    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var settings = ViewContext.HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<TurnstileSettings>>().Value;

        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = null;

        if (!settings.Enabled)
        {
            return;
        }

        var content = TurnstileExtensions.TurnstileWidget(settings.SiteKey, CssClass, Appearance, Size, Theme, Language, Action, CData, Execution,
            Callback, ErrorCallback, ExpiredCallback, BeforeInteractiveCallback, AfterInteractiveCallback,
            UnsupportedCallback, TimeoutCallback, Retry, RetryInterval, RefreshExpired, RefreshTimeout,
            FeedbackEnabled);
        output.Content.AppendHtml(content);
    }
}
