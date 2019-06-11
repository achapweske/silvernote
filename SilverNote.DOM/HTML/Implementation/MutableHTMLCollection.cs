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
    public class MutableHTMLCollection : HTMLCollection
    {
        #region Fields

        List<Node> _Items;

        #endregion

        #region Constructors

        public MutableHTMLCollection()
        {
            _Items = new List<Node>();
        }

        public MutableHTMLCollection(IEnumerable<Node> items)
        {
            _Items = new List<Node>(items);
        }

        #endregion

        #region HTMLCollection

        public int Length
        {
            get { return _Items.Count; }
        }

        public Node this[int index] 
        {
            get
            {
                if (index >= 0 && index < _Items.Count)
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
            for (int i = 0; i < _Items.Count; i++)
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

        #region Extensions

        /// <summary>
        /// Add a node to the end of this list
        /// </summary>
        /// <param name="newNode">The node to be added</param>
        /// <returns>The added node</returns>
        public Node AppendNode(Node newNode)
        {
            _Items.Remove(newNode);

            _Items.Add(newNode);

            return newNode;
        }

        /// <summary>
        /// Inserts a node into this list.
        /// </summary>
        /// <param name="newNode">The node to be inserted</param>
        /// <param name="refNode">Insert newNode before this node. If null, newNode is added to the end of this list.</param>
        /// <returns>The node being inserted</returns>
        public Node InsertBefore(Node newNode, Node refNode)
        {
            if (refNode == null)
            {
                return AppendNode(newNode);
            }

            _Items.Remove(newNode);

            int index = _Items.IndexOf(refNode);

            if (index != -1)
            {
                _Items.Insert(index, newNode);
            }
            else
            {
                _Items.Add(newNode);
            }

            return newNode;
        }

        /// <summary>
        /// Remove a node from this list.
        /// </summary>
        /// <param name="oldNode">The node to be removed</param>
        /// <returns>The removed node</returns>
        /// <exception cref="DOMException">NOT_FOUND_ERR: Raised if oldNode is not found in this collection.</exception>
        public Node RemoveNode(Node oldNode)
        {
            if (!_Items.Remove(oldNode))
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }

            return oldNode;
        }

        #endregion
    }
}
