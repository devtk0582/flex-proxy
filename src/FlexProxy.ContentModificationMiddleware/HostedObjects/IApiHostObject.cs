using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.HostedObjects
{
    public interface IApiHostObject
    {
        void CleanUp();
    }
}
