using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace MultipartActionResult.Tests;

public class MultipartActionResultIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public MultipartActionResultIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10000)]
    public async Task Multipart_WithValidContent_ReturnsValidMultipartResponse(int resultCount)
    {
        // Arrange 

        // Act
        HttpResponseMessage multipartResponse = await _client.GetAsync($"/api/multipart/results?resultCount={resultCount}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, multipartResponse.StatusCode);

        MultipartMemoryStreamProvider multipartProvider = await multipartResponse.Content.ReadAsMultipartAsync();
        List<HttpContent> contents = multipartProvider.Contents.ToList();

        Assert.Equal(resultCount, contents.Count);
        for (int i = 0; i < contents.Count; i++)
        {
            HttpContent content = contents[i];

            Assert.Equal("text/plain", content.Headers.ContentType!.MediaType);
            Assert.Equal("utf-8", content.Headers.ContentType.CharSet);
            Assert.Equal("attachment", content.Headers.ContentDisposition!.DispositionType);
            Assert.Equal($"file{i + 1}.txt", content.Headers.ContentDisposition.FileName);
            Assert.Equal($"here is content{i + 1}", await content.ReadAsStringAsync());
        }
    }

    [Theory]
    [InlineData("resultsWithException")]
    [InlineData("resultsWithStreamErrors")]
    public async Task Multipart_WithErrors_ReturnsPartialStreams(string testCase)
    {
        // Arrange 
        TestMultipartStreamProvider streamProvider = new TestMultipartStreamProvider();

        // Act
        var multipartResponse = await _client.GetAsync($"/api/multipart/{testCase}", HttpCompletionOption.ResponseHeadersRead);
        _ = await Assert.ThrowsAsync<IOException>(() =>
            multipartResponse.Content.ReadAsMultipartAsync(streamProvider!)
        );

        // Assert
        // We may not even get to read the first stream if the response stream is aborted by the server.
        Assert.True(streamProvider.Results.Count < 2); 
    }

    private class TestMultipartStreamProvider : MultipartStreamProvider
    {
        public List<(HttpContentHeaders headers, MemoryStream)> Results { get; } = new List<(HttpContentHeaders headers, MemoryStream)>();

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            MemoryStream ms = new MemoryStream();
            Results.Add((headers, ms));
            return ms;
        }
    }
}
