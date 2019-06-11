/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript.Core
{
    public class Document : Node
    {
        private readonly DOM.Document _Document;

        public Document(ScriptEngine engine, DOM.Document element)
            : base(new Node(engine, element), element)
        {
            _Document = element;
            PopulateFields();
            PopulateFunctions();
        }

        public Document(ObjectInstance prototype, DOM.Document element)
            : base(prototype, element)
        {
            _Document = element;
        }
    }
}
