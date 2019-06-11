/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using DOM.XPath.Internal;

namespace DOM.XPath
{
    public static class NodeExtensions
    {
        #region IXPathNavigable

        public static Node[] Select(this Node node, string xpath)
        {
            if (node is IXPathNavigable)
            {
                var navigator = ((IXPathNavigable)node).CreateNavigator();

                var results = navigator.Select(xpath).Cast<XPathDOMNavigator>();

                return results.Select((result) => result.CurrentNode).ToArray();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static Node SelectSingleNode(this Node node, string xpath)
        {
            if (node is IXPathNavigable)
            {
                var navigator = ((IXPathNavigable)node).CreateNavigator();

                var result = navigator.SelectSingleNode(xpath) as XPathDOMNavigator;

                if (result != null)
                {
                    return result.CurrentNode;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
