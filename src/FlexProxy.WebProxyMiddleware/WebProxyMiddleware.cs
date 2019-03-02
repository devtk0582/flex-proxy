using FlexProxy.Core;
using FlexProxy.Core.Models;
using FlexProxy.WebProxyMiddleware.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Threading.Tasks;

namespace FlexProxy.WebProxyMiddleware
{
    public class WebProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IOptions<WebProxyOptions> _webProxyoptions;
        private readonly IOptions<HostMappingOptions> _hostMappingOptions;
        private readonly IRequestBuilder _requestBuilder;
        private readonly IResponseBuilder _responseBuilder;

        public WebProxyMiddleware(RequestDelegate next, 
            ILoggerFactory loggerFactory, 
            IOptions<WebProxyOptions> webProxyOptions, 
            IOptions<HostMappingOptions> hostMappingOptions,
            IRequestBuilder requestBuilder, 
            IResponseBuilder responseBuilder)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<WebProxyMiddleware>();
            _webProxyoptions = webProxyOptions;
            _hostMappingOptions = hostMappingOptions;
            _requestBuilder = requestBuilder;
            _responseBuilder = responseBuilder;

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalRequest = context.Request;

            var mapping = _hostMappingOptions.Value;

            var reqPath = $"{mapping.DownstreamScheme}{new HostString(mapping.DownstreamHost).ToUriComponent()}{context.Request.Path.ToUriComponent()}{context.Request.QueryString.ToUriComponent()}";

            var remoteRequest = WebRequest.CreateHttp(reqPath);

            var transportItems = context.Items[Constants.HTTP_CONTEXT_TRANSPORT_ITEMS] as TransportItems;

            ProcessTransportItems(transportItems, remoteRequest);

            _requestBuilder.CopyHeaders(context, remoteRequest, mapping);
            _requestBuilder.CopyCookies(context, remoteRequest, mapping, transportItems?.Cookies, _webProxyoptions.Value.InternalCookies);

            if (originalRequest.Body != null
                && (originalRequest.Method.Equals(HttpMethods.Patch, StringComparison.OrdinalIgnoreCase)
                || originalRequest.Method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase)
                || originalRequest.Method.Equals(HttpMethods.Put, StringComparison.OrdinalIgnoreCase)
                || originalRequest.Method.Equals(HttpMethods.Options, StringComparison.OrdinalIgnoreCase)))
            {
                using (var remoteRequestStream = remoteRequest.GetRequestStream())
                {
                    originalRequest.Body.CopyTo(remoteRequestStream);
                }
            }

            HttpWebResponse remoteResponse = null;

            try
            {
                remoteResponse = remoteRequest.GetResponse() as HttpWebResponse;

                await ProcessRemoteResponse(context, remoteResponse, mapping);
            }
            catch (WebException we)
            {
                remoteResponse = (HttpWebResponse)we.Response;

                await ProcessRemoteResponse(context, remoteResponse, mapping);
            }
            finally
            {
                remoteRequest = null;
                remoteResponse?.Dispose();
            }
        }

        private async Task ProcessRemoteResponse(HttpContext context, HttpWebResponse remoteResponse, HostMappingOptions mapping)
        {
            if (remoteResponse == null)
            {
                _logger.LogError($"Unable to get response from {context.Request.Scheme}{context.Request.Host.ToString().ToLower()}{context.Request.Path}{context.Request.QueryString}");

                await context.Response.WriteAsync("Unable to process the request");

                return;
            }

            await _next.Invoke(context);

            LogRemoteResponse(context, remoteResponse);

            context.Response.StatusCode = (int)remoteResponse.StatusCode;

            _responseBuilder.CopyHeaders(context, remoteResponse, mapping);

            if (remoteResponse.StatusCode == HttpStatusCode.NoContent || remoteResponse.StatusCode == HttpStatusCode.Found)
            {
                return;
            }

            context.Response.Body.Write(new byte[0], 0, 0);

            using (var remoteResponseStream = remoteResponse.GetResponseStream())
            {
                remoteResponseStream?.CopyTo(context.Response.Body);
            }
        }

        private void LogRemoteResponse(HttpContext context, HttpWebResponse remoteResponse)
        {
            int statusCode = (int)remoteResponse.StatusCode;
            if (statusCode < 400 || statusCode > 500)
            {
                return;
            }

            var requestedWithHeader = context.Request.Headers[Constants.HTTP_REQUEST_HEADER_REQUESTED_WITH];

            if (!string.IsNullOrEmpty(requestedWithHeader) && requestedWithHeader == Constants.AJAX_HEADER_IDENTIFIER)
            {
                _logger.LogInformation($"Remote Ajax Exception: {statusCode} -  {remoteResponse.StatusDescription}");
            }
            else
            {
                _logger.LogInformation($"Remote Exception: {statusCode} -  {remoteResponse.StatusDescription}");
            }
        }

        private void ProcessTransportItems(TransportItems transportItems, HttpWebRequest remoteRequest)
        {
            if (transportItems == null)
            {
                return;
            }

            foreach (var header in transportItems.Headers)
            {
                switch (header)
                {
                    case HeaderNames.Expect:
                        remoteRequest.ServicePoint.Expect100Continue = false;
                        break;
                }
            }
        }

    }
}
