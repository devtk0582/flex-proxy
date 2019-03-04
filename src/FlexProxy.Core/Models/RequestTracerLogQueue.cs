using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlexProxy.Core.Models
{
    public class RequestTracerLogQueue<T>
    {
        private ConcurrentQueue<T> _logs = new ConcurrentQueue<T>();
        private ConcurrentQueue<T> _externalLogs = new ConcurrentQueue<T>();
        private readonly object _syncObject = new object();
        private int _capacity;

        public int Capacity { get { return _capacity; } }

        public RequestTracerLogQueue(int capacity)
        {
            _capacity = capacity;
        }

        public List<T> GetAll()
        {
            return _logs.ToList();
        }

        public void Enqueue(T obj)
        {
            _logs.Enqueue(obj);
            _externalLogs.Enqueue(obj);
            lock (_syncObject)
            {
                T overflow;
                while (_logs.Count > _capacity && _logs.TryDequeue(out overflow))
                {
                    continue;
                }
            }
        }

        public List<T> GetExternalLogs()
        {
            lock (_syncObject)
            {
                return _externalLogs.ToList();
            }
        }

        public void ClearExternalLogs()
        {
            lock (_syncObject)
            {
                T item;
                while (_externalLogs.Count > 0 && _externalLogs.TryDequeue(out item))
                {
                    continue;
                }
            }
        }
    }
}
