/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DOM.Internal;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// http://www.w3.org/TR/DOM-Level-2-Style/css.html#CSS-CSSStyleDeclaration
    /// </summary>
    public class CSSStyleDeclarationBase : DOMList<string>, CSS3StyleDeclaration, ICloneable
    {
        #region Constructors

        public CSSStyleDeclarationBase()
        {

        }

        #endregion

        #region CSSStyleDeclaration

        /// <summary>
        /// The CSS rule that contains this declaration block or null if this CSSStyleDeclaration is not attached to a CSSRule.
        /// </summary>
        public virtual CSSRule ParentRule 
        {
            get { throw new NotImplementedException(); } 
        }

        /// <summary>
        /// The number of properties that have been explicitly set in this declaration block. 
        /// 
        /// The range of valid indices is 0 to length-1 inclusive.
        /// </summary>
        public override int Length
        {
            get { return Properties.Count; }
        }

        /// <summary>
        /// Get the name of a CSS property set in this declaration block. 
        /// 
        /// This method can be used to iterate over all properties in this declaration block.
        /// </summary>
        /// <param name="index">Index of the property name to retrieve.</param>
        /// <returns>The name of the property at this ordinal position. The empty string if no property exists at this position.</returns>
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

        /// <summary>
        /// Set a property value and priority within this declaration block.
        /// </summary>
        /// <param name="propertyName">The name of the CSS property.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="priority">The new priority of the property, or the empty string if none.</param>
        public void SetProperty(string propertyName, string value, string priority)
        {
            propertyName = propertyName.ToLower();

            if (String.IsNullOrEmpty(value))
            {
                RemoveProperty(propertyName);
                return;
            }

            if (CSSProperties.IsShorthandProperty(propertyName))
            {
                SetShorthandProperty(propertyName, value, priority);
            }
            else
            {
                SetLonghandProperty(propertyName, value, priority);
            }
        }

        /// <summary>
        /// Remove a CSS property.
        /// </summary>
        /// <param name="propertyName">Name of the CSS property.</param>
        /// <returns>The previous value of the property, or String.Empty if not set.</returns>
        public string RemoveProperty(string propertyName)
        {
            return RemoveLonghandProperty(propertyName);
        }

        /// <summary>
        /// Get the value of the property with the given name.
        /// </summary>
        /// <param name="propertyName">Name of the property to be retrieved.</param>
        /// <returns>The value of the property, or String.Empty if not set.</returns>
        public string GetPropertyValue(string propertyName)
        {
            if (CSSProperties.IsShorthandProperty(propertyName))
            {
                return GetShorthandPropertyValue(propertyName);
            }
            else
            {
                return GetLonghandPropertyValue(propertyName);
            }
        }

        /// <summary>
        /// Get the object representation of the value of a CSS property.
        /// </summary>
        /// <param name="propertyName">Name of the CSS property.</param>
        /// <returns>The value of the property, or null if not set.</returns>
        public CSSValue GetPropertyCSSValue(string propertyName)
        {
            if (CSSProperties.IsShorthandProperty(propertyName))
            {
                return GetShorthandPropertyCSSValue(propertyName);
            }
            else
            {
                return GetLonghandPropertyCSSValue(propertyName);
            }
        }

        /// <summary>
        /// Get the priority of a CSS property (i.e. the "!important" qualifier).
        /// </summary>
        /// <param name="propertyName">Name of the CSS property.</param>
        /// <returns>A string representing the priority, or String.Empty if not set</returns>
        public string GetPropertyPriority(string propertyName)
        {
            return GetLonghandPropertyPriority(propertyName);
        }

        /// <summary>
        /// Get the value of the property with the given name AND priority.
        /// </summary>
        /// <param name="propertyName">Name of the property to be retrieved.</param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public string GetPropertyWithPriority(string propertyName, string priority)
        {
            return GetLonghandPropertyWithPriority(propertyName, priority);
        }

        private string _CssText;

        /// <summary>
        /// The parsable textual representation of the declaration block 
        /// (excluding the surrounding curly braces). 
        /// 
        /// Setting this attribute will result in the parsing of the new value
        /// and resetting of all the properties in the declaration block 
        /// including the removal or addition of properties.
        /// </summary>
        public virtual string CssText
        {
            get
            {
                if (_CssText == null)
                {
                    _CssText = CSSFormatter.FormatStyleDeclaration(this);
                }

                return _CssText;
            }
            set
            {
                _CssText = value;

                if (Length == 0)
                {
                    CSSParser.ParseStyleDeclaration(value, this);
                }
                else
                {
                    var newStyle = CSSParser.ParseStyleDeclaration(value);
                    Set(newStyle);
                }
            }
        }

        #endregion

        #region CSS2Properties

        public string Azimuth 
        {
            get { return GetPropertyValue(CSSProperties.Azimth); }
            set { SetProperty(CSSProperties.Azimth, value, ""); } 
        }

        public string Background 
		{
            get { return GetPropertyValue(CSSProperties.Background); }
            set { SetProperty(CSSProperties.Background, value, ""); }
		}

        public string BackgroundAttachment 
		{
            get { return GetPropertyValue(CSSProperties.BackgroundAttachment); }
            set { SetProperty(CSSProperties.BackgroundAttachment, value, ""); }
		}

        public string BackgroundColor 
		{
            get { return GetPropertyValue(CSSProperties.BackgroundColor); }
            set { SetProperty(CSSProperties.BackgroundColor, value, ""); }
		}

        public string BackgroundImage 
		{
            get { return GetPropertyValue(CSSProperties.BackgroundImage); }
            set { SetProperty(CSSProperties.BackgroundImage, value, ""); }
		}

        public string BackgroundPosition 
		{
            get { return GetPropertyValue(CSSProperties.BackgroundPosition); }
            set { SetProperty(CSSProperties.BackgroundPosition, value, ""); }
		}

        public string BackgroundRepeat 
		{
            get { return GetPropertyValue(CSSProperties.BackgroundRepeat); }
            set { SetProperty(CSSProperties.BackgroundRepeat, value, ""); }
		}

        public string Border 
		{
			get { return GetPropertyValue(CSSProperties.Border); }
            set { SetProperty(CSSProperties.Border, value, ""); }
		}

        public string BorderCollapse 
		{
            get { return GetPropertyValue(CSSProperties.BorderCollapse); }
            set { SetProperty(CSSProperties.BorderCollapse, value, ""); }
		}

        public string BorderColor 
		{
            get { return GetPropertyValue(CSSProperties.BorderColor); }
            set { SetProperty(CSSProperties.BorderColor, value, ""); }
		}

        public string BorderSpacing 
		{
            get { return GetPropertyValue(CSSProperties.BorderSpacing); }
            set { SetProperty(CSSProperties.BorderSpacing, value, ""); }
		}

        public string BorderStyle 
		{
            get { return GetPropertyValue(CSSProperties.BorderStyle); }
            set { SetProperty(CSSProperties.BorderStyle, value, ""); }
		}

        public string BorderTop 
		{
            get { return GetPropertyValue(CSSProperties.BorderTop); }
            set { SetProperty(CSSProperties.BorderTop, value, ""); }
		}

        public string BorderRight 
		{
            get { return GetPropertyValue(CSSProperties.BorderRight); }
            set { SetProperty(CSSProperties.BorderRight, value, ""); }
		}

        public string BorderBottom 
		{
            get { return GetPropertyValue(CSSProperties.BorderBottom); }
            set { SetProperty(CSSProperties.BorderBottom, value, ""); }
		}

        public string BorderLeft 
		{
            get { return GetPropertyValue(CSSProperties.BorderLeft); }
            set { SetProperty(CSSProperties.BorderLeft, value, ""); }
		}

        public string BorderTopColor 
		{
            get { return GetPropertyValue(CSSProperties.BorderTopColor); }
            set { SetProperty(CSSProperties.BorderTopColor, value, ""); }
		}

        public string BorderRightColor 
		{
            get { return GetPropertyValue(CSSProperties.BorderRightColor); }
            set { SetProperty(CSSProperties.BorderRightColor, value, ""); }
		}

        public string BorderBottomColor 
		{
            get { return GetPropertyValue(CSSProperties.BorderBottomColor); }
            set { SetProperty(CSSProperties.BorderBottomColor, value, ""); }
		}

        public string BorderLeftColor 
		{
            get { return GetPropertyValue(CSSProperties.BorderLeftColor); }
            set { SetProperty(CSSProperties.BorderLeftColor, value, ""); }
		}

        public string BorderTopStyle 
		{
            get { return GetPropertyValue(CSSProperties.BorderTopStyle); }
            set { SetProperty(CSSProperties.BorderTopStyle, value, ""); }
		}

        public string BorderRightStyle 
		{
            get { return GetPropertyValue(CSSProperties.BorderRightStyle); }
            set { SetProperty(CSSProperties.BorderRightStyle, value, ""); }
		}

        public string BorderBottomStyle 
		{
            get { return GetPropertyValue(CSSProperties.BorderBottomStyle); }
            set { SetProperty(CSSProperties.BorderBottomStyle, value, ""); }
		}

        public string BorderLeftStyle 
		{
            get { return GetPropertyValue(CSSProperties.BorderLeftStyle); }
            set { SetProperty(CSSProperties.BorderLeftStyle, value, ""); }
		}

        public string BorderTopWidth 
		{
            get { return GetPropertyValue(CSSProperties.BorderTopWidth); }
            set { SetProperty(CSSProperties.BorderTopWidth, value, ""); }
		}

        public string BorderRightWidth 
		{
            get { return GetPropertyValue(CSSProperties.BorderRightWidth); }
            set { SetProperty(CSSProperties.BorderRightWidth, value, ""); }
		}

        public string BorderBottomWidth 
		{
            get { return GetPropertyValue(CSSProperties.BorderBottomWidth); }
            set { SetProperty(CSSProperties.BorderBottomWidth, value, ""); }
		}

        public string BorderLeftWidth 
		{
            get { return GetPropertyValue(CSSProperties.BorderLeftWidth); }
            set { SetProperty(CSSProperties.BorderLeftWidth, value, ""); }
		}

        public string BorderWidth 
		{
            get { return GetPropertyValue(CSSProperties.BorderWidth); }
            set { SetProperty(CSSProperties.BorderWidth, value, ""); }
		}

        public string Bottom 
		{
            get { return GetPropertyValue(CSSProperties.Bottom); }
            set { SetProperty(CSSProperties.Bottom, value, ""); }
		}

        public string CaptionSide 
		{
            get { return GetPropertyValue(CSSProperties.CaptionSide); }
            set { SetProperty(CSSProperties.CaptionSide, value, ""); }
		}

        public string Clear 
		{
            get { return GetPropertyValue(CSSProperties.Clear); }
            set { SetProperty(CSSProperties.Clear, value, ""); }
		}

        public string Clip 
		{
            get { return GetPropertyValue(CSSProperties.Clip); }
            set { SetProperty(CSSProperties.Clip, value, ""); }
		}

        public string Color 
		{
            get { return GetPropertyValue(CSSProperties.Color); }
            set { SetProperty(CSSProperties.Color, value, ""); }
		}

        public string Content 
		{
            get { return GetPropertyValue(CSSProperties.Content); }
            set { SetProperty(CSSProperties.Content, value, ""); }
		}

        public string CounterIncrement 
		{
            get { return GetPropertyValue(CSSProperties.CounterIncrement); }
            set { SetProperty(CSSProperties.CounterIncrement, value, ""); }
		}

        public string CounterReset 
		{
            get { return GetPropertyValue(CSSProperties.CounterReset); }
            set { SetProperty(CSSProperties.CounterReset, value, ""); }
		}

        public string Cue 
		{
            get { return GetPropertyValue(CSSProperties.Cue); }
            set { SetProperty(CSSProperties.Cue, value, ""); }
		}

        public string CueAfter 
		{
            get { return GetPropertyValue(CSSProperties.CueAfter); }
            set { SetProperty(CSSProperties.CueAfter, value, ""); }
		}

        public string CueBefore 
		{
            get { return GetPropertyValue(CSSProperties.CueBefore); }
            set { SetProperty(CSSProperties.CueBefore, value, ""); }
		}

        public string Cursor 
		{
            get { return GetPropertyValue(CSSProperties.Cursor); }
            set { SetProperty(CSSProperties.Cursor, value, ""); }
		}

        public string Direction 
		{
            get { return GetPropertyValue(CSSProperties.Direction); }
            set { SetProperty(CSSProperties.Direction, value, ""); }
		}

        public string Display 
		{
            get { return GetPropertyValue(CSSProperties.Display); }
            set { SetProperty(CSSProperties.Display, value, ""); }
		}

        public string Elevation 
		{
            get { return GetPropertyValue(CSSProperties.Elevation); }
            set { SetProperty(CSSProperties.Elevation, value, ""); }
		}

        public string EmptyCells 
		{
            get { return GetPropertyValue(CSSProperties.EmptyCells); }
            set { SetProperty(CSSProperties.EmptyCells, value, ""); }
		}

        public string CssFloat 
		{
            get { return GetPropertyValue(CSSProperties.Float); }
            set { SetProperty(CSSProperties.Float, value, ""); }
		}

        public string Font 
		{
            get { return GetPropertyValue(CSSProperties.Font); }
            set { SetProperty(CSSProperties.Font, value, ""); }
		}

        public string FontFamily 
		{
            get { return GetPropertyValue(CSSProperties.FontFamily); }
            set { SetProperty(CSSProperties.FontFamily, value, ""); }
		}

        public string FontSize 
		{
            get { return GetPropertyValue(CSSProperties.FontSize); }
            set { SetProperty(CSSProperties.FontSize, value, ""); }
		}

        public string FontSizeAdjust 
		{
            get { return GetPropertyValue(CSSProperties.FontSizeAdjust); }
            set { SetProperty(CSSProperties.FontSizeAdjust, value, ""); }
		}

        public string FontStretch 
		{
            get { return GetPropertyValue(CSSProperties.FontStretch); }
            set { SetProperty(CSSProperties.FontStretch, value, ""); }
		}

        public string FontStyle 
		{
            get { return GetPropertyValue(CSSProperties.FontStyle); }
            set { SetProperty(CSSProperties.FontStyle, value, ""); }
		}

        public string FontVariant 
		{
            get { return GetPropertyValue(CSSProperties.FontVariant); }
            set { SetProperty(CSSProperties.FontVariant, value, ""); }
		}

        public string FontWeight 
		{
            get { return GetPropertyValue(CSSProperties.FontWeight); }
            set { SetProperty(CSSProperties.FontWeight, value, ""); }
		}

        public string Height 
		{
            get { return GetPropertyValue(CSSProperties.Height); }
            set { SetProperty(CSSProperties.Height, value, ""); }
		}

        public string Left 
		{
            get { return GetPropertyValue(CSSProperties.Left); }
            set { SetProperty(CSSProperties.Left, value, ""); }
		}

        public string LetterSpacing 
		{
            get { return GetPropertyValue(CSSProperties.LetterSpacing); }
            set { SetProperty(CSSProperties.LetterSpacing, value, ""); }
		}

        public string LineHeight 
		{
            get { return GetPropertyValue(CSSProperties.LineHeight); }
            set { SetProperty(CSSProperties.LineHeight, value, ""); }
		}

        public string ListStyle 
		{
            get { return GetPropertyValue(CSSProperties.ListStyle); }
            set { SetProperty(CSSProperties.ListStyle, value, ""); }
		}

        public string ListStyleImage 
		{
            get { return GetPropertyValue(CSSProperties.ListStyleImage); }
            set { SetProperty(CSSProperties.ListStyleImage, value, ""); }
		}

        public string ListStylePosition 
		{
            get { return GetPropertyValue(CSSProperties.ListStylePosition); }
            set { SetProperty(CSSProperties.ListStylePosition, value, ""); }
		}

        public string ListStyleType 
		{
            get { return GetPropertyValue(CSSProperties.ListStyleType); }
            set { SetProperty(CSSProperties.ListStyleType, value, ""); }
		}

        public string Margin 
		{
            get { return GetPropertyValue(CSSProperties.Margin); }
            set { SetProperty(CSSProperties.Margin, value, ""); }
		}

        public string MarginTop 
		{
            get { return GetPropertyValue(CSSProperties.MarginTop); }
            set { SetProperty(CSSProperties.MarginTop, value, ""); }
		}

        public string MarginRight 
		{
            get { return GetPropertyValue(CSSProperties.MarginRight); }
            set { SetProperty(CSSProperties.MarginRight, value, ""); }
		}

        public string MarginBottom 
		{
            get { return GetPropertyValue(CSSProperties.MarginBottom); }
            set { SetProperty(CSSProperties.MarginBottom, value, ""); }
		}

        public string MarginLeft 
		{
            get { return GetPropertyValue(CSSProperties.MarginLeft); }
            set { SetProperty(CSSProperties.MarginLeft, value, ""); }
		}

        public string MarkerOffset 
		{
            get { return GetPropertyValue(CSSProperties.MarkerOffset); }
            set { SetProperty(CSSProperties.MarkerOffset, value, ""); }
		}

        public string Marks 
		{
			get { return GetPropertyValue("marks"); }
			set { SetProperty("marks", value, ""); }
		}

        public string MaxHeight 
		{
			get { return GetPropertyValue("max-height"); }
			set { SetProperty("max-height", value, ""); }
		}

        public string MaxWidth 
		{
			get { return GetPropertyValue("max-width"); }
			set { SetProperty("max-width", value, ""); }
		}

        public string MinHeight 
		{
			get { return GetPropertyValue("min-height"); }
			set { SetProperty("min-height", value, ""); }
		}

        public string MinWidth 
		{
			get { return GetPropertyValue("min-width"); }
			set { SetProperty("min-width", value, ""); }
		}

        public string Orphans 
		{
			get { return GetPropertyValue("orphans"); }
			set { SetProperty("orphans", value, ""); }
		}

        public string Outline 
		{
			get { return GetPropertyValue("outline"); }
			set { SetProperty("outline", value, ""); }
		}

        public string OutlineColor 
		{
			get { return GetPropertyValue("outline-color"); }
			set { SetProperty("outline-color", value, ""); }
		}

        public string OutlineStyle 
		{
			get { return GetPropertyValue("outline-style"); }
			set { SetProperty("outline-style", value, ""); }
		}

        public string OutlineWidth 
		{
			get { return GetPropertyValue("outline-width"); }
			set { SetProperty("outline-width", value, ""); }
		}

        public string Overflow 
		{
			get { return GetPropertyValue("overflow"); }
			set { SetProperty("overflow", value, ""); }
		}

        public string Padding 
		{
			get { return GetPropertyValue("padding"); }
			set { SetProperty("padding", value, ""); }
		}

        public string PaddingTop 
		{
			get { return GetPropertyValue("padding-top"); }
			set { SetProperty("padding-top", value, ""); }
		}

        public string PaddingRight 
		{
			get { return GetPropertyValue("padding-right"); }
			set { SetProperty("padding-right", value, ""); }
		}

        public string PaddingBottom 
		{
			get { return GetPropertyValue("padding-bottom"); }
			set { SetProperty("padding-bottom", value, ""); }
		}

        public string PaddingLeft 
		{
			get { return GetPropertyValue("padding-left"); }
			set { SetProperty("padding-left", value, ""); }
		}

        public string Page 
		{
			get { return GetPropertyValue("page"); }
			set { SetProperty("page", value, ""); }
		}

        public string PageBreakAfter 
		{
			get { return GetPropertyValue("page-break-after"); }
			set { SetProperty("page-break-after", value, ""); }
		}

        public string PageBreakBefore 
		{
			get { return GetPropertyValue("page-break-before"); }
			set { SetProperty("page-break-before", value, ""); }
		}

        public string PageBreakInside 
		{
			get { return GetPropertyValue("page-break-inside"); }
			set { SetProperty("page-break-inside", value, ""); }
		}

        public string Pause 
		{
			get { return GetPropertyValue("pause"); }
			set { SetProperty("pause", value, ""); }
		}

        public string PauseAfter 
		{
			get { return GetPropertyValue("pause-after"); }
			set { SetProperty("pause-after", value, ""); }
		}

        public string PauseBefore 
		{
			get { return GetPropertyValue("pause-before"); }
			set { SetProperty("pause-before", value, ""); }
		}

        public string Pitch 
		{
			get { return GetPropertyValue("pitch"); }
			set { SetProperty("pitch", value, ""); }
		}

        public string PitchRange 
		{
			get { return GetPropertyValue("pitch-range"); }
			set { SetProperty("pitch-range", value, ""); }
		}

        public string PlayDuring 
		{
			get { return GetPropertyValue("play-during"); }
			set { SetProperty("play-during", value, ""); }
		}

        public string Position 
		{
			get { return GetPropertyValue("position"); }
			set { SetProperty("position", value, ""); }
		}

        public string Quotes 
		{
			get { return GetPropertyValue("quotes"); }
			set { SetProperty("quotes", value, ""); }
		}

        public string Richness 
		{
			get { return GetPropertyValue("richness"); }
			set { SetProperty("richness", value, ""); }
		}

        public string Right 
		{
			get { return GetPropertyValue("right"); }
			set { SetProperty("right", value, ""); }
		}

        public string Size 
		{
			get { return GetPropertyValue("size"); }
			set { SetProperty("size", value, ""); }
		}

        public string Speak 
		{
			get { return GetPropertyValue("speak"); }
			set { SetProperty("speak", value, ""); }
		}

        public string SpeakHeader 
		{
			get { return GetPropertyValue("speak-header"); }
			set { SetProperty("speak-header", value, ""); }
		}

        public string SpeakNumeral 
		{
			get { return GetPropertyValue("speak-numeral"); }
			set { SetProperty("speak-numeral", value, ""); }
		}

        public string SpeakPunctuation 
		{
			get { return GetPropertyValue("speak-punctuation"); }
			set { SetProperty("speak-punctuation", value, ""); }
		}

        public string SpeechRate 
		{
            get { return GetPropertyValue(CSSProperties.SpeechRate); }
            set { SetProperty(CSSProperties.SpeechRate, value, ""); }
		}

        public string Stress 
		{
            get { return GetPropertyValue(CSSProperties.Stress); }
            set { SetProperty(CSSProperties.Stress, value, ""); }
		}

        public string TableLayout 
		{
            get { return GetPropertyValue(CSSProperties.TableLayout); }
            set { SetProperty(CSSProperties.TableLayout, value, ""); }
		}

        public string TextAlign 
		{
			get { return GetPropertyValue(CSSProperties.TextAlign); }
            set { SetProperty(CSSProperties.TextAlign, value, ""); }
		}

        public string TextDecoration 
		{
            get { return GetPropertyValue(CSSProperties.TextDecoration); }
            set { SetProperty(CSSProperties.TextDecoration, value, ""); }
		}

        public string TextIndent 
		{
            get { return GetPropertyValue(CSSProperties.TextIndent); }
            set { SetProperty(CSSProperties.TextIndent, value, ""); }
		}

        public string TextShadow 
		{
            get { return GetPropertyValue(CSSProperties.TextShadow); }
            set { SetProperty(CSSProperties.TextShadow, value, ""); }
		}

        public string TextTransform 
		{
            get { return GetPropertyValue(CSSProperties.TextTransform); }
            set { SetProperty(CSSProperties.TextTransform, value, ""); }
		}

        public string Top 
		{
            get { return GetPropertyValue(CSSProperties.Top); }
            set { SetProperty(CSSProperties.Top, value, ""); }
		}

        public string UnicodeBidi 
		{
            get { return GetPropertyValue(CSSProperties.UnicodeBidi); }
            set { SetProperty(CSSProperties.UnicodeBidi, value, ""); }
		}

        public string VerticalAlign 
		{
            get { return GetPropertyValue(CSSProperties.VerticalAlign); }
            set { SetProperty(CSSProperties.VerticalAlign, value, ""); }
		}

        public string Visibility 
		{
            get { return GetPropertyValue(CSSProperties.Visibility); }
            set { SetProperty(CSSProperties.Visibility, value, ""); }
		}

        public string VoiceFamily 
		{
            get { return GetPropertyValue(CSSProperties.VoiceFamily); }
            set { SetProperty(CSSProperties.VoiceFamily, value, ""); }
		}

        public string Volume 
		{
            get { return GetPropertyValue(CSSProperties.Volume); }
            set { SetProperty(CSSProperties.Volume, value, ""); }
		}

        public string WhiteSpace 
		{
            get { return GetPropertyValue(CSSProperties.WhiteSpace); }
            set { SetProperty(CSSProperties.WhiteSpace, value, ""); }
		}

        public string Widows 
		{
            get { return GetPropertyValue(CSSProperties.Widows); }
            set { SetProperty(CSSProperties.Widows, value, ""); }
		}

        public string Width 
		{
            get { return GetPropertyValue(CSSProperties.Width); }
            set { SetProperty(CSSProperties.Width, value, ""); }
		}

        public string WordSpacing 
		{
            get { return GetPropertyValue(CSSProperties.WordSpacing); }
            set { SetProperty(CSSProperties.WordSpacing, value, ""); }
		}

        public string ZIndex 
		{
            get { return GetPropertyValue(CSSProperties.ZIndex); }
            set { SetProperty(CSSProperties.ZIndex, value, ""); }
		}

        #endregion ICSS2Properties

        #region CSS3Properties

        public string BorderRadius
        {
            get { return GetPropertyValue(CSSProperties.BorderRadius); }
            set { SetProperty(CSSProperties.BorderRadius, value, ""); }
        }

        public string BorderBottomLeftRadius
        {
            get { return GetPropertyValue(CSSProperties.BorderBottomLeftRadius); }
            set { SetProperty(CSSProperties.BorderBottomLeftRadius, value, ""); }
        }

        public string BorderBottomRightRadius
        {
            get { return GetPropertyValue(CSSProperties.BorderBottomRightRadius); }
            set { SetProperty(CSSProperties.BorderBottomRightRadius, value, ""); }
        }

        public string BorderTopLeftRadius
        {
            get { return GetPropertyValue(CSSProperties.BorderTopLeftRadius); }
            set { SetProperty(CSSProperties.BorderTopLeftRadius, value, ""); }
        }
        
        public string BorderTopRightRadius
        {
            get { return GetPropertyValue(CSSProperties.BorderTopRightRadius); }
            set { SetProperty(CSSProperties.BorderTopRightRadius, value, ""); }
        }

        #endregion

        #region Shorthand Properties

        /// <summary>
        /// Set a shorthand property
        /// </summary>
        private void SetShorthandProperty(string propertyName, string value, string priority)
        {
            CSSProperties.SetShorthandProperty(this, propertyName, value, priority);
        }

        /// <summary>
        /// Get a shorthand property value
        /// </summary>
        private string GetShorthandPropertyValue(string propertyName)
        {
            return CSSProperties.GetShorthandPropertyValue(this, propertyName);
        }

        /// <summary>
        /// Get a shorthand property value
        /// </summary>
        private CSSValue GetShorthandPropertyCSSValue(string propertyName)
        {
            string value = GetShorthandPropertyValue(propertyName);

            if (!String.IsNullOrEmpty(value))
            {
                return CSSProperties.GetPropertyCSSValue(propertyName, value);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Extensions

        public void Set(CSSStyleDeclarationBase newDeclaration)
        {
            var oldProperties = new List<CSSStyleProperty>();

            int oldCount = this.Length;
            for (int i = 0; i < oldCount; i++)
            {
                string name = this[i];
                string value = this.GetPropertyValue(name);
                string priority = this.GetPropertyPriority(name);

                oldProperties.Add(new CSSStyleProperty(name, value, priority));
            }

            var newProperties = new List<CSSStyleProperty>();

            int newCount = newDeclaration.Length;
            for (int i = 0; i < newCount; i++)
            {
                string name = newDeclaration[i];
                string value = newDeclaration.GetPropertyValue(name);
                string priority = newDeclaration.GetPropertyPriority(name);

                newProperties.Add(new CSSStyleProperty(name, value, priority));
            }

            if (newProperties.SequenceEqual(oldProperties))
            {
                return;
            }

            var removedProperties = oldProperties.Except(newProperties);
            var removedPropertyNames = removedProperties.Select((p) => p.Name).ToList();
            var addedProperties = newProperties.Except(oldProperties);
            var addedPropertyNames = addedProperties.Select((p) => p.Name).ToList();

            SuppressCollectionChanged = true;

            try
            {
                foreach (var property in removedProperties)
                {
                    this.RemoveProperty(property.Name);
                }

                foreach (var property in newProperties)
                {
                    this.SetProperty(property.Name, property.Value, property.Priority);
                }
            }
            finally
            {
                SuppressCollectionChanged = false;
            }

            RaiseCollectionChanged(removedPropertyNames, addedPropertyNames);
        }

        #endregion

        #region Implementation

        List<string> _PropertyNames;

        List<string> PropertyNames
        {
            get
            {
                if (_PropertyNames == null)
                {
                    _PropertyNames = new List<string>();
                }

                return _PropertyNames;
            }
        }

        Dictionary<string, CSSStyleProperty> _Properties;

        Dictionary<string, CSSStyleProperty> Properties
        {
            get
            {
                if (_Properties == null)
                {
                    _Properties = new Dictionary<string, CSSStyleProperty>();
                }

                return _Properties;
            }
        }

        private CSSStyleProperty FindProperty(string propertyName)
        {
            CSSStyleProperty property;

            if (_Properties != null && _Properties.TryGetValue(propertyName, out property))
            {
                return property;
            }
            else
            {
                return null;
            }
        }

        private static IList<string> _EmptyNames = new string[0];

        protected virtual IList<string> Items
        {
            get 
            {
                if (_PropertyNames != null)
                {
                    return _PropertyNames;
                }
                else
                {
                    return _EmptyNames;
                }
            }
        }

        private int _ImportantPropertyCount;

        protected virtual bool HasImportantProperties
        {
            get { return _ImportantPropertyCount > 0; }
        }

        protected virtual void SetLonghandProperty(string propertyName, string value, string priority)
        {
            var property = FindProperty(propertyName);

            if (property == null)
            {
                property = new CSSStyleProperty(propertyName, value, priority);
                PropertyNames.Add(propertyName);
                Properties.Add(propertyName, property);
                _ImportantPropertyCount += Math.Min(property.Priority.Length, 1);
                RaiseCollectionChanged(null, propertyName);
            }
            else if (value != property.Value || priority != property.Priority)
            {
                property.Value = value;
                _ImportantPropertyCount -= Math.Min(property.Priority.Length, 1);
                property.Priority = priority;
                _ImportantPropertyCount += Math.Min(property.Priority.Length, 1);
                RaiseCollectionChanged(propertyName, propertyName);
            }
        }

        protected virtual string RemoveLonghandProperty(string propertyName)
        {
            var property = FindProperty(propertyName);

            if (property != null)
            {
                _ImportantPropertyCount -= Math.Min(property.Priority.Length, 1);
                PropertyNames.Remove(propertyName);
                Properties.Remove(propertyName);
                RaiseCollectionChanged(propertyName, null);
                return property.Value;
            }
            else
            {
                return String.Empty;
            }
        }

        protected virtual string GetLonghandPropertyValue(string propertyName)
        {
            var property = FindProperty(propertyName);

            if (property != null)
            {
                return property.Value;
            }
            else
            {
                return String.Empty;
            }
        }

        protected virtual string GetLonghandPropertyWithPriority(string propertyName, string priority)
        {
            if (priority.Length > 0 && !HasImportantProperties)
            {
                return String.Empty;
            }

            var property = FindProperty(propertyName);

            if (property == null || property.Priority != priority)
            {
                return String.Empty;
            }

            return property.Value;
        }

        protected virtual CSSValue GetLonghandPropertyCSSValue(string propertyName)
        {
            string value = GetLonghandPropertyValue(propertyName);

            if (!String.IsNullOrEmpty(value))
            {
                return CSSProperties.GetPropertyCSSValue(propertyName, value);
            }
            else
            {
                return null;
            }
        }

        protected virtual string GetLonghandPropertyPriority(string propertyName)
        {
            var property = FindProperty(propertyName);

            if (property != null)
            {
                return property.Priority;
            }
            else
            {
                return String.Empty;
            }
        }

        protected override void OnCollectionChanged(IList<string> removedItems, IList<string> addedItems)
        {
            base.OnCollectionChanged(removedItems, addedItems);

            _CssText = null;
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            var result = new CSSStyleDeclarationBase();
            result.Set(this);
            return result;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CssText;
        }

        #endregion
    }

    internal class CSSStyleProperty
    {
        public CSSStyleProperty()
        {

        }

        public CSSStyleProperty(string name, string value, string priority)
        {
            Name = name;
            Value = value;
            Priority = priority;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public string Priority { get; set; }
    };
}
