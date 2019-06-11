/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.CSS
{
    public interface CSSStyleDeclaration
    {
        CSSRule ParentRule { get; }
        int Length { get; }
        string this[int index] { get; }
        void SetProperty(string propertyName, string value, string priority);
        string RemoveProperty(string propertyName);
        string GetPropertyValue(string propertyName);
        CSSValue GetPropertyCSSValue(string propertyName);
        string GetPropertyPriority(string propertyName);
        string GetPropertyWithPriority(string propertyName, string priority);   // Extension
        string CssText { get; set; }
    }
}
