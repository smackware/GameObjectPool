
using System.Collections;
using System.Collections.Generic;

namespace Smackware.ObjectPool

{ 
    public abstract class DynamicObjectPool<T> : IObjectPool<T> where T : UnityEngine.Object
    {
        private int _maxSize;        
        private Queue<T> _pool = new Queue<T>();
        private HashSet<T> _borrowed = new HashSet<T>();



        public DynamicObjectPool(T prefab, int maxSize)
        {
            _maxSize = maxSize;
        }

        public int BorrowedCount
        {
            get
            {
                return _borrowed.Count;
            }
        }

        public int MaxSize
        {
            get
            {
                return _maxSize;
            }
            set
            {
                _maxSize = value;
            }
        }

        public abstract T CreateNew();

        public int Size
        {
            get
            {
                return _pool.Count + _borrowed.Count;
            }
        }

        public T Borrow()
        {
            T obj;
            // First, check if we have something in the pool
            if (_pool.Count != 0)
            {
                obj = _pool.Dequeue();
                _borrowed.Add(obj);
                return obj;
            }
            // Otherwise we check if we can generate a new one
            if (Size >= _maxSize)
            {
                throw new MaxPoolSizeException();
            }
            // Generate a new object of the pool is empty
            obj = CreateNew();
            _borrowed.Add(obj);
            return obj;
        }

        public void Revert(T obj)
        {
            // Verify its a borrowed object and remove it from the borrowed set
            _borrowed.Remove(obj);
            _pool.Enqueue(obj);
        }


    }
}