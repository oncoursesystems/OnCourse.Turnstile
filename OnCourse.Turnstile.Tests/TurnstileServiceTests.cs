using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace OnCourse.Turnstile.Tests;

public class TurnstileServiceTests
{
    private TurnstileService CreateService(IHttpClientFactory httpClientFactory, TurnstileSettings? turnstileSettings = null)
    {
        turnstileSettings ??= new TurnstileSettings()
        {
            SiteKey = "3x00000000000000000000FF",
            SecretKey = "1x0000000000000000000000000000000AA"
        };

        var turnstileSettingsMock = new Mock<IOptionsSnapshot<TurnstileSettings>>();
        turnstileSettingsMock.Setup(x => x.Value).Returns(turnstileSettings);

        return new TurnstileService(turnstileSettingsMock.Object, httpClientFactory);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TestVerifyAsync(bool successResult)
    {
        var turnstileResponse = new TurnstileVerifyResult
        {
            On = DateTimeOffset.Now,
            Hostname = "Test",
            Success = successResult
        };

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _ = mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(turnstileResponse),
                Encoding.UTF8,
                "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var reCaptchaService = CreateService(mockHttpClientFactory.Object);

        var result = await reCaptchaService.VerifyAsync(new TurnstileVerifyRequest
        {
            Token = "test-token",
            SecretKey = "123",
        });

        mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        Assert.Equal(successResult, result?.Success);
    }
}
