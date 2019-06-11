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
    public interface INodeSource
    {
        NodeType GetNodeType(NodeContext context);
        string GetNodeName(NodeContext context);
        object GetParentNode(NodeContext context);
        IEnumerable<object> GetChildNodes(NodeContext context);
        object CreateNode(NodeContext context);
        void AppendNode(NodeContext context, object newChild);
        void InsertNode(NodeContext context, object newChild, object refChild);
        void RemoveNode(NodeContext context, object oldChild);
        IList<string> GetNodeAttributes(ElementContext context);
        string GetNodeAttribute(ElementContext context, string name);
        void SetNodeAttribute(ElementContext context, string name, string value);
        void ResetNodeAttribute(ElementContext context, string name);
        event NodeEventHandler NodeEvent;
    }

    public delegate void NodeEventHandler(IEventSource evt);
}
