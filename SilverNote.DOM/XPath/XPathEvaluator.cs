/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.XPath
{
    public interface XPathEvaluator
    {
        XPathExpression CreateExpression(string expression, XPathNSResolver resolver);
        XPathNSResolver CreateNSResolver(Node nodeResolver);
        object Evaluate(string expression, NodeContext contextNode, XPathNSResolver resolver, ushort type, object result);
    }
}
