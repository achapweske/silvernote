/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Xml;
using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.SVG;
using System.ComponentModel;

namespace SilverNote.Editor
{
    public class TextProperties : GenericTextRunProperties, ICloneable
    {
        #region Static Members

        new public static readonly TextProperties Default = new TextProperties();

        #endregion

        #region Fields

        string _FontClass;
        double _FontSize;
        object _ToolTip;
        string _HyperlinkURL;
        Brush _HyperlinkBrush;
        Brush _DesiredBackgroundBrush;
        Brush _ActualBackgroundBrush;

        #endregion

        #region Constructors

        public TextProperties()
        {
            _FontClass = Editor.FontClass.Normal.ID;
            _FontSize = 16.0;
            _ToolTip = null;
            _HyperlinkURL = null;
            _HyperlinkBrush = Brushes.Blue;
        }

        public TextProperties(TextProperties copy)
            : base(copy)
        {
            _FontClass = copy._FontClass;
            _FontSize = copy._FontSize;
            _ToolTip = copy._ToolTip;
            _HyperlinkURL = copy._HyperlinkURL;
            _HyperlinkBrush = copy._HyperlinkBrush;

            if (_ToolTip is ICloneable)
            {
                _ToolTip = ((ICloneable)_ToolTip).Clone();
            }

            if (_HyperlinkBrush != null && !_HyperlinkBrush.IsFrozen)
            {
                _HyperlinkBrush = _HyperlinkBrush.Clone();
            }
        }

        #endregion

        #region Properties

        public virtual string FontClass
        {
            get { return _FontClass; }
        }

        public virtual double FontSize
        {
            get { return _FontSize; }
        }

        public virtual object ToolTip
        {
            get { return _ToolTip; }
        }

        public string HyperlinkURL
        {
            get { return _HyperlinkURL; }
        }

        public Brush HyperlinkBrush
        {
            get { return _HyperlinkBrush; }
        }

        public override Brush BackgroundBrush
        {
            get
            {
                if (base.BackgroundBrush == null || base.BackgroundBrush == Brushes.Transparent)
                {
                    return base.BackgroundBrush;
                }
                if (base.BackgroundBrush != _DesiredBackgroundBrush)
                {
                    _DesiredBackgroundBrush = base.BackgroundBrush;
                    _ActualBackgroundBrush = _DesiredBackgroundBrush.Clone();
                    _ActualBackgroundBrush.Opacity = 0.5;
                    _ActualBackgroundBrush.Freeze();
                }
                return _ActualBackgroundBrush;
            }
        }

        /// <summary>
        /// Superscripts are rendered at baseline with a translation effect applied
        /// </summary>
        public override BaselineAlignment BaselineAlignment
        {
            get
            {
                if (base.BaselineAlignment == BaselineAlignment.Superscript)
                {
                    return BaselineAlignment.Baseline;
                }
                else
                {
                    return base.BaselineAlignment;
                }
            }
        }

        /// <summary>
        /// Superscripts and subscripts are rendered at 65% their actual size
        /// </summary>
        public override double FontRenderingEmSize
        {
            get
            {
                if (base.BaselineAlignment == BaselineAlignment.Superscript ||
                    base.BaselineAlignment == BaselineAlignment.Subscript)
                {
                    return _FontSize * 0.65;
                }
                else
                {
                    return _FontSize;
                }
            }
        }

        public override Brush ForegroundBrush
        {
            get 
            {
                if (!String.IsNullOrEmpty(HyperlinkURL) && base.ForegroundBrush == Brushes.Black)
                {
                    return HyperlinkBrush;
                }
                else
                {
                    return base.ForegroundBrush;
                }
            }
        }

        /// <summary>
        /// Superscripts are rendered with a translation transform
        /// </summary>
        public override TextEffectCollection TextEffects
        {
            get
            {
                if (base.BaselineAlignment == BaselineAlignment.Superscript)
                {
                    TextEffectCollection textEffects;
                    if (base.TextEffects != null)
                    {
                        textEffects = new TextEffectCollection(base.TextEffects);
                    }
                    else
                    {
                        textEffects = new TextEffectCollection();
                    }

                    var translate = new TranslateTransform(0, Math.Round(-FontSize * 0.35));
                    var superScriptEffect = new TextEffect(translate, null, null, 0, Int32.MaxValue);
                    textEffects.Add(superScriptEffect);

                    return textEffects;
                }
                else
                {
                    return base.TextEffects;
                }
            }
        }

        #endregion

        #region Methods

        public void SetFontClass(string fontClass)
        {
            if (fontClass != _FontClass)
            {
                var oldValue = _FontClass;
                _FontClass = fontClass;
                if (!String.IsNullOrEmpty(fontClass))
                {
                    LoadFontClass(fontClass, oldValue);
                }
                RaiseFormatChanged(FontClassProperty);
            }
        }

