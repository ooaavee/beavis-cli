﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeavisCli.Middlewares
{
    public class BeavisCliMiddleware
    {
        private const string DefaultPath = "/beaviscli";

        private readonly RequestDelegate _next;
        private readonly ILogger<BeavisCliMiddleware> _logger;
        private readonly BeavisCliOptions _options;

        public BeavisCliMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<BeavisCliOptions> options)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<BeavisCliMiddleware>();
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            BeavisCliRequestTypes type = GetRequestType(httpContext.Request);

            if (type == BeavisCliRequestTypes.None)
            {
                await _next(httpContext);
                return;
            }

            // all required services
            ITerminalBehaviour behaviour = httpContext.RequestServices.GetRequiredService<ITerminalBehaviour>();
            IJobPool jobs = httpContext.RequestServices.GetRequiredService<IJobPool>();
            IFileStorage files = httpContext.RequestServices.GetRequiredService<IFileStorage>();
            IRequestExecutor executor = httpContext.RequestServices.GetRequiredService<IRequestExecutor>();

            bool accessible = behaviour.IsRequestHandlerAccessible(httpContext, type);
            if (!accessible)
            {
                await _next(httpContext);
                return;
            }

            _logger.LogInformation($"Started to process a request for a path '{httpContext.Request.Path.ToString()}'.");

            try
            {
                switch (type)
                {
                    case BeavisCliRequestTypes.TerminalHtml:
                        {
                            await RenderHtmlAsync(httpContext);
                            break;
                        }

                    case BeavisCliRequestTypes.TerminalCss:
                        {
                            await RenderCssAsync(httpContext);
                            break;
                        }

                    case BeavisCliRequestTypes.TerminalJs:
                        {
                            await RenderJsAsync(httpContext);
                            break;
                        }

                    case BeavisCliRequestTypes.Initialize:
                        {
                            Response response = new Response(httpContext);
                            response.Messages.AddRange(behaviour.EnumInitMessages(httpContext));
                            response.AddJavaScript(behaviour.EnumInitStatements(httpContext));
                            await RenderResponseAsync(response, httpContext);
                            break;
                        }

                    case BeavisCliRequestTypes.InvokeJob:
                        {
                            Response response = new Response(httpContext);
                            string key = httpContext.Request.Query["key"];
                            await jobs.RunAsync(key, httpContext, response);
                            await RenderResponseAsync(response, httpContext);
                            break;
                        }

                    case BeavisCliRequestTypes.InvokeCommand:
                        {
                            string body = await ReadBodyAsync(httpContext);
                            Request request = JsonConvert.DeserializeObject<Request>(body);
                            Response response = new Response(httpContext);
                            await executor.ExecuteAsync(request, response, httpContext);
                            await RenderResponseAsync(response, httpContext);
                            break;
                        }

                    case BeavisCliRequestTypes.Upload:
                        {
                            if (!behaviour.IsUploadEnabled(httpContext))
                            {
                                throw new InvalidOperationException("File upload functionality is not currently enabled.");
                            }
                            string body = await ReadBodyAsync(httpContext);
                            Response response = new Response(httpContext);
                            FileContent file = JsonConvert.DeserializeObject<FileContent>(body);
                            string id = await files.StoreAsync(file);
                            response.WriteInformation("File upload completed, the file ID is:");
                            response.WriteInformation(id);
                            await RenderResponseAsync(response, httpContext);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred while processing the request type of '{type}'.", e);
                await WriteErrorResponseAsync(e, httpContext);
                return;
            }

            _logger.LogInformation($"Processed a request for a path '{httpContext.Request.Path.ToString()}'.");
        }

        private BeavisCliRequestTypes GetRequestType(HttpRequest request)
        {
            bool IsPotentialMatch()
            {
                return request.Path.StartsWithSegments(_options.Path, StringComparison.InvariantCultureIgnoreCase) ||
                       request.Path.StartsWithSegments(DefaultPath, StringComparison.InvariantCultureIgnoreCase);
            }

            bool Match(string path, string method)
            {
                return request.Method == method &&
                       request.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase);
            }

            if (!IsPotentialMatch())
            {
                return BeavisCliRequestTypes.None;
            }

            if (Match(_options.Path, HttpMethods.Get))
            {
                return BeavisCliRequestTypes.TerminalHtml;
            }

            if (Match($"{DefaultPath}/content/css", HttpMethods.Get))
            {
                return BeavisCliRequestTypes.TerminalCss;
            }

            if (Match($"{DefaultPath}/content/js", HttpMethods.Get))
            {
                return BeavisCliRequestTypes.TerminalJs;
            }

            if (Match($"{DefaultPath}/api/initialize", HttpMethods.Post))
            {
                return BeavisCliRequestTypes.Initialize;
            }

            if (Match($"{DefaultPath}/api/job", HttpMethods.Post))
            {
                return BeavisCliRequestTypes.InvokeJob;
            }

            if (Match($"{DefaultPath}/api/request", HttpMethods.Post))
            {
                return BeavisCliRequestTypes.InvokeCommand;
            }

            if (Match($"{DefaultPath}/api/upload", HttpMethods.Post))
            {
                return BeavisCliRequestTypes.Upload;
            }

            return BeavisCliRequestTypes.None;
        }

        private async Task WriteErrorResponseAsync(Exception e, HttpContext httpContext)
        {
            string text = _options.DisplayExceptions ?
                e.ToString() :
                "An error occurred. Please check your application logs for more details.";

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "text/plain";
            await httpContext.Response.WriteAsync(text, Encoding.UTF8);
        }

        private static async Task<string> ReadBodyAsync(HttpContext httpContext)
        {
            using (var stream = httpContext.Request.Body)
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        private static async Task RenderHtmlAsync(HttpContext httpContext)
        {
            string text = await ReadEmbeddedResourcesAsync("BeavisCli.Resources.html.index.html");

            await WriteAsync(text, httpContext.Response, "text/html");
        }

        private static async Task RenderCssAsync(HttpContext httpContext)
        {
            string[] files = {
                "BeavisCli.Resources.css.jquery.terminal.min.css",
                "BeavisCli.Resources.css.site.css"
            };

            string text = await ReadEmbeddedResourcesAsync(files);

            await WriteAsync(text, httpContext.Response, "text/css");
        }

        private static async Task RenderJsAsync(HttpContext httpContext)
        {
            string[] files = {
                "BeavisCli.Resources.js.jquery.min.js",
                "BeavisCli.Resources.js.jquery.terminal.min.js",
                "BeavisCli.Resources.js.jquery.mousewheel-min.js",
                "BeavisCli.Resources.js.angular.min.js",
                "BeavisCli.Resources.js.download.js",
                "BeavisCli.Resources.js.beaviscli.js"
            };

            string text = await ReadEmbeddedResourcesAsync(files);

            await WriteAsync(text, httpContext.Response, "application/javascript");
        }

        private static async Task RenderResponseAsync(Response response, HttpContext httpContext)
        {
            if (response.Messages.Any())
            {
                response.WriteEmptyLine();
            }

            response.OnSending();

            string text = JsonConvert.SerializeObject(response);

            await WriteAsync(text, httpContext.Response, "application/json");
        }

        private static async Task WriteAsync(string text, HttpResponse response, string contentType)
        {
            byte[] content = Encoding.UTF8.GetBytes(text);
            response.ContentType = contentType;
            response.StatusCode = (int)HttpStatusCode.OK;
            await response.Body.WriteAsync(content, 0, content.Length);
        }

        private static async Task<string> ReadEmbeddedResourcesAsync(params string[] files)
        {
            var buf = new StringBuilder();
            foreach (string file in files)
            {
                using (var stream = Assembly.GetAssembly(typeof(BeavisCliMiddleware)).GetManifestResourceStream(file))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var s = await reader.ReadToEndAsync();
                        buf.AppendLine(s);
                    }
                }
            }
            return buf.ToString();
        }
    }
}
