﻿using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MultipartActionResult;

namespace Microsoft.Extensions.DependencyInjection;

public static class AsyncMultipartDependencyInjectionExtensions
{
    public static IServiceCollection AddMultipartSupport(this IServiceCollection services)
    {
        return services
            .AddSingleton<IActionResultExecutor<AsyncMultipartActionResult>>(
                new AsyncMultipartActionResultExecutor());
    }
}
