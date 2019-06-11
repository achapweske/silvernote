/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript.SVG
{
    public class SVGElement : DOM.Javascript.Core.Element
    {
        private DOM.SVG.SVGElement _Element;

        public SVGElement(ScriptEngine engine, DOM.SVG.SVGElement element)
            : base(new DOM.Javascript.Core.Element(engine, element), element)
        {
            _Element = element;
            PopulateFields();
            PopulateFunctions();
        }

        public SVGElement(ObjectInstance prototype, DOM.SVG.SVGElement element)
            : base(prototype, element)
        {
            _Element = element;
        }

        [JSProperty(Name = "style")]
        public DOM.Javascript.CSS.CSS3StyleDeclaration Style
        {
            get 
            {
                var element = _Element as DOM.CSS.ElementCSSInlineStyle;
                if (element != null)
                {
                    return (DOM.Javascript.CSS.CSS3StyleDeclaration)JSFactory.CreateObject(Engine, element.Style);
                }
                else
                {
                    return null;
                }
            }
        }

    }
}
