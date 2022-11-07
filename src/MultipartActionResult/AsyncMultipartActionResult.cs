using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MultipartActionResult;

public class AsyncMultipartActionResult : IActionResult
{
    public AsyncMultipartActionResult(
        IEnumerable<Task<(IHeaderDictionary headers, Stream contentStream)>> contents)
    {
        Contents = contents;
    }

    public IEnumerable<Task<(IHeaderDictionary headers, Stream contentStream)>> Contents { get; }

    public Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var executor = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IActionResultExecutor<AsyncMultipartActionResult>>();

        return executor.ExecuteAsync(context, this);
    }
}
