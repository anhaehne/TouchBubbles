using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace TouchBubbles.Server.Utils
{
    /// <summary>
    /// Middle ware to remove brotli from the accepted encoding when the request source is the ingress reverse proxy.
    /// The reverse proxy will forward the accepted encoding containing brotli even though, brotli is not supported.
    /// </summary>
    public class AcceptEncodingMiddleware
    {
        private static readonly IPAddress _ingressHost = new IPAddress(new byte[] { 172, 30, 32, 2 });
        private readonly RequestDelegate _next;
        private readonly ILogger<AcceptEncodingMiddleware> _logger;

        public AcceptEncodingMiddleware(RequestDelegate next, ILogger<AcceptEncodingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString().Split(",").Select(x => x.Trim()).ToList();

            // Check if the request source is the ingress reverse proxy and the accept encoding contains brotli (br). If so remove it from the list.
            if (Equals(context.Connection.RemoteIpAddress.MapToIPv4(), _ingressHost) && acceptEncoding.Contains("br"))
                context.Request.Headers["Accept-Encoding"] =
                    new StringValues(string.Join(", ", acceptEncoding.Except(new[] { "br" })));
            
            await _next(context);
        }
    }
}