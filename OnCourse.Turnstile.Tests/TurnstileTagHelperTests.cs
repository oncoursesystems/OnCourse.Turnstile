using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace OnCourse.Turnstile.Tests;

public class TurnstileTagHelperTests
{
    private TurnstileTagHelper CreateTagHelper(TurnstileSettings? settings = null)
    {
        settings ??= new TurnstileSettings
        {
            SiteKey = "3x00000000000000000000FF",
            SecretKey = "1x0000000000000000000000000000000AA",
            Enabled = true
        };

        var serviceCollection = new ServiceCollection();
        var optionsSnapshot = new Mock<IOptionsSnapshot<TurnstileSettings>>();
        optionsSnapshot.Setup(x => x.Value).Returns(settings);
        serviceCollection.AddSingleton(optionsSnapshot.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        var viewContext = new ViewContext
        {
            HttpContext = httpContext
        };

        return new TurnstileTagHelper
        {
            ViewContext = viewContext
        };
    }

    [Fact]
    public void Process_WithOptionalProperties_RendersDataAttributes()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Appearance = "always";
        tagHelper.Size = "compact";
        tagHelper.Theme = "dark";
        tagHelper.Language = "en";
        tagHelper.Action = "login";
        tagHelper.CData = "test-data";
        tagHelper.Execution = "render";
        tagHelper.Callback = "onSuccess";
        tagHelper.ErrorCallback = "onError";
        tagHelper.ExpiredCallback = "onExpired";
        tagHelper.BeforeInteractiveCallback = "onBeforeInteractive";
        tagHelper.AfterInteractiveCallback = "onAfterInteractive";
        tagHelper.UnsupportedCallback = "onUnsupported";
        tagHelper.TimeoutCallback = "onTimeout";
        tagHelper.Retry = "auto";
        tagHelper.RetryInterval = 8000;
        tagHelper.RefreshExpired = "auto";
        tagHelper.RefreshTimeout = "auto";
        tagHelper.FeedbackEnabled = true;
        tagHelper.CssClass = "custom-class";

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains("data-sitekey=\"3x00000000000000000000FF\"", content);
        Assert.Contains("data-appearance=\"always\"", content);
        Assert.Contains("data-size=\"compact\"", content);
        Assert.Contains("data-theme=\"dark\"", content);
        Assert.Contains("data-language=\"en\"", content);
        Assert.Contains("data-action=\"login\"", content);
        Assert.Contains("data-cdata=\"test-data\"", content);
        Assert.Contains("data-execution=\"render\"", content);
        Assert.Contains("data-callback=\"onSuccess\"", content);
        Assert.Contains("data-error-callback=\"onError\"", content);
        Assert.Contains("data-expired-callback=\"onExpired\"", content);
        Assert.Contains("data-before-interactive-callback=\"onBeforeInteractive\"", content);
        Assert.Contains("data-after-interactive-callback=\"onAfterInteractive\"", content);
        Assert.Contains("data-unsupported-callback=\"onUnsupported\"", content);
        Assert.Contains("data-timeout-callback=\"onTimeout\"", content);
        Assert.Contains("data-retry=\"auto\"", content);
        Assert.Contains("data-retry-interval=\"8000\"", content);
        Assert.Contains("data-refresh-expired=\"auto\"", content);
        Assert.Contains("data-refresh-timeout=\"auto\"", content);
        Assert.Contains("data-feedback-enabled=\"true\"", content);
        Assert.Contains("class=\"cf-turnstile custom-class\"", content);
    }

    [Fact]
    public void Process_WithNullProperties_DoesNotRenderDataAttributes()
    {
        var tagHelper = CreateTagHelper();

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains("data-sitekey=\"3x00000000000000000000FF\"", content);
        Assert.DoesNotContain("data-appearance=", content);
        Assert.DoesNotContain("data-size=", content);
        Assert.DoesNotContain("data-theme=", content);
        Assert.DoesNotContain("data-language=", content);
        Assert.DoesNotContain("data-action=", content);
        Assert.DoesNotContain("data-cdata=", content);
        Assert.DoesNotContain("data-execution=", content);
        Assert.DoesNotContain("data-callback=", content);
        Assert.DoesNotContain("data-error-callback=", content);
        Assert.DoesNotContain("data-expired-callback=", content);
        Assert.DoesNotContain("data-before-interactive-callback=", content);
        Assert.DoesNotContain("data-after-interactive-callback=", content);
        Assert.DoesNotContain("data-unsupported-callback=", content);
        Assert.DoesNotContain("data-timeout-callback=", content);
        Assert.DoesNotContain("data-retry=", content);
        Assert.DoesNotContain("data-retry-interval=", content);
        Assert.DoesNotContain("data-refresh-expired=", content);
        Assert.DoesNotContain("data-refresh-timeout=", content);
        Assert.DoesNotContain("data-feedback-enabled=", content);
    }

    [Fact]
    public void Process_WhenDisabled_RendersNothing()
    {
        var settings = new TurnstileSettings
        {
            SiteKey = "3x00000000000000000000FF",
            SecretKey = "1x0000000000000000000000000000000AA",
            Enabled = false
        };

        var tagHelper = CreateTagHelper(settings);

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Empty(content);
    }

