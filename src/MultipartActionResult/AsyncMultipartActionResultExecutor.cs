using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace MultipartActionResult;

public class AsyncMultipartActionResultExecutor : IActionResultExecutor<AsyncMultipartActionResult>
{
    private const string CrLf = "\r\n";
    private static readonly Encoding DefaultHttpEncoding = Encoding.GetEncoding(28591);
    private static readonly string Boundarydary = Guid.NewGuid().ToString();

    public async Task ExecuteAsync(ActionContext context, AsyncMultipartActionResult result)
    {
        var response = context.HttpContext.Response;

        var responseHeaders = response.GetTypedHeaders();
        responseHeaders.ContentType = new MediaTypeHeaderValue("multipart/mixed")
        {
            Boundary = Boundarydary
        };

        var responseStream = response.Body;

        try
        {
            // Write start boundary.
            await EncodeStringToStreamAsync(responseStream, "--" + Boundarydary + CrLf).ConfigureAwait(false);

            // Write each nested content.
            var scratch = new StringBuilder();
            int contentIndex = 0;
            foreach (var task in result.Contents)
            {
                var (headers, contentStream) = await task.ConfigureAwait(false);
                var partHeaders = SerializeHeadersToString(scratch, contentIndex, headers);
                contentIndex += 1;
                await EncodeStringToStreamAsync(responseStream, partHeaders).ConfigureAwait(false);
                await contentStream.CopyToAsync(responseStream).ConfigureAwait(false);
            }

            // Write footer boundary.
            await EncodeStringToStreamAsync(responseStream, CrLf + "--" + Boundarydary + "--" + CrLf).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Don't throw this exception, it's most likely caused by the client disconnecting.
            // However, if it was cancelled for any other reason we need to prevent empty responses.
            context.HttpContext.Abort();
        }
    }

    private static ValueTask EncodeStringToStreamAsync(Stream stream, string input)
    {
        byte[] buffer = DefaultHttpEncoding.GetBytes(input);
        return stream.WriteAsync(new ReadOnlyMemory<byte>(buffer));
    }

    private string SerializeHeadersToString(StringBuilder scratch, int contentIndex, IHeaderDictionary headers)
    {
        // TODO: there are more efficient ways to write the headers. 

        scratch.Clear();

        // Add divider.
        if (contentIndex != 0) // Write divider for all but the first content.
        {
            scratch.Append(CrLf + "--"); // const strings
            scratch.Append(Boundarydary);
            scratch.Append(CrLf);
        }

        // Add headers.
        foreach (var headerPair in headers)
        {
            scratch.Append(headerPair.Key);

            scratch.Append(": ");
            string delim = string.Empty;
            foreach (var value in headerPair.Value)
            {
                scratch.Append(delim);
                scratch.Append(value);
                delim = ", ";
            }
            scratch.Append(CrLf);
        }

        // Extra CRLF to end headers (even if there are no headers).
        scratch.Append(CrLf);

        return scratch.ToString();
    }
}
