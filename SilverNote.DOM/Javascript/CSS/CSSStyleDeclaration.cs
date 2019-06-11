/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript.CSS
{
    public class CSSStyleDeclaration : ObjectInstance
    {
        private DOM.CSS.CSSStyleDeclaration _Owner;

        public CSSStyleDeclaration(ScriptEngine engine, DOM.CSS.CSSStyleDeclaration owner)
            : base(engine)
        {
            _Owner = owner;
            PopulateFields();
            PopulateFunctions();
        }

        public CSSStyleDeclaration(ObjectInstance prototype, DOM.CSS.CSSStyleDeclaration owner)
            : base(prototype)
        {
            _Owner = owner;
        }

        [JSProperty(Name = "length")]
        public int Length
        {
            get { return _Owner.Length; }
        }

        [JSProperty]
        new public string this[int index]
        {
            get { return _Owner[index]; }
        }

        [JSFunction(Name = "setProperty")]
        public void SetProperty(string propertyName, string value, string priority)
        {
            _Owner.SetProperty(propertyName, value, priority);
        }

        [JSFunction(Name = "removeProperty")]
        public string RemoveProperty(string propertyName)
        {
            return _Owner.RemoveProperty(propertyName);
        }

        [JSFunction(Name = "getPropertyValue")]
        new public string GetPropertyValue(string propertyName)
        {
            return _Owner.GetPropertyValue(propertyName);
        }

        [JSProperty(Name = "cssText")]
        public string CssText
        {
            get { return _Owner.CssText; }
            set { _Owner.CssText = value; }
        }
    }
}
