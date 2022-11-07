using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace MultipartActionResult.TestProject.Controllers
{
    [Route("api/multipart")]
    public class MultipartController : Controller
    {
        [HttpGet("results")]
        public IActionResult GetEmptyMultipartContent([FromQuery] int resultCount)
        {
            if (resultCount < 0)
                throw new ArgumentOutOfRangeException(nameof(resultCount), $"{nameof(resultCount)} should be greater than or equal to 0.");

            var parts = Enumerable.Range(1, resultCount)
                .Select(i =>
                {
                    var result = 
                    (
                        GetHeaders("text/plain", $"file{i}.txt"), 
                        (Stream)new MemoryStream(Encoding.UTF8.GetBytes($"here is content{i}"))
                    );

                    return Task.FromResult(result);
                });

            return this.Multipart(parts);
        }

        [HttpGet("resultsWithException")]
        public IActionResult GetEmptyMultipartContentWithErrors()
        {
            var result = Task.FromResult(
                (
                    GetHeaders("text/plain", $"file1.txt"),
                    (Stream)new MemoryStream(Encoding.UTF8.GetBytes($"here is content1"))
                ));

            var error = Task.FromException<(IHeaderDictionary headers, Stream contentStream)>(
                new Exception("something bad happened"));
            
            return this.Multipart(new[] { result, error });
        }

        [HttpGet("resultsWithStreamErrors")]
        public IActionResult GetEmptyMultipartContentWithErrors2()
        {
            var result = Task.FromResult(
                (
                    GetHeaders("text/plain", $"file1.txt"),
                    (Stream)new MemoryStream(Encoding.UTF8.GetBytes($"here is content1"))
                ));

            var closedStream = new MemoryStream(Encoding.UTF8.GetBytes($"here is content2"));
            closedStream.Close();

            var closedStreamResult = Task.FromResult(
            (
                GetHeaders("text/plain", $"file2.txt"),
                (Stream)closedStream
            ));

            return this.Multipart(new[] { result, closedStreamResult });
        }

        [HttpGet("example")]
        public IActionResult Example()
        {
            return this.Multipart(new[]
            {
                Task.FromResult<(IHeaderDictionary, Stream)>(
                    (
                        GetHeaders("text/plain", "file1.txt"),
                        new MemoryStream(Encoding.UTF8.GetBytes("Some text content"))
                    )),
                Task.FromResult<(IHeaderDictionary, Stream)>(
                    (
                        GetHeaders("text/csv", "file2.csv"),
                        new MemoryStream(Encoding.UTF8.GetBytes("A,B\r\n1,2\r\n"))
                    ))
            });
        }

        private static IHeaderDictionary GetHeaders(string contentType, string fileName)
        {
            IHeaderDictionary result = new HeaderDictionary();

            var contentDisposition = new ContentDispositionHeaderValue("attachment");
            contentDisposition.SetHttpFileName(fileName);
            result[HeaderNames.ContentDisposition] = contentDisposition.ToString();

            result[HeaderNames.ContentType] = new MediaTypeHeaderValue(contentType)
            {
                Charset = "utf-8"
            }.ToString();

            return result;
        }
    }
}
