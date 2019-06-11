using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class DynamicNodeList : NodeList, IEnumerable<Node>
    {
        #region Fields

        Element _RootNode;
        Element _CurrentNode;
        int _CurrentIndex;
        string _TagName;
        string _NamespaceURI;
        string _LocalName;
        int _Length;

        #endregion

        #region Constructors

        public DynamicNodeList(Element rootNode, string tagName)
            : this(rootNode)
        {
            _TagName = tagName;
        }

        public DynamicNodeList(Element rootNode, string namespaceURI, string localName)
            : this(rootNode)
        {
            _NamespaceURI = namespaceURI;
            _LocalName = localName;
        }

        DynamicNodeList(Element rootNode)
        {
            _RootNode = rootNode;
            _CurrentNode = null;
            _CurrentIndex = -1;
            _Length = -1;
        }

        #endregion

        #region NodeList

        public int Length
        {
            get 
            {
                if (_Length == -1)
                {
                    _Length = Count();
                }

                return _Length; 
            }
        }

        public Node this[int index]
        {
            get { return NodeAt(index); }
        }

        #endregion

        #region IEnumerable

        public IEnumerator<Node> GetEnumerator()
        {
            if (_TagName == "*")
            {
                Element element = _RootNode;

                while (element != null)
                {
                    yield return element;

                    element = NextElement(_RootNode, element);
                }
            }
            else if (_TagName != null)
            {
                Element element = FindFirst(_RootNode, _TagName);

                while (element != null)
                {
                    yield return element;

                    element = FindNext(_RootNode, element, _TagName);
                }
            }
            else
            {
                Element element = FindFirst(_RootNode, _NamespaceURI, _LocalName);

                while (element != null)
                {
                    yield return element;

                    element = FindNext(_RootNode, element, _NamespaceURI, _LocalName);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Get the number of elements associated with this list
        /// 
        /// The search begins at _RootNode.
        /// </summary>
        /// <returns></returns>
        int Count()
        {
            if (_TagName != null)
            {
                return Count(_RootNode, _TagName);
            }
            else
            {
                return Count(_RootNode, _NamespaceURI, _LocalName);
            }
        }

        /// <summary>
        /// Get an element associated with this list
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Node NodeAt(int index)
        {
            if (index == _CurrentIndex)
            {
                return _CurrentNode;
            }
            else if (index == 0)
            {
                _CurrentNode = FindFirst();
            }
            else if (index == _CurrentIndex + 1)
            {
                _CurrentNode = FindNext();
            }
            else
            {
                _CurrentNode = FindNth(index);
            }

            _CurrentIndex = index;

            return _CurrentNode;
        }

        /// <summary>
        /// Find the first element associated with this list.
        /// 
        /// The search begins at _RootNode.
        /// </summary>
        /// <returns>The first matching element, or null if none</returns>
        Element FindFirst()
        {
            if (_TagName == "*")
            {
                return _RootNode;
            }
            else if (_TagName != null)
            {
                return FindFirst(_RootNode, _TagName);
            }
            else
            {
                return FindFirst(_RootNode, _NamespaceURI, _LocalName);
            }
        }

        /// <summary>
        /// Find the next element associated with this list.
        /// 
        /// The search begins at _CurrentNode.
        /// </summary>
        /// <returns>The next matching element, or null if none</returns>
        Element FindNext()
        {
            if (_CurrentNode == null)
            {
                return null;
            }

            if (_TagName == "*")
            {
                return NextElement(_RootNode, _CurrentNode);
            }
            else if (_TagName != null)
            {
                return FindNext(_RootNode, _CurrentNode, _TagName);
            }
            else
            {
                return FindNext(_RootNode, _CurrentNode, _NamespaceURI, _LocalName);
            }
        }

        /// <summary>
        /// Find the nth element associated with this list.
        /// 
        /// The search begins at _RootNode
        /// </summary>
        /// <param name="n"></param>
        /// <returns>The nth matching element, or null if none</returns>
        Element FindNth(int n)
        {
            if (_TagName == "*")
            {
                return NthElement(_RootNode, n);
            }
            else if (_TagName != null)
            {
                return FindNth(_RootNode, n, _TagName);
            }
            else
            {
                return FindNth(_RootNode, n, _NamespaceURI, _LocalName);
            }
        }

        /// <summary>
        /// Get the number of elements with the given tagName
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="tagName">Element name to search for</param>
        /// <returns></returns>
        static int Count(Element root, string tagName)
        {
            int result = 0;

            Element element = FindFirst(root, tagName);

            while (element != null)
            {
                result++;

                element = FindNext(root, element, tagName);
            }

            return result;
        }

        /// <summary>
        /// Get the number of elements with the given namespace URI and local name
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="namespaceURI">Namespace URI to search for</param>
        /// <param name="localName">Element name to search for</param>
        /// <returns></returns>
        static int Count(Element root, string namespaceURI, string localName)
        {
            int result = 0;

            Element element = FindFirst(root, namespaceURI, localName);

            while (element != null)
            {
                result++;

                element = FindNext(root, element, namespaceURI, localName);
            }

            return result;
        }

        /// <summary>
        /// Get the first element with the given tagName
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="tagName">Element name to search for</param>
        /// <returns>The first matching element, or null if none</returns>
        static Element FindFirst(Element root, string tagName)
        {
            if (String.Equals(root.TagName, tagName, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            return FindNext(root, root, tagName);
        }

        /// <summary>
        /// Get the first element with the given namespace URI and local name
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="namespaceURI">Namespace URI to search for</param>
        /// <param name="localName">Element name to search for</param>
        /// <returns>The first matching element, or null if none</returns>
        static Element FindFirst(Element root, string namespaceURI, string localName)
        {
            if (String.Equals(root.NamespaceURI, namespaceURI, StringComparison.OrdinalIgnoreCase) &&
                String.Equals(root.LocalName, localName, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            return FindNext(root, root, namespaceURI, localName);
        }

        /// <summary>
        /// Get the next element with the given tagName
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="element">Begin the search here.</param>
        /// <param name="tagName">Element name to search for</param>
        /// <returns>The next matching element, or null if none</returns>
        static Element FindNext(Element root, Element element, string tagName)
        {
            Element next = element;

            do
            {
                next = NextElement(root, next);

            } while (next != null && !String.Equals(next.TagName, tagName, StringComparison.OrdinalIgnoreCase));

            return next;
        }

        /// <summary>
        /// Get the next element with the given namespace URI and local name
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="element">Begin the search here.</param>
        /// <param name="namespaceURI">Namespace URI to search for</param>
        /// <param name="localName">Local name to search for</param>
        /// <returns>The next matching element, or null if none</returns>
        static Element FindNext(Element root, Element element, string namespaceURI, string localName)
        {
            Element next = element;

            do
            {
                next = NextElement(root, next);

            } while (next != null && (
                !String.Equals(next.NamespaceURI, namespaceURI, StringComparison.OrdinalIgnoreCase) ||
                !String.Equals(next.LocalName, localName, StringComparison.OrdinalIgnoreCase)));

            return next;
        }

        /// <summary>
        /// Get the nth element with the given tagName
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="n">Begin the search here.</param>
        /// <param name="tagName">Element name to search for</param>
        /// <returns>The nth matching element, or null if not found</returns>
        static Element FindNth(Element root, int n, string tagName)
        {
            Element element = FindFirst(root, tagName);

            while (n > 0 && element != null)
            {
                n--;

                element = FindNext(root, element, tagName);
            }

            return element;
        }

        /// <summary>
        /// Get the nth element with the given namespace URI and local name
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="n">Begin the search here.</param>
        /// <param name="namespaceURI">Namespace URI to search for</param>
        /// <param name="localName">Element name to search for</param>
        /// <returns>The nth matching element, or null if not found</returns>
        static Element FindNth(Element root, int n, string namespaceURI, string localName)
        {
            Element element = FindFirst(root, namespaceURI, localName);

            while (n > 0 && element != null)
            {
                n--;

                element = FindNext(root, element, namespaceURI, localName);
            }

            return element;
        }

        /// <summary>
        /// Get the nth element in document order
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <returns>The nth element, or null if not found</returns>
        static Element NthElement(Element root, int n)
        {
            Element element = root;

            while (n > 0 && element != null)
            {
                n--;

                element = NextElement(root, element);
            }

            return element;
        }

        /// <summary>
        /// Get the next element node in document order
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched</param>
        /// <param name="element">Begin the search here.</param>
        /// <returns>The next element, or null if none</returns>
        static Element NextElement(Element root, Element element)
        {
            Node next = element;

            do
            {
                next = NextNode(root, next);

            } while (next != null && next.NodeType != NodeType.ELEMENT_NODE);

            return (Element)next;
        }

        /// <summary>
        /// Get the next node in document order
        /// </summary>
        /// <param name="root">Root of the sub-tree being searched.</param>
        /// <param name="node">Begin the search here.</param>
        /// <returns>The next node, or null if none</returns>
        static Node NextNode(Node root, Node node)
        {
            if (node.HasChildNodes())
            {
                return node.FirstChild;
            }
            
            if (node.NextSibling != null)
            {
                return node.NextSibling;
            }

            while (node.ParentNode != root && 
                node.ParentNode != null /* for malformed documents */)
            {
                node = node.ParentNode;

                if (node.NextSibling != null)
                {
                    return node.NextSibling;
                }
            }

            return null;
        }

        #endregion
    }
}
