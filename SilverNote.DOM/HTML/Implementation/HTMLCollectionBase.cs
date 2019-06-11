/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.HTML.Internal
{
    public class HTMLCollectionBase : HTMLCollection
    {
        #region Fields

        Node[] _Items;

        #endregion

        #region Constructors

        public HTMLCollectionBase(IEnumerable<Node> items)
        {
            _Items = items.ToArray();
        }

        #endregion

        #region HTMLCollection

        public int Length
        {
            get { return _Items.Length; }
        }

        public Node this[int index] 
        {
            get
            {
                if (index >= 0 && index < _Items.Length)
                {
                    return _Items[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public Node NamedItem(string name)
        {
            for (int i = 0; i < _Items.Length; i++)
            {
                Node node = _Items[i];

                if (String.Equals(node.NodeName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }
            }

            return null;
        }

        #endregion
    }
}
