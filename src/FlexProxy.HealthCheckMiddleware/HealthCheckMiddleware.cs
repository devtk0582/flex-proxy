using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Threading.Tasks;

namespace FlexProxy.HealthCheckMiddleware
{
    public class HealthCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IOptions<HealthCheckOptions> _options;
        private readonly string _uuid;

        public HealthCheckMiddleware(RequestDelegate next, 
            ILoggerFactory loggerFactory, 
            IOptions<HealthCheckOptions> options)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<HealthCheckMiddleware>();
            _options = options;
            _uuid = Guid.NewGuid().ToString();
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers[HeaderNames.CacheControl] = "no-cache";
            context.Response.Headers[HeaderNames.Expires] = "0";
            context.Response.Headers[HeaderNames.Pragma] = "no-cache";
            context.Response.Headers[HeaderNames.ContentType] = "text/html";
            
            var result = PerformHealthCheck(context, _uuid);
            var html = GenerateResponsePage(context, result.Success, result);
            await WriteResponse(html, result.Success, context);
        }

        private async Task WriteResponse(string html, bool success, HttpContext context)
        {
            context.Response.StatusCode = success ? 200 : 503;
            await context.Response.WriteAsync(html);
        }

        private HealthCheckValidationResult PerformHealthCheck(HttpContext context, string uuid)
        {
            var content = GetHealthcheckContent(uuid);
            return ValidateResponse(content);
        }

        private ApiClientResult<string, string> GetHealthcheckContent(string uuid)
        {
            //TODO: performance actual health check for external apis
            var response = new ApiClientResult<string, string>();
            response.HttpStatusCode = HttpStatusCode.OK;
            response.ResponseTime = TimeSpan.FromSeconds(1);
            response.Result = "Success";

            return response;
        }

        private HealthCheckValidationResult ValidateResponse(ApiClientResult<string, string> response)
        {
            if (response.ErrorResult != null)
            {
                return new HealthCheckValidationResult() { Success = false, ResponseTime = response.ResponseTime, Reason = HealthCheckReason.ErrorCallingUrl };
            }

            response.Result = response.Result.Replace("\n", "");
            response.Result = response.Result.Replace("\r", "");

            if (!response.Result.StartsWith("<"))
            {
                return new HealthCheckValidationResult() { Success = false, ResponseTime = response.ResponseTime, Reason = HealthCheckReason.InvalidHtml };
            }
            var timerExceeded = response.ResponseTime.Seconds > _options.Value.MaxResponseTimeInSeconds;

            if (timerExceeded)
            {
                return new HealthCheckValidationResult() { Success = false, ResponseTime = response.ResponseTime, Reason = HealthCheckReason.SlowResponseTime };
            }

            //TODO: custom health check text from config
            var healthCheckText = "Success";

            if (string.IsNullOrEmpty(healthCheckText))
            {
                return new HealthCheckValidationResult() { Success = true, ResponseTime = response.ResponseTime };
            }

            if (response.Result.Contains(healthCheckText))
            {
                return new HealthCheckValidationResult() { Success = true, ResponseTime = response.ResponseTime };
            }

            return new HealthCheckValidationResult() { Success = false, ResponseTime = response.ResponseTime, Reason = HealthCheckReason.HealthCheckText };
        }

        private string GenerateResponsePage(HttpContext context, bool success, HealthCheckValidationResult result = null)
        {

            var successMessage = "";

            if (success)
            {
                successMessage = "Success";
            }
            else
            {
                successMessage = "Fail";
            }

            var html = $@"
                        <html>
	                        <head>
		                        <title>HealthCheck - {context.Request.Host.ToString().ToLower()}</title>
	                        </head>
	                        <body>
		                        <h1>Health Check - {successMessage}</h1>
		                        <h1>Response Time - {result?.ResponseTime}</h1>
		                        <h1>Reason for Failure - {result?.Reason}</h1>
                                <h2>{context.Request.Host.ToString().ToLower()}</h2>
	                        </body>
                        </html>";

            return html;
        }
    }
}
