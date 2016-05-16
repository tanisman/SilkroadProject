namespace SCommon
{
    using System;
    using System.Collections.Concurrent;

    public class ObjectPool<T> where T : IDisposable
    {
        private ConcurrentBag<T> _objects;
        private Func<T> _objectGenerator;
        private Func<bool> _disposeCondition;

        public int Count => _objects.Count;

        public ObjectPool(Func<T> objectGenerator, Func<bool> disposeCondition)
        {
            if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");
            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
            _disposeCondition = disposeCondition;
        }

        public T GetObject()
        {
            T item;
            if (_objects.TryTake(out item)) return item;
            return _objectGenerator();
        }

        public void PutObject(T item)
        {
            if (_disposeCondition())
                item.Dispose();
            else
                _objects.Add(item);
        }

        public void Reserve(int num)
        {
            for (int i = 0; i < num; i++)
                _objects.Add(_objectGenerator());
        }

        public void Shrink(int fitSize)
        {
            if (_objects.Count > fitSize)
            {
                for (int i = 0; i < _objects.Count - fitSize; i++)
                {
                    T item;
                    if(_objects.TryTake(out item))
                        item.Dispose();
                }
            }
        }
    }

}
