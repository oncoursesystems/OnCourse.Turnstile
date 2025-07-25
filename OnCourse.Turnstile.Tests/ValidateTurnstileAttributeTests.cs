using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace OnCourse.Turnstile.Tests;

public class ValidateTurnstileAttributeTests
{
    private ValidateTurnstileFilter CreateFilter(ITurnstileService turnstileService, TurnstileSettings? settings = null)
    {
        settings ??= new TurnstileSettings
        {
            SiteKey = "3x00000000000000000000FF",
            SecretKey = "1x0000000000000000000000000000000AA",
            Enabled = true
        };

        var optionsSnapshot = new Mock<IOptionsSnapshot<TurnstileSettings>>();
        optionsSnapshot.Setup(x => x.Value).Returns(settings);

        return new ValidateTurnstileFilter(
            turnstileService,
            "cf-turnstile-response",
            "Your request could not be completed because you failed the Cloudflare Turnstile verification.",
            optionsSnapshot.Object);
    }

    private ActionExecutingContext CreateActionContext(bool hasFormContentType = true, string? turnstileToken = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";

        if (hasFormContentType)
        {
            httpContext.Request.ContentType = "application/x-www-form-urlencoded";

            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                ["cf-turnstile-response"] = turnstileToken ?? ""
            });

