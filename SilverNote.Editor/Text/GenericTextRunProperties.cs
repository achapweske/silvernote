/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.SVG;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace SilverNote.Editor
{
    public class GenericTextRunProperties : TextRunProperties, IFormattable, IStyleable, ICloneable
    {
        #region Static Members

        public static readonly GenericTextRunProperties Default = new GenericTextRunProperties();

        #endregion

        #region Fields

        Brush _BackgroundBrush;
        BaselineAlignment _BaselineAlignment;
        CultureInfo _CultureInfo;
        double _FontHintingEmSize;
        double _FontRenderingEmSize;
        Brush _ForegroundBrush;
        NumberSubstitution _NumberSubstitution;
        TextDecorationCollection _TextDecorations;
        TextEffectCollection _TextEffects;
        FontFamily _FontFamily;
        FontStyle _FontStyle;
        FontWeight _FontWeight;
        FontStretch _FontStretch;
        Typeface _Typeface;
        TextRunTypographyProperties _TypographyProperties;

        #endregion

        #region Constructors

        public GenericTextRunProperties()
        {   
            _BaselineAlignment = BaselineAlignment.Baseline;
            _CultureInfo = CultureInfo.CurrentUICulture;
            _FontRenderingEmSize = 16.0;
            _ForegroundBrush = Brushes.Black;
            _FontFamily = new FontFamily("Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif");
            _FontStyle = FontStyles.Normal;
            _FontWeight = FontWeights.Normal;
            _FontStretch = FontStretches.Normal;
            _TypographyProperties = new GenericTextRunTypographyProperties();
        }

        public GenericTextRunProperties(GenericTextRunProperties copy)
        {
            _BackgroundBrush = copy._BackgroundBrush;
            _BaselineAlignment = copy._BaselineAlignment;
            _CultureInfo = copy._CultureInfo;
            _FontHintingEmSize = copy._FontHintingEmSize;
            _FontRenderingEmSize = copy._FontRenderingEmSize;
            _ForegroundBrush = copy._ForegroundBrush;
            _NumberSubstitution = copy._NumberSubstitution;
            _TextDecorations = copy._TextDecorations;
            _TextEffects = copy._TextEffects;
            _FontFamily = copy._FontFamily;
            _FontStyle = copy._FontStyle;
            _FontWeight = copy._FontWeight;
            _FontStretch = copy._FontStretch;
            _Typeface = copy._Typeface;
            _TypographyProperties = copy._TypographyProperties;


            if (_BackgroundBrush != null && !_BackgroundBrush.IsFrozen)
            {
                _BackgroundBrush = _BackgroundBrush.Clone();
            }

            if (_ForegroundBrush != null && !_ForegroundBrush.IsFrozen)
            {
                _ForegroundBrush = _ForegroundBrush.Clone();
            }

            if (_TextDecorations != null && !_TextDecorations.IsFrozen)
            {
                _TextDecorations = _TextDecorations.Clone();
            }

            if (_TextEffects != null && !_TextEffects.IsFrozen)
            {
                _TextEffects = _TextEffects.Clone();
            }

            if (_Typeface != null)
            {
                _Typeface = new Typeface(_Typeface.FontFamily, _Typeface.Style, _Typeface.Weight, _Typeface.Stretch);
            }

            if (_TypographyProperties != null && _TypographyProperties is ICloneable)
            {
                _TypographyProperties = (TextRunTypographyProperties)((ICloneable)_TypographyProperties).Clone();
            }
        }

        #endregion

        #region Properties

        public override Brush BackgroundBrush
        {
            get { return _BackgroundBrush; }
        }

        public override BaselineAlignment BaselineAlignment
        {
            get { return _BaselineAlignment; }
        }

        public override CultureInfo CultureInfo
        {
            get { return _CultureInfo; }
        }

        public override double FontHintingEmSize
        {
            get { return _FontHintingEmSize; }
        }

        public override double FontRenderingEmSize
        {
            get { return _FontRenderingEmSize; }
        }

        public override Brush ForegroundBrush
        {
            get { return _ForegroundBrush; }
        }

        public override NumberSubstitution NumberSubstitution
        {
            get { return _NumberSubstitution; }
        }

        public override TextDecorationCollection TextDecorations
        {
            get { return _TextDecorations; }
        }

        public override TextEffectCollection TextEffects
        {
            get { return _TextEffects; }
        }

        public virtual FontFamily FontFamily
        {
            get { return _FontFamily; }
        }

        public virtual FontStyle FontStyle
        {
            get { return _FontStyle; }
        }

        public virtual FontWeight FontWeight
        {
            get { return _FontWeight; }
        }

        public virtual FontStretch FontStretch
        {
            get { return _FontStretch; }
        }

        public override Typeface Typeface
        {
            get 
            {
                if (_Typeface == null && FontFamily != null)
                {
                    _Typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
                }
                return _Typeface;
            }
        }

        /// <summary>
        /// If TypographyProperties is null, TextFormatter will use its own internal TextRunTypographyProperties object
        /// to supply default values. This internal object cannot be mixed with user-defined ones; i.e., if any run within
        /// a paragraph returns a user-defined TextRunTypographyProperties object, they ALL must return that type of object.
        /// </summary>
        public override TextRunTypographyProperties TypographyProperties
        {
            get { return _TypographyProperties; }
        }

        #endregion

        #region Methods

        public virtual void SetBackgroundBrush(Brush backgroundBrush)
        {
            if (backgroundBrush != _BackgroundBrush)
            {
                _BackgroundBrush = backgroundBrush;
                RaiseFormatChanged(BackgroundBrushProperty);
            }
        }

        public virtual void SetBaselineAlignment(BaselineAlignment baselineAlignment)
        {
            if (baselineAlignment != _BaselineAlignment)
            {
                _BaselineAlignment = baselineAlignment;
                RaiseFormatChanged(BaselineAlignmentProperty);
            }
        }

        public virtual void SetCultureInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo != _CultureInfo)
            {
                _CultureInfo = cultureInfo;
                RaiseFormatChanged(CultureInfoProperty);
            }
        }

        public virtual void SetFontHintingEmSize(double fontHintingEmSize)
        {
            if (fontHintingEmSize != _FontHintingEmSize)
            {
                _FontHintingEmSize = fontHintingEmSize;
                RaiseFormatChanged(FontHintingEmSizeProperty);
            }
        }

        public virtual void SetFontRenderingEmSize(double fontRenderingEmSize)
        {
            if (fontRenderingEmSize != _FontRenderingEmSize)
            {
                _FontRenderingEmSize = fontRenderingEmSize;
                RaiseFormatChanged(FontRenderingEmSizeProperty);
            }
        }

        public virtual void SetForegroundBrush(Brush foregroundBrush)
        {
            if (foregroundBrush != _ForegroundBrush)
            {
                _ForegroundBrush = foregroundBrush;
                RaiseFormatChanged(ForegroundBrushProperty);
            }
        }

        public virtual void SetNumberSubstitution(NumberSubstitution numberSubstitution)
        {
            if (numberSubstitution != _NumberSubstitution)
            {
                _NumberSubstitution = numberSubstitution;
                RaiseFormatChanged(NumberSubstitutionProperty);
            }
        }

        public virtual void SetTextDecorations(TextDecorationCollection textDecorations)
        {
            if (textDecorations != _TextDecorations)
            {
                _TextDecorations = textDecorations;
                RaiseFormatChanged(TextDecorationsProperty);
            }
        }

        public virtual void SetTextEffects(TextEffectCollection textEffects)
        {
            if (textEffects != _TextEffects)
            {
                _TextEffects = textEffects;
                RaiseFormatChanged(TextEffectsProperty);
            }
        }

        public virtual void SetFontFamily(FontFamily fontFamily)
        {
            if (fontFamily != _FontFamily)
            {
                _FontFamily = fontFamily;
                _Typeface = null;
                RaiseFormatChanged(FontFamilyProperty);
                RaiseFormatChanged(TypefaceProperty);
            }
        }

        public virtual void SetFontStyle(FontStyle fontStyle)
        {
            if (fontStyle != _FontStyle)
            {
                _FontStyle = fontStyle;
                _Typeface = null;
                RaiseFormatChanged(FontStyleProperty);
                RaiseFormatChanged(TypefaceProperty);
            }
        }

        public virtual void SetFontWeight(FontWeight fontWeight)
        {
            if (fontWeight != _FontWeight)
            {
                _FontWeight = fontWeight;
                _Typeface = null;
                RaiseFormatChanged(FontWeightProperty);
                RaiseFormatChanged(TypefaceProperty);
            }
        }

        public virtual void SetFontStretch(FontStretch fontStretch)
        {
            if (fontStretch != _FontStretch)
            {
                _FontStretch = FontStretch;
                _Typeface = null;
                RaiseFormatChanged(FontStretchProperty);
                RaiseFormatChanged(TypefaceProperty);
            }
        }

        public virtual void SetTypeface(Typeface typeface)
        {
            if (typeface != _Typeface)
            {
                _Typeface = typeface;
                RaiseFormatChanged(TypefaceProperty);
            }
        }

        public virtual void SetTypographyProperties(TextRunTypographyProperties typographyProperties)
        {
            if (_TypographyProperties != typographyProperties)
            {
                _TypographyProperties = typographyProperties;
                RaiseFormatChanged(TypographyPropertiesProperty);
            }
        }

        #endregion

        #region IFormattable

        public const string BackgroundBrushProperty = "background-brush";
        public const string BaselineAlignmentProperty = "baseline-alignment";
        public const string CultureInfoProperty = "culture-info";
        public const string FontHintingEmSizeProperty = "font-hinting-em-size";
        public const string FontRenderingEmSizeProperty = "font-rendering-em-size";
        public const string ForegroundBrushProperty = "foreground-brush";
        public const string NumberSubstitutionProperty = "number-substitution";
        public const string TextDecorationsProperty = "text-decorations";
        public const string TextEffectsProperty = "text-effects";
        public const string FontFamilyProperty = "font-family";
        public const string FontStyleProperty = "font-style";
        public const string FontWeightProperty = "font-weight";
        public const string FontStretchProperty = "font-stretch";
        public const string TypefaceProperty = "typeface";
        public const string TypographyPropertiesProperty = "typography-properties";

        public event EventHandler<FormatChangedEventArgs> FormatChanged;

        protected virtual void RaiseFormatChanged(string propertyName)
        {
            if (FormatChanged != null)
            {
                FormatChanged(this, new FormatChangedEventArgs(propertyName));
            }
        }

        public virtual bool HasProperty(string name)
        {
            switch (name)
            {
                case BackgroundBrushProperty:
                case BaselineAlignmentProperty:
                case CultureInfoProperty:
                case FontHintingEmSizeProperty:
                case FontRenderingEmSizeProperty:
                case ForegroundBrushProperty:
                case NumberSubstitutionProperty:
                case TextDecorationsProperty:
                case TextEffectsProperty:
                case FontFamilyProperty:
                case FontStyleProperty:
                case FontWeightProperty:
                case FontStretchProperty:
                case TypefaceProperty:
                case TypographyPropertiesProperty:
                    return true;
                default:
                    return false;
            }
        }

        public virtual object GetProperty(string name)
        {
            switch (name)
            {
                case BackgroundBrushProperty:
                    return BackgroundBrush;
                case BaselineAlignmentProperty:
                    return BaselineAlignment;
                case CultureInfoProperty:
                    return CultureInfo;
                case FontHintingEmSizeProperty:
                    return FontHintingEmSize;
                case FontRenderingEmSizeProperty:
                    return FontRenderingEmSize;
                case ForegroundBrushProperty:
                    return ForegroundBrush;
                case NumberSubstitutionProperty:
                    return NumberSubstitution;
                case TextDecorationsProperty:
                    return TextDecorations;
                case TextEffectsProperty:
                    return TextEffects;
                case FontFamilyProperty:
                    return FontFamily;
                case FontStyleProperty:
                    return FontStyle;
                case FontWeightProperty:
                    return FontWeight;
                case FontStretchProperty:
                    return FontStretch;
                case TypefaceProperty:
                    return Typeface;
                case TypographyPropertiesProperty:
                    return TypographyProperties;
                default:
                    return null;
            }
        }

        public virtual void SetProperty(string name, object value)
        {
            switch (name)
            {
                case BackgroundBrushProperty:
                    SetBackgroundBrush(SafeConvert.ToBrush(value));
                    break;
                case BaselineAlignmentProperty:
                    SetBaselineAlignment(SafeConvert.ToBaselineAlignment(value));
                    break;
                case CultureInfoProperty:
                    SetCultureInfo(value as CultureInfo);
                    break;
                case FontHintingEmSizeProperty:
                    SetFontHintingEmSize(SafeConvert.ToDouble(value));
                    break;
                case FontRenderingEmSizeProperty:
                    SetFontRenderingEmSize(SafeConvert.ToDouble(value));
                    break;
                case ForegroundBrushProperty:
                    SetForegroundBrush(SafeConvert.ToBrush(value, Brushes.Black));
                    break;
                case NumberSubstitutionProperty:
                    SetNumberSubstitution(value as NumberSubstitution);
                    break;
                case TextDecorationsProperty:
                    SetTextDecorations(SafeConvert.ToTextDecorations(value));
                    break;
                case TextEffectsProperty:
                    SetTextEffects(value as TextEffectCollection);
                    break;
                case FontFamilyProperty:
                    SetFontFamily(SafeConvert.ToFontFamily(value));
                    break;
                case FontStyleProperty:
                    SetFontStyle(SafeConvert.ToFontStyle(value, FontStyles.Normal));
                    break;
                case FontWeightProperty:
                    SetFontWeight(SafeConvert.ToFontWeight(value, FontWeights.Normal));
                    break;
                case FontStretchProperty:
                    SetFontStretch((FontStretch)value);
                    break;
                case TypefaceProperty:
                    SetTypeface(value as Typeface);
                    break;
                case TypographyPropertiesProperty:
                    SetTypographyProperties(value as TextRunTypographyProperties);
                    break;
                default:
                    break;
            }
        }

        public virtual void ResetProperties()
        {

        }

        public virtual int ChangeProperty(string name, object oldValue, object newValue)
        {
            var value = GetProperty(name);
            if (Object.Equals(value, oldValue))
            {
                SetProperty(name, newValue);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region IStyleable

        public virtual IList<string> GetSupportedStyles(ElementContext context)
        {
            if (context.OwnerDocument is HTMLDocument && context.NodeName != SVGElements.TSPAN)
            {
                return GetHTMLSupportedStyles(context);
            }
            else
            {
                return GetSVGSupportedStyles(context);
            }
        }

        public virtual CSSValue GetStyleProperty(ElementContext context, string name)
        {
            if (context.OwnerDocument is HTMLDocument && context.NodeName != SVGElements.TSPAN)
            {
                return GetHTMLStyleProperty(context, name);
            }
            else
            {
                return GetSVGStyleProperty(context, name);
            }
        }

        public virtual void SetStyleProperty(ElementContext context, string name, CSSValue value)
        {
            if (context.OwnerDocument is HTMLDocument && context.NodeName != SVGElements.TSPAN)
            {
                SetHTMLStyleProperty(context, name, value);
            }
            else
            {
                SetSVGStyleProperty(context, name, value);
            }
        }

        #region HTML

        private static readonly string[] _HTMLSupportedStyles = new string[] {
            CSSProperties.FontFamily,
            CSSProperties.FontSize,
            CSSProperties.FontWeight,
            CSSProperties.FontStyle,
            CSSProperties.TextDecoration,
            CSSProperties.VerticalAlign,
            CSSProperties.Color,
            CSSProperties.BackgroundColor
        };

        protected virtual IList<string> GetHTMLSupportedStyles(ElementContext context)
        {
            return _HTMLSupportedStyles;
        }

        protected virtual CSSValue GetHTMLStyleProperty(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.FontFamily:
                    return CSSConverter.ToCSSValue(FontFamily);
                case CSSProperties.FontSize:
                    return CSSValues.Points(FontRenderingEmSize * 72.0 / 96.0);
                case CSSProperties.FontWeight:
                    return CSSConverter.ToCSSValue(FontWeight);
                case CSSProperties.FontStyle:
                    return CSSConverter.ToCSSValue(FontStyle);
                case CSSProperties.TextDecoration:
                    return CSSConverter.ToCSSValue(TextDecorations);
                case CSSProperties.VerticalAlign:
                    return CSSConverter.ToCSSValue(BaselineAlignment);
                case CSSProperties.Color:
                    return CSSConverter.ToCSSValue(ForegroundBrush, CSSValues.Black);
                case CSSProperties.BackgroundColor:
                    return CSSConverter.ToCSSValue(BackgroundBrush, CSSValues.Transparent);
                default:
                    return null;
            }
        }

        protected virtual void SetHTMLStyleProperty(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.FontFamily:
                    SetFontFamily(value);
                    break;
                case CSSProperties.FontSize:
                    SetFontRenderingEmSize(value);
                    break;
                case CSSProperties.FontWeight:
                    SetFontWeight(value);
                    break;
                case CSSProperties.FontStyle:
                    SetFontStyle(value);
                    break;
                case CSSProperties.TextDecoration:
                    SetTextDecorations(value);
                    break;
                case CSSProperties.VerticalAlign:
                    SetBaselineAlignment(value);
                    break;
                case CSSProperties.Color:
                    SetForegroundBrush(value);
                    break;
                case CSSProperties.BackgroundColor:
                    SetBackgroundBrush(value);
                    break;
                default:
                    break;
            }
        }

        protected virtual void SetBackgroundBrush(CSSValue cssValue)
        {
            Brush backgroundBrush = CSSConverter.ToBrush(cssValue, BackgroundBrush);
            if (backgroundBrush == Brushes.White)
                backgroundBrush = null;
            SetBackgroundBrush(backgroundBrush);
        }

        protected virtual void SetBaselineAlignment(CSSValue cssValue)
        {
            BaselineAlignment baselineAlignment = CSSConverter.ToBaselineAlignment(cssValue, BaselineAlignment);
            SetBaselineAlignment(baselineAlignment);
        }

        protected virtual void SetFontRenderingEmSize(CSSValue cssValue)
        {
            double fontSize = CSSConverter.ToLength(cssValue);
            if (fontSize > 0)
            {
                SetFontRenderingEmSize(fontSize);
            }
        }

        protected virtual void SetForegroundBrush(CSSValue cssValue)
        {
            Brush foregroundBrush = CSSConverter.ToBrush(cssValue, ForegroundBrush);
            SetForegroundBrush(foregroundBrush);
        }

        protected virtual void SetTextDecorations(CSSValue cssValue)
        {
            var textDecorations = CSSConverter.ToTextDecorations(cssValue, TextDecorations);
            SetTextDecorations(textDecorations);
        }

        protected virtual void SetFontFamily(CSSValue cssValue)
        {
            FontFamily fontFamily = CSSConverter.ToFontFamily(cssValue);
            SetFontFamily(fontFamily);
        }

        protected virtual void SetFontStyle(CSSValue cssValue)
        {
            FontStyle fontStyle = CSSConverter.ToFontStyle(cssValue, FontStyle);
            SetFontStyle(fontStyle);
        }

        protected virtual void SetFontWeight(CSSValue cssValue)
        {
            FontWeight fontWeight = CSSConverter.ToFontWeight(cssValue, FontWeight);
            SetFontWeight(fontWeight);
        }

        #endregion

        #region SVG

        static readonly string[] _SVGSupportedStyles = new string[] {
        };

        protected virtual IList<string> GetSVGSupportedStyles(ElementContext context)
        {
            return _SVGSupportedStyles;
        }

        protected virtual CSSValue GetSVGStyleProperty(ElementContext context, string name)
        {
            return null;
        }

        protected virtual void SetSVGStyleProperty(ElementContext context, string name, CSSValue value)
        {

        }

        #endregion

        #endregion

        #region INodeSource

        public virtual NodeType GetNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        public virtual string GetNodeName(NodeContext context)
        {
            if (context is SVGDocument || context.OwnerDocument is SVGDocument)
            {
                return GetSVGNodeName(context);
            }
            else
            {
                return GetHTMLNodeName(context);
            }
        }

        public virtual IList<string> GetNodeAttributes(ElementContext context)
        {
            if (context.OwnerDocument is HTMLDocument && context.NodeName != SVGElements.TSPAN)
            {
                return GetHTMLNodeAttributes(context);
            }
            else
            {
                return GetSVGNodeAttributes(context);
            }
        }

        public virtual string GetNodeAttribute(ElementContext context, string name)
        {
            if (context.OwnerDocument is HTMLDocument && context.NodeName != SVGElements.TSPAN)
            {
                return GetHTMLNodeAttribute(context, name);
            }
            else
            {
                return GetSVGNodeAttribute(context, name);
            }
        }

        public virtual void SetNodeAttribute(ElementContext context, string name, string value)
        {
            if (context.OwnerDocument is HTMLDocument && context.NodeName != SVGElements.TSPAN)
            {
                SetHTMLNodeAttribute(context, name, value);
            }
            else
            {
                SetSVGNodeAttribute(context, name, value);
            }
        }

        public virtual void ResetNodeAttribute(ElementContext context, string name)
        {
            if (context.OwnerDocument is HTMLDocument && context.NodeName != SVGElements.TSPAN)
            {
                ResetHTMLNodeAttribute(context, name);
            }
            else
            {
                ResetSVGNodeAttribute(context, name);
            }
        }

        #region HTML

        protected virtual string GetHTMLNodeName(NodeContext context)
        {
            if (BaselineAlignment == BaselineAlignment.Superscript)
            {
                return HTMLElements.SUP;
            }
            else if (BaselineAlignment == BaselineAlignment.Subscript)
            {
                return HTMLElements.SUB;
            }
            else if (FontWeight == FontWeights.Bold)
            {
                return HTMLElements.B;
            }
            else if (FontStyle == FontStyles.Italic)
            {
                return HTMLElements.I;
            }
            else if (TextDecorations != null && TextDecorations.Contains(System.Windows.TextDecorations.Underline[0]))
            {
                return HTMLElements.U;
            }
            else
            {
                return HTMLElements.SPAN;
            }
        }

        static readonly string[] _HTMLNodeAttributes = new string[] {
        };

        protected virtual IList<string> GetHTMLNodeAttributes(ElementContext context)
        {
            return _HTMLNodeAttributes;
        }

        protected virtual string GetHTMLNodeAttribute(ElementContext context, string name)
        {
            return null;
        }

        protected virtual void SetHTMLNodeAttribute(ElementContext context, string name, string value)
        {

        }

        protected virtual void ResetHTMLNodeAttribute(ElementContext context, string name)
        {

        }

        #endregion

        #region SVG

        static readonly string[] _SVGNodeAttributes = new string[] {
            SVGAttributes.FONT_FAMILY,
            SVGAttributes.FONT_SIZE,
            SVGAttributes.FONT_WEIGHT,
            SVGAttributes.FONT_STYLE,
            SVGAttributes.TEXT_DECORATION,
            SVGAttributes.BASELINE_SHIFT,
            SVGAttributes.FILL
        };

        protected virtual string GetSVGNodeName(NodeContext context)
        {
            return SVGElements.NAMESPACE + " " + SVGElements.TSPAN;
        }

        protected virtual IList<string> GetSVGNodeAttributes(ElementContext context)
        {
            return _SVGNodeAttributes;
        }

        protected virtual string GetSVGNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.FONT_FAMILY:
                    return CSSConverter.ToCSSValue(FontFamily).CssText;
                case SVGAttributes.FONT_SIZE:
                    return CSSValues.Number(FontRenderingEmSize).CssText;
                case SVGAttributes.FONT_WEIGHT:
                    return CSSConverter.ToCSSValue(FontWeight).CssText;
                case SVGAttributes.FONT_STYLE:
                    return CSSConverter.ToCSSValue(FontStyle).CssText;
                case SVGAttributes.TEXT_DECORATION:
                    return CSSConverter.ToCSSValue(TextDecorations).CssText;
                case SVGAttributes.BASELINE_SHIFT:
                    return CSSConverter.ToCSSValue(BaselineAlignment).CssText;
                case SVGAttributes.FILL:
                    return CSSConverter.ToCSSValue(ForegroundBrush, CSSValues.Black).CssText;
                default:
                    return null;
            }
        }

        protected virtual void SetSVGNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.FONT_FAMILY:
                    SetFontFamily(CSSProperties.GetPropertyCSSValue(CSSProperties.FontFamily, value));
                    break;
                case SVGAttributes.FONT_SIZE:
                    SetFontRenderingEmSize(CSSProperties.GetPropertyCSSValue(CSSProperties.FontSize, value));
                    break;
                case SVGAttributes.FONT_WEIGHT:
                    SetFontWeight(CSSProperties.GetPropertyCSSValue(CSSProperties.FontWeight, value));
                    break;
                case SVGAttributes.FONT_STYLE:
                    SetFontStyle(CSSProperties.GetPropertyCSSValue(CSSProperties.FontStyle, value));
                    break;
                case SVGAttributes.TEXT_DECORATION:
                    SetTextDecorations(CSSProperties.GetPropertyCSSValue(CSSProperties.TextDecoration, value));
                    break;
                case SVGAttributes.BASELINE_SHIFT:
                    SetBaselineAlignment(CSSProperties.GetPropertyCSSValue(CSSProperties.BaselineShift, value));
                    break;
                case SVGAttributes.FILL:
                    SetForegroundBrush(CSSProperties.GetPropertyCSSValue(CSSProperties.Color, value));
                    break;
                default:
                    break;
            }
        }

        protected virtual void ResetSVGNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.FONT_FAMILY:
                    SetFontFamily(Default.FontFamily);
                    break;
                case SVGAttributes.FONT_SIZE:
                    SetFontRenderingEmSize(Default.FontRenderingEmSize);
                    break;
                case SVGAttributes.FONT_WEIGHT:
                    SetFontWeight(Default.FontWeight);
                    break;
                case SVGAttributes.FONT_STYLE:
                    SetFontStyle(Default.FontStyle);
                    break;
                case SVGAttributes.TEXT_DECORATION:
                    SetTextDecorations(Default.TextDecorations);
                    break;
                case SVGAttributes.BASELINE_SHIFT:
                    SetBaselineAlignment(Default.BaselineAlignment);
                    break;
                case SVGAttributes.FILL:
                    SetForegroundBrush(Default.ForegroundBrush);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new GenericTextRunProperties(this);
        }

        #endregion

        #region Object
        
        public override bool Equals(object obj)
        {
            var other = obj as GenericTextRunProperties;
            if (other == null)
            {
                return false;
            }

            return Object.Equals(this.BackgroundBrush, other.BackgroundBrush) &&
                Object.Equals(this.BaselineAlignment, other.BaselineAlignment) &&
                Object.Equals(this.CultureInfo, other.CultureInfo) &&
                Object.Equals(this.FontHintingEmSize, other.FontHintingEmSize) &&
                Object.Equals(this.FontRenderingEmSize, other.FontRenderingEmSize) &&
                Object.Equals(this.ForegroundBrush, other.ForegroundBrush) &&
                Object.Equals(this.NumberSubstitution, other.NumberSubstitution) &&
                Object.Equals(this.TextDecorations, other.TextDecorations) &&
                Object.Equals(this.TextEffects, other.TextEffects) &&
                (this.Typeface == null && other.Typeface == null || 
                this.Typeface != null && other.Typeface != null &&
                    Object.Equals(this.Typeface.FontFamily, other.Typeface.FontFamily) &&
                    Object.Equals(this.Typeface.Style, other.Typeface.Style) &&
                    Object.Equals(this.Typeface.Weight, other.Typeface.Weight) &&
                    Object.Equals(this.Typeface.Stretch, other.Typeface.Stretch)) &&
                Object.Equals(this.TypographyProperties, other.TypographyProperties);
        }

        public override int GetHashCode()
        {
            return
                ((_BackgroundBrush == null) ? 0 : _BackgroundBrush.GetHashCode()) ^
                ((int)_BaselineAlignment << 3) ^
                ((int)_CultureInfo.GetHashCode() << 6) ^
                _FontHintingEmSize.GetHashCode() ^
                _FontRenderingEmSize.GetHashCode() ^
                ((_ForegroundBrush == null) ? 0 : _ForegroundBrush.GetHashCode()) ^
                ((_NumberSubstitution == null) ? 0 : _NumberSubstitution.GetHashCode()) ^
                ((_TextDecorations == null) ? 0 : _TextDecorations.GetHashCode()) ^
                ((_TextEffects == null) ? 0 : _TextEffects.GetHashCode()) ^
                ((_Typeface == null) ? 0 : (
                    (_Typeface.FontFamily == null) ? 0 : _Typeface.FontFamily.GetHashCode() ^
                    _Typeface.Style.GetHashCode() ^
                    _Typeface.Weight.GetHashCode() ^
                    _Typeface.Stretch.GetHashCode())) ^
                ((_TypographyProperties == null) ? 0 : _TypographyProperties.GetHashCode());
        }

        #endregion
    }
}
