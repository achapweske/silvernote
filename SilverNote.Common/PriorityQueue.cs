/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Common
{
    public class PriorityQueue<P, T> : ICollection, IEnumerable<T>, IEnumerable
    {
        private int _Count;

        public int Count
        {
            get { return _Count; }
        }

        public void Clear()
        {
            _Collection.Clear();
            _Count = 0;
        }

        public bool Contains(T item)
        {
            foreach (var node in _Collection)
            {
                if (node.Value.Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(T[] array, int index)
        {
            foreach (var node in _Collection)
            {
                node.Value.CopyTo(array, index);
                index += node.Value.Count;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {

        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var node in _Collection)
            {
                foreach (var item in node.Value)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enqueue(T item, P priority)
        {
            GetQueue(priority).Enqueue(item);
            _Count++;
        }

        public T Dequeue()
        {
            var node = _Collection.First();
            T result = node.Value.Dequeue();
            if (node.Value.Count == 0)
            {
                _Collection.Remove(node.Key);
            }
            _Count--;
            return result;
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        public T Peek()
        {
            var node = _Collection.First();
            return node.Value.Peek();
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        SortedDictionary<P, Queue<T>> _Collection = new SortedDictionary<P, Queue<T>>();

        private Queue<T> GetQueue(P priority)
        {
            Queue<T> result;
            if (!_Collection.TryGetValue(priority, out result))
            {
                result = new Queue<T>();
                _Collection.Add(priority, result);
            }

            return result;
        }
    }
}
