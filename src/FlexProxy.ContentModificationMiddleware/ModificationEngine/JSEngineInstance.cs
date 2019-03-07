using FlexProxy.ContentModificationMiddleware.HostedObjects;
using FlexProxy.Core.Models;
using JavaScriptEngineSwitcher.ChakraCore;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.ModificationEngine
{
    public class JSEngineInstance : IDisposable
    {
        private ChakraCoreJsEngine _scriptEngine = new ChakraCoreJsEngine();

        private IRequestApi _requestApi;
        private IResponseApi _responseApi;
        private IConsoleLogApi _consoleLogApi;
        private HtmlDocumentApi _htmlDocumentApi;
        private FormApi _formApi;
        private JavascriptApi _javascriptApi;
        private JsonApi _jsonApi;
        private ContentApi _contentApi;

        public JSEngineInstance(IRequestApi requestApi, 
            IResponseApi responseApi,
            IConsoleLogApi consoleLogApi)
        {
            _requestApi = requestApi;
            _responseApi = responseApi;
            _consoleLogApi = consoleLogApi;
            _htmlDocumentApi = new HtmlDocumentApi();
            _formApi = new FormApi();
            _javascriptApi = new JavascriptApi();
            _jsonApi = new JsonApi();
            _contentApi = new ContentApi();

            AddHostObject("request", _requestApi);
            AddHostObject("response", _responseApi);
            AddHostObject("log", _consoleLogApi);
            AddHostObject("document", _htmlDocumentApi);
            AddHostObject("form", _formApi);
            AddHostObject("js", _javascriptApi);
            AddHostObject("json", _jsonApi);
            AddHostObject("content", _contentApi);
        }

        public void Execute(string code)
        {
            _scriptEngine.Execute(code);
        }

        public TResult Evaluate<TResult>(string code)
        {
            object result = _scriptEngine.Evaluate(code);

            return result == null ? default(TResult) : (TResult)result;
        }

        public void AddHostObject(string itemName, object target)
        {
            _scriptEngine.SetVariableValue(itemName, target);
        }

        public void AddHostType(Type type)
        {
            _scriptEngine.EmbedHostType(nameof(type), type);
        }

        public void Dispose()
        {
            _scriptEngine?.Dispose();
        }

        public void Clean()
        {
            _htmlDocumentApi?.CleanUp();
            _formApi?.CleanUp();
            _javascriptApi?.CleanUp();
            _jsonApi?.CleanUp();
            _contentApi?.CleanUp();
        }

        public void InitializeHtmlDocumentApi(HtmlDocument htmlDocument)
        {
            _htmlDocumentApi?.Initialize(htmlDocument);
        }

        public void InitializeFormApi(IDictionary<string, List<string>> formFields, FormFileCollection files)
        {
            _formApi?.Initialize(formFields, files);
        }

        public void InitializeJavascriptApi(StringBuilder content)
        {
            _javascriptApi?.Initialize(content);
        }

        public void InitializeJsonApi(StringBuilder content)
        {
            _jsonApi?.Initialize(content);
        }

        public void InitializeContentApi(StringBuilder content)
        {
            _contentApi?.Initialize(content);
        }
    }
}
