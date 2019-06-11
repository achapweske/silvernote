using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.Internal
{
    /// <summary>
    /// An ordered collection of DOMObjects.
    /// 
    /// Items are accessible via a zero-based index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DOMList<T> : IEnumerable<T> where T : class
    {
        #region Constructors

        public DOMList()
        {

        }

        public DOMList(IEnumerable<T> items)
        {
            _Items = items.ToArray();
        }

        #endregion

        #region DOM Core

        private T[] _Items;

        /// <summary>
        /// Get the number of items in this collection.
        /// </summary>
        public virtual int Length
        {
            get
            {
                if (_Items != null)
                {
                    return _Items.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Get an item from this collection. 
        /// 
        /// If index >= Length, this returns null.
        /// </summary>
        /// <param name="index">0-based index of the item to retrieve</param>
        /// <returns>The retrieved item, or null if index is out of range.</returns>
        public virtual T this[int index]
        {
            get
            {
                if (_Items != null && index >= 0 && index < _Items.Length)
                {
                    return _Items[index];
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Triggered when this collection changes
        /// </summary>
        public event DOMCollectionChangedEventHandler<T> CollectionChanged;

        /// <summary>
        /// Suppress the CollectionChanged event
        /// </summary>
        protected bool SuppressCollectionChanged { get; set; }

        /// <summary>
        /// Raise the CollectionChanged event
        /// </summary>
        protected void RaiseCollectionChanged(T removedItem, T addedItem)
        {
            IList<T> removedItems;
            IList<T> addedItems;

            if (removedItem != null)
            {
                removedItems = new List<T>();
                removedItems.Add(removedItem);
            }
            else
            {
                removedItems = EmptyItems;
            }

            if (addedItem != null)
            {
                addedItems = new List<T>();
                addedItems.Add(addedItem);
            }
            else
            {
                addedItems = EmptyItems;
            }

            RaiseCollectionChanged(removedItems, addedItems);
        }

        /// <summary>
        /// Raise the CollectionChanged event
        /// </summary>
        protected void RaiseCollectionChanged(IList<T> removedItems, IList<T> addedItems)
        {
            OnCollectionChanged(removedItems, addedItems);

            if (!SuppressCollectionChanged && CollectionChanged != null)
            {
                CollectionChanged(this, new DOMCollectionChangedEventArgs<T>(removedItems, addedItems));
            }
        }

        #endregion

        #region IEnumerable

        public virtual IEnumerator<T> GetEnumerator()
        {
            int length = this.Length;

            for (int i = 0; i < length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation

        private IList<T> _EmptyItems;

        /// <summary>
        /// Get an empty list
        /// </summary>
        protected IList<T> EmptyItems
        {
            get
            {
                if (_EmptyItems == null)
                {
                    _EmptyItems = new T[0];
                }

                return _EmptyItems;
            }
        }

        /// <summary>
        /// Called after this collection has changed
        /// </summary>
        protected virtual void OnCollectionChanged(IList<T> removedItems, IList<T> addedItems)
        {

        }

        #endregion
    }

    public delegate void DOMCollectionChangedEventHandler<T>(object sender, DOMCollectionChangedEventArgs<T> e);

    public class DOMCollectionChangedEventArgs<T> : EventArgs
    {
        public DOMCollectionChangedEventArgs(IList<T> removedItems, IList<T> addedItems)
        {
            _RemovedItems = removedItems;
            _AddedItems = addedItems;
        }

        IList<T> _RemovedItems;
        IList<T> _AddedItems;

        public IList<T> RemovedItems
        {
            get { return _RemovedItems; }
        }

        public IList<T> AddedItems
        {
            get { return _AddedItems; }
        }
    }
}
