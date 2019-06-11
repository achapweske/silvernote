using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.Internal
{
    /// <summary>
    /// An ordered collection of nodes.
    /// 
    /// Items are accessible via a zero-based index.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Core-20001113/core.html#ID-536297177
    /// </summary>
    public class NodeListBase : DOMList<Node>, NodeList
    {
        #region Fields

        public static readonly NodeListBase Empty = new NodeListBase();

        #endregion

        #region Constructors

        public NodeListBase()
        {

        }

        public NodeListBase(IEnumerable<Node> items)
            : base(items)
        {

        }

        #endregion
    }

}
