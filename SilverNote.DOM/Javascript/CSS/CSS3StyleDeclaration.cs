/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript.CSS
{
    public class CSS3StyleDeclaration : DOM.Javascript.CSS.CSSStyleDeclaration
    {
        private DOM.CSS.CSS3StyleDeclaration _Owner;

        public CSS3StyleDeclaration(ScriptEngine engine, DOM.CSS.CSS3StyleDeclaration element)
            : base(new DOM.Javascript.CSS.CSSStyleDeclaration(engine, element), element)
        {
            _Owner = element;
            PopulateFields();
            PopulateFunctions();
        }

        public CSS3StyleDeclaration(ObjectInstance prototype, DOM.CSS.CSS3StyleDeclaration element)
            : base(prototype, element)
        {
            _Owner = element;
        }

        [JSProperty(Name = "height")]
        public string Height
        {
            get { return _Owner.Height; }
            set { _Owner.Height = value; }
        }

        [JSProperty(Name = "width")]
        public string Width
        {
            get { return _Owner.Width; }
            set {  _Owner.Width = value; }
        }

    }
}
