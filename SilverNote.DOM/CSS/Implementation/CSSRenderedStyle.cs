/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// A CSSStyleDeclaration implementation that delegates getting/setting
    /// of properties to an ICSSStyleDeclarationSource object.
    /// 
    /// The delegate advertises which styles it supports, and all other
    /// styles are get/set internally.
    /// </summary>
    public class CSSRenderedStyle : CSSStyleDeclarationBase
    {
        #region Fields

        readonly ElementContext _OwnerElement;

        #endregion

        #region Constructors

        public CSSRenderedStyle(ElementContext ownerElement)
        {
            _OwnerElement = ownerElement;
        }

        public CSSRenderedStyle(ElementContext ownerElement, ICSSRenderer source)
            : this(ownerElement)
        {
            Source = source;
        }

        #endregion

        #region CSSStyleDeclaration

        /// <summary>
        /// Delegate for getting/setting property values
        /// </summary>
        public ICSSRenderer Source { get; set; }

        public override int Length
        {
            get
            {
                if (Source == null)
                {
                    return 0;
                }

                var styles = Source.GetSupportedStyles(_OwnerElement);
                
                if (styles != null)
                {
                    return styles.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override string this[int index]
        {
            get
            {
                if (Source != null)
                {
                    IList<string> items = Source.GetSupportedStyles(_OwnerElement);

                    if (items != null && index >= 0 && index < items.Count)
                    {
                        return items[index];
                    }
                }

                return String.Empty;
            }
        }

        public void SetRenderedProperty(string propertyName, CSSValue value)
        {
            if (Source != null)
            {
                Source.SetStyleProperty(_OwnerElement, propertyName, value);
            }
        }

        protected override void SetLonghandProperty(string propertyName, string value, string priority)
        {
            var cssValue = CSSProperties.GetPropertyCSSValue(propertyName, value);

            SetRenderedProperty(propertyName, cssValue);
        }

        protected override string RemoveLonghandProperty(string propertyName)
        {
            return String.Empty;
        }

        protected override string GetLonghandPropertyValue(string propertyName)
        {
            var value = GetLonghandPropertyCSSValue(propertyName);
            if (value != null)
            {
                return value.CssText;
            }
            else
            {
                return String.Empty;
            }
        }

        protected override CSSValue GetLonghandPropertyCSSValue(string propertyName)
        {
            if (Source != null)
            {
                return Source.GetStyleProperty(_OwnerElement, propertyName);
            }
            else
            {
                return null;
            }
        }

        protected override string GetLonghandPropertyPriority(string propertyName)
        {
            return String.Empty;
        }

        #endregion

        #region IEnumerable

        /// <summary>
        /// Override to provide a more performant implementation
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<string> GetEnumerator()
        {
            if (Source != null)
            {
                IList<string> items = Source.GetSupportedStyles(_OwnerElement);
                if (items != null)
                {
                    return items.GetEnumerator();
                }
            }

            return new List<string>().GetEnumerator();
        }

        #endregion
    }
}