    [Theory]
    [InlineData("render")]
    [InlineData("execute")]
    public void Process_WithValidExecutionValues_RendersSuccessfully(string execution)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Execution = execution;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains($"data-execution=\"{execution}\"", content);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("RENDER")]
    [InlineData("Execute")]
    [InlineData("")]
    [InlineData("auto")]
    public void Process_WithInvalidExecutionValues_ThrowsArgumentException(string execution)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Execution = execution;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var exception = Assert.Throws<ArgumentException>(() => tagHelper.Process(context, output));
        Assert.Equal("Execution attribute must be either 'render' or 'execute' (Parameter 'execution')", exception.Message);
    }

    [Fact]
    public void Process_WithNullExecution_RendersSuccessfully()
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Execution = null;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.DoesNotContain("data-execution=", content);
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    [InlineData("auto")]
    public void Process_WithValidThemeValues_RendersSuccessfully(string theme)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Theme = theme;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains($"data-theme=\"{theme}\"", content);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("LIGHT")]
    [InlineData("Dark")]
    [InlineData("")]
    [InlineData("blue")]
    public void Process_WithInvalidThemeValues_ThrowsArgumentException(string theme)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Theme = theme;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var exception = Assert.Throws<ArgumentException>(() => tagHelper.Process(context, output));
        Assert.Equal("Theme attribute must be either 'light', 'dark', or 'auto' (Parameter 'theme')", exception.Message);
    }

    [Theory]
    [InlineData("normal")]
    [InlineData("compact")]
    [InlineData("flexible")]
    public void Process_WithValidSizeValues_RendersSuccessfully(string size)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Size = size;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains($"data-size=\"{size}\"", content);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("NORMAL")]
    [InlineData("Compact")]
    [InlineData("")]
    [InlineData("large")]
    public void Process_WithInvalidSizeValues_ThrowsArgumentException(string size)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Size = size;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var exception = Assert.Throws<ArgumentException>(() => tagHelper.Process(context, output));
        Assert.Equal("Size attribute must be either 'normal', 'compact', or 'flexible' (Parameter 'size')", exception.Message);
    }

    [Theory]
    [InlineData("auto")]
    [InlineData("never")]
    public void Process_WithValidRetryValues_RendersSuccessfully(string retry)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Retry = retry;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains($"data-retry=\"{retry}\"", content);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("AUTO")]
    [InlineData("Never")]
    [InlineData("")]
    [InlineData("always")]
    public void Process_WithInvalidRetryValues_ThrowsArgumentException(string retry)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Retry = retry;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var exception = Assert.Throws<ArgumentException>(() => tagHelper.Process(context, output));
        Assert.Equal("Retry attribute must be either 'auto' or 'never' (Parameter 'retry')", exception.Message);
    }

    [Theory]
    [InlineData("auto")]
    [InlineData("manual")]
    [InlineData("never")]
    public void Process_WithValidRefreshExpiredValues_RendersSuccessfully(string refreshExpired)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshExpired = refreshExpired;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains($"data-refresh-expired=\"{refreshExpired}\"", content);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("AUTO")]
    [InlineData("Manual")]
    [InlineData("")]
    [InlineData("always")]
    public void Process_WithInvalidRefreshExpiredValues_ThrowsArgumentException(string refreshExpired)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshExpired = refreshExpired;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var exception = Assert.Throws<ArgumentException>(() => tagHelper.Process(context, output));
        Assert.Equal("RefreshExpired attribute must be either 'auto', 'manual', or 'never' (Parameter 'refreshExpired')", exception.Message);
    }

    [Theory]
    [InlineData("auto")]
    [InlineData("manual")]
    [InlineData("never")]
    public void Process_WithValidRefreshTimeoutValues_RendersSuccessfully(string refreshTimeout)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshTimeout = refreshTimeout;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains($"data-refresh-timeout=\"{refreshTimeout}\"", content);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("AUTO")]
    [InlineData("Manual")]
    [InlineData("")]
    [InlineData("always")]
    public void Process_WithInvalidRefreshTimeoutValues_ThrowsArgumentException(string refreshTimeout)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.RefreshTimeout = refreshTimeout;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var exception = Assert.Throws<ArgumentException>(() => tagHelper.Process(context, output));
        Assert.Equal("RefreshTimeout attribute must be either 'auto', 'manual', or 'never' (Parameter 'refreshTimeout')", exception.Message);
    }

    [Theory]
    [InlineData("always")]
    [InlineData("execute")]
    [InlineData("interaction-only")]
    public void Process_WithValidAppearanceValues_RendersSuccessfully(string appearance)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Appearance = appearance;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        var content = output.Content.GetContent();

        Assert.Contains($"data-appearance=\"{appearance}\"", content);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ALWAYS")]
    [InlineData("Execute")]
    [InlineData("")]
    [InlineData("never")]
    public void Process_WithInvalidAppearanceValues_ThrowsArgumentException(string appearance)
    {
        var tagHelper = CreateTagHelper();
        tagHelper.Appearance = appearance;

        var context = new TagHelperContext(
            [],
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var output = new TagHelperOutput(
            "turnstile",
            [],
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        var exception = Assert.Throws<ArgumentException>(() => tagHelper.Process(context, output));
        Assert.Equal("Appearance attribute must be either 'always', 'execute', or 'interaction-only' (Parameter 'appearance')", exception.Message);
    }
}