            httpContext.Request.Form = formCollection;
        }

        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);

        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            new object());
    }

    [Fact]
    public async Task ValidateTurnstile_SuccessfulValidation_DoesNotAddModelStateErrors()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();
        mockTurnstileService.Setup(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TurnstileVerifyResult
            {
                Success = true,
                On = DateTimeOffset.Now,
                Hostname = "test.com"
            });

        var filter = CreateFilter(mockTurnstileService.Object);
        var context = CreateActionContext(hasFormContentType: true, turnstileToken: "valid-token");

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.True(context.ModelState.IsValid);
        Assert.Empty(context.ModelState);
    }

    [Fact]
    public async Task ValidateTurnstile_UnsuccessfulValidation_AddsProperErrorMessageToModelState()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();
        mockTurnstileService.Setup(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TurnstileVerifyResult
            {
                Success = false,
                On = DateTimeOffset.Now,
                Hostname = "test.com"
            });

        var filter = CreateFilter(mockTurnstileService.Object);
        var context = CreateActionContext(hasFormContentType: true, turnstileToken: "invalid-token");

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.False(context.ModelState.IsValid);
        Assert.True(context.ModelState.ContainsKey("Cloudflare Turnstile"));
        Assert.Equal("Your request could not be completed because you failed the Cloudflare Turnstile verification.",
            context.ModelState["Cloudflare Turnstile"]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateTurnstile_CustomErrorMessage_AddsCustomErrorToModelState()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();
        mockTurnstileService.Setup(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TurnstileVerifyResult
            {
                Success = false,
                On = DateTimeOffset.Now,
                Hostname = "test.com"
            });

        var settings = new TurnstileSettings
        {
            SiteKey = "test-site-key",
            SecretKey = "test-secret-key",
            Enabled = true
        };

        var optionsSnapshot = new Mock<IOptionsSnapshot<TurnstileSettings>>();
        optionsSnapshot.Setup(x => x.Value).Returns(settings);

        var customErrorMessage = "Custom validation failed message";
        var filter = new ValidateTurnstileFilter(
            mockTurnstileService.Object,
            "cf-turnstile-response",
            customErrorMessage,
            optionsSnapshot.Object);

        var context = CreateActionContext(hasFormContentType: true, turnstileToken: "invalid-token");

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.False(context.ModelState.IsValid);
        Assert.True(context.ModelState.ContainsKey("Cloudflare Turnstile"));
        Assert.Equal(customErrorMessage, context.ModelState["Cloudflare Turnstile"]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateTurnstile_WhenDisabled_DoesNotAddModelStateErrors()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();

        var settings = new TurnstileSettings
        {
            SiteKey = "test-site-key",
            SecretKey = "test-secret-key",
            Enabled = false
        };

        var filter = CreateFilter(mockTurnstileService.Object, settings);
        var context = CreateActionContext(hasFormContentType: true, turnstileToken: "any-token");

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.True(context.ModelState.IsValid);
        Assert.Empty(context.ModelState);
        mockTurnstileService.Verify(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateTurnstile_MissingFormField_AddsErrorToModelState()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();
        var filter = CreateFilter(mockTurnstileService.Object);
        var context = CreateActionContext(hasFormContentType: true, turnstileToken: null);

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.False(context.ModelState.IsValid);
        Assert.True(context.ModelState.ContainsKey("Cloudflare Turnstile"));
        Assert.Equal("Your request could not be completed because you failed the Cloudflare Turnstile verification.",
            context.ModelState["Cloudflare Turnstile"]!.Errors[0].ErrorMessage);
        mockTurnstileService.Verify(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateTurnstile_EmptyFormField_AddsErrorToModelState()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();
        var filter = CreateFilter(mockTurnstileService.Object);
        var context = CreateActionContext(hasFormContentType: true, turnstileToken: "");

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.False(context.ModelState.IsValid);
        Assert.True(context.ModelState.ContainsKey("Cloudflare Turnstile"));
        Assert.Equal("Your request could not be completed because you failed the Cloudflare Turnstile verification.",
            context.ModelState["Cloudflare Turnstile"]!.Errors[0].ErrorMessage);
        mockTurnstileService.Verify(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateTurnstile_WhitespaceFormField_AddsErrorToModelState()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();
        var filter = CreateFilter(mockTurnstileService.Object);
        var context = CreateActionContext(hasFormContentType: true, turnstileToken: "   ");

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.False(context.ModelState.IsValid);
        Assert.True(context.ModelState.ContainsKey("Cloudflare Turnstile"));
        Assert.Equal("Your request could not be completed because you failed the Cloudflare Turnstile verification.",
            context.ModelState["Cloudflare Turnstile"]!.Errors[0].ErrorMessage);
        mockTurnstileService.Verify(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateTurnstile_NonFormContentType_AddsErrorToModelState()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();
        var filter = CreateFilter(mockTurnstileService.Object);
        var context = CreateActionContext(hasFormContentType: false);

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.False(context.ModelState.IsValid);
        Assert.Single(context.ModelState);
        Assert.True(context.ModelState.ContainsKey(""));
        Assert.Equal("Your request could not be completed because you failed the Cloudflare Turnstile verification.",
            context.ModelState[""]!.Errors[0].ErrorMessage);
        mockTurnstileService.Verify(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateTurnstile_ServiceReturnsNull_AddsErrorToModelState()
    {
        var mockTurnstileService = new Mock<ITurnstileService>();
        mockTurnstileService.Setup(x => x.VerifyAsync(It.IsAny<TurnstileVerifyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TurnstileVerifyResult?)null);

        var filter = CreateFilter(mockTurnstileService.Object);
        var context = CreateActionContext(hasFormContentType: true, turnstileToken: "valid-token");

        await filter.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(
            context,
            [],
            new object())));

        Assert.False(context.ModelState.IsValid);
        Assert.True(context.ModelState.ContainsKey("Cloudflare Turnstile"));
        Assert.Equal("Your request could not be completed because you failed the Cloudflare Turnstile verification.",
            context.ModelState["Cloudflare Turnstile"]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public void ValidateTurnstileAttribute_DefaultConstructor_SetsDefaultValues()
    {
        var attribute = new ValidateTurnstileAttribute();

        Assert.Equal("Your request could not be completed because you failed the Cloudflare Turnstile verification.", attribute.ErrorMessage);
        Assert.Equal("cf-turnstile-response", attribute.FormField);
        Assert.True(attribute.IsReusable);
    }

    [Fact]
    public void ValidateTurnstileAttribute_CustomErrorMessageConstructor_SetsCustomErrorMessage()
    {
        var customMessage = "Custom error message";
        var attribute = new ValidateTurnstileAttribute(customMessage);

        Assert.Equal(customMessage, attribute.ErrorMessage);
        Assert.Equal("cf-turnstile-response", attribute.FormField);
        Assert.True(attribute.IsReusable);
    }

    [Fact]
    public void ValidateTurnstileAttribute_CreateInstance_ReturnsValidateTurnstileFilter()
    {
        var attribute = new ValidateTurnstileAttribute();

        var serviceCollection = new ServiceCollection();
        var mockTurnstileService = new Mock<ITurnstileService>();
        var mockOptionsSnapshot = new Mock<IOptionsSnapshot<TurnstileSettings>>();
        mockOptionsSnapshot.Setup(x => x.Value).Returns(new TurnstileSettings
        {
            SiteKey = "test-site-key",
            SecretKey = "test-secret-key"
        });

        serviceCollection.AddSingleton(mockTurnstileService.Object);
        serviceCollection.AddSingleton(mockOptionsSnapshot.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var filter = attribute.CreateInstance(serviceProvider);

        Assert.IsType<ValidateTurnstileFilter>(filter);
    }
}
