using FlexProxy.ContentModificationMiddleware.ContentAbstractionProvider;
using FlexProxy.Core.Models;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware
{
    public class ContentModificationOptions
    {
        private ContentAbstractionProviderCollection _abstractions;
        private FormOptions _formOptions;
        private LogApiOptions _logApiOptions;

        public ContentModificationOptions()
        {
            _abstractions = new ContentAbstractionProviderCollection();
            _formOptions = new FormOptions();
            _logApiOptions = new LogApiOptions();
        }

        public ContentAbstractionProviderCollection ContentProviders => _abstractions;

        public FormOptions FormOptions => _formOptions;

        public LogApiOptions LogApiOptions => _logApiOptions;
    }
}
