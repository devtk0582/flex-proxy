using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.ModificationEngine
{
    public interface IJSEngine : IDisposable
    {
        void Start();
        void Stop();
        void Execute(string code);
        TResult Evaluate<TResult>(string code);
        void AddHostObject(string objectName, object target);
        void AddHostType(Type type);
        void InitializeHtmlDocumentApi(HtmlDocument htmlDocument);
        void InitializeFormApi(IDictionary<string, List<string>> formFields, FormFileCollection files);
        void InitializeJavascriptApi(StringBuilder content);
        void InitializeJsonApi(StringBuilder content);
        void InitializeContentApi(StringBuilder content);
    }
}
