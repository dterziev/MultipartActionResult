using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MultipartActionResult;

namespace Microsoft.AspNetCore.Mvc
{
    public static class AsyncMultipartActionResultControllerExtensions
    {
        public static AsyncMultipartActionResult Multipart(
            this ControllerBase controller,
            IEnumerable<Task<(IHeaderDictionary headers, Stream contentStream)>> contents)
        {
            return new AsyncMultipartActionResult(contents);
        }
    }
}
