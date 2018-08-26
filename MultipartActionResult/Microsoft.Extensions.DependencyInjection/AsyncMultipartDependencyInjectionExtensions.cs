using Microsoft.AspNetCore.Mvc.Infrastructure;
using MultipartActionResult;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AsyncMultipartDependencyInjectionExtensions
    {
        public static void AddMultipartSupport(this IServiceCollection services)
        {
            services.AddSingleton<IActionResultExecutor<AsyncMultipartActionResult>>(new AsyncMultipartActionResultExecutor());
        }
    }
}
