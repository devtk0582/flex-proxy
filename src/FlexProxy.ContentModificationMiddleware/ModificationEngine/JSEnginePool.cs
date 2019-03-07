using FlexProxy.ContentModificationMiddleware.HostedObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FlexProxy.ContentModificationMiddleware.ModificationEngine
{
    public class JSEnginePool : IJSEnginePool
    {
        private ConcurrentQueue<JSEngineInstance> _availableEngines = new ConcurrentQueue<JSEngineInstance>();

        private IRequestApi _requestApi;
        private IResponseApi _responseApi;
        private IConsoleLogApi _consoleLogApi;

        public JSEnginePool(IRequestApi requestApi, 
            IResponseApi responseApi, 
            IConsoleLogApi consoleLogApi)
        {
            _requestApi = requestApi;
            _responseApi = responseApi;
            _consoleLogApi = consoleLogApi;
        }

        public JSEngineInstance GetInstance()
        {
            JSEngineInstance engineToReturn;

            if (_availableEngines.TryDequeue(out engineToReturn))
            {
                return engineToReturn;
            }

            return new JSEngineInstance(_requestApi, _responseApi, _consoleLogApi);
        }

        public void ReleaseInstance(JSEngineInstance instance)
        {
            instance.Clean();

            Task.Run(() => _availableEngines.Enqueue(instance));
        }
    }
}
