/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;
using DOM.Style;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// This class implements CSS 2.1 section 6.1.1 "Specified Values"
    /// 
    /// http://www.w3.org/TR/2011/REC-CSS2-20110607/cascade.html#specified-value
    /// </summary>
    public class CSSSpecifiedStyle : CSSStyleDeclarationBase
    {
        #region Constructors

        public CSSSpecifiedStyle(CSSElement element)
            : this(element, String.Empty)
        {

        }

        public CSSSpecifiedStyle(CSSElement element, string pseudoElement)
        {
            _Element = element;
            _PseudoElement = pseudoElement;

            element.InlineStyle.CollectionChanged += InlineStyle_Changed;
        }

        #endregion

        #region Properties

        private CSSElement _Element;

        /// <summary>
        /// The element whose style we are computing
        /// </summary>
        public CSSElement Element
        {
            get { return _Element; }
        }

        private string _PseudoElement;

        /// <summary>
        /// The pseudo-element whose style we are computing (optional)
        /// </summary>
        public string PseudoElement
        {
            get { return _PseudoElement; }
        }

        #endregion

        #region CSSStyleDeclaration

        protected override string GetLonghandPropertyValue(string propertyName)
        {
            return GetSpecifiedValue(propertyName);
        }

        protected override string GetLonghandPropertyPriority(string propertyName)
        {
            return String.Empty;
        }

        protected override void SetLonghandProperty(string propertyName, string value, string priority)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        protected override string RemoveLonghandProperty(string propertyName)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        #endregion

        #region Extensions

        /// <summary>
        /// Get the "specified value" for the given property.
        /// 
        /// This resolves all inherited properties (it never returns "inherit")
        /// </summary>
        public string GetSpecifiedValue(string propertyName)
        {
            return GetSpecifiedValue(propertyName, CSSOrigin.All);
        }

        /// <summary>
        /// Get the "specified value" for the given property.
        /// 
        /// This resolves all inherited properties (it never returns "inherit")
        /// </summary>
        public string GetSpecifiedValue(string propertyName, CSSOrigin origin)
        {
            // "User agents must first assign a specified value to each property 
            // based on the following mechanisms (in order of precedence):
            // 
            // 1. If the cascade results in a value, use it.
            // 2. Otherwise, if the property is inherited and the element is not the 
            //    root of the document tree, use the computed value of the parent element.
            // 3. Otherwise use the property's initial value. The initial value of each 
            //    property is indicated in the property's definition."

            string value = GetCascadedValue(propertyName, origin);

            if (value.Length > 0 && value != "inherit")
            {
                return value;
            }
            else if (value == "inherit" || CSSProperties.IsInherited(propertyName))
            {
                return GetInheritedValue(propertyName);
            }
            else if (origin.HasFlag(CSSOrigin.Initial))
            {
                return CSSProperties.GetInitialValue(propertyName);
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Get a "cascaded value" of the given property according to:
        /// 
        /// http://www.w3.org/TR/2011/REC-CSS2-20110607/cascade.html#cascading-order
        /// </summary>
        protected string GetCascadedValue(string propertyName, CSSOrigin origin)
        {
            // Ascending order of precedence:
            //
            // 1. User agent declarations
            // 2. User normal declarations
            // 3. Author normal declarations 
            // 4. Author important declarations
            // 5. User important declarations
            //
            // For rules with the same origin (user agent, user, or author) and importance 
            // (normal or important), the rule with the highest specificity takes precedent. 
            // Among those, the last rule to be specified takes precedent.

            string value = String.Empty;

            // User important declarations

            CSSStyleSheet userStyle = null;
            
            if (origin.HasFlag(CSSOrigin.User))
            {
                userStyle = UserStyle;

                if (userStyle != null)
                {
                    value = CSSRuleListBase.GetPropertyValue(userStyle.CssRules, Element, propertyName, "!important");

                    if (value.Length > 0)
                    {
                        return value;
                    }
                }
            }

            if ((origin & CSSOrigin.Author) != 0)
            {
                // Author important declarations

                value = GetAuthorValue(propertyName, "!important", origin);

                if (value.Length > 0)
                {
                    return value;
                }

                // Author normal declarations

                value = GetAuthorValue(propertyName, "", origin);

                if (value.Length > 0)
                {
                    return value;
                }
            }

            // User normal declarations

            if (origin.HasFlag(CSSOrigin.User))
            {
                if (userStyle != null)
                {
                    value = CSSRuleListBase.GetPropertyValue(userStyle.CssRules, Element, propertyName, "");

                    if (value.Length > 0)
                    {
                        return value;
                    }
                }
            }

            // User agent declarations

            if (origin.HasFlag(CSSOrigin.UserAgent))
            {
                value = CSSRuleListBase.GetPropertyValue(UserAgentStyle.CssRules, Element, propertyName);
            }

            return value;
        }

        /// <summary>
        /// Get an author-specified property value
        /// </summary>
        protected string GetAuthorValue(string propertyName, string priority, CSSOrigin source)
        {
            // An author may specific style inline (via "style" attribute),
            // or at the document level (via <style> and <link> elements)

            string value = String.Empty;

            if (source.HasFlag(CSSOrigin.Inline))
            {
                value = Element.InlineStyle.GetPropertyWithPriority(propertyName, priority);

                if (value.Length > 0)
                {
                    return value;
                }
            }

            if (source.HasFlag(CSSOrigin.Document))
            {
                var documentStyle = DocumentStyle;

                if (documentStyle != null)
                {
                    foreach (var sheet in documentStyle.Reverse())
                    {
                        value = CSSRuleListBase.GetPropertyValue(sheet.CssRules, Element, propertyName, priority);

                        if (value.Length > 0)
                        {
                            return value;
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Get the user-defined style sheet (null if none)
        /// </summary>
        public CSSStyleSheet UserStyle
        {
            get
            {
                var documentStyle = Element.OwnerDocument as DocumentCSS;

                if (documentStyle != null)
                {
                    return documentStyle.UserStyleSheet as CSSStyleSheet;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the document-level style sheets
        /// </summary>
        public IEnumerable<CSSStyleSheet> DocumentStyle
        {
            get
            {
                var ownerDocument = Element.OwnerDocument as DocumentStyle;

                if (ownerDocument != null &&
                    ownerDocument.StyleSheets != null && 
                    ownerDocument.StyleSheets.Length > 0)
                {
                    return ownerDocument.StyleSheets.OfType<CSSStyleSheet>();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the user agent style sheet
        /// </summary>
        public CSSStyleSheet UserAgentStyle
        {
            get
            {
                return UASettings.Style;
            }
        }

        /// <summary>
        /// Get an inherited property value
        /// </summary>
        protected string GetInheritedValue(string propertyName)
        {
            return Element.ComputedStyle.GetInheritedValue(propertyName);
        }

        private void InlineStyle_Changed(object sender, DOMCollectionChangedEventArgs<string> e)
        {
            RaiseCollectionChanged(e.RemovedItems, e.AddedItems);
        }

        #endregion
    }
}
