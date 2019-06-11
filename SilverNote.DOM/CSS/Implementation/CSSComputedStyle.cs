/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DOM.Internal;

namespace DOM.CSS.Internal
{
    public class CSSComputedStyle : CSSStyleDeclarationBase
    {
        #region Fields

        CSSElement _Element;
        string _PseudoElement;

        #endregion

        #region Constructors

        public CSSComputedStyle(CSSElement element)
            : this(element, String.Empty)
        {

        }

        public CSSComputedStyle(CSSElement element, string pseudoElement)
        {
            _Element = element;
            _PseudoElement = pseudoElement;

            OnParentNodeChanged(null, Element.ParentNode);
            element.ParentNodeChanged += Element_ParentNodeChanged;
            element.SpecifiedStyle.CollectionChanged += SpecifiedStyle_CollectionChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The element whose style we are computing
        /// </summary>
        public CSSElement Element
        {
            get { return _Element; }
        }

        /// <summary>
        /// The pseudo-element whose style we are computing (optional)
        /// </summary>
        public string PseudoElement
        {
            get { return _PseudoElement; }
        }

        #endregion

        #region CSSStyleDeclaration

        public override int Length
        {
            get
            {
                return PropertyNames.Count;
            }
        }

        public override string this[int index]
        {
            get
            {
                if (index >= 0 && index < PropertyNames.Count)
                {
                    return PropertyNames[index];
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        protected override string GetLonghandPropertyValue(string propertyName)
        {
            return GetComputedValue(propertyName);
        }

        protected override CSSValue GetLonghandPropertyCSSValue(string propertyName)
        {
            return GetComputedCSSValue(propertyName);
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
        /// Get the computed value for the given property
        /// </summary>
        /// <param name="propertyName">Target property name</param>
        /// <returns>Computed value, or an empty string if none</returns>
        public string GetComputedValue(string propertyName)
        {
            return GetComputedValue(propertyName, CSSOrigin.All);
        }

        /// <summary>
        /// Get the computed value for the given property
        /// </summary>
        /// <param name="propertyName">Target property name</param>
        /// <param name="source">Property sources to include</param>
        /// <returns>Computed value, or an empty string if none</returns>
        public string GetComputedValue(string propertyName, CSSOrigin source)
        {
            CSSValue cssValue = GetComputedCSSValue(propertyName, source);

            if (cssValue != null)
            {
                return cssValue.CssText;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Get the computed value for the given property
        /// </summary>
        /// <param name="propertyName">Target property name</param>
        /// <returns>Computed value, or null if none</returns>
        public CSSValue GetComputedCSSValue(string propertyName)
        {
            return GetComputedCSSValue(propertyName, CSSOrigin.All);
        }

        /// <summary>
        /// Get the computed value for the given property
        /// </summary>
        /// <param name="propertyName">Target property name</param>
        /// <param name="source">Property sources to include</param>
        /// <returns>Computed value, or null if none</returns>
        public CSSValue GetComputedCSSValue(string propertyName, CSSOrigin source)
        {
            CSSValue result;

            // Cache lookup
            if (source == CSSOrigin.All)
            {
                if (Cache.TryGetValue(propertyName, out result))
                {
                    return result;
                }
            }

            string specifiedValue = Element.SpecifiedStyle.GetSpecifiedValue(propertyName, source);

            result = ComputeValue(propertyName, specifiedValue);

            // Update cache
            if (source == CSSOrigin.All)
            {
                Cache[propertyName] = result;
            }

            return result;
        }

        /// <summary>
        /// Get the inherited value for the given property.
        /// </summary>
        /// <param name="propertyName">The property to be retrieved</param>
        /// <returns>A computed value</returns>
        public string GetInheritedValue(string propertyName)
        {
            CSSValue cssValue = GetInheritedCSSValue(propertyName);

            if (cssValue != null)
            {
                return cssValue.CssText;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Get the inherited valeu for the given property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public CSSValue GetInheritedCSSValue(string propertyName)
        {
            var parent = Element.ParentNode as CSSElement;

            if (parent != null)
            {
                return parent.ComputedStyle.GetPropertyCSSValue(propertyName);
            }
            else
            {
                string initialValue = CSSProperties.GetInitialValue(propertyName);

                return ComputeValue(propertyName, initialValue);
            }
        }


        /// <summary>
        /// Reset the computed value cache
        /// </summary>
        public void ResetCache()
        {
            _PropertyNames = null;

            if (_Cache != null)
            {
                _Cache.Clear();
            }
        }

        #endregion

        #region Implementation

        private Dictionary<string, CSSValue> _Cache;

        protected Dictionary<string, CSSValue> Cache
        {
            get
            {
                if (_Cache == null)
                {
                    _Cache = new Dictionary<string, CSSValue>();
                }

                return _Cache;
            }
        }

        private IList<string> _PropertyNames;

        protected IList<string> PropertyNames
        {
            get
            {
                if (_PropertyNames == null)
                {
                    _PropertyNames = CSSProperties.GetApplicableProperties(Element).ToArray();
                }

                return _PropertyNames;
            }
        }

        /// <summary>
        /// Get the computed value for the given specified value
        /// </summary>
        /// <param name="propertyName">Name of the property whose value is to be computed</param>
        /// <param name="specifiedValue">Target property's specified value</param>
        /// <returns>Computed value, or null if none</returns>
        protected CSSValue ComputeValue(string propertyName, string specifiedValue)
        {
            CSSValue cssValue = CSSProperties.GetPropertyCSSValue(propertyName, specifiedValue);

            return ComputeValue(propertyName, cssValue);
        }

        /// <summary>
        /// Get the computed value for the given specified value
        /// </summary>
        /// <param name="propertyName">Name of the property whose value is to be computed</param>
        /// <param name="specifiedValue">Target property's specified value</param>
        /// <returns>Computed value, or null if none</returns>
        protected CSSValue ComputeValue(string propertyName, CSSValue specifiedValue)
        {
            try
            {
                switch (propertyName)
                {
                    case CSSProperties.BorderTopWidth:
                        return ComputeBorderWidth(specifiedValue, "top");
                    case CSSProperties.BorderRightWidth:
                        return ComputeBorderWidth(specifiedValue, "right");
                    case CSSProperties.BorderBottomWidth:
                        return ComputeBorderWidth(specifiedValue, "bottom");
                    case CSSProperties.BorderLeftWidth:
                        return ComputeBorderWidth(specifiedValue, "left");
                    case CSSProperties.Bottom:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.Display:
                        return ComputeDisplay(specifiedValue);
                    case CSSProperties.Float:
                        return ComputeFloat(specifiedValue);
                    case CSSProperties.FontSize:
                        return ComputeFontSize(specifiedValue);
                    case CSSProperties.FontWeight:
                        return ComputeFontWeight(specifiedValue);
                    case CSSProperties.Height:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.Left:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.LineHeight:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.MarginTop:
                    case CSSProperties.MarginRight:
                    case CSSProperties.MarginBottom:
                    case CSSProperties.MarginLeft:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.MaxWidth:
                    case CSSProperties.MaxHeight:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.MinWidth:
                    case CSSProperties.MinHeight:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.PaddingTop:
                    case CSSProperties.PaddingRight:
                    case CSSProperties.PaddingBottom:
                    case CSSProperties.PaddingLeft:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.Right:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.TextIndent:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.Top:
                        return ComputeLength(specifiedValue);
                    case CSSProperties.Width:
                        return ComputeLength(specifiedValue);
                    default:
                        return specifiedValue;
                }
            }
            catch
            {
                return specifiedValue;
            }
        }


        /// <summary>
        /// http://www.w3.org/TR/CSS2/box.html#value-def-border-width
        /// </summary>
        protected CSSValue ComputeBorderWidth(CSSValue specifiedValue, string side)
        {
            string propertyName = String.Format("border-{0}-style", side);

            CSSValue borderStyle = GetComputedCSSValue(propertyName);

            if (borderStyle == CSSValues.None || borderStyle == CSSValues.Hidden)
            {
                return CSSValues.Zero;
            }

            var primitive = specifiedValue as CSSPrimitiveValueBase;

            if (primitive == null)
            {
                return specifiedValue;
            }
            else if (primitive.PrimitiveType == CSSPrimitiveType.CSS_IDENT)
            {
                return UASettings.Default.GetBorderWidth(primitive);
            }
            else if (primitive.IsLength())
            {
                return ComputeLength(primitive);
            }
            else
            {
                return specifiedValue;
            }
        }

        /// <summary>
        /// See CSS 2.1 section 9.7 (http://www.w3.org/TR/CSS2/visuren.html#dis-pos-flo)
        /// </summary>
        /// <param name="specifiedValue"></param>
        /// <returns></returns>
        protected CSSValue ComputeDisplay(CSSValue specifiedValue)
        {
            // If 'display' has the value 'none', then 'position' and 'float' 
            // do not apply. In this case, the element generates no box.

            if (specifiedValue == CSSValues.None)
            {
                return CSSValues.None;
            }

            // 2. Otherwise, if 'position' has the value 'absolute' or 'fixed', 
            // the box is absolutely positioned, the computed value of 'float' 
            // is 'none', and display is set according to the table below. The 
            // position of the box will be determined by the 'top', 'right', 
            // 'bottom' and 'left' properties and the box's containing block.

            string position = Element.SpecifiedStyle.Position;
            if (position == CSSValues.Absolute.CssText || position == CSSValues.Fixed.CssText)
            {
                return NormalizeDisplay(specifiedValue);
            }

            // 3. Otherwise, if 'float' has a value other than 'none', the box 
            // is floated and 'display' is set according to the table below.

            string cssFloat = Element.SpecifiedStyle.CssFloat;
            if (cssFloat != CSSValues.None.CssText)
            {
                return NormalizeDisplay(specifiedValue);
            }

            // 4. Otherwise, if the element is the root element, 'display' is 
            // set according to the table below, except that it is undefined 
            // in CSS 2.1 whether a specified value of 'list-item' becomes a 
            // computed value of 'block' or 'list-item'.

            var root = Element.OwnerDocument.FirstChild;
            if (Element == root)
            {
                return NormalizeDisplay(specifiedValue);
            }

            // 5. Otherwise, the remaining 'display' property values apply as 
            // specified.

            return specifiedValue;
        }

        /// <summary>
        /// See CSS 2.1 section 9.7 (http://www.w3.org/TR/CSS2/visuren.html#dis-pos-flo)
        /// </summary>
        /// <param name="specifiedValue"></param>
        /// <returns></returns>
        private CSSValue NormalizeDisplay(CSSValue specifiedValue)
        {
            if (specifiedValue == CSSValues.InlineTable)
            {
                return CSSValues.Table;
            }
            else if (
                specifiedValue == CSSValues.Inline ||
                specifiedValue == CSSValues.TableRowGroup ||
                specifiedValue == CSSValues.TableColumn ||
                specifiedValue == CSSValues.TableColumnGroup ||
                specifiedValue == CSSValues.TableHeaderGroup ||
                specifiedValue == CSSValues.TableFooterGroup ||
                specifiedValue == CSSValues.TableRow ||
                specifiedValue == CSSValues.TableCell ||
                specifiedValue == CSSValues.TableCaption ||
                specifiedValue == CSSValues.InlineBlock)
            {
                return CSSValues.Block;
            }
            else
            {
                return specifiedValue;
            }
        }

        /// <summary>
        /// See CSS 2.1 section 9.7 (http://www.w3.org/TR/CSS2/visuren.html#dis-pos-flo)
        /// </summary>
        /// <param name="specifiedValue"></param>
        /// <returns></returns>
        protected CSSValue ComputeFloat(CSSValue specifiedValue)
        {
            string position = Element.SpecifiedStyle.Position;
            if (position == CSSValues.Absolute.CssText || position == CSSValues.Fixed.CssText)
            {
                return CSSValues.None;
            }
            else
            {
                return specifiedValue;
            }
        }

        /// <summary>
        /// http://www.w3.org/TR/CSS2/fonts.html#propdef-font-size
        /// </summary>
        protected CSSValue ComputeFontSize(CSSValue specifiedValue)
        {
            var primitive = specifiedValue as CSSPrimitiveValueBase;

            if (primitive == null)
            {
                return specifiedValue;
            }
            else if (primitive.PrimitiveType == CSSPrimitiveType.CSS_IDENT)
            {
                // <relative-size>

                if (primitive == CSSValues.Smaller)
                {
                    return ComputeSmallerSize(InheritedEmSize);
                }

                if (primitive == CSSValues.Larger)
                {
                    return ComputeLargerSize(InheritedEmSize);
                }

                // <absolute-size>

                return UASettings.Default.GetFontEmSize(primitive);
            }
            else if (primitive.PrimitiveType == CSSPrimitiveType.CSS_PERCENTAGE)
            {
                // <percentage>

                return ComputePercentage(primitive, InheritedEmSize);
            }
            else if (primitive.IsLength())
            {
                // <length> 

                return ComputeLength(primitive, isFontSize: true);
            }
            else
            {
                return specifiedValue;
            }
        }

        /// <summary>
        /// http://www.w3.org/TR/CSS2/fonts.html#propdef-font-weight
        /// </summary>
        protected CSSValue ComputeFontWeight(CSSValue specifiedValue)
        {
            var primitive = specifiedValue as CSSPrimitiveValueBase;

            if (primitive == CSSValues.Bolder)
            {
                CSSValue refWeight = GetInheritedCSSValue(CSSProperties.FontWeight);

                int refWeightClass = GetFontWeightClass(refWeight);

                if (refWeightClass < 400)
                {
                    return CSSValues.Normal;
                }
                else if (refWeightClass < 600)
                {
                    return CSSValues.Bold;
                }
                else
                {
                    return CSSValues.Number(900);
                }
            }

            if (primitive == CSSValues.Lighter)
            {
                CSSValue refWeight = GetInheritedCSSValue(CSSProperties.FontWeight);

                int refWeightClass = GetFontWeightClass(refWeight);

                if (refWeightClass > 700)
                {
                    return CSSValues.Bold;
                }
                else if (refWeightClass >= 600)
                {
                    return CSSValues.Normal;
                }
                else
                {
                    return CSSValues.Number(100);
                }
            }

            return specifiedValue;
        }

        const int DEFAULT_FONT_WEIGHT = 400;
        const int NORMAL_FONT_WEIGHT = 400;
        const int BOLD_FONT_WEIGHT = 700;

        protected int GetFontWeightClass(CSSValue fontWeight)
        {
            var primitive = fontWeight as CSSPrimitiveValueBase;

            if (primitive == null)
            {
                return DEFAULT_FONT_WEIGHT;
            }
            else if (primitive.PrimitiveType == CSSPrimitiveType.CSS_NUMBER)
            {
                return (int)primitive.GetFloatValue(primitive.PrimitiveType);
            }
            else if (primitive == CSSValues.Normal)
            {
                return NORMAL_FONT_WEIGHT;
            }
            else if (primitive == CSSValues.Bold)
            {
                return BOLD_FONT_WEIGHT;
            }
            else
            {
                return DEFAULT_FONT_WEIGHT;
            }
        }

        /// <summary>
        /// Get the current computed font em size
        /// </summary>
        protected CSSPrimitiveValueBase EmSize
        {
            get
            {
                return (CSSPrimitiveValueBase)GetComputedCSSValue(CSSProperties.FontSize);
            }
        }

        /// <summary>
        /// Get the current computed font x-height
        /// </summary>
        protected CSSPrimitiveValueBase ExSize
        {
            get
            {
                return GetExSize(EmSize, null);
            }
        }

        /// <summary>
        /// Get the inherited font em size
        /// </summary>
        protected CSSPrimitiveValueBase InheritedEmSize
        {
            get
            {
                return (CSSPrimitiveValueBase)GetInheritedCSSValue(CSSProperties.FontSize);
            }
        }

        /// <summary>
        /// Get the inherited font x-height
        /// </summary>
        protected CSSPrimitiveValueBase InheritedExSize
        {
            get
            {
                return GetExSize(InheritedEmSize, null);
            }
        }

        /// <summary>
        /// http://www.w3.org/TR/CSS2/syndata.html#value-def-length
        /// </summary>
        protected CSSValue ComputeLength(CSSValue value, bool isFontSize = false)
        {
            var primitive = value as CSSPrimitiveValueBase;

            if (primitive == null)
            {
                return null;
            }

            switch (primitive.PrimitiveType)
            {
                case CSSPrimitiveType.CSS_EMS:
                    return ComputeEmSize(primitive, isFontSize ? InheritedEmSize : EmSize);
                case CSSPrimitiveType.CSS_EXS:
                    return ComputeExSize(primitive, isFontSize ? InheritedExSize : ExSize);
                default:
                    return primitive;
            }
        }

        /// <summary>
        /// Multiply emSize by refSize and return the product
        /// </summary>
        protected static CSSPrimitiveValueBase ComputeEmSize(CSSPrimitiveValueBase emSize, CSSPrimitiveValueBase refSize)
        {
            double multiplier = emSize.GetFloatValue(CSSPrimitiveType.CSS_EMS);

            return Multiply(refSize, multiplier);
        }

        /// <summary>
        /// Multiply exSize by refSize and return the product
        /// </summary>
        protected static CSSPrimitiveValueBase ComputeExSize(CSSPrimitiveValueBase exSize, CSSPrimitiveValueBase refSize)
        {
            double multiplier = exSize.GetFloatValue(CSSPrimitiveType.CSS_EXS);

            return Multiply(refSize, multiplier);
        }

        /// <summary>
        /// Multiply percent by refSize / 100 and return the product
        /// </summary>
        protected static CSSPrimitiveValueBase ComputePercentage(CSSPrimitiveValueBase percent, CSSPrimitiveValueBase refSize)
        {
            double multiplier = percent.GetFloatValue(CSSPrimitiveType.CSS_PERCENTAGE);

            multiplier /= 100.0;

            return Multiply(refSize, multiplier);
        }

        /// <summary>
        /// Get the next smallest font size
        /// </summary>
        protected static CSSPrimitiveValueBase ComputeSmallerSize(CSSPrimitiveValueBase emSize)
        {
            return emSize;  // TODO
        }

        /// <summary>
        /// Get the next largest font size
        /// </summary>
        protected static CSSPrimitiveValueBase ComputeLargerSize(CSSPrimitiveValueBase emSize)
        {
            return emSize;  // TODO
        }

        /// <summary>
        /// Multiply refSize by multiplier and return the product
        /// </summary>
        protected static CSSPrimitiveValueBase Multiply(CSSPrimitiveValueBase refSize, double multiplier)
        {
            double value = refSize.GetFloatValue(refSize.PrimitiveType);

            value *= multiplier;

            return new CSSPrimitiveValueBase(value, refSize.PrimitiveType);
        }

        /// <summary>
        /// Get the x-height for the given font
        /// </summary>
        protected static CSSPrimitiveValueBase GetExSize(CSSPrimitiveValueBase emSize, CSSPrimitiveValueBase fontFamily)
        {
            // See http://www.w3.org/TR/CSS2/syndata.html#value-def-length

            return Multiply(emSize, 0.5);   // TODO
        }

        protected void RaiseCollectionChanged()
        {
            var allItems = CSSProperties.AllProperties;

            RaiseCollectionChanged(allItems, allItems);
        }

        private void Element_ParentNodeChanged(object sender, NodeChangedEventArgs e)
        {
            OnParentNodeChanged(e.OldValue, Element.ParentNode);
        }

        private void OnParentNodeChanged(Node oldValue, Node newValue)
        {
            var oldElement = oldValue as CSSElement;
            if (oldElement != null)
            {
                oldElement.ComputedStyle.CollectionChanged -= ParentStyle_CollectionChanged;
            }

            var newElement = newValue as CSSElement;
            if (newElement != null)
            {
                newElement.ComputedStyle.CollectionChanged += ParentStyle_CollectionChanged;
            }

            if (newElement != null)
            {
                OnParentStyleChanged();
            }
        }

        private void ParentStyle_CollectionChanged(object sender, DOMCollectionChangedEventArgs<string> e)
        {
            // In case we moved in the DOM without being notified 
            // (an issue with INodeSource implementations)

            var parentElement = _Element.ParentNode as CSSElement;
            if (parentElement != null && Object.ReferenceEquals(parentElement.ComputedStyle, sender))
            {
                OnParentStyleChanged();
            }
            else
            {
                ((CSSComputedStyle)sender).CollectionChanged -= ParentStyle_CollectionChanged;
            }
        }

        private void OnParentStyleChanged()
        {
            ResetCache();
            RaiseCollectionChanged();
        }

        private void SpecifiedStyle_CollectionChanged(object sender, EventArgs e)
        {
            ResetCache();
            RaiseCollectionChanged();
        }

        #endregion
    }
}
