/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM
{
    public static class NodeExtensions
    {
        public static void Bind(this Node node, INodeSource source, bool render = false)
        {
            var proxy = node as DOM.Internal.NodeProxy;

            if (proxy != null)
            {
                if (render)
                {
                    proxy.Render(source);
                }
                else
                {
                    proxy.Bind(source);
                }
            }
        }
    }
}
