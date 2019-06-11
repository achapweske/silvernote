using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public static class NodeExtensions
    {
        public static object SourceOrSelf(this Node node)
        {
            var nodeProxy = node as NodeProxy;

            if (nodeProxy != null && nodeProxy.Source != null)
            {
                return nodeProxy.Source;
            }
            else
            {
                return node;
            }
        }
    }
}
