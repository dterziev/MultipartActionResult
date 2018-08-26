# MultipartActionResult

This is an implementaion of an asynchronous streaming multipart IActionResult for ASP.NET Core MVC.

## Usage

The multipart action executor needs to be registered in the ConfigureServices method of the Startup class.
```c#
public void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddMultipartSupport();
    // ...
}
```

Then use the `Multipart` extension method to return the multipart content to the client.

```c#
public IActionResult GetMultipartContent()
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
```

The resulting HTTP message is:

```text
HTTP/1.1 200 OK
Content-Type: multipart/mixed; boundary=93f0339c-3919-4842-9e2c-f656d6b05420
Server: Kestrel
X-Powered-By: ASP.NET
Date: Sun, 26 Aug 2018 18:03:45 GMT
Content-Length: 399

--93f0339c-3919-4842-9e2c-f656d6b05420
Content-Disposition: attachment; filename=file1.txt; filename*=UTF-8''file1.txt
Content-Type: text/plain; charset=utf-8

Some text content
--93f0339c-3919-4842-9e2c-f656d6b05420
Content-Disposition: attachment; filename=file2.csv; filename*=UTF-8''file2.csv
Content-Type: text/csv; charset=utf-8

A,B
1,2

--93f0339c-3919-4842-9e2c-f656d6b05420--
```

Parsing the multipart content can be done using the `ReadAsMultipartAsync` extension methods in `Microsoft.AspNet.WebApi.Client` package.

```c#
 HttpClient httpClient = new HttpClient();
var response = await httpClient.GetAsync(
    "http://localhost:19701/api/multipart/example",
    HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

var tempFolder = Path.Combine(Path.GetTempPath(), $"multipart_{Guid.NewGuid()}");

try
{
    Directory.CreateDirectory(tempFolder);

    var results = await response.Content.ReadAsMultipartAsync(
        new MultipartFileStreamProvider(tempFolder)).ConfigureAwait(false);

    foreach (var fileData in results.FileData)
    {
        Console.WriteLine($"Local file: {fileData.LocalFileName}");
        foreach (var header in fileData.Headers)
        {
            Console.WriteLine($"{header.Key}: {String.Join(",", header.Value)}");
        }

        Console.WriteLine("-------------------------------------");
    }
}
finally
{
    if (Directory.Exists(tempFolder))
    {
        Directory.Delete(tempFolder, true);
    }
}
```
