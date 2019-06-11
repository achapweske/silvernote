/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Data;
using SilverNote.Common;
using SilverNote.Models;
using System.Collections.Specialized;
using System.Collections;

namespace SilverNote.ViewModels
{
    public abstract class ViewModelBase<M, VM> : ObservableObject
        where M : class
        where VM : ViewModelBase<M, VM>, new()
    {
        /// <summary>
        /// Do not instantiate a ViewModel directly - use FromModel() instead
        /// </summary>
        protected ViewModelBase()
        {
            
        }

        /// <summary>
        /// Internal initialization occurs here
        /// </summary>
        /// <param name="model"></param>
        protected void Initialize(M model)
        {
            Model = model;

            var notifyable = model as INotifyPropertyChanged;
            if (notifyable != null)
            {
                notifyable.PropertyChanged += Model_PropertyChanged;
            }

            OnInitialize();
        }

        /// <summary>
        /// Implement this in derived classes to perform initialization
        /// </summary>
        protected virtual void OnInitialize()
        {

        }

        #region Factory

        /// <summary>
        /// The set of all ViewModels, indexed by their corresponding Models
        /// </summary>
        public static Dictionary<M, VM> ViewModels = new Dictionary<M, VM>();

        /// <summary>
        /// Get a ViewModel corresponding to the given Model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static VM FromModel(M model)
        {
            if (model == null)
            {
                return null;
            }

            VM viewModel;
            if (!ViewModels.TryGetValue(model, out viewModel))
            {
                viewModel = new VM();
                ViewModels.Add(model, viewModel);
                viewModel.Initialize(model);
            }
            return viewModel;
        }

        public static IList<VM> FromCollection(IList<M> models)
        {
            if (models == null)
            {
                return null;
            }
            
            if (models is INotifyCollectionChanged)
            {
                return new ViewModelCollectionProxy<M, VM>(models);
            }

            return models.Select(model => FromModel(model)).ToList();
        }
		

        #endregion

        #region Model

        /// <summary>
        /// The Model corresponding to this ViewModel
        /// </summary>
        public M Model { get; private set; }

        /// <summary>
        /// Called when a property of our corresponding Model has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnModelPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Called when a property of our corresponding Model has changed
        /// 
        /// Proxy ViewModels should override this and call:
        /// 
        ///   RaisePropertyChanged(propertyName)
        ///   
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed</param>
        protected virtual void OnModelPropertyChanged(string propertyName)
        {
            // Proxy the event

            if (GetType().GetProperty(propertyName) != null)
            {
                RaisePropertyChanged(propertyName);
            }
        }

        #endregion
    }

    public class ViewModelCollectionProxy<M, VM> : IList<VM>, ICollection<VM>, IEnumerable<VM>, IList, ICollection, IEnumerable, INotifyCollectionChanged
        where M : class
        where VM : ViewModelBase<M, VM>, new()
    {
        private IList<M> _Collection;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ViewModelCollectionProxy()
        {
        }

        public ViewModelCollectionProxy(IList<M> collection)
        {
            _Collection = collection;

            var notifiable = collection as INotifyCollectionChanged;
            if (notifiable != null)
            {
                notifiable.CollectionChanged += Collection_CollectionChanged;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }
        
        public bool IsReadOnly
        {
            get
            {
                return _Collection.IsReadOnly;
            }
        }
        
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return null;
            }
        }

        public int Count
        {
            get
            {
                return _Collection.Count;
            }
        }

        public VM this[int index]
        {
            get
            {
                return ViewModelBase<M, VM>.FromModel(_Collection[index]);
            }
            set
            {
                _Collection[index] = value.Model;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (VM)((object)value);
            }
        }

        
        public void Add(VM item)
        {
            _Collection.Add(item.Model);
        }

        int System.Collections.IList.Add(object item)
        {
            int count = Count;
            Add((VM)((object)item));
            return count;
        }

        public void Clear()
        {
            _Collection.Clear();
        }

        public bool Contains(VM item)
        {
            return _Collection.Contains(item.Model);
        }

        bool IList.Contains(object item)
        {
            return Contains((VM)((object)item));
        }

        public void CopyTo(VM[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex++] = this[i];
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array.SetValue(this[i], arrayIndex++);
            }
        }

        public IEnumerator<VM> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(VM value)
        {
            return _Collection.IndexOf(value.Model);
        }

        int IList.IndexOf(object value)
        {
            if (value is VM)
            {
                return IndexOf((VM)((object)value));
            }
            return -1;
        }

        public void Insert(int index, VM item)
        {
            _Collection.Insert(index, item.Model);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (VM)((object)value));
        }

        public bool Remove(VM item)
        {
            return _Collection.Remove(item.Model);
        }

        void IList.Remove(object value)
        {
            Remove((VM)((object)value));
        }

        public void RemoveAt(int index)
        {
            _Collection.RemoveAt(index);
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs args = null;

            IList newItems = null;
            if (e.NewItems != null)
            {
                newItems = ViewModelBase<M, VM>.FromCollection(e.NewItems.OfType<M>().ToList()).ToArray<VM>();
            }
            IList oldItems = null;
            if (e.OldItems != null)
            {
                oldItems = ViewModelBase<M, VM>.FromCollection(e.OldItems.OfType<M>().ToList()).ToArray<VM>();
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    args = new NotifyCollectionChangedEventArgs(e.Action, (IList)newItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    args = new NotifyCollectionChangedEventArgs(e.Action, (IList)oldItems, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    args = new NotifyCollectionChangedEventArgs(e.Action, (IList)newItems, (IList)oldItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    args = new NotifyCollectionChangedEventArgs(e.Action, (IList)newItems, e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    args = new NotifyCollectionChangedEventArgs(e.Action);
                    break;
            }

            RaiseCollectionChanged(args);
        }
    }

    public class ViewModelConverter<M, VM> : IValueConverter
        where M : class, INotifyPropertyChanged
        where VM : ViewModelBase<M, VM>, new()
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ViewModelBase<M, VM>.FromModel(value as M);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModel = value as VM;
            if (viewModel != null)
            {
                return viewModel.Model;
            }
            else
            {
                return null;
            }
        }
    }

    public class ViewModelsConverter<M, VM> : IValueConverter
        where M : class, INotifyPropertyChanged
        where VM : ViewModelBase<M, VM>, new()
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ViewModelBase<M, VM>.FromCollection(value as IList<M>);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModels = value as IEnumerable<VM>;
            if (viewModels == null)
            {
                return null;
            }

            var models = new List<M>();
            foreach (var viewModel in viewModels)
            {
                models.Add(viewModel.Model);
            }

            return models.ToArray();
        }
    }
}
