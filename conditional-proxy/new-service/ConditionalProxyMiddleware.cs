using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace new_service
{
    public class ConditionalProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;

        public ConditionalProxyMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _httpClient = httpClientFactory.CreateClient();
        }
        
        public async Task Invoke(HttpContext httpContext)
        {
            Program.GoToLegacy = !Program.GoToLegacy;
            
            if (Program.GoToLegacy)
            {
                var request = httpContext.Request;
                
                var path = request.Path;
                var uri = new Uri($"http://localhost:5555{path.Value}");
                
                var requestMessage = new HttpRequestMessage();
                var requestMethod = request.Method;
                if (!HttpMethods.IsGet(requestMethod) &&
                    !HttpMethods.IsHead(requestMethod) &&
                    !HttpMethods.IsDelete(requestMethod) &&
                    !HttpMethods.IsTrace(requestMethod))
                {
                    var streamContent = new StreamContent(request.Body);
                    requestMessage.Content = streamContent;
                }
                
                // copy headers
                foreach (var header in request.Headers)
                {
                    if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                    {
                        requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                requestMessage.Headers.Host = uri.Authority;
                requestMessage.RequestUri = uri;
                requestMessage.Method = new HttpMethod(request.Method);
                
                var responseMessage = await _httpClient.SendAsync(requestMessage,  HttpCompletionOption.ResponseHeadersRead, httpContext.RequestAborted);

                var response = httpContext.Response;
                response.StatusCode = (int) responseMessage.StatusCode;
                foreach (var header in responseMessage.Headers)
                {
                    response.Headers[header.Key] = header.Value.ToArray();
                }
                
                foreach (var header in responseMessage.Content.Headers)
                {
                    response.Headers[header.Key] = header.Value.ToArray();
                }

                // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                response.Headers.Remove("transfer-encoding");
                
                await using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
                await responseStream.CopyToAsync(response.Body, 81920, httpContext.RequestAborted);
            }
            else
                await _next(httpContext);
        }
    }
}