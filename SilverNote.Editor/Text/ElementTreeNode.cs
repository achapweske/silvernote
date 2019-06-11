/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote.Editor
{
    public class ElementTreeNode
    {
        #region Fields

        readonly UIElement _Owner;
        readonly Func<UIElement, ElementTreeNode> _Selector;
        readonly ObservableCollection<UIElement> _Children;

        #endregion

        #region Constructors

        public ElementTreeNode(UIElement owner, Func<UIElement, ElementTreeNode> selector)
        {
            _Owner = owner;
            _Selector = selector;
            _Children = new ObservableCollection<UIElement>();
        }

        #endregion

        #region Properties

        public UIElement Parent { get; set; }

        public ObservableCollection<UIElement> Children
        {
            get { return _Children; }
        }

        public IEnumerable<UIElement> SelfAndDescendants
        {
            get
            {
                yield return _Owner;

                foreach (var descendant in Descendants)
                {
                    yield return descendant;
                }
            }
        }

        public IEnumerable<UIElement> Descendants
        {
            get
            {
                foreach (var child in Children)
                {
                    yield return child;

                    var node = _Selector(child);
                    if (node != null)
                    {
                        foreach (var descendant in node.Descendants)
                        {
                            yield return descendant;
                        }
                    }
                }
            }
        }

        #endregion

    }
}
