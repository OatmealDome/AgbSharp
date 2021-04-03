using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Util
{
    internal class UniqueQueue<T>
    {
        private Queue<T> Queue = new Queue<T>();
        private HashSet<T> AddedObjects = new HashSet<T>();

        public int Count
        {
            get
            {
                return Queue.Count;
            }
        }

        public void Enqueue(T item)
        {
            if (item == null)
            {
                throw new InvalidOperationException("Cannot add null to UniqueQueue");
            }

            if (AddedObjects.Contains(item))
            {
                return;
            }

            Queue.Enqueue(item);
            AddedObjects.Add(item);
        }

        public T Dequeue()
        {
            T item = Queue.Dequeue();

            AddedObjects.Remove(item);

            return item;
        }

        public bool TryDequeue(out T item)
        {
            if (Queue.Count > 0)
            {
                item = Dequeue();

                return true;
            }
            else
            {
                item = default(T);

                return false;
            }
        }

    }
}