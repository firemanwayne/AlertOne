using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AlertManagement.Client.IntegrationTests.Helpers;

/// <summary>
/// Helper class for creating mock HttpRequestData and HttpResponseData for testing Azure Functions.
/// </summary>
public static class HttpRequestDataHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Creates a mock HttpRequestData with the specified body.
    /// </summary>
    public static HttpRequestData CreateMockRequest<T>(T body, FunctionContext context) where T : class
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        return CreateMockRequest(json, context);
    }

    /// <summary>
    /// Creates a mock HttpRequestData with the specified JSON body.
    /// </summary>
    public static HttpRequestData CreateMockRequest(string jsonBody, FunctionContext context)
    {
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody));

        var mockRequest = new Mock<HttpRequestData>(context);
        mockRequest.Setup(r => r.Body).Returns(memoryStream);
        mockRequest.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        mockRequest.Setup(r => r.CreateResponse()).Returns(() => CreateMockResponse(mockRequest.Object));

        return mockRequest.Object;
    }

    /// <summary>
    /// Creates an empty mock HttpRequestData.
    /// </summary>
    public static HttpRequestData CreateEmptyMockRequest(FunctionContext context)
    {
        var memoryStream = new MemoryStream();

        var mockRequest = new Mock<HttpRequestData>(context);
        mockRequest.Setup(r => r.Body).Returns(memoryStream);
        mockRequest.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        mockRequest.Setup(r => r.CreateResponse()).Returns(() => CreateMockResponse(mockRequest.Object));

        return mockRequest.Object;
    }

    /// <summary>
    /// Creates a mock FunctionContext with a properly configured ServiceProvider.
    /// </summary>
    public static FunctionContext CreateMockFunctionContext()
    {
        var services = new ServiceCollection();
        services.Configure<WorkerOptions>(options =>
        {
            options.Serializer = new JsonObjectSerializer(JsonOptions);
        });

        var serviceProvider = services.BuildServiceProvider();

        var mockContext = new Mock<FunctionContext>();
        mockContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        mockContext.Setup(c => c.InstanceServices).Returns(serviceProvider);

        return mockContext.Object;
    }

    private static HttpResponseData CreateMockResponse(HttpRequestData request)
    {
        return new MockHttpResponseData(request);
    }
}

/// <summary>
/// Mock implementation of HttpResponseData for testing.
/// </summary>
public sealed class MockHttpResponseData : HttpResponseData
{
    public MockHttpResponseData(HttpRequestData request) : base(request.FunctionContext)
    {
        Headers = new HttpHeadersCollection();
        Body = new MemoryStream();
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
    public override HttpCookies Cookies => throw new NotImplementedException();

    /// <summary>
    /// Gets the response body as a string.
    /// </summary>
    public string GetBodyAsString()
    {
        Body.Position = 0;
        using var reader = new StreamReader(Body, leaveOpen: true);
        var content = reader.ReadToEnd();
        Body.Position = 0;
        return content;
    }

    /// <summary>
    /// Gets the response body deserialized as the specified type.
    /// </summary>
    public T? GetBodyAs<T>()
    {
        var json = GetBodyAsString();

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