        public void SetFontSize(double fontSize)
        {
            if (fontSize != _FontSize)
            {
                _FontSize = fontSize;
                RaiseFormatChanged(FontSizeProperty);
                RaiseFormatChanged(FontRenderingEmSizeProperty);
            }
        }

        public void SetToolTip(object tooltip)
        {
            if (tooltip != _ToolTip)
            {
                _ToolTip = tooltip;
                RaiseFormatChanged(ToolTipProperty);
            }
        }

        public void SetHyperlinkURL(string hyperlinkURL)
        {
            if (hyperlinkURL != _HyperlinkURL)
            {
                _HyperlinkURL = hyperlinkURL;
                RaiseFormatChanged(HyperlinkURLProperty);
                RaiseFormatChanged(ForegroundBrushProperty);
            }
        }

        public void SetHyperlinkBrush(Brush hyperlinkBrush)
        {
            if (hyperlinkBrush != _HyperlinkBrush)
            {
                _HyperlinkBrush = hyperlinkBrush;
                RaiseFormatChanged(HyperlinkBrushProperty);
                RaiseFormatChanged(ForegroundBrushProperty);
            }
        }

        #endregion

        #region IFormattable

        public const string FontClassProperty = "font-class";
        public const string FontSizeProperty = "font-size";
        public const string ToolTipProperty = "tooltip";
        public const string HyperlinkURLProperty = "hyperlink-url";
        public const string HyperlinkBrushProperty = "hyperlink-brush";

        public override bool HasProperty(string name)
        {
            switch (name)
            {
                case FontClassProperty:
                case FontSizeProperty:
                case ToolTipProperty:
                case HyperlinkURLProperty:
                case HyperlinkBrushProperty:
                    return true;
                default:
                    return base.HasProperty(name);
            }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case BackgroundBrushProperty:
                    return base.BackgroundBrush;
                case BaselineAlignmentProperty:
                    return base.BaselineAlignment;
                case FontClassProperty:
                    return FontClass;
                case FontSizeProperty:
                    return FontSize;
                case ToolTipProperty:
                    return ToolTip;
                case HyperlinkURLProperty:
                    return HyperlinkURL;
                case HyperlinkBrushProperty:
                    return HyperlinkBrush;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case FontClassProperty:
                    SetFontClass(SafeConvert.ToString(value, null));
                    break;
                case FontSizeProperty:
                    SetFontSize(SafeConvert.ToDouble(value, FontSize));
                    break;
                case ToolTipProperty:
                    SetToolTip(value);
                    break;
                case HyperlinkURLProperty:
                    SetHyperlinkURL(SafeConvert.ToString(value, null));
                    break;
                case HyperlinkBrushProperty:
                    SetHyperlinkBrush(SafeConvert.ToBrush(value, null));
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        #endregion

        #region HTML

        protected override string GetHTMLNodeName(NodeContext context)
        {
            if (!String.IsNullOrEmpty(HyperlinkURL))
            {
                return HTMLElements.A;
            }
            else if (!String.IsNullOrEmpty(FontClass))
            {
                return HTMLElements.SPAN;
            }
            else
            {
                return base.GetHTMLNodeName(context);
            }
        }

        protected override CSSValue GetHTMLStyleProperty(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.FontSize:
                    return CSSValues.Points(FontSize * 72.0 / 96.0);
                case CSSProperties.BackgroundColor:
                    return CSSConverter.ToCSSValue(base.BackgroundBrush, CSSValues.Transparent);
                case CSSProperties.Color:
                    return CSSConverter.ToCSSValue(base.ForegroundBrush, CSSValues.Black);
                case CSSProperties.VerticalAlign:
                    return CSSConverter.ToCSSValue(base.BaselineAlignment);
                default:
                    return base.GetHTMLStyleProperty(context, name);
            }
        }

        protected override void SetHTMLStyleProperty(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.FontSize:
                    SetFontSize(value);
                    break;
                default:
                    base.SetHTMLStyleProperty(context, name, value);
                    break;
            }
        }

        protected virtual void SetFontSize(CSSValue cssValue)
        {
            double fontSize = CSSConverter.ToLength(cssValue);
            if (fontSize > 0)
            {
                SetFontSize(fontSize);
            }
        }

        static readonly string[] _HTMLNodeAttributes = new string[] {
            HTMLAttributes.CLASS,
            HTMLAttributes.HREF
        };

        protected override IList<string> GetHTMLNodeAttributes(ElementContext context)
        {
            return _HTMLNodeAttributes.Concat(base.GetHTMLNodeAttributes(context)).ToList();
        }

        protected override string GetHTMLNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.CLASS:
                    if (String.IsNullOrWhiteSpace(FontClass) || FontClass == Editor.FontClass.Normal.ID)
                    {
                        return null;
                    }
                    return FontClass;

                case HTMLAttributes.HREF:
                    if (String.IsNullOrWhiteSpace(HyperlinkURL))
                    {
                        return null;
                    }
                    return HyperlinkURL;

