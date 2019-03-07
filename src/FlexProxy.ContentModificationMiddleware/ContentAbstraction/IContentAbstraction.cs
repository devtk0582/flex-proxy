using FlexProxy.ContentModificationMiddleware.ModificationEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ContentAbstraction
{
    public interface IContentAbstraction
    {
        Task<Stream> ReadAsStream();

        string[] ContentTypes { get; }

        Task Register(ModificationContext modificationContext, IJSEngine engine);

        Task<bool> ValidateAsync(ModificationContext content);
    }
}
