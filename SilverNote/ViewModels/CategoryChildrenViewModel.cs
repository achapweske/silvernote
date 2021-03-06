﻿/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace SilverNote.ViewModels
{
    class CategoryChildrenViewModel : CompositeCollection
    {
        IList<CategoryViewModel> _Parent;
        IList<CategoryViewModel> _Children;

        public CategoryChildrenViewModel(CategoryViewModel category)
        {
            _Parent = new List<CategoryViewModel>();
            _Parent.Add(new ParentCategoryViewModel(category));
            _Children = category.Children;
            Add(new CollectionContainer { Collection = _Children });

            var notifiable = _Children as INotifyCollectionChanged;
            if (notifiable != null)
            {
                notifiable.CollectionChanged += Children_CollectionChanged;
                Children_CollectionChanged(notifiable, null);
            }
        }

        protected void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (_Children.Count == 0)
            {
                // When children = 0, remove parent category from the list

                while (Count > 1)
                {
                    RemoveAt(0);
                }
            }
            else if (Count == 1)
            {
                // When children > 0, add parent category to the list

                Insert(0, new CollectionContainer { Collection = _Parent });
                Insert(1, new Separator());
            }
        }
    }
}
