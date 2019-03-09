using FlexProxy.Core;
using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IConsoleLogApi
    {
        Task Error(string data, string message = null);
        Task Info(string data, string message = null);
        Task Debug(string data, string message = null);
        Task Critical(string data, string message = null);
    }
    public class ConsoleLogApi : IConsoleLogApi
    {
        private HttpClient _apiClient;
        private LogApiOptions _options;
        private ILogger _logger;
        private string _logApiUri;
        private List<string> _logLevels;
        private IHttpContextAccessor _httpContextAccessor;

        public ConsoleLogApi(ILoggerFactory loggerFactory,
            IOptions<ContentModificationOptions> options,
            IHttpContextAccessor httpContextAccessor)
        {
            _options = options.Value.LogApiOptions;
            _httpContextAccessor = httpContextAccessor;
            _logger = loggerFactory.CreateLogger<ConsoleLogApi>();
            var logApiUri = new Uri(_options.LogApiBaseUrl);
            _logApiUri = $"{logApiUri}api/consolelogs";
            _logLevels = _options.LogLevels.Split(',').ToList();
        }

        public async Task Debug(string data, string message = null)
        {
            await PostLogAsync(LogType.Debug, data, message).ConfigureAwait(false);
        }

        public async Task Error(string data, string message = null)
        {
            await PostLogAsync(LogType.Error, data, message).ConfigureAwait(false);
        }

        public async Task Info(string data, string message = null)
        {
            await PostLogAsync(LogType.Info, data, message).ConfigureAwait(false);
        }

        public async Task Critical(string data, string message = null)
        {
            await PostLogAsync(LogType.Critical, data, message).ConfigureAwait(false);
        }

        private async Task PostLogAsync(LogType type, string data, string message)
        {
            if (!_logLevels.Contains(type.ToString()))
            {
                return;
            }

            var logEntry = new ConsoleLog()
            {
                LogType = type,
                Data = data,
                Message = message
            };

            var response = await _apiClient.PostAsync(_logApiUri, 
                new StringContent(JsonConvert.SerializeObject(logEntry))).ConfigureAwait(false);

            if (response != null && !response.IsSuccessStatusCode)
            {
                _logger.LogError(JsonConvert.SerializeObject(response));
            }
        }
    }
}
