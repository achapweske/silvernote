/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS.Internal;
using DOM.Views;

namespace DOM.CSS
{
    public static class CSSProperties
    {
        #region Names

        public const string Azimth = "azimuth";
        public const string BackgroundAttachment = "background-attachment";
        public const string BackgroundColor = "background-color";
        public const string BackgroundImage = "background-image";
        public const string BackgroundPosition = "background-position";
        public const string BackgroundRepeat = "background-repeat";
        public const string Background = "background";
        public const string BaselineShift = "baseline-shift";
        public const string BorderCollapse = "border-collapse";
        public const string BorderColor = "border-color";
        public const string BorderSpacing = "border-spacing";
        public const string BorderStyle = "border-style";
        public const string BorderTop = "border-top";
        public const string BorderRight = "border-right";
        public const string BorderBottom = "border-bottom";
        public const string BorderLeft = "border-left";
        public const string BorderTopColor = "border-top-color";
        public const string BorderRightColor = "border-right-color";
        public const string BorderBottomColor = "border-bottom-color";
        public const string BorderLeftColor = "border-left-color";
        public const string BorderTopStyle = "border-top-style";
        public const string BorderRightStyle = "border-right-style";
        public const string BorderBottomStyle = "border-bottom-style";
        public const string BorderLeftStyle = "border-left-style";
        public const string BorderTopWidth = "border-top-width";
        public const string BorderRightWidth = "border-right-width";
        public const string BorderBottomWidth = "border-bottom-width";
        public const string BorderLeftWidth = "border-left-width";
        public const string BorderWidth = "border-width";
        public const string BorderTopLeftRadius = "border-top-left-radius";
        public const string BorderTopRightRadius = "border-top-right-radius";
        public const string BorderBottomRightRadius = "border-bottom-right-radius";
        public const string BorderBottomLeftRadius = "border-bottom-left-radius";
        public const string BorderRadius = "border-radius";
        public const string Border = "border";
        public const string Bottom = "bottom";
        public const string BoxSizing = "box-sizing";
        public const string CaptionSide = "caption-side";
        public const string Clear = "clear";
        public const string Clip = "clip";
        public const string Color = "color";
        public const string Content = "content";
        public const string CounterIncrement = "counter-increment";
        public const string CounterReset = "counter-reset";
        public const string CueAfter = "cue-after";
        public const string CueBefore = "cue-before";
        public const string Cue = "cue";
        public const string Cursor = "cursor";
        public const string Direction = "direction";
        public const string Display = "display";
        public const string Elevation = "elevation";
        public const string EmptyCells = "empty-cells";
        public const string Float = "float";
        public const string FontFamily = "font-family";
        public const string FontSize = "font-size";
        public const string FontSizeAdjust = "font-size-adjust";
        public const string FontStretch = "font-stretch";
        public const string FontStyle = "font-style";
        public const string FontVariant = "font-variant";
        public const string FontWeight = "font-weight";
        public const string Font = "font";
        public const string Height = "height";
        public const string Left = "left";
        public const string LetterSpacing = "letter-spacing";
        public const string LineHeight = "line-height";
        public const string ListStyleImage = "list-style-image";
        public const string ListStylePosition = "list-style-position";
        public const string ListStyleType = "list-style-type";
        public const string ListStyle = "list-style";
        public const string MarginRight = "margin-right";
        public const string MarginLeft = "margin-left";
        public const string MarginTop = "margin-top";
        public const string MarginBottom = "margin-bottom";
        public const string Margin = "margin";
        public const string MarkerOffset = "marker-offset";
        public const string MaxHeight = "max-height";
        public const string MaxWidth = "max-width";
        public const string MinHeight = "min-height";
        public const string MinWidth = "min-width";
        public const string Orphans = "orphans";
        public const string OutlineColor = "outline-color";
        public const string OutlineStyle = "outline-style";
        public const string OutlineWidth = "outline-width";
        public const string Outline = "outline";
        public const string Overflow = "overflow";
        public const string PaddingTop = "padding-top";
        public const string PaddingRight = "padding-right";
        public const string PaddingBottom = "padding-bottom";
        public const string PaddingLeft = "padding-left";
        public const string Padding = "padding";
        public const string PageBreakAfter = "page-break-after";
        public const string PageBreakBefore = "page-break-before";
        public const string PageBreakInside = "page-break-inside";
        public const string PauseAfter = "pause-after";
        public const string PauseBefore = "pause-before";
        public const string Pause = "pause";
        public const string PitchRange = "pitch-range";
        public const string Pitch = "pitch";
        public const string PlayDuring = "play-during";
        public const string Position = "position";
        public const string Quotes = "quotes";
        public const string Richness = "richness";
        public const string Right = "right";
        public const string SpeakHeader = "speak-header";
        public const string SpeakNumeral = "speak-numeral";
        public const string SpeakPunctuation = "speak-punctuation";
        public const string Speak = "speak";
        public const string SpeechRate = "speech-rate";
        public const string Stress = "stress";
        public const string TableLayout = "table-layout";
        public const string TextAlign = "text-align";
        public const string TextDecoration = "text-decoration";
        public const string TextIndent = "text-indent";
        public const string TextShadow = "text-shadow";
        public const string TextTransform = "text-transform";
        public const string Top = "top";
        public const string UnicodeBidi = "unicode-bidi";
        public const string VerticalAlign = "vertical-align";
        public const string Visibility = "visibility";
        public const string VoiceFamily = "voice-family";
        public const string Volume = "volume";
        public const string WhiteSpace = "white-space";
        public const string Widows = "widows";
        public const string Width = "width";
        public const string WordSpacing = "word-spacing";
        public const string ZIndex = "z-index";

        #endregion

        #region Definitions

        private delegate bool AppliesToDelegate(Element element);

        private delegate CSSValue ParserDelegate(string text);

        private delegate void SetPropertyDelegate(CSSStyleDeclaration style, string propertyName, string value, string priority);

        private delegate string GetPropertyDelegate(CSSStyleDeclaration style, string propertyName);

        /// <summary>
        /// CSS Property Definition
        /// </summary>
        private class CSSPropertyDef
        {
            public string PropertyName;
            public bool IsShorthand;
            public SetPropertyDelegate SetShorthand;
            public GetPropertyDelegate GetShorthand;
            public CSSValue InitialValue;
            public bool IsInherited;
            public AppliesToDelegate AppliesTo;
            public ParserDelegate Parser;
        }

