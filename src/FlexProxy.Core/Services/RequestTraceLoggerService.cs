using FlexProxy.Core.Models;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Timers;

namespace FlexProxy.Core.Services
{
    public interface IRequestTraceLoggerService
    {
        void Log(string logMessage);
    }

    public class RequestTraceLoggerService : IRequestTraceLoggerService
    {
        private readonly ILogger _logger;
        private RequestTracerLogQueue<ProcessLogHistoryEntry> LogHistory { get; set; }
        private readonly Timer _logTimer;
        private readonly ILog _traceLogger;
        private readonly string _domain;

        public RequestTraceLoggerService(IOptions<RequestTraceLoggerServiceOptions> options, 
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RequestTraceLoggerService>();
            GlobalContext.Properties["TraceLoggerName"] = $"RequestTraceLogs-{options.Value.ProcessName}.txt";
            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(),
               typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(repo, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "requestTraceLogger.xml")));
            _traceLogger = LogManager.GetLogger(repo.Name, options.Value.LoggerConfigName);
            LogHistory = new RequestTracerLogQueue<ProcessLogHistoryEntry>(50);
            _logTimer = new Timer(options.Value.Interval);
            _logTimer.Elapsed += OnLogTimer;
            _logTimer.Start();
        }

        public void Log(string logMessage)
        {
            var newLog = new ProcessLogHistoryEntry()
            {
                TimeStamp = DateTime.Now,
                LogText = logMessage
            };
            LogHistory.Enqueue(newLog);
        }

        private void OnLogTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                _logTimer.Stop();
                WriteLogs();
            }
            catch (Exception ex)
            {
                _logger.LogError("Trace Logger Error : " + ex.Message);
            }
            finally
            {
                _logTimer.Start();
            }
        }

        private void WriteLogs()
        {
            var logs = LogHistory.GetExternalLogs();
            foreach (var log in logs)
            {
                _traceLogger.Info($"\"Log Time\":\"{log.TimeStamp}\",{log.LogText}");
            }
            LogHistory.ClearExternalLogs();
        }
    }
}
