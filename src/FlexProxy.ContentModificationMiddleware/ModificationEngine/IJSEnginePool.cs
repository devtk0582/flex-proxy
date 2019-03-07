using System;
using System.Collections.Generic;
using System.Text;

namespace FlexProxy.ContentModificationMiddleware.ModificationEngine
{
    public interface IJSEnginePool
    {
        JSEngineInstance GetInstance();
        void ReleaseInstance(JSEngineInstance instance);
    }
}
