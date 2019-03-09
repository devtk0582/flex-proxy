using FlexProxy.ContentModificationMiddleware.ContentAbstraction;
using FlexProxy.ContentModificationMiddleware.ContentAbstractionProviders;
using FlexProxy.ContentModificationMiddleware.HostedObjects;
using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace FlexProxy.ContentModificationMiddleware
{
    public class ContentModificationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IContentAbstractionProvider _contentAbstractionProvider;
        private readonly ILogger _logger;
        private readonly IOptions<ContentModificationOptions> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRequestApi _requestApi;
        private readonly IResponseApi _responseApi;
        private readonly IConsoleLogApi _consoleLogApi;
        private readonly IOptions<ModifierOptions> _modifierOptions;

        public ContentModificationMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            IOptions<ContentModificationOptions> options,
            IContentAbstractionProvider contentAbstractionProvider,
            IServiceProvider serviceProvider,
            IRequestApi requestApi,
            IResponseApi responseApi,
            IConsoleLogApi consoleLogApi,
            IOptions<ModifierOptions> modifierOptions)
        {
            _logger = loggerFactory.CreateLogger<ContentModificationMiddleware>();
            _next = next;
            _options = options;
            _contentAbstractionProvider = contentAbstractionProvider;
            _serviceProvider = serviceProvider;
            _requestApi = requestApi;
            _responseApi = responseApi;
            _consoleLogApi = consoleLogApi;
            _modifierOptions = modifierOptions;
        }

        public async Task Invoke(HttpContext context)
        {

            var originalRequestBodyStream = context.Request.Body;
            var originalResponseBodyStream = context.Response.Body;

            var requestModifiers = GetModifiers(context, RequestPhase.Request);

            if (requestModifiers != null && requestModifiers.Any() &&
                (
                    context.Request.Method.Equals(HttpMethods.Patch, StringComparison.OrdinalIgnoreCase) ||
                    context.Request.Method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase) ||
                    context.Request.Method.Equals(HttpMethods.Put, StringComparison.OrdinalIgnoreCase) ||
                    context.Request.Method.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase) ||
                    context.Request.Method.Equals(HttpMethods.Options, StringComparison.OrdinalIgnoreCase)
                ))
            {
                var requestModificationContext = await GetModificationContext(context, RequestPhase.Request);

                var requestContentAbstraction = await _contentAbstractionProvider.GetAbstractionProviderAsync(requestModificationContext);

                await ExecuteRules(context, requestContentAbstraction, requestModificationContext, requestModifiers);

                await UpdateRequestStream(context, requestContentAbstraction);
            }

            using (MemoryStream responseStreamBuffer = new MemoryStream())
            {
                context.Response.Body = responseStreamBuffer;

                await _next.Invoke(context);

                var responseModificationContext = await GetModificationContext(context, RequestPhase.Response);

                var responseContentAbstraction = await _contentAbstractionProvider.GetAbstractionProviderAsync(responseModificationContext);

                var responseModifiers = GetModifiers(context, RequestPhase.Response);

                if (responseContentAbstraction == null || responseModifiers == null || !responseModifiers.Any())
                {
                    await responseModificationContext.ContentStream.CopyToAsync(originalResponseBodyStream);
                }
                else
                {
                    await ExecuteRules(context, responseContentAbstraction, responseModificationContext, responseModifiers);

                    await UpdateResponseStream(context, responseContentAbstraction, originalResponseBodyStream);
                }

                context.Request.Body = originalRequestBodyStream;
                context.Response.Body = originalResponseBodyStream;
            }
        }

        private IEnumerable<JavascriptModifier> GetModifiers(HttpContext context, RequestPhase phase)
        {
            var modifierCollection = _modifierOptions.Value.Modifiers.Where(modifier => modifier.RequestPhase == phase);

            if (phase == RequestPhase.Request)
            {
                modifierCollection = string.IsNullOrEmpty(context.Request.ContentType)
                    ? modifierCollection.Where(modifier => string.IsNullOrEmpty(modifier.TargetContentType))
                    : modifierCollection.Where(modifier => !string.IsNullOrEmpty(modifier.TargetContentType) && context.Request.ContentType.Contains(modifier.TargetContentType));
            }
            else if (phase == RequestPhase.Response)
            {
                modifierCollection = string.IsNullOrEmpty(context.Response.ContentType)
                    ? modifierCollection.Where(modifier => string.IsNullOrEmpty(modifier.TargetContentType))
                    : modifierCollection.Where(modifier => !string.IsNullOrEmpty(modifier.TargetContentType) && context.Response.ContentType.Contains(modifier.TargetContentType));
            }

            return modifierCollection.OrderBy(modifier => modifier.Priority);
        }

        private async Task ExecuteRules(HttpContext context, IContentAbstraction contentAbstraction, ModificationContext modificationContext, IEnumerable<JavascriptModifier> modifiers)
        {
            using (var jsEngine = _serviceProvider.GetService<IJSEngine>())
            {
                jsEngine.Start();

                await contentAbstraction.Register(modificationContext, jsEngine);

                foreach (var modifier in modifiers)
                {
                    try
                    {
                        bool executeModificationFunction = string.IsNullOrEmpty(modifier.DiscoveryFunction) || jsEngine.Evaluate<bool>(modifier.DiscoveryFunction) ? true : false;

                        if (executeModificationFunction)
                        {
                            jsEngine.Execute(modifier.ModificationFunction);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Rule Exception: {ex}");
                    }
                }
            }
        }

        private async Task<ModificationContext> GetModificationContext(HttpContext context, RequestPhase phase)
        {
            ModificationContext modificationContext = null;

            if (phase == RequestPhase.Request)
            {
                var requestBodyBuffer = new MemoryStream();

                await context.Request.Body.CopyToAsync(requestBodyBuffer);

                requestBodyBuffer.Seek(0, SeekOrigin.Begin);

                return new ModificationContext(requestBodyBuffer, context.Request.ContentType);
            }

            if (phase == RequestPhase.Response)
            {
                var responseStreamBuffer = context.Response.Body;

                responseStreamBuffer.Position = 0;

                return new ModificationContext(responseStreamBuffer, context.Response.ContentType);
            }

            return modificationContext;
        }

        private async Task UpdateRequestStream(HttpContext context, IContentAbstraction contentAbstraction)
        {
            var finalOutputStream = await contentAbstraction.ReadAsStream();

            finalOutputStream.Position = 0;

            context.Request.Body = finalOutputStream;

            context.Request.Headers.Remove(HeaderNames.ContentLength);

            context.Request.ContentLength = finalOutputStream.Length;
        }

        private async Task UpdateResponseStream(HttpContext context, IContentAbstraction contentAbstraction, Stream originalResponseBodyStream)
        {
            var finalResponseStream = await contentAbstraction.ReadAsStream();

            context.Response.Headers.Remove(HeaderNames.ContentLength);

            context.Response.ContentLength = finalResponseStream.Length;

            await finalResponseStream.CopyToAsync(originalResponseBodyStream);
        }
    }
}
