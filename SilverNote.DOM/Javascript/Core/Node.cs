/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript.Core
{
    public class Node : ObjectInstance
    {
        private readonly DOM.Node _Node;

        public Node(ScriptEngine engine, DOM.Node node)
            : base(engine)
        {
            _Node = node;
            PopulateFields();
            PopulateFunctions();
        }

        public Node(ObjectInstance prototype, DOM.Node node)
            : base(prototype)
        {
            _Node = node;
        }

        [JSProperty(Name = "parentNode")]
        public Node ParentNode
        {
            get
            {
                if (_Node.ParentNode != null)
                {
                    return (Node) JSFactory.CreateObject(Engine, _Node.ParentNode);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
