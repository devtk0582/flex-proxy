using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.ModificationEngine
{
    public class JSEngine : IJSEngine
    {
        private bool _disposed = false;
        private IJSEnginePool _jsEnginePool;
        private JSEngineInstance _engineInstance;

        public JSEngine(IJSEnginePool jsEnginePool)
        {
            _jsEnginePool = jsEnginePool;
        }

        public void Start()
        {
            if (_engineInstance != null)
            {
                return;
            }

            _engineInstance = _jsEnginePool.GetInstance();
        }


        public void Stop()
        {
            if (_engineInstance == null)
            {
                throw new InvalidOperationException("Stop() cannot be called when the engine is not running");
            }

            _jsEnginePool.ReleaseInstance(_engineInstance);

            _engineInstance = null;
        }


        public void Execute(string code)
        {
            if (_engineInstance == null)
            {
                throw new InvalidOperationException();
            }

            _engineInstance.Execute(code);
        }

        public TResult Evaluate<TResult>(string code)
        {
            if (_engineInstance == null)
            {
                throw new InvalidOperationException();
            }

            return _engineInstance.Evaluate<TResult>(code);
        }

        public void AddHostObject(string objectName, object target)
        {
            if (_engineInstance == null)
            {
                throw new InvalidOperationException();
            }

            _engineInstance.AddHostObject(objectName, target);
        }

        public void AddHostType(Type type)
        {
            if (_engineInstance == null)
            {
                throw new InvalidOperationException();
            }

            _engineInstance.AddHostType(type);
        }

        public void InitializeHtmlDocumentApi(HtmlDocument htmlDocument)
        {
            _engineInstance?.InitializeHtmlDocumentApi(htmlDocument);
        }

        public void InitializeFormApi(IDictionary<string, List<string>> formFields, FormFileCollection files)
        {
            _engineInstance?.InitializeFormApi(formFields, files);
        }

        public void InitializeJavascriptApi(StringBuilder content)
        {
            _engineInstance?.InitializeJavascriptApi(content);
        }

        public void InitializeJsonApi(StringBuilder content)
        {
            _engineInstance?.InitializeJsonApi(content);
        }

        public void InitializeContentApi(StringBuilder content)
        {
            _engineInstance?.InitializeContentApi(content);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }


        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_engineInstance != null)
                {
                    _jsEnginePool.ReleaseInstance(_engineInstance);
                }

                _disposed = true;
            }
        }

        ~JSEngine()
        {
            Dispose(false);
        }
    }
}
