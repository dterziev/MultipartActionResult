namespace SampleClient;

class Program
{
    static void Main(string[] args)
    {
        MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    static async Task MainAsync(string[] args)
    {
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
    }
}