                default:
                    return base.GetHTMLNodeAttribute(context, name);
            }
        }

        protected override void SetHTMLNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case HTMLAttributes.CLASS:
                    SetFontClass(value);
                    break;
                case HTMLAttributes.HREF:
                    SetHyperlinkURL(value);
                    break;
                default:
                    base.SetHTMLNodeAttribute(context, name, value);
                    break;
            }
        }

        protected override void ResetHTMLNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.CLASS:
                    SetFontClass(null);
                    break;
                case HTMLAttributes.HREF:
                    SetHyperlinkURL(null);
                    break;
                default:
                    base.ResetHTMLNodeAttribute(context, name);
                    break;
            }
        }

        #endregion

        #region SVG

        static readonly string[] _SVGNodeAttributes = new string[] {
            SVGAttributes.CLASS,
            SVGAttributes.HREF
        };

        protected override IList<string> GetSVGNodeAttributes(ElementContext context)
        {
            return _SVGNodeAttributes.Concat(base.GetSVGNodeAttributes(context)).ToList();
        }

        protected override string GetSVGNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    return !String.IsNullOrWhiteSpace(FontClass) ? FontClass : null;
                case SVGAttributes.FONT_SIZE:
                    return CSSValues.Number(FontSize).CssText;
                case SVGAttributes.FILL:
                    return CSSConverter.ToCSSValue(base.ForegroundBrush, CSSValues.Black).CssText;
                case SVGAttributes.HREF:
                    return !String.IsNullOrWhiteSpace(HyperlinkURL) ? HyperlinkURL : null;
                case SVGAttributes.BASELINE_SHIFT:
                    return CSSConverter.ToCSSValue(base.BaselineAlignment).CssText;
                default:
                    return base.GetSVGNodeAttribute(context, name);
            }
        }

        protected override void SetSVGNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    SetFontClass(value);
                    break;
                case SVGAttributes.FONT_SIZE:
                    SetFontSize(SafeConvert.ToCssSize(value, FontSize));
                    break;
                case SVGAttributes.HREF:
                    SetHyperlinkURL(value);
                    break;
                default:
                    base.SetSVGNodeAttribute(context, name, value);
                    break;
            }
        }

        protected override void ResetSVGNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    SetFontClass(null);
                    break;
                case SVGAttributes.FONT_SIZE:
                    SetFontSize(Default.FontSize);
                    break;
                case SVGAttributes.HREF:
                    SetHyperlinkURL(null);
                    break;
                default:
                    base.ResetSVGNodeAttribute(context, name);
                    break;
            }
        }

        #endregion

        #region Implementation

        void LoadFontClass(string newValue, string oldValue)
        {
            var newClass = Editor.FontClass.FromID(newValue);
            var oldClass = Editor.FontClass.FromID(oldValue);

            if (newClass != null)
            {
                SetFontFamily(newClass.FontFamily);

                if (newClass.FontSize.HasValue)
                {
                    SetFontSize(newClass.FontSize.Value * 96.0 / 72.0);
                }
                else if (oldClass != null && oldClass.FontSize.HasValue)
                {
                    SetFontSize(12.0 * 96.0 / 72.0);
                }

                if (newClass.FontWeight.HasValue)
                {
                    SetFontWeight(newClass.FontWeight.Value);
                }
                else if (oldClass != null && oldClass.FontWeight.HasValue)
                {
                    SetFontWeight(FontWeights.Normal);
                }

                if (newClass.FontStyle.HasValue)
                {
                    SetFontStyle(newClass.FontStyle.Value);
                }
                else if (oldClass != null && oldClass.FontStyle.HasValue)
                {
                    SetFontStyle(FontStyles.Normal);
                }
            }
        }

        #endregion

        #region IClonable

        public override object Clone()
        {
            return new TextProperties(this);
        }

        #endregion

        #region Object

        public override bool Equals(object obj)
        {
            var other = obj as TextProperties;
            if (other == null)
            {
                return false;
            }

            return base.Equals(obj) &&
                Object.Equals(this.FontClass, other.FontClass) &&
                Object.Equals(this.FontSize, other.FontSize) &&
                Object.Equals(this.ToolTip, other.ToolTip) &&
                Object.Equals(this.HyperlinkURL, other.HyperlinkURL) &&
                Object.Equals(this.HyperlinkBrush, other.HyperlinkBrush);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^
                ((FontClass == null) ? 0 : FontClass.GetHashCode()) ^
                FontSize.GetHashCode() ^
                ((ToolTip == null) ? 0 : ToolTip.GetHashCode()) ^
                ((HyperlinkURL == null) ? 0 : HyperlinkURL.GetHashCode()) ^
                ((HyperlinkBrush == null) ? 0 : HyperlinkBrush.GetHashCode());
        }

        #endregion

    }

    public class TextPropertyValue
    {
        public object Value { get; set; }
        public int Length { get; set; }
    }

    public class TextPropertiesValue
    {
        public GenericTextRunProperties Value { get; set; }
        public int Length { get; set; }
    }
}