        /// <summary>
        /// All property definitions.
        /// 
        /// This implements the following table:
        /// 
        /// http://www.w3.org/TR/CSS2/propidx.html
        /// </summary>
        private static CSSPropertyDef[] Properties = new CSSPropertyDef[]
        {
            // azimuth
            new CSSPropertyDef
            {
                PropertyName = Azimth,
                IsShorthand = false,
                InitialValue = CSSValues.Center,
                IsInherited = true,
                AppliesTo = (element) => true,
                Parser = CSSValueListBase.Parse
            },
            // background-attachment
            new CSSPropertyDef
            {
                PropertyName = BackgroundAttachment,
                IsShorthand = false,
                InitialValue = CSSValues.Scroll,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // background-color
            new CSSPropertyDef
            {
                PropertyName = BackgroundColor,
                IsShorthand = false,
                InitialValue = CSSValues.Transparent,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // background-image
            new CSSPropertyDef
            {
                PropertyName = BackgroundImage,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // background-position
            new CSSPropertyDef
            {
                PropertyName = BackgroundPosition,
                IsShorthand = false,
                InitialValue = CSSValueListBase.Parse("0% 0%"),
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // background-repeat
            new CSSPropertyDef
            {
                PropertyName = BackgroundRepeat,
                IsShorthand = false,
                InitialValue = CSSValues.Repeat,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // background
            new CSSPropertyDef
            {
                PropertyName = Background,
                IsShorthand = true,
                SetShorthand = SetBackground,
                GetShorthand = GetBackground,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-collapse
            new CSSPropertyDef
            {
                PropertyName = BorderCollapse,
                IsShorthand = false,
                InitialValue = CSSValues.Separate,
                IsInherited = true,
                AppliesTo = IsDisplayType("table", "inline-table"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-color
            new CSSPropertyDef
            {
                PropertyName = BorderColor,
                IsShorthand = true,
                SetShorthand = SetBorderColor,
                GetShorthand = GetBorderColor,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-spacing
            new CSSPropertyDef
            {
                PropertyName = BorderSpacing,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = true,
                AppliesTo = IsDisplayType("table", "inline-table"),
                Parser = CSSValueListBase.Parse
            },
            // border-style
            new CSSPropertyDef
            {
                PropertyName = BorderStyle,
                IsShorthand = true,
                SetShorthand = SetBorderStyle,
                GetShorthand = GetBorderStyle,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-top
            new CSSPropertyDef
            {
                PropertyName = BorderTop,
                IsShorthand = true,
                SetShorthand = SetBorderTop,
                GetShorthand = GetBorderTop,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-right
            new CSSPropertyDef
            {
                PropertyName = BorderRight,
                IsShorthand = true,
                SetShorthand = SetBorderRight,
                GetShorthand = GetBorderRight,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-bottom
            new CSSPropertyDef
            {
                PropertyName = BorderBottom,
                IsShorthand = true,
                SetShorthand = SetBorderBottom,
                GetShorthand = GetBorderBottom,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-left
            new CSSPropertyDef
            {
                PropertyName = BorderLeft,
                IsShorthand = true,
                SetShorthand = SetBorderLeft,
                GetShorthand = GetBorderLeft,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-top-color
            new CSSPropertyDef
            {
                PropertyName = BorderTopColor,
                IsShorthand = false,
                // TODO: initial value depends on the 'color' property
                InitialValue = CSSValues.Color("black"),
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-right-color
            new CSSPropertyDef
            {
                PropertyName = BorderRightColor,
                IsShorthand = false,
                // TODO: initial value depends on the 'color' property
                InitialValue = CSSValues.Color("black"),
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-bottom-color
            new CSSPropertyDef
            {
                PropertyName = BorderBottomColor,
                IsShorthand = false,
                // TODO: initial value depends on the 'color' property
                InitialValue = CSSValues.Color("black"),
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-left-color
            new CSSPropertyDef
            {
                PropertyName = BorderLeftColor,
                IsShorthand = false,
                // TODO: initial value depends on the 'color' property
                InitialValue = CSSValues.Color("black"),
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-top-style
            new CSSPropertyDef
            {
                PropertyName = BorderTopStyle,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-right-style
            new CSSPropertyDef
            {
                PropertyName = BorderRightStyle,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-bottom-style
            new CSSPropertyDef
            {
                PropertyName = BorderBottomStyle,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-left-style
            new CSSPropertyDef
            {
                PropertyName = BorderLeftStyle,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-top-width
            new CSSPropertyDef
            {
                PropertyName = BorderTopWidth,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-right-width
            new CSSPropertyDef
            {
                PropertyName = BorderRightWidth,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-bottom-width
            new CSSPropertyDef
            {
                PropertyName = BorderBottomWidth,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-left-width
            new CSSPropertyDef
            {
                PropertyName = BorderLeftWidth,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-width
            new CSSPropertyDef
            {
                PropertyName = BorderWidth,
                IsShorthand = true,
                SetShorthand = SetBorderWidth,
                GetShorthand = GetBorderWidth,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // border-top-left-radius
            new CSSPropertyDef
            {
                PropertyName = BorderTopLeftRadius,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-top-right-radius
            new CSSPropertyDef
            {
                PropertyName = BorderTopRightRadius,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-bottom-right-radius
            new CSSPropertyDef
            {
                PropertyName = BorderBottomRightRadius,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-bottom-left-radius
            new CSSPropertyDef
            {
                PropertyName = BorderBottomLeftRadius,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border-radius
            new CSSPropertyDef
            {
                PropertyName = BorderRadius,
                IsShorthand = true,
                SetShorthand = SetBorderRadius,
                GetShorthand = GetBorderRadius,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // border
            new CSSPropertyDef
            {
                PropertyName = Border,
                IsShorthand = true,
                SetShorthand = SetBorder,
                GetShorthand = GetBorder,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // bottom
            new CSSPropertyDef
            {
                PropertyName = Bottom,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = IsPositionedElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // box-sizing
            new CSSPropertyDef
            {
                PropertyName = BoxSizing,
                IsShorthand = false,
                InitialValue = CSSValues.ContentBox,
                IsInherited = false,
                // all elements that accept width or height 
                // => all elements but non-replaced inline elements, table rows, and row groups
                AppliesTo = (element) => !(IsInlineElement(element) && !IsReplacedElement(element) || 
                        IsDisplayType(element, "table-row", "table-row-group")),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // caption-side
            new CSSPropertyDef
            {
                PropertyName = CaptionSide,
                IsShorthand = false,
                InitialValue = CSSValues.Top,
                IsInherited = true,
                AppliesTo = IsDisplayType("table-caption"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // clear
            new CSSPropertyDef
            {
                PropertyName = Clear,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = IsBlockElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // clip
            new CSSPropertyDef
            {
                PropertyName = Clip,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = (element) => GetComputedValue(element, Position) == "absolute",
                Parser = CSSPrimitiveValueBase.Parse
            },
            // color
            new CSSPropertyDef
            {
                PropertyName = Color,
                IsShorthand = false,
                // TODO: depends on user agent
                InitialValue = CSSValues.Color("black"),
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // content
            new CSSPropertyDef
            {
                PropertyName = Content,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = false,
                // TODO: applies to pseudo-elements
                AppliesTo = (element) => false,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // counter-increment
            new CSSPropertyDef
            {
                PropertyName = CounterIncrement,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // counter-reset
            new CSSPropertyDef
            {
                PropertyName = CounterReset,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // cue-after
            new CSSPropertyDef
            {
                PropertyName = CueAfter,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // cue-before
            new CSSPropertyDef
            {
                PropertyName = CueBefore,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // cue
            new CSSPropertyDef
            {
                PropertyName = Cue,
                IsShorthand = true,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // cursor
            new CSSPropertyDef
            {
                PropertyName = Cursor,
                IsShorthand = false,
                InitialValue = CSSValueListBase.Parse("auto"),
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // direction
            new CSSPropertyDef
            {
                PropertyName = Direction,
                IsShorthand = false,
                InitialValue = CSSValues.Ltr,
                IsInherited = true,
                // TODO: applies to all elements, but with caveats
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // display
            new CSSPropertyDef
            {
                PropertyName = Display,
                IsShorthand = false,
                InitialValue = CSSValues.Inline,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // elevation
            new CSSPropertyDef
            {
                PropertyName = Elevation,
                IsShorthand = false,
                InitialValue = CSSValues.Level,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // empty-cells
            new CSSPropertyDef
            {
                PropertyName = EmptyCells,
                IsShorthand = false,
                InitialValue = CSSValues.Show,
                IsInherited = true,
                AppliesTo = IsDisplayType("table-cell"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // float
            new CSSPropertyDef
            {
                PropertyName = Float,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                // TODO: applies to all, but with caveats
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // font-family
            new CSSPropertyDef
            {
                PropertyName = FontFamily,
                IsShorthand = false,
                // TODO: depends on user agent
                InitialValue = CSSPrimitiveValueBase.Ident("serif"),
                IsInherited = true,
                AppliesTo = null,
                Parser = (str) => CSSValueListBase.Parse(str, ',')
            },
            // font-size
            new CSSPropertyDef
            {
                PropertyName = FontSize,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // font-size-adjust
            new CSSPropertyDef
            {
                PropertyName = FontSizeAdjust,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // font-stretch
            new CSSPropertyDef
            {
                PropertyName = FontStretch,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // font-style
            new CSSPropertyDef
            {
                PropertyName = FontStyle,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // font-variant
            new CSSPropertyDef
            {
                PropertyName = FontVariant,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // font-weight
            new CSSPropertyDef
            {
                PropertyName = FontWeight,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // font
            new CSSPropertyDef
            {
                PropertyName = Font,
                IsShorthand = true,
                SetShorthand = SetFont,
                GetShorthand = GetFont,
                InitialValue = null,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // height
            new CSSPropertyDef
            {
                PropertyName = Height,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                // all elements but non-replaced inline elements, table columns, and column groups
                AppliesTo = (element) => !(IsInlineElement(element) && !IsReplacedElement(element) || 
                        IsDisplayType(element, "table-column", "table-column-group")),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // left
            new CSSPropertyDef
            {
                PropertyName = Left,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = IsPositionedElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // letter-spacing
            new CSSPropertyDef
            {
                PropertyName = LetterSpacing,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // line-height
            new CSSPropertyDef
            {
                PropertyName = LineHeight,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // list-style-image
            new CSSPropertyDef
            {
                PropertyName = ListStyleImage,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = true,
                AppliesTo = IsDisplayType("list-item"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // list-style-position
            new CSSPropertyDef
            {
                PropertyName = ListStylePosition,
                IsShorthand = false,
                InitialValue = CSSValues.Outside,
                IsInherited = true,
                AppliesTo = IsDisplayType("list-item"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // list-style-type
            new CSSPropertyDef
            {
                PropertyName = ListStyleType,
                IsShorthand = false,
                InitialValue = CSSValues.Disc,
                IsInherited = true,
                AppliesTo = IsDisplayType("list-item"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // list-style
            new CSSPropertyDef
            {
                PropertyName = ListStyle,
                IsShorthand = true,
                SetShorthand = SetListStyle,
                GetShorthand = GetListStyle,
                InitialValue = null,
                IsInherited = true,
                AppliesTo = IsDisplayType("list-item"),
                Parser = CSSValueListBase.Parse
            },
            // margin-right
            new CSSPropertyDef
            {
                PropertyName = MarginRight,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements except elements with table display types 
                // other than table-caption, table and inline-table
                AppliesTo = IsDisplayType("table-row-group", "table-header-group", "table-footer-group", 
                        "table-row", "table-column-group", "table-column", "table-cell"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // margin-left
            new CSSPropertyDef
            {
                PropertyName = MarginLeft,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements except elements with table display types 
                // other than table-caption, table and inline-table
                AppliesTo = IsDisplayType("table-row-group", "table-header-group", "table-footer-group", 
                        "table-row", "table-column-group", "table-column", "table-cell"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // margin-top
            new CSSPropertyDef
            {
                PropertyName = MarginTop,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements except elements with table display types 
                // other than table-caption, table and inline-table
                AppliesTo = IsDisplayType("table-row-group", "table-header-group", "table-footer-group", 
                        "table-row", "table-column-group", "table-column", "table-cell"),                
                Parser = CSSPrimitiveValueBase.Parse
            },
            // margin-bottom
            new CSSPropertyDef
            {
                PropertyName = MarginBottom,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements except elements with table display types 
                // other than table-caption, table and inline-table
                AppliesTo = IsDisplayType("table-row-group", "table-header-group", "table-footer-group", 
                        "table-row", "table-column-group", "table-column", "table-cell"),                
                Parser = CSSPrimitiveValueBase.Parse
            },
            // margin
            new CSSPropertyDef
            {
                PropertyName = Margin,
                IsShorthand = true,
                SetShorthand = SetMargin,
                GetShorthand = GetMargin,
                InitialValue = null,
                IsInherited = false,
                // all elements except elements with table display types 
                // other than table-caption, table and inline-table
                AppliesTo = IsDisplayType("table-row-group", "table-header-group", "table-footer-group", 
                        "table-row", "table-column-group", "table-column", "table-cell"),
                Parser = CSSValueListBase.Parse
            },
            // marker-offset
            new CSSPropertyDef
            {
                PropertyName = MarkerOffset,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = IsDisplayType("marker"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // max-height
            new CSSPropertyDef
            {
                PropertyName = MaxHeight,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                // all elements but non-replaced inline elements, table columns, and column groups
                AppliesTo = (element) => !(IsInlineElement(element) && !IsReplacedElement(element) ||
                        IsDisplayType(element, "table-column", "table-column-group")),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // max-width
            new CSSPropertyDef
            {
                PropertyName = MaxWidth,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                // all elements but non-replaced inline elements, table rows, and row groups
                AppliesTo = (element) => !(IsInlineElement(element) && !IsReplacedElement(element) ||
                        IsDisplayType(element, "table-row", "table-row-group")),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // min-height
            new CSSPropertyDef
            {
                PropertyName = MinHeight,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements but non-replaced inline elements, table columns, and column groups
                AppliesTo = (element) => !(IsInlineElement(element) && !IsReplacedElement(element) ||
                        IsDisplayType(element, "table-column", "table-column-group")),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // min-width
            new CSSPropertyDef
            {
                PropertyName = MinWidth,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements but non-replaced inline elements, table rows, and row groups
                AppliesTo = (element) => !(IsInlineElement(element) && !IsReplacedElement(element) ||
                        IsDisplayType(element, "table-row", "table-row-group")),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // orphans
            new CSSPropertyDef
            {
                PropertyName = Orphans,
                IsShorthand = false,
                InitialValue = CSSValues.Number(2),
                IsInherited = true,
                AppliesTo = IsBlockElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // outline-color
            new CSSPropertyDef
            {
                PropertyName = OutlineColor,
                IsShorthand = false,
                InitialValue = CSSValues.Invert,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // outline-style
            new CSSPropertyDef
            {
                PropertyName = OutlineStyle,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // outline-width
            new CSSPropertyDef
            {
                PropertyName = OutlineWidth,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // outline
            new CSSPropertyDef
            {
                PropertyName = Outline,
                IsShorthand = true,
                SetShorthand = SetOutline,
                GetShorthand = GetOutline,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // overflow
            new CSSPropertyDef
            {
                PropertyName = Overflow,
                IsShorthand = false,
                InitialValue = CSSValues.Visible,
                IsInherited = false,
                // TODO: block containers (not block element)
                AppliesTo = IsBlockElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // padding-top
            new CSSPropertyDef
            {
                PropertyName = PaddingTop,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements except table-row-group, table-header-group, 
                // table-footer-group, table-row, table-column-group and 
                // table-column
                AppliesTo = (element) => !IsDisplayType(element, "table-row-group", "table-header-group", 
                        "table-footer-group", "table-row", "table-column-group", "table-column"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // padding-right
            new CSSPropertyDef
            {
                PropertyName = PaddingRight,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements except table-row-group, table-header-group, 
                // table-footer-group, table-row, table-column-group and 
                // table-column
                AppliesTo = (element) => !IsDisplayType(element, "table-row-group", "table-header-group", 
                        "table-footer-group", "table-row", "table-column-group", "table-column"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // padding-bottom
            new CSSPropertyDef
            {
                PropertyName = PaddingBottom,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements except table-row-group, table-header-group, 
                // table-footer-group, table-row, table-column-group and 
                // table-column
                AppliesTo = (element) => !IsDisplayType(element, "table-row-group", "table-header-group", 
                        "table-footer-group", "table-row", "table-column-group", "table-column"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // padding-left
            new CSSPropertyDef
            {
                PropertyName = PaddingLeft,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                // all elements except table-row-group, table-header-group, 
                // table-footer-group, table-row, table-column-group and 
                // table-column
                AppliesTo = (element) => !IsDisplayType(element, "table-row-group", "table-header-group", 
                        "table-footer-group", "table-row", "table-column-group", "table-column"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // padding
            new CSSPropertyDef
            {
                PropertyName = Padding,
                IsShorthand = true,
                SetShorthand = SetPadding,
                GetShorthand = GetPadding,
                InitialValue = null,
                IsInherited = false,
                // all elements except table-row-group, table-header-group, 
                // table-footer-group, table-row, table-column-group and 
                // table-column
                AppliesTo = (element) => !IsDisplayType(element, "table-row-group", "table-header-group", 
                        "table-footer-group", "table-row", "table-column-group", "table-column"),
                Parser = CSSValueListBase.Parse
            },
            // page-break-after
            new CSSPropertyDef
            {
                PropertyName = PageBreakAfter,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                // TODO: block-level elements (but see text)
                AppliesTo = IsBlockElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // page-break-before
            new CSSPropertyDef
            {
                PropertyName = PageBreakBefore,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                // TODO: block-level elements (but see text)
                AppliesTo = IsBlockElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // page-break-inside
            new CSSPropertyDef
            {
                PropertyName = PageBreakInside,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                // TODO: block-level elements (but see text)
                AppliesTo = IsBlockElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // pause-after
            new CSSPropertyDef
            {
                PropertyName = PauseAfter,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // pause-before
            new CSSPropertyDef
            {
                PropertyName = PauseBefore,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // pause
            new CSSPropertyDef
            {
                PropertyName = Pause,
                IsShorthand = true,
                InitialValue = null,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // pitch-range
            new CSSPropertyDef
            {
                PropertyName = PitchRange,
                IsShorthand = false,
                InitialValue = CSSValues.Number(50),
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // pitch
            new CSSPropertyDef
            {
                PropertyName = Pitch,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // play-during
            new CSSPropertyDef
            {
                PropertyName = PlayDuring,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // position
            new CSSPropertyDef
            {
                PropertyName = Position,
                IsShorthand = false,
                InitialValue = CSSValues.Static,
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // quotes
            new CSSPropertyDef
            {
                PropertyName = Quotes,
                IsShorthand = false,
                // TODO: initial value depends on user agent
                InitialValue = CSSValueListBase.Parse("\'\"\' \'\"\' \"\'\" \"\'\""),
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // richness
            new CSSPropertyDef
            {
                PropertyName = Richness,
                IsShorthand = false,
                InitialValue = CSSValues.Number(50),
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // right
            new CSSPropertyDef
            {
                PropertyName = Right,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = IsPositionedElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // speak-header
            new CSSPropertyDef
            {
                PropertyName = SpeakHeader,
                IsShorthand = false,
                InitialValue = CSSValues.Once,
                IsInherited = true,
                // TODO: is there a more general way to determine which elements
                // have table header information?
                AppliesTo = (element) => element.NodeName == "th",
                Parser = CSSPrimitiveValueBase.Parse
            },
            // speak-numeral
            new CSSPropertyDef
            {
                PropertyName = SpeakNumeral,
                IsShorthand = false,
                InitialValue = CSSValues.Continuous,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // speak-punctuation
            new CSSPropertyDef
            {
                PropertyName = SpeakPunctuation,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // speak
            new CSSPropertyDef
            {
                PropertyName = Speak,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // speech-rate
            new CSSPropertyDef
            {
                PropertyName = SpeechRate,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // stress
            new CSSPropertyDef
            {
                PropertyName = Stress,
                IsShorthand = false,
                InitialValue = CSSValues.Number(50),
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // table-layout
            new CSSPropertyDef
            {
                PropertyName = TableLayout,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = IsDisplayType("table", "inline-table"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // text-align
            new CSSPropertyDef
            {
                PropertyName = TextAlign,
                IsShorthand = false,
                // TODO: initial value should be a nameless value that acts as 
                // 'left' if 'direction' is 'ltr', 'right' if 'direction' is 'rtl'
                InitialValue = CSSValues.Left,
                IsInherited = true,
                // TODO: block containers
                AppliesTo = IsBlockElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // text-decoration
            new CSSPropertyDef
            {
                PropertyName = TextDecoration,
                IsShorthand = false,
                InitialValue = CSSValueListBase.Empty,
                // TODO: not inherited, but with caveats
                IsInherited = false,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // text-indent
            new CSSPropertyDef
            {
                PropertyName = TextIndent,
                IsShorthand = false,
                InitialValue = CSSValues.Zero,
                IsInherited = true,
                // TODO: applies to block containers
                AppliesTo = IsBlockElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // text-shadow
            new CSSPropertyDef
            {
                PropertyName = TextShadow,
                IsShorthand = false,
                InitialValue = CSSValueListBase.Empty,
                IsInherited = true,
                AppliesTo = null,
                Parser = (str) => CSSValueListBase.Parse(str, ',')
            },
            // text-transform
            new CSSPropertyDef
            {
                PropertyName = TextTransform,
                IsShorthand = false,
                InitialValue = CSSValues.None,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // top
            new CSSPropertyDef
            {
                PropertyName = Top,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = IsPositionedElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // unicode-bidi
            new CSSPropertyDef
            {
                PropertyName = UnicodeBidi,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = false,
                // TODO: applies to all elements, but with caveats
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // vertical-align
            new CSSPropertyDef
            {
                PropertyName = VerticalAlign,
                IsShorthand = false,
                InitialValue = CSSValues.Baseline,
                IsInherited = false,
                AppliesTo = (element) => IsInlineElement(element) || IsDisplayType(element, "table-cell"),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // visibility
            new CSSPropertyDef
            {
                PropertyName = Visibility,
                IsShorthand = false,
                InitialValue = CSSValues.Visible,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // voice-family
            new CSSPropertyDef
            {
                PropertyName = VoiceFamily,
                IsShorthand = false,
                // TODO: initial value depends on user agent
                InitialValue = CSSValueListBase.Parse("male"),
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSValueListBase.Parse
            },
            // volume
            new CSSPropertyDef
            {
                PropertyName = Volume,
                IsShorthand = false,
                InitialValue = CSSValues.Medium,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // white-space
            new CSSPropertyDef
            {
                PropertyName = WhiteSpace,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // widows
            new CSSPropertyDef
            {
                PropertyName = Widows,
                IsShorthand = false,
                InitialValue = CSSValues.Number(2),
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // width
            new CSSPropertyDef
            {
                PropertyName = Width,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                // all elements but non-replaced inline elements, table rows, and row groups
                AppliesTo = (element) => !(IsInlineElement(element) && !IsReplacedElement(element) || 
                        IsDisplayType(element, "table-row", "table-row-group")),
                Parser = CSSPrimitiveValueBase.Parse
            },
            // word-spacing
            new CSSPropertyDef
            {
                PropertyName = WordSpacing,
                IsShorthand = false,
                InitialValue = CSSValues.Normal,
                IsInherited = true,
                AppliesTo = null,
                Parser = CSSPrimitiveValueBase.Parse
            },
            // z-index
            new CSSPropertyDef
            {
                PropertyName = ZIndex,
                IsShorthand = false,
                InitialValue = CSSValues.Auto,
                IsInherited = false,
                AppliesTo = IsPositionedElement,
                Parser = CSSPrimitiveValueBase.Parse
            },
        };


        private static AppliesToDelegate IsDisplayType(params string[] types)
        {
            return (element) => 
                {
                    string display = GetComputedValue(element, Display);

                    return types.Contains(display);
                };
        }

        /// <summary>
        /// All property definitions indexed by their property names
        /// </summary>
        private static Dictionary<string, CSSPropertyDef> PropertiesMap;

        /// <summary>
        /// Initialize the properties map
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, CSSPropertyDef> BuildPropertiesMap()
        {
            var result = new Dictionary<string, CSSPropertyDef>();

            foreach (var property in Properties)
            {
                result.Add(property.PropertyName, property);
            }

            return result;
        }

        /// <summary>
        /// Get the property definition for the given property name
        /// </summary>
        private static CSSPropertyDef GetProperty(string propertyName)
        {
            if (PropertiesMap == null)
            {
                PropertiesMap = BuildPropertiesMap();
            }

            CSSPropertyDef property;

            if (PropertiesMap.TryGetValue(propertyName, out property))
            {
                return property;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region AllProperties

        private static string[] _AllProperties;

        public static string[] AllProperties
        {
            get 
            {
                if (_AllProperties == null)
                {
                    _AllProperties = Properties.Select((defn) => defn.PropertyName).ToArray();
                }

                return _AllProperties; 
            }
        }

        #endregion

        #region Shorthand Properties

        private static string[] _ShorthandProperties;

        /// <summary>
        /// Get the names of all shorthand properties
        /// </summary>
        public static string[] ShorthandProperties
        {
            get
            {
                if (_ShorthandProperties == null)
                {
                    _ShorthandProperties = AllProperties.Where(IsShorthandProperty).ToArray();
                }

                return _ShorthandProperties;
            }
        }

        /// <summary>
        /// Determine if the given property is a shorthand property
        /// </summary>
        public static bool IsShorthandProperty(string propertyName)
        {
            var property = GetProperty(propertyName);

            if (property != null)
            {
                return property.IsShorthand;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set the longhand properties associated with the given shorthand value.
        /// </summary>
        /// <param name="style">The style whose properties are to be set</param>
        /// <param name="propertyName">Shorthand property name</param>
        /// <param name="value">Shorthand property value</param>
        /// <param name="priority">Property priority</param>
        public static void SetShorthandProperty(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            var property = GetProperty(propertyName);

            if (property.SetShorthand != null)
            {
                property.SetShorthand(style, propertyName, value, priority);
            }
        }

        /// <summary>
        /// Get a shorthand property value.
        /// </summary>
        /// <param name="style">The style whose properties are to be retrieved</param>
        /// <param name="propertyName">Name of the shorthand property</param>
        /// <returns>A shorthand value, or String.Empty on error</returns>
        public static string GetShorthandPropertyValue(CSSStyleDeclaration style, string propertyName)
        {
            var property = GetProperty(propertyName);

            if (property.GetShorthand != null)
            {
                return property.GetShorthand(style, propertyName);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// The 'background' property is a shorthand property for setting the 
        /// individual background properties (i.e., 'background-color', 
        /// 'background-image', 'background-repeat', 'background-attachment' 
        /// and 'background-position') at the same place in the style sheet.
        /// 
        /// http://www.w3.org/TR/CSS2/colors.html#propdef-background
        /// </summary>
        private static void SetBackground(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            style.SetProperty(BackgroundColor, GetInitialValue(BackgroundColor), priority);
            style.SetProperty(BackgroundImage, GetInitialValue(BackgroundImage), priority);
            style.SetProperty(BackgroundRepeat, GetInitialValue(BackgroundRepeat), priority);
            style.SetProperty(BackgroundAttachment, GetInitialValue(BackgroundAttachment), priority);
            style.SetProperty(BackgroundPosition, GetInitialValue(BackgroundPosition), priority);

            var valueList = CSSValueListBase.Parse(value);

            foreach (var cssValue in valueList.OfType<CSSPrimitiveValueBase>())
            {
                if (cssValue.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR ||
                    cssValue == CSSValues.Transparent)
                {
                    style.SetProperty(BackgroundColor, cssValue.CssText, priority);
                }
                else if (cssValue.PrimitiveType == CSSPrimitiveType.CSS_URI ||
                    cssValue == CSSValues.None)
                {
                    style.SetProperty(BackgroundImage, cssValue.CssText, priority);
                }
                else if (cssValue == CSSValues.Repeat || cssValue == CSSValues.RepeatX ||
                    cssValue == CSSValues.RepeatY || cssValue == CSSValues.NoRepeat)
                {
                    style.SetProperty(BackgroundRepeat, cssValue.CssText, priority);
                }
                else if (cssValue == CSSValues.Scroll || cssValue == CSSValues.Fixed)
                {
                    style.SetProperty(BackgroundAttachment, cssValue.CssText, priority);
                }
                else
                {
                    style.SetProperty(BackgroundPosition, cssValue.CssText, priority);
                }
            }
        }

        private static string GetBackground(CSSStyleDeclaration style, string propertyName)
        {
            var result = new StringBuilder();

            string backgroundColor = style.GetPropertyValue(BackgroundColor);

            if (!String.IsNullOrEmpty(backgroundColor))
            {
                result.Append(backgroundColor + " ");
            }

            string backgroundImage = style.GetPropertyValue(BackgroundImage);

            if (!String.IsNullOrEmpty(backgroundImage))
            {
                result.Append(backgroundImage + " ");
            }

            string backgroundRepeat = style.GetPropertyValue(BackgroundRepeat);

            if (!String.IsNullOrEmpty(backgroundRepeat))
            {
                result.Append(backgroundRepeat + " ");
            }

            string backgroundAttachment = style.GetPropertyValue(BackgroundAttachment);

            if (!String.IsNullOrEmpty(backgroundAttachment))
            {
                result.Append(backgroundAttachment + " ");
            }

            string backgroundPosition = style.GetPropertyValue(BackgroundPosition);

            if (!String.IsNullOrEmpty(backgroundPosition))
            {
                result.Append(backgroundPosition + " ");
            }

            return result.ToString().TrimEnd();
        }

        /// <summary>
        /// The 'border-color' property can have from one to four component 
        /// values, and the values are set on the different sides as for 
        /// 'border-width'.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#propdef-border-color
        /// </summary>
        /// <param name="value"></param>
        /// <param name="priority"></param>
        private static void SetBorderColor(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            string[] values = ExpandBoxValues(value);

            if (values != null)
            {
                style.SetProperty(BorderTopColor, values[0], priority);
                style.SetProperty(BorderRightColor, values[1], priority);
                style.SetProperty(BorderBottomColor, values[2], priority);
                style.SetProperty(BorderLeftColor, values[3], priority);
            }
        }

        private static string GetBorderColor(CSSStyleDeclaration style, string propertyName)
        {
            var values = new string[]
            {
                style.GetPropertyValue(BorderTopColor),
                style.GetPropertyValue(BorderRightColor),
                style.GetPropertyValue(BorderBottomColor),
                style.GetPropertyValue(BorderLeftColor),
            };

            return CollapseBoxValues(values);
        }

        /// <summary>
        /// http://dev.w3.org/csswg/css3-background/#the-border-radius
        /// </summary>
        private static void SetBorderRadius(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            // Value:	[ <length> | <percentage> ]{1,4} [ / [ <length> | <percentage> ]{1,4} ]?

            string[] xRadii;
            string[] yRadii;

            string[] tokens = value.Split('/');

            if (tokens.Length == 0)
            {
                xRadii = yRadii = new string[] { "", "", "", "" };
            }
            else if (tokens.Length == 1)
            {
                xRadii = yRadii = ExpandBoxValues(tokens[0]);
            }
            else
            {
                xRadii = ExpandBoxValues(tokens[0]);
                yRadii = ExpandBoxValues(tokens[1]);
            }

            string[] radii = new string[]
            {
                xRadii[0] + " " + yRadii[0],
                xRadii[1] + " " + yRadii[1],
                xRadii[2] + " " + yRadii[2],
                xRadii[3] + " " + yRadii[3]
            };

            style.SetProperty(BorderTopLeftRadius, radii[0], priority);
            style.SetProperty(BorderTopRightRadius, radii[1], priority);
            style.SetProperty(BorderBottomRightRadius, radii[2], priority);
            style.SetProperty(BorderBottomLeftRadius, radii[3], priority);
        }

        private static string GetBorderRadius(CSSStyleDeclaration style, string propertyName)
        {
            string[] radii = new string[4];

            radii[0] = style.GetPropertyValue(BorderTopLeftRadius);
            radii[1] = style.GetPropertyValue(BorderTopRightRadius);
            radii[2] = style.GetPropertyValue(BorderBottomRightRadius);
            radii[3] = style.GetPropertyValue(BorderBottomLeftRadius);

            string[] xRadii = new string[4];
            string[] yRadii = new string[4];

            SplitBorderRadius(radii[0], out xRadii[0], out yRadii[0]);
            SplitBorderRadius(radii[1], out xRadii[1], out yRadii[1]);
            SplitBorderRadius(radii[2], out xRadii[2], out yRadii[2]);
            SplitBorderRadius(radii[3], out xRadii[3], out yRadii[3]);

            string xRadius = CollapseBoxValues(xRadii);
            string yRadius = CollapseBoxValues(yRadii);

            if (xRadius == yRadius)
            {
                return xRadius;
            }
            else
            {
                return xRadius + " / " + yRadius;
            }
        }

        private static void SplitBorderRadius(string radius, out string xRadius, out string yRadius)
        {
            string[] tokens = radius.Split(" \t\r\n\f".ToCharArray());

            if (tokens.Length > 0)
            {
                xRadius = tokens[0];
            }
            else
            {
                xRadius = String.Empty;
            }

            if (tokens.Length > 1)
            {
                yRadius = tokens[1];
            }
            else
            {
                yRadius = xRadius;
            }
        }

        /// <summary>
        /// The 'border-style' property can have from one to four component 
        /// values, and the values are set on the different sides as for 
        /// 'border-width'.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#propdef-border-style
        /// </summary>
        private static void SetBorderStyle(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            string[] values = ExpandBoxValues(value);

            if (values != null)
            {
                style.SetProperty(BorderTopStyle, values[0], priority);
                style.SetProperty(BorderRightStyle, values[1], priority);
                style.SetProperty(BorderBottomStyle, values[2], priority);
                style.SetProperty(BorderLeftStyle, values[3], priority);
            }

        }

        private static string GetBorderStyle(CSSStyleDeclaration style, string propertyName)
        {
            var values = new string[]
            {
                style.GetPropertyValue(BorderTopStyle),
                style.GetPropertyValue(BorderRightStyle),
                style.GetPropertyValue(BorderBottomStyle),
                style.GetPropertyValue(BorderLeftStyle),
            };

            return CollapseBoxValues(values);
        }

        /// <summary>
        /// This property is a shorthand property for setting 'border-top-width', 
        /// 'border-right-width', 'border-bottom-width', and 'border-left-width' 
        /// at the same place in the style sheet.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#propdef-border-width
        /// </summary>
        private static void SetBorderWidth(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            string[] values = ExpandBoxValues(value);

            if (values != null)
            {
                style.SetProperty(BorderTopWidth, values[0], priority);
                style.SetProperty(BorderRightWidth, values[1], priority);
                style.SetProperty(BorderBottomWidth, values[2], priority);
                style.SetProperty(BorderLeftWidth, values[3], priority);
            }

        }

        private static string GetBorderWidth(CSSStyleDeclaration style, string propertyName)
        {
            var values = new string[]
            {
                style.GetPropertyValue(BorderTopWidth),
                style.GetPropertyValue(BorderRightWidth),
                style.GetPropertyValue(BorderBottomWidth),
                style.GetPropertyValue(BorderLeftWidth),
            };

            return CollapseBoxValues(values);
        }

        /// <summary>
        /// The 'border' property is a shorthand property for setting the same 
        /// width, color, and style for all four borders of a box.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#propdef-border
        /// </summary>
        /// <param name="value"></param>
        /// <param name="priority"></param>
        private static void SetBorder(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            SetBorderTop(style, BorderTop, value, priority);
            SetBorderRight(style, BorderRight, value, priority);
            SetBorderBottom(style, BorderBottom, value, priority);
            SetBorderLeft(style, BorderLeft, value, priority);
        }

        private static string GetBorder(CSSStyleDeclaration style, string propertyName)
        {
            string borderTop = GetBorderTop(style, BorderTop);
            string borderRight = GetBorderRight(style, BorderRight);
            string borderBottom = GetBorderBottom(style, BorderBottom);
            string borderLeft = GetBorderLeft(style, BorderLeft);

            if (borderTop == borderBottom &&
                borderRight == borderLeft &&
                borderTop == borderRight)
            {
                return borderTop;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// This is a shorthand property for setting the width, style, and 
        /// color of the top border of a box.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#border-shorthand-properties
        /// </summary>
        private static void SetBorderTop(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            SetBorderProperties(style, "top", value, priority);
        }

        private static string GetBorderTop(CSSStyleDeclaration style, string propertyName)
        {
            return GetBorderProperties(style, "top");
        }

        /// <summary>
        /// This is a shorthand property for setting the width, style, and 
        /// color of the right border of a box.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#border-shorthand-properties
        /// </summary>
        private static void SetBorderRight(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            SetBorderProperties(style, "right", value, priority);
        }

        private static string GetBorderRight(CSSStyleDeclaration style, string propertyName)
        {
            return GetBorderProperties(style, "right");
        }

        /// <summary>
        /// This is a shorthand property for setting the width, style, and 
        /// color of the bottom border of a box.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#border-shorthand-properties
        /// </summary>
        private static void SetBorderBottom(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            SetBorderProperties(style, "bottom", value, priority);
        }

        private static string GetBorderBottom(CSSStyleDeclaration style, string propertyName)
        {
            return GetBorderProperties(style, "bottom");
        }

        /// <summary>
        /// This is a shorthand property for setting the width, style, and 
        /// color of the left border of a box.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#border-shorthand-properties
        /// </summary>
        private static void SetBorderLeft(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            SetBorderProperties(style, "left", value, priority);
        }

        private static string GetBorderLeft(CSSStyleDeclaration style, string propertyName)
        {
            return GetBorderProperties(style, "left");
        }

        private static void SetBorderProperties(CSSStyleDeclaration style, string borderSide, string value, string priority)
        {
            var valueList = CSSValueListBase.Parse(value);

            string borderWidth;
            string borderStyle;
            string borderColor;

            if (valueList.Length > 0)
            {
                borderWidth = valueList[0].CssText;
            }
            else
            {
                borderWidth = GetInitialValue(String.Format("border-{0}-width", borderSide));
            }

            if (valueList.Length > 1)
            {
                borderStyle = valueList[1].CssText;
            }
            else
            {
                borderStyle = GetInitialValue(String.Format("border-{0}-style", borderSide));
            }

            if (valueList.Length > 2)
            {
                borderColor = valueList[2].CssText;
            }
            else
            {
                borderColor = GetInitialValue(String.Format("border-{0}-color", borderSide));
            }

            style.SetProperty(String.Format("border-{0}-width", borderSide), borderWidth, priority);
            style.SetProperty(String.Format("border-{0}-style", borderSide), borderStyle, priority);
            style.SetProperty(String.Format("border-{0}-color", borderSide), borderColor, priority);
        }

        private static string GetBorderProperties(CSSStyleDeclaration style, string borderSide)
        {
            string borderWidth = style.GetPropertyValue(String.Format("border-{0}-width", borderSide));
            string borderStyle = style.GetPropertyValue(String.Format("border-{0}-style", borderSide));
            string borderColor = style.GetPropertyValue(String.Format("border-{0}-color", borderSide));

            if (!String.IsNullOrEmpty(borderWidth) &&
                !String.IsNullOrEmpty(borderStyle) &&
                !String.IsNullOrEmpty(borderColor))
            {
                return borderWidth + " " + borderStyle + " " + borderColor;
            }

            if (!String.IsNullOrEmpty(borderWidth) &&
                !String.IsNullOrEmpty(borderStyle) &&
                String.IsNullOrEmpty(borderColor))
            {
                return borderWidth + " " + borderStyle;
            }

            if (!String.IsNullOrEmpty(borderWidth) &&
                String.IsNullOrEmpty(borderStyle) &&
                String.IsNullOrEmpty(borderColor))
            {
                return borderWidth;
            }

            return String.Empty;
        }

        /// <summary>
        /// The 'font' property is, except as described below, a shorthand 
        /// property for setting 'font-style', 'font-variant', 'font-weight', 
        /// 'font-size', 'line-height' and 'font-family' at the same place 
        /// in the style sheet. 
        /// 
        /// http://www.w3.org/TR/CSS2/fonts.html#propdef-font
        /// </summary>
        /// <param name="value"></param>
        /// <param name="priority"></param>
        private static void SetFont(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            /*
            SetProperty("font-style", GetInitialValue("font-style"), priority);
            SetProperty("font-variant", GetInitialValue("font-variant"), priority);
            SetProperty("font-weight", GetInitialValue("font-weight"), priority);
            SetProperty("font-size", GetInitialValue("font-size"), priority);
            SetProperty("line-height", GetInitialValue("line-height"), priority);
            SetProperty("font-family", GetInitialValue("font-family"), priority);
            */
        }

        private static string GetFont(CSSStyleDeclaration style, string propertyName)
        {
            return String.Empty;
        }

        /// <summary>
        /// The 'list-style' property is a shorthand property for setting 
        /// 'list-style-type', 'list-style-image', and 'list-style-position'.
        /// 
        /// http://www.w3.org/TR/CSS2/generate.html#propdef-list-style
        /// </summary>
        private static void SetListStyle(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            if (value == "inherit")
            {
                style.SetProperty(ListStyleType, value, priority);
                style.SetProperty(ListStylePosition, value, priority);
                style.SetProperty(ListStyleImage, value, priority);
                return;
            }

            string[] tokens = value.Split(" \t\r\n\f".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            bool hasNone = false;
            bool hasImage = false;
            bool hasType = false;

            foreach (string token in tokens)
            {
                if (token == "none")
                {
                    hasNone = true;
                }
                else if (token == "inside" || token == "outside")
                {
                    style.SetProperty(ListStylePosition, token, priority);
                }
                else if (token.StartsWith("url("))
                {
                    style.SetProperty(ListStyleImage, token, priority);
                    hasImage = true;
                }
                else
                {
                    style.SetProperty(ListStyleType, token, priority);
                    hasType = true;
                }
            }

            // "A value of 'none' within the 'list-style' property sets 
            // whichever of 'list-style-type' and 'list-style-image' are 
            // not otherwise specified to 'none'. However, if both are 
            // otherwise specified, the declaration is in error (and thus 
            // ignored)."

            if (hasNone)
            {
                if (!hasImage)
                {
                    style.SetProperty(ListStyleImage, "none", priority);
                }
                if (!hasType)
                {
                    style.SetProperty(ListStyleType, "none", priority);
                }
            }

        }

        private static string GetListStyle(CSSStyleDeclaration style, string propertyName)
        {
            return String.Empty;    // TODO
        }

        /// <summary>
        /// The 'margin' property is a shorthand property for setting 'margin-top', 
        /// 'margin-right', 'margin-bottom', and 'margin-left'.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#propdef-margin
        /// </summary>
        private static void SetMargin(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            string[] values = ExpandBoxValues(value);

            if (values != null)
            {
                style.SetProperty(MarginTop, values[0], priority);
                style.SetProperty(MarginRight, values[1], priority);
                style.SetProperty(MarginBottom, values[2], priority);
                style.SetProperty(MarginLeft, values[3], priority);
            }

        }

        private static string GetMargin(CSSStyleDeclaration style, string propertyName)
        {
            var values = new string[]
            {
                style.GetPropertyValue(MarginTop),
                style.GetPropertyValue(MarginRight),
                style.GetPropertyValue(MarginBottom),
                style.GetPropertyValue(MarginLeft),
            };

            return CollapseBoxValues(values);
        }

        /// <summary>
        /// The 'outline' property is a shorthand property for setting 'outline-style', 'outline-width', and 'outline-color'.
        /// </summary>
        private static void SetOutline(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {

        }

        private static string GetOutline(CSSStyleDeclaration style, string propertyName)
        {
            return String.Empty;    // TODO
        }

        /// <summary>
        /// The 'padding' property is a shorthand property for setting 'padding-top', 
        /// 'padding-right', 'padding-bottom', and 'padding-left' at the same place in 
        /// the style sheet.
        /// 
        /// http://www.w3.org/TR/CSS2/box.html#propdef-padding
        /// </summary>
        private static void SetPadding(CSSStyleDeclaration style, string propertyName, string value, string priority)
        {
            string[] values = ExpandBoxValues(value);

            if (values != null)
            {
                style.SetProperty(PaddingTop, values[0], priority);
                style.SetProperty(PaddingRight, values[1], priority);
                style.SetProperty(PaddingBottom, values[2], priority);
                style.SetProperty(PaddingLeft, values[3], priority);
            }
        }

        private static string GetPadding(CSSStyleDeclaration style, string propertyName)
        {
            var values = new string[]
            {
                style.GetPropertyValue(PaddingTop),
                style.GetPropertyValue(PaddingRight),
                style.GetPropertyValue(PaddingBottom),
                style.GetPropertyValue(PaddingLeft),
            };

            return CollapseBoxValues(values);
        }


        /// <summary>
        /// Expand a variable-length value list to 4 box values.
        /// 
        /// This is used for setting the "margin" and "padding" properties
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static string[] ExpandBoxValues(string value)
        {
            string[] values = value.Split(" \t\r\n\f".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (values.Length == 0)
            {
                return null;
            }

            string[] result = new string[4];

            if (values.Length == 1)
            {
                // If there is only one component value, it applies to all sides

                result[0] = values[0];
                result[1] = values[0];
                result[2] = values[0];
                result[3] = values[0];
            }
            else if (values.Length == 2)
            {
                // If there are two values, the top and bottom components are set 
                // to the first value and the right and left components are set to
                // the second.

                result[0] = values[0];
                result[1] = values[1];
                result[2] = values[0];
                result[3] = values[1];
            }
            else if (values.Length == 3)
            {
                // If there are three values, the top is set to the first value, 
                // the left and right are set to the second, and the bottom is 
                // set to the third.

                result[0] = values[0];
                result[1] = values[1];
                result[2] = values[2];
                result[3] = values[1];
            }
            else
            {
                // If there are four values, they apply to the top, right, 
                // bottom, and left, respectively.

                result[0] = values[0];
                result[1] = values[1];
                result[2] = values[2];
                result[3] = values[3];
            }

            return result;
        }

        /// <summary>
        /// Collapse 4 box values into a variable-length value list.
        /// 
        /// This is used for retrieving the "margin" and "padding" properties
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static string CollapseBoxValues(string[] values)
        {
            if (values.Length < 4 ||
                String.IsNullOrEmpty(values[0]) ||
                String.IsNullOrEmpty(values[1]) ||
                String.IsNullOrEmpty(values[2]) ||
                String.IsNullOrEmpty(values[3]))
            {
                return String.Empty;
            }

            var result = new StringBuilder();

            if (values[3] == values[1])
            {
                if (values[0] == values[2])
                {
                    if (values[3] == values[0])
                    {
                        result.Append(values[0]);
                    }
                    else
                    {
                        result.Append(values[0] + " ");
                        result.Append(values[1]);
                    }
                }
                else
                {
                    result.Append(values[0] + " ");
                    result.Append(values[1] + " ");
                    result.Append(values[2]);
                }
            }
            else
            {
                result.Append(values[0] + " ");
                result.Append(values[1] + " ");
                result.Append(values[2] + " ");
                result.Append(values[3]);
            }

            return result.ToString();
        }

        #endregion

        #region Longhand Properties

        private static string[] _LonghandProperties;

        /// <summary>
        /// Get the names of all longhand properties
        /// </summary>
        public static string[] LonghandProperties
        {
            get
            {
                if (_LonghandProperties == null)
                {
                    _LonghandProperties = AllProperties.Where(IsLonghandProperty).ToArray();
                }

                return _LonghandProperties;
            }
        }

        /// <summary>
        /// Determine if the given property is a longhand property
        /// 
        /// Unrecognized properties are assumed to be longhand properties
        /// </summary>
        public static bool IsLonghandProperty(string propertyName)
        {
            return !IsShorthandProperty(propertyName);
        }

        #endregion

        #region Property Attributes

        /// <summary>
        /// Parse a property's value
        /// </summary>
        /// <param name="propertyName">Name of the property whose value is to be parsed</param>
        /// <param name="value">Property value to be parsed</param>
        /// <returns>Parsed property value</returns>
        public static CSSValue GetPropertyCSSValue(string propertyName, string value)
        {
            var property = GetProperty(propertyName);

            if (property != null)
            {
                return property.Parser(value);
            }
            else
            {
                return CSSPrimitiveValueBase.Parse(value);
            }
        }

        /// <summary>
        /// Get a property's initial (default) value.
        /// </summary>
        public static CSSValue GetInitialCSSValue(string propertyName)
        {
            var property = GetProperty(propertyName);

            if (property != null)
            {
                return property.InitialValue;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get a property's initial (default) value.
        /// </summary>
        public static string GetInitialValue(string propertyName)
        {
            CSSValue initialValue = GetInitialCSSValue(propertyName);

            if (initialValue != null)
            {
                return initialValue.CssText;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Determine if the given property is inherited.
        /// </summary>
        public static bool IsInherited(string propertyName)
        {
            var property = GetProperty(propertyName);

            if (property != null)
            {
                return property.IsInherited;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determine if the given property applies to the given element
        /// </summary>
        public static bool AppliesTo(string propertyName, Element element)
        {
            var property = GetProperty(propertyName);

            if (property != null && property.AppliesTo != null)
            {
                return property.AppliesTo(element);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Get all properties that apply to the given element
        /// </summary>
        public static string[] GetApplicableProperties(Element element)
        {
            return AllProperties.Where((propertyName) => AppliesTo(propertyName, element)).ToArray();
        }

        #endregion

        #region Element Attributes

        /// <summary>
        /// Determine if the given element's display property is one of the
        /// given values.
        /// </summary>
        public static bool IsDisplayType(Element element, params string[] args)
        {
            string display = GetComputedValue(element, Display);

            return args.Contains(display);
        }

        /// <summary>
        /// Determine if the given element is a block-level element
        /// 
        /// http://www.w3.org/TR/CSS2/visuren.html#block-boxes
        /// </summary>
        public static bool IsBlockElement(Element element)
        {
            // "The following values of the 'display' property make an element 
            // block-level: 'block', 'list-item', and 'table'."

            return IsDisplayType(element, "block", "list-item", "table");
        }

        /// <summary>
        /// Determine if the given element is an inline element
        /// 
        /// http://www.w3.org/TR/CSS2/visuren.html#inline-boxes
        /// </summary>
        public static bool IsInlineElement(Element element)
        {
            // "The following values of the 'display' property make an element 
            // inline-level: 'inline', 'inline-table', and 'inline-block'."

            return IsDisplayType(element, "inline", "inline-table", "inline-block");
        }

        /// <summary>
        /// Determine if the given element is a positioned element
        /// 
        /// http://www.w3.org/TR/CSS2/visuren.html#position-props
        /// </summary>
        public static bool IsPositionedElement(Element element)
        {
            // "An element is said to be positioned if its 'position' property 
            // has a value other than 'static'."

            return GetComputedValue(element, Position) != "static";
        }

        /// <summary>
        /// Determine if the given element is a replaced element
        /// 
        /// http://www.w3.org/TR/CSS21/conform.html
        /// </summary>
        public static bool IsReplacedElement(Element element)
        {
            // "A replaced element is any element whose appearance and 
            // dimensions are defined by an external resource."

            switch (element.NodeName)
            {
                case "img":
                case "object":
                case "button":
                case "textarea":
                case "input":
                case "select":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get the computed value for the given property
        /// </summary>
        private static string GetComputedValue(Element element, string propertyName, string pseudoElt = "")
        {
            var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, pseudoElt);

            if (style != null)
            {
                return style.GetPropertyValue(propertyName);
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion
    }

}
