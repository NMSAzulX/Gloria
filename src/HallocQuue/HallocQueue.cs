using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Halloc
{
    public class HallocQueue<T> where T : AbstractHalloc
    {
        private ConcurrentQueue<T>[] _pubQueues;
        private ConcurrentQueue<T>[] _subQueues;
        private int _op;

        public HallocQueue()
        {
            _op = (int)Math.Ceiling(Hallocation.ProduceCount / 3 * 1.0);
            if ((_op & 1)==0)
            {
                _op -= 1;
            }
            _pubQueues = new ConcurrentQueue<T>[_op+1];
            _subQueues = new ConcurrentQueue<T>[_op+1];
            for (int i = 0; i <= _op; i+=1)
            {
                _pubQueues[i] = new ConcurrentQueue<T>();
                _subQueues[i] = new ConcurrentQueue<T>();
            }
        }

        public void PostResult(T item)
        {
            _subQueues[item.Index & _op].Enqueue(item);
        }


        public T GetResult()
        {
            for (int i = 0; i <= _op; i += 1)
            {
                if (!_subQueues[i].IsEmpty)
                {
                    T t;
                    _subQueues[i].TryDequeue(out t);
                    return t;
                }
            }
            return null;
        }


        public void PostTask(T item)
        {
            _pubQueues[item.Index & _op].Enqueue(item);
        }


        public T GetTask() 
        {
            for (int i = 0; i <= _op; i+=1)
            {
                if (!_pubQueues[i].IsEmpty)
                {
                    T t;
                    _pubQueues[i].TryDequeue(out t);
                    return t;
                }
            }
            return null;
        }

        public int Count {
            get
            {
                int result = 0;
                for (int i = 0; i < _pubQueues.Length; i+=1)
                {
                    result += _pubQueues[i].Count;
                }
                return result;
            }
        }
    }
}
