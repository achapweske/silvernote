/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;
using DOM.CSS.Internal;

namespace DOM
{
    /// <summary>
    /// User agent settings.
    /// </summary>
    public class UASettings
    {
        private static UASettings _Default;

        public static UASettings Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = new UASettings();
                }

                return _Default;
            }
        }

        static UASettings()
        {
            Style = CSSParser.ParseStylesheet(USER_AGENT_STYLE);
        }

        public UASettings()
        {
            ThinBorderWidth = 1;
            MediumBorderWidth = 2;
            ThickBorderWidth = 4;
            DefaultFontSize = 12;
        }

        #region Border Width

        /// <summary>
        /// Thin border width (units = pixels)
        /// </summary>
        public double ThinBorderWidth { get; set; }

        /// <summary>
        /// Medium border width (units = pixels)
        /// </summary>
        public double MediumBorderWidth { get; set; }

        /// <summary>
        /// Thick border width (units = pixels)
        /// </summary>
        public double ThickBorderWidth { get; set; }

        /// <summary>
        /// http://www.w3.org/TR/CSS2/box.html#value-def-border-width
        /// </summary>
        public CSSPrimitiveValueBase GetBorderWidth(CSSValueBase value)
        {
            if (value == CSSValues.Thin)
            {
                return new CSSPrimitiveValueBase(ThinBorderWidth, CSSPrimitiveType.CSS_PX);
            }

            if (value == CSSValues.Medium)
            {
                return new CSSPrimitiveValueBase(MediumBorderWidth, CSSPrimitiveType.CSS_PX);
            }

            if (value == CSSValues.Thick)
            {
                return new CSSPrimitiveValueBase(ThickBorderWidth, CSSPrimitiveType.CSS_PX);
            }

            return new CSSPrimitiveValueBase(MediumBorderWidth, CSSPrimitiveType.CSS_PX);
        }

        #endregion

        #region Font Sizes

        /// <summary>
        /// Default font size (units = points)
        /// </summary>
        public double DefaultFontSize { get; set; }

        /// <summary>
        /// http://www.w3.org/TR/CSS2/fonts.html#value-def-absolute-size
        /// </summary>
        public CSSPrimitiveValueBase GetFontEmSize(CSSValueBase absoluteSize, string fontFamily = "")
        {
            if (absoluteSize == CSSValues.XXSmall)
            {
                return new CSSPrimitiveValueBase(DefaultFontSize * 0.4, CSSPrimitiveType.CSS_PT);
            }

            if (absoluteSize == CSSValues.XSmall)
            {
                return new CSSPrimitiveValueBase(DefaultFontSize * 0.6, CSSPrimitiveType.CSS_PT);
            }

            if (absoluteSize == CSSValues.Small)
            {
                return new CSSPrimitiveValueBase(DefaultFontSize * 0.8, CSSPrimitiveType.CSS_PT);
            }

            if (absoluteSize == CSSValues.Medium)
            {
                return new CSSPrimitiveValueBase(DefaultFontSize, CSSPrimitiveType.CSS_PT);
            }

            if (absoluteSize == CSSValues.Large)
            {
                return new CSSPrimitiveValueBase(DefaultFontSize * 1.2, CSSPrimitiveType.CSS_PT);
            }

            if (absoluteSize == CSSValues.XLarge)
            {
                return new CSSPrimitiveValueBase(DefaultFontSize * 1.4, CSSPrimitiveType.CSS_PT);
            }

            if (absoluteSize == CSSValues.XXLarge)
            {
                return new CSSPrimitiveValueBase(DefaultFontSize * 1.6, CSSPrimitiveType.CSS_PT);
            }

            return new CSSPrimitiveValueBase(DefaultFontSize, CSSPrimitiveType.CSS_PT);
        }

        #endregion

        #region System Fonts

        /// <summary>
        /// http://www.w3.org/TR/CSS2/fonts.html#propdef-font
        /// </summary>
        public string GetSystemFont(string name)
        {
            // TODO
            return "12pt sans-serif";
        }

        #endregion

        #region Style

        /// <summary>
        /// http://www.w3.org/TR/CSS21/sample.html
        /// </summary>
        public const string USER_AGENT_STYLE =
           @"html, address,
            blockquote,
            body, dd, div,
            dl, dt, fieldset, form,
            frame, frameset,
            h1, h2, h3, h4,
            h5, h6, noframes,
            ol, p, ul, center,
            dir, hr, menu, pre   { display: block; unicode-bidi: embed }
            li              { display: list-item }
            head            { display: none }
            meta            { display: none }
            title           { display: none }
            link            { display: none }
            style           { display: none }
            script          { display: none }
            table           { display: table }
            tr              { display: table-row }
            thead           { display: table-header-group }
            tbody           { display: table-row-group }
            tfoot           { display: table-footer-group }
            col             { display: table-column }
            colgroup        { display: table-column-group }
            td, th          { display: table-cell }
            caption         { display: table-caption }
            th              { font-weight: bolder; text-align: center }
            caption         { text-align: center }
            body            { margin: 8px; }
            h1              { font-size: 2em; margin: .67em 0; }
            h2              { font-size: 1.5em; margin: .75em 0 }
            h3              { font-size: 1.17em; margin: .83em 0 }
            h4, p,
            blockquote, ul,
            fieldset, form,
            ol, dl, dir,
            menu            { margin: 1.12em 0 }
            h5              { font-size: .83em; margin: 1.5em 0 }
            h6              { font-size: .75em; margin: 1.67em 0 }
            h1, h2, h3, h4,
            h5, h6, b,
            strong          { font-weight: bolder }
            blockquote      { margin-left: 40px; margin-right: 40px }
            i, cite, em,
            var, address    { font-style: italic }
            pre, tt, code,
            kbd, samp       { font-family: monospace }
            pre             { white-space: pre }
            button, textarea,
            input, select   { display: inline-block }
            big             { font-size: 1.17em }
            small, sub, sup { font-size: .83em }
            sub             { vertical-align: sub }
            sup             { vertical-align: super }
            table           { border-spacing: 2px; }
            thead, tbody,
            tfoot           { vertical-align: middle }
            td, th, tr      { vertical-align: inherit }
            s, strike, del  { text-decoration: line-through }
            hr              { border: 1px inset }
            ol, ul, dir,
            menu, dd        { margin-left: 40px }
            ol              { list-style-type: decimal }
            ol ul, ul ol,
            ul ul, ol ol    { margin-top: 0; margin-bottom: 0 }
            u, ins          { text-decoration: underline }
            br:before       { content: ""\A""; white-space: pre-line }
            center          { text-align: center }
            :link, :visited { text-decoration: underline }
            :focus          { outline: thin dotted invert }

            /* Begin bidirectionality settings (do not change) */
            BDO[DIR=""ltr""]  { direction: ltr; unicode-bidi: bidi-override }
            BDO[DIR=""rtl""]  { direction: rtl; unicode-bidi: bidi-override }

            *[DIR=""ltr""]    { direction: ltr; unicode-bidi: embed }
            *[DIR=""rtl""]    { direction: rtl; unicode-bidi: embed }

            @media print {
              h1            { page-break-before: always }
              h1, h2, h3,
              h4, h5, h6    { page-break-after: avoid }
              ul, ol, dl    { page-break-before: avoid }
            }";

        /// <summary>
        /// User agent-defined style
        /// </summary>
        public static CSSStyleSheet Style { get; set; }

        #endregion
    }
}
