using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class MutableNodeList : NodeListBase
    {
        #region Fields

        List<Node> _Items;

        #endregion

        #region Constructors

        public MutableNodeList()
        {
            _Items = new List<Node>();
        }

        public MutableNodeList(IEnumerable<Node> items)
        {
            _Items = new List<Node>(items);
        }

        #endregion

        #region NodeList

        public override int Length
        {
            get { return _Items.Count; }
        }

        public override Node this[int index]
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

        #endregion

        #region Extensions

        /// <summary>
        /// Add a node to the end of this list
        /// </summary>
        /// <param name="newNode">The node to be added</param>
        /// <returns>The added node</returns>
        public Node AppendNode(Node newNode)
        {
            Node oldNode = null;

            if (_Items.Remove(newNode))
            {
                oldNode = newNode;
            }

            _Items.Add(newNode);

            RaiseCollectionChanged(oldNode, newNode);

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

            Node oldNode = null;

            if (_Items.Remove(newNode))
            {
                oldNode = newNode;
            }

            int index = _Items.IndexOf(refNode);
            if (index != -1)
            {
                _Items.Insert(index, newNode);
            }
            else
            {
                _Items.Add(newNode);
            }

            RaiseCollectionChanged(oldNode, newNode);

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

            RaiseCollectionChanged(oldNode, null);

            return oldNode;
        }

        #endregion
    }
}
