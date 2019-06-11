/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript.Core
{
    public class Element : Node
    {
        private readonly DOM.Element _Element;

        public Element(ScriptEngine engine, DOM.Element element)
            : base(new Node(engine, element), element)
        {
            _Element = element;
            PopulateFields();
            PopulateFunctions();
        }

        public Element(ObjectInstance prototype, DOM.Element element)
            : base(prototype, element)
        {
            _Element = element;
        }

        [JSFunction(Name = "getAttribute")]
        public string GetAttribute(string name)
        {
            return _Element.GetAttribute(name);
        }

        [JSFunction(Name = "setAttribute")]
        public void SetAttribute(string name, string value)
        {
            _Element.SetAttribute(name, value);
        }

        [JSFunction(Name = "querySelector")]
        public Element QuerySelector(string selectors)
        {
            var result = _Element.QuerySelector(selectors);
            if (result != null)
            {
                return (Element)JSFactory.CreateObject(Engine, result);
            }
            else
            {
                return null;
            }
        }

    }
}
