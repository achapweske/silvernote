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

namespace DOM.CSS.Internal
{
    public class CSSElement : ElementBase
    {
        #region Fields

        CSSStyleDeclarationBase _InlineStyle;
        CSSSpecifiedStyle _SpecifiedStyle;
        CSSComputedStyle _ComputedStyle;
        CSSRenderedStyle _RenderedStyle;
        bool _IsUpdating;

        #endregion

        #region Constructors

        public CSSElement(Document ownerDocument, string nodeName)
            : base(ownerDocument, nodeName)
        {
            
        }

        public CSSElement(Document ownerDocument, string namespaceURI, string qualifiedName)
            : base(ownerDocument, namespaceURI, qualifiedName)
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Determine if UpdateStyle() is in progress
        /// </summary>
        public bool IsUpdating
        {
            get { return _IsUpdating; }
        }

        /// <summary>
        /// This element's inline style
        /// </summary>
        public CSSStyleDeclarationBase InlineStyle
        {
            get
            {
                if (_InlineStyle == null)
                {
                    _InlineStyle = new CSSStyleDeclarationBase();
                }

                return _InlineStyle;
            }
        }

        /// <summary>
        /// This element's specified style.
        /// </summary>
        public CSSSpecifiedStyle SpecifiedStyle
        {
            get
            {
                if (_SpecifiedStyle == null)
                {
                    _SpecifiedStyle = new CSSSpecifiedStyle(this);
                }

                return _SpecifiedStyle;
            }
        }

        /// <summary>
        /// This element's computed style.
        /// </summary>
        public CSSComputedStyle ComputedStyle
        {
            get
            {
                if (_ComputedStyle == null)
                {
                    _ComputedStyle = new CSSComputedStyle(this);
                    _ComputedStyle.CollectionChanged += ComputedStyle_Changed;
                }

                return _ComputedStyle;
            }
        }

        /// <summary>
        /// This element's rendered style
        /// </summary>
        public CSSRenderedStyle RenderedStyle
        {
            get
            {
                if (_RenderedStyle == null)
                {
                    _RenderedStyle = new CSSRenderedStyle(this, Source as ICSSRenderer);
                }

                return _RenderedStyle;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Update RenderedStyle based on ComputedStyle
        /// </summary>
        public void RenderStyle()
        {
            if (_RenderedStyle != null && _RenderedStyle.Source != null)
            {
                var propertyNames = RenderedStyle.Source.GetSupportedStyles(this);

                if (propertyNames != null)
                {
                    foreach (var propertyName in propertyNames)
                    {
                        CSSValue value = ComputedStyle.GetComputedCSSValue(propertyName);

                        RenderedStyle.SetRenderedProperty(propertyName, value);
                    }
                }
            }
        }

        /// <summary>
        /// Update InlineStyle based on RenderedStyle
        /// <param name="deep">If true, update all descendants as well</param>
        /// </summary>
        public void UpdateStyle(bool deep)
        {
            _IsUpdating = true;
            try
            {
                UpdateStyle();

                if (deep)
                {
                    foreach (var childNode in ChildNodes.OfType<CSSElement>())
                    {
                        childNode.UpdateStyle(true);
                    }
                }
            }
            finally
            {
                _IsUpdating = false;
            }
        }

        /// <summary>
        /// Update InlineStyle based on RenderedStyle
        /// </summary>
        private void UpdateStyle()
        {
            foreach (string propertyName in RenderedStyle)
            {
                CSSValue renderedValue = RenderedStyle.GetPropertyCSSValue(propertyName);
                CSSValue computedValue = ComputedStyle.GetPropertyCSSValue(propertyName);

                if (!CSSValueBase.AreEquivalent(computedValue, renderedValue))
                {
                    // Determine what the computed value would be if the inline value were removed
                    var nonInlineSource = CSSOrigin.All ^ CSSOrigin.Inline;
                    CSSValue nonInlineValue = ComputedStyle.GetComputedCSSValue(propertyName, nonInlineSource);

                    if (!CSSValueBase.AreEquivalent(renderedValue, nonInlineValue) && renderedValue != null)
                    {
                        InlineStyle.SetProperty(propertyName, renderedValue.CssText, "");
                    }
                    else
                    {
                        InlineStyle.RemoveProperty(propertyName);
                    }
                }
            }
        }

        #endregion

        #region Node

        public override bool HasAttributes()
        {
            if (InlineStyle.Length > 0)
            {
                return true;
            }
            else
            {
                return base.HasAttributes();
            }
        }

        public override NamedNodeMap Attributes
        {
            get
            {
                var attributes = (AttrMap)base.Attributes;
                if (InlineStyle.Length > 0)
                {
                    attributes.SetAttribute(DOM.Attributes.STYLE, InlineStyle.CssText);
                }
                else
                {
                    attributes.RemoveAttribute(DOM.Attributes.STYLE);
                }
                return attributes;
            }
        }

        public override bool HasAttribute(string name)
        {
            if (name == DOM.Attributes.STYLE)
            {
                return InlineStyle.Length > 0;
            }
            else
            {
                return base.HasAttribute(name);
            }
        }

        public override string GetAttribute(string name)
        {
            if (name == DOM.Attributes.STYLE)
            {
                return InlineStyle.CssText;
            }
            else
            {
                return base.GetAttribute(name);
            }
        }

        public override void SetAttribute(string name, string value)
        {
            if (name == DOM.Attributes.STYLE)
            {
                InlineStyle.CssText = value;
            }
            else
            {
                base.SetAttribute(name, value);
            }
        }

        public override void RemoveAttribute(string name)
        {
            if (name == DOM.Attributes.STYLE)
            {
                InlineStyle.CssText = String.Empty;
            }
            else
            {
                base.RemoveAttribute(name);
            }
        }

        #endregion

        #region Implementation

        protected override void OnRender(INodeSource newSource)
        {
            base.OnRender(newSource);

            RenderedStyle.Source = newSource as ICSSRenderer;

            if (RenderedStyle.Source != null)
            {
                RenderStyle();
            }
        }

        protected override void OnSourceChanging(INodeSource newSource)
        {
            base.OnSourceChanging(newSource);

            if (_RenderedStyle != null)
            {
                _RenderedStyle.Source = newSource as ICSSRenderer;
            }
        }

        private void ComputedStyle_Changed(object sender, EventArgs e)
        {
            if (Source is ICSSRenderer && !IsSelfOrAncestorUpdating)
            {
                RenderStyle();
            }
        }

        private bool IsSelfOrAncestorUpdating
        {
            get
            {
                var selfOrAncestor = this;
                do
                {
                    if (selfOrAncestor.IsUpdating)
                    {
                        return true;
                    }
                    selfOrAncestor = selfOrAncestor.ParentNode as CSSElement;

                } while (selfOrAncestor != null);

                return false;
            }
        }

        #endregion
    }
}
