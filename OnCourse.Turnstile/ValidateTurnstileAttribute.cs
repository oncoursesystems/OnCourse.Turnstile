using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OnCourse.Turnstile;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class ValidateTurnstileAttribute : Attribute, IFilterFactory
{
    public string ErrorMessage { get; set; } = "Your request could not be completed because you failed the Cloudflare Turnstile verification.";

    public string FormField { get; set; } = "cf-turnstile-response";

    public bool IsReusable => true;

    public ValidateTurnstileAttribute()
    {
    }

    public ValidateTurnstileAttribute(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<ITurnstileService>();
        var settings = serviceProvider.GetRequiredService<IOptionsSnapshot<TurnstileSettings>>();

        return new ValidateTurnstileFilter(service, FormField, ErrorMessage, settings);
    }
}

public class ValidateTurnstileFilter(ITurnstileService turnstileService, string formField, string errorMessage, IOptionsSnapshot<TurnstileSettings> settings) : IAsyncActionFilter, IAsyncPageFilter
{
    private readonly TurnstileSettings _settings = settings.Value;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await ValidateTurnstile(context);
        await next();
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (ShouldValidate(context))
        {
            await ValidateTurnstile(context);
        }

        await next();

        static bool ShouldValidate(ActionContext context)
        {
            return !HttpMethods.IsGet(context.HttpContext.Request.Method) &&
                   !HttpMethods.IsHead(context.HttpContext.Request.Method) &&
                   !HttpMethods.IsOptions(context.HttpContext.Request.Method);
        }
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    private async Task ValidateTurnstile(ActionContext context)
    {
        if (!_settings.Enabled)
        {
            return;
        }

        if (!context.HttpContext.Request.HasFormContentType)
        {
            context.ModelState.AddModelError("", errorMessage);
        }
        else
        {
            if (context.HttpContext.Request.Form.TryGetValue(formField, out var token) && !string.IsNullOrWhiteSpace(token))
            {
                var response = await turnstileService.VerifyAsync(new TurnstileVerifyRequest
                {
                    Token = token.ToString(),
                    SecretKey = _settings.SecretKey,
                    UserIpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                });

                if (response?.Success ?? false)
                {
                    return;
                }
            }

            context.ModelState.AddModelError("Cloudflare Turnstile", errorMessage);
        }
    }
}
