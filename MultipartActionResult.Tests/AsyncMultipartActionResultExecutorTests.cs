//using System;
//using System.IO;
//using System.Text;
//using System.Threading.Tasks;
//using AspNetMvc.Extensions;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Net.Http.Headers;
//using Microsoft.Extensions.DependencyInjection;
//using Xunit;

//namespace MultipartActionResult.Tests
//{
//    public class AsyncMultipartActionResultExecutorTests : 
//        IClassFixture<CustomWebApplicationFactory>
//    {
//        private static IServiceCollection CreateServices()
//        {
//            var services = new ServiceCollection();
//            services.AddMultipartSupport();
//            return services;
//        }

//        private HttpContext GetCreateContext()
//        {
//            var services = CreateServices();

//            var httpContext = new DefaultHttpContext();
//            httpContext.RequestServices = services.BuildServiceProvider();

//            return httpContext;
//        }

//        [Fact]
//        public async Task Test2()
//        {




//        }


//        [Fact]
//        public async Task Test1()
//        {
//            var executor = new AsyncMultipartActionResultExecutor();

//            var httpContext = new DefaultHttpContext();
//            var actionContext = new ActionContext() { HttpContext = httpContext };
//            httpContext.Request.Headers[HeaderNames.Accept] = "application/xml"; // This will not be used
//            httpContext.Response.Body = new MemoryStream();

//            var result = new AsyncMultipartActionResult(new[] {
//                Task.FromResult((GetHeaders("text/plain", "file1.txt"), (Stream)new MemoryStream(Encoding.UTF8.GetBytes("here is contnet1")))),
//                Task.FromResult((GetHeaders("text/plain", "file2.txt"), (Stream)new MemoryStream(Encoding.UTF8.GetBytes("here is contnet1"))))
//            });

//            // Act
//            await executor.ExecuteAsync(actionContext, result);

//            // Assert
//        }

//        IHeaderDictionary GetHeaders(string contentType, string fileName)
//        {
//            var result = new HeaderDictionary();

//            var contentDisposition = new ContentDispositionHeaderValue("attachment");
//            contentDisposition.SetHttpFileName(fileName);
//            result[HeaderNames.ContentDisposition] = contentDisposition.ToString();

//            result[HeaderNames.ContentType] = new MediaTypeHeaderValue(contentType)
//            {
//                Charset = "utf-8"
//            }.ToString();

//            return result;
//        }
//    }
//}
