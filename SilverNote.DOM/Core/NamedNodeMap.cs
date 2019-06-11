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
    public interface NamedNodeMap : IEnumerable<Node>
    {
        int Length { get; }
        Node this[int index] { get; }
        Node GetNamedItem(string name);
        Node SetNamedItem(Node arg);
        Node RemoveNamedItem(string name);
        Node GetNamedItemNS(string namespaceURI, string localName);
        Node SetNamedItemNS(Node arg);
        Node RemoveNamedItemNS(string namespaceURI, string localName);
    }
}
