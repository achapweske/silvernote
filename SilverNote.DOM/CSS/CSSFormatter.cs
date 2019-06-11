/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DOM.CSS.Internal;

namespace DOM.CSS
{
    /// <summary>
    /// http://www.w3.org/TR/css3-selectors/#w3cselgrammar
    /// </summary>
    public static class CSSFormatter
    {
        public static string FormatStyleSheet(CSSStyleSheet styleSheet)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatStyleSheet(writer, styleSheet);
                return writer.ToString();
            }
        }

        public static void FormatStyleSheet(TextWriter writer, CSSStyleSheet styleSheet)
        {
            foreach (CSSRule rule in styleSheet.CssRules)
            {
                switch (rule.Type)
                {
                    case CSSRuleType.STYLE_RULE:
                        FormatStyleRule(writer, (CSSStyleRule)rule);
                        break;
                    default:
                        break;
                }
            }
        }

        public static string FormatStyleRule(CSSStyleRule styleRule)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatStyleRule(writer, styleRule);
                return writer.ToString();
            }
        }

        public static void FormatStyleRule(TextWriter writer, CSSStyleRule styleRule)
        {
            writer.Write(styleRule.SelectorText);
            writer.Write(" { ");
            writer.Write(styleRule.Style.CssText);
            writer.Write(" }");
        }

        // selectors_group
        //   : selector [ COMMA S* selector ]*
        //   ;

        public static string FormatSelectorGroup(CSSSelectorGroup group)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatSelectorGroup(writer, group);
                return writer.ToString();
            }
        }

        public static void FormatSelectorGroup(TextWriter writer, CSSSelectorGroup group)
        {
            if (group.Count == 0)
            {
                return;
            }

            // selector
            CSSFormatter.FormatSelector(writer, group[0]);

            // [ COMMA S* selector ]*
            for (int i = 1; i < group.Count; i++)
            {
                // COMMA
                writer.Write(',');

                // selector
                CSSFormatter.FormatSelector(writer, group[i]);
            }
        }

        // selector
        //   : simple_selector_sequence [ combinator simple_selector_sequence ]*
        //   ;

        public static string FormatSelector(CSSSelector selector)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatSelector(writer, selector);
                return writer.ToString();
            }
        }

        public static void FormatSelector(TextWriter writer, CSSSelector selector)
        {
            foreach (var item in selector)
            {
                FormatSelectorItem(writer, item);
            }
        }

        public static string FormatSelectorItem(CSSSelectorItem item)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatSelectorItem(writer, item);
                return writer.ToString();
            }
        }

        public static void FormatSelectorItem(TextWriter writer, CSSSelectorItem item)
        {
            CSSFormatter.FormatSimpleSelectorSequence(writer, item.Sequence);
            CSSFormatter.FormatCombinator(writer, item.Combinator);
        }

        // combinator
        //   /* combinators can be surrounded by whitespace */
        //   : PLUS S* | GREATER S* | TILDE S* | S+
        //   ;

        public static void FormatCombinator(TextWriter writer, CSSCombinator combinator)
        {
            switch (combinator)
            {
                case CSSCombinator.Adjacent:
                    writer.Write('+');
                    break;
                case CSSCombinator.Child:
                    writer.Write('>');
                    break;
                case CSSCombinator.Sibling:
                    writer.Write('~');
                    break;
                case CSSCombinator.Descendant:
                    writer.Write(' ');
                    break;
                case CSSCombinator.None:
                default:
                    break;
            }
        }

        // simple_selector_sequence
        //   : [ type_selector | universal ]
        //     [ HASH | class | attrib | pseudo | negation ]*
        //   | [ HASH | class | attrib | pseudo | negation ]+
        //   ;

        public static string FormatSimpleSelectorSequence(CSSSimpleSelectorSequence sequence)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatSimpleSelectorSequence(writer, sequence);
                return writer.ToString();
            }
        }

        public static void FormatSimpleSelectorSequence(TextWriter writer, CSSSimpleSelectorSequence sequence)
        {
            foreach (var item in sequence)
            {
                FormatSimpleSelector(writer, item);
            }
        }

        public static string FormatSimpleSelector(CSSSimpleSelector selector)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatSimpleSelector(writer, selector);
                return writer.ToString();
            }
        }

        public static void FormatSimpleSelector(TextWriter writer, CSSSimpleSelector selector)
        {
            switch (selector.Type)
            {
                case CSSSimpleSelectorType.Universal:
                    FormatUniversalSelector(writer, selector);
                    break;
                case CSSSimpleSelectorType.Type:
                    FormatTypeSelector(writer, selector);
                    break;
                case CSSSimpleSelectorType.ID:
                    FormatIDSelector(writer, selector);
                    break;
                case CSSSimpleSelectorType.Class:
                    FormatClassSelector(writer, selector);
                    break;
                case CSSSimpleSelectorType.Attribute:
                    FormatAttributeSelector(writer, selector);
                    break;
                case CSSSimpleSelectorType.PseudoClass:
                    FormatPseudoClassSelector(writer, selector);
                    break;
                case CSSSimpleSelectorType.PseudoElement:
                    FormatPseudoElementSelector(writer, selector);
                    break;
                default:
                    break;
            }
        }

        // universal
        //  : [ namespace_prefix ]? '*'
        //  ;

        private static void FormatUniversalSelector(TextWriter writer, CSSSimpleSelector selector)
        {
            // [ namespace_prefix ]?
            if (!String.IsNullOrEmpty(selector.Namespace))
            {
                FormatNamespacePrefix(writer, selector.Namespace);
            }

            // '*'
            writer.Write('*');
        }

        // type_selector
        //   : [ namespace_prefix ]? element_name
        //   ;

        private static void FormatTypeSelector(TextWriter writer, CSSSimpleSelector selector)
        {
            // [ namespace_prefix ]?
            if (!String.IsNullOrEmpty(selector.Namespace))
            {
                FormatNamespacePrefix(writer, selector.Namespace);
            }

            // element_name
            //   : IDENT
            //   ;
            writer.Write(selector.Name);
        }

        // id_selector
        //   : HASH
        //   ;

        private static void FormatIDSelector(TextWriter writer, CSSSimpleSelector selector)
        {
            writer.Write('#');
            writer.Write(selector.Name);
        }

        // class
        //   : '.' IDENT
        //   ;

        private static void FormatClassSelector(TextWriter writer, CSSSimpleSelector selector)
        {
            writer.Write('.');
            writer.Write(selector.Name);
        }

        // attrib
        //   : '[' S* [ namespace_prefix ]? IDENT S*
        //         [ [ PREFIXMATCH |
        //             SUFFIXMATCH |
        //             SUBSTRINGMATCH |
        //             '=' |
        //             INCLUDES |
        //             DASHMATCH ] S* [ IDENT | STRING ] S*
        //         ]? ']'
        // ;

        private static void FormatAttributeSelector(TextWriter writer, CSSSimpleSelector selector)
        {
            // '['
            writer.Write('[');

            // [ namespace_prefix ]?
            if (!String.IsNullOrEmpty(selector.Namespace))
            {
                FormatNamespacePrefix(writer, selector.Namespace);
            }

            // IDENT
            writer.Write(selector.Name);

            // [ [ PREFIXMATCH | SUFFIXMATCH | SUBSTRINGMATCH | '=' | INCLUDES | DASHMATCH ] S* [ IDENT | STRING ] S*]?
            if (selector.Operation != CSSSimpleSelectorOperation.None)
            {
                // [ PREFIXMATCH | ... ]
                FormatSimpleSelectorOperation(writer, selector.Operation);

                // [ IDENT | STRING ]
                writer.Write(selector.Operand);
            }

            // ']'
            writer.Write(']');
        }

        private static void FormatSimpleSelectorOperation(TextWriter writer, CSSSimpleSelectorOperation operation)
        {
            switch (operation)
            {
                case CSSSimpleSelectorOperation.Equals:
                    writer.Write('=');
                    break;
                case CSSSimpleSelectorOperation.Includes:
                    writer.Write("~=");
                    break;
                case CSSSimpleSelectorOperation.PrefixMatch:
                    writer.Write("^=");
                    break;
                case CSSSimpleSelectorOperation.SuffixMatch:
                    writer.Write("$=");
                    break;
                case CSSSimpleSelectorOperation.SubstringMatch:
                    writer.Write("*=");
                    break;
                case CSSSimpleSelectorOperation.DashMatch:
                    writer.Write("|=");
                    break;
                default:
                    break;
            }
        }

        // pseudo
        //   /* '::' starts a pseudo-element, ':' a pseudo-class */
        //   /* Exceptions: :first-line, :first-letter, :before and :after. */
        //   /* Note that pseudo-elements are restricted to one per selector and */
        //   /* occur only in the last simple_selector_sequence. */
        //   : ':' ':'? [ IDENT | functional_pseudo ]
        //   ;

        private static void FormatPseudoClassSelector(TextWriter writer, CSSSimpleSelector selector)
        {
            writer.Write(':');
            writer.Write(selector.Name);
        }

        private static void FormatPseudoElementSelector(TextWriter writer, CSSSimpleSelector selector)
        {
            writer.Write("::");
            writer.Write(selector.Name);
        }

        // namespace_prefix
        //   : [ IDENT | '*' ]? '|'
        //   ;

        private static void FormatNamespacePrefix(TextWriter writer, string prefix)
        {
            writer.Write(prefix);
            writer.Write('|');
        }

        // Note: a CSSStyleDeclaration is a collection of one or more individual declarations
        //
        // declaration? [ ';' S* declaration? ]*

        public static string FormatStyleDeclaration(CSSStyleDeclaration declaration)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatStyleDeclaration(writer, declaration);
                return writer.ToString();
            }
        }

        public static void FormatStyleDeclaration(TextWriter writer, CSSStyleDeclaration declaration)
        {
            // S* declaration? [ ';' S* declaration? ]*

            var property = new CSSStyleProperty();
            var ignore = new HashSet<string>();

            // Format shorthand properties

            string margin = declaration.GetPropertyValue(CSSProperties.Margin);
            if (!String.IsNullOrEmpty(margin))
            {
                property.Name = CSSProperties.Margin;
                property.Value = margin;
                property.Priority = declaration.GetPropertyPriority(CSSProperties.Margin);
                FormatStyleProperty(writer, property);
                writer.Write(';');

                ignore.Add(CSSProperties.MarginTop);
                ignore.Add(CSSProperties.MarginRight);
                ignore.Add(CSSProperties.MarginBottom);
                ignore.Add(CSSProperties.MarginLeft);
            }

            string padding = declaration.GetPropertyValue(CSSProperties.Padding);
            if (!String.IsNullOrEmpty(padding))
            {
                property.Name = CSSProperties.Padding;
                property.Value = padding;
                property.Priority = declaration.GetPropertyPriority(CSSProperties.Padding);
                FormatStyleProperty(writer, property);
                writer.Write(';');

                ignore.Add(CSSProperties.PaddingTop);
                ignore.Add(CSSProperties.PaddingRight);
                ignore.Add(CSSProperties.PaddingBottom);
                ignore.Add(CSSProperties.PaddingLeft);
            }

            string borderColor = declaration.GetPropertyValue(CSSProperties.BorderColor);
            if (!String.IsNullOrEmpty(borderColor))
            {
                property.Name = CSSProperties.BorderColor;
                property.Value = borderColor;
                property.Priority = declaration.GetPropertyPriority(CSSProperties.BorderColor);
                FormatStyleProperty(writer, property);
                writer.Write(';');

                ignore.Add(CSSProperties.BorderTopColor);
                ignore.Add(CSSProperties.BorderRightColor);
                ignore.Add(CSSProperties.BorderBottomColor);
                ignore.Add(CSSProperties.BorderLeftColor);
            }

            string borderRadius = declaration.GetPropertyValue(CSSProperties.BorderRadius);
            if (!String.IsNullOrEmpty(borderRadius))
            {
                property.Name = CSSProperties.BorderRadius;
                property.Value = borderRadius;
                property.Priority = declaration.GetPropertyPriority(CSSProperties.BorderRadius);
                FormatStyleProperty(writer, property);
                writer.Write(';');

                ignore.Add(CSSProperties.BorderTopLeftRadius);
                ignore.Add(CSSProperties.BorderTopRightRadius);
                ignore.Add(CSSProperties.BorderBottomRightRadius);
                ignore.Add(CSSProperties.BorderBottomLeftRadius);
            }

            // Format longhand properties

            for (int i = 0; i < declaration.Length; i++)
            {
                property.Name = declaration[i];
                if (!ignore.Contains(property.Name))
                {
                    property.Value = declaration.GetPropertyValue(property.Name);
                    property.Priority = declaration.GetPropertyPriority(property.Name);
                    FormatStyleProperty(writer, property);
                    writer.Write(';');
                }
            }
        }

        // declaration : name ':' S* value;

        internal static void FormatStyleProperty(TextWriter writer, CSSStyleProperty property)
        {
            // name
            writer.Write(property.Name);

            // ':'
            writer.Write(':');

            // value
            writer.Write(property.Value);

            // [ S+ priority ]
            if (!String.IsNullOrEmpty(property.Priority))
            {
                writer.Write(' ');
                writer.Write(property.Priority);
            }
        }

        public static string FormatValueList(CSSValueListBase valueList)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatValueList(writer, valueList);
                return writer.ToString();
            }
        }

        public static void FormatValueList(TextWriter writer, CSSValueListBase valueList)
        {
            int length = valueList.Length;

            if (length == 0)
            {
                writer.Write("none");
                return;
            }

            for (int i = 0; i < length; i++)
            {
                writer.Write(valueList[i].CssText);

                if (i < length - 1)
                {
                    writer.Write(valueList.Separator);

                    if (!Char.IsWhiteSpace(valueList.Separator))
                    {
                        writer.Write(' ');
                    }
                }
            }
        }

        public static string FormatPrimitiveValue(CSSPrimitiveValue value)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatPrimitiveValue(writer, value);
                return writer.ToString();
            }
        }

        public static void FormatPrimitiveValue(TextWriter writer, CSSPrimitiveValue value)
        {
            switch (value.PrimitiveType)
            {
                case CSSPrimitiveType.CSS_UNKNOWN:
                    break;
                case CSSPrimitiveType.CSS_NUMBER:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    break;
                case CSSPrimitiveType.CSS_PERCENTAGE:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("%");
                    break;
                case CSSPrimitiveType.CSS_EMS:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("em");
                    break;
                case CSSPrimitiveType.CSS_EXS:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("ex");
                    break;
                case CSSPrimitiveType.CSS_PX:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("px");
                    break;
                case CSSPrimitiveType.CSS_CM:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("cm");
                    break;
                case CSSPrimitiveType.CSS_MM:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("mm");
                    break;
                case CSSPrimitiveType.CSS_IN:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("in");
                    break;
                case CSSPrimitiveType.CSS_PT:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("pt");
                    break;
                case CSSPrimitiveType.CSS_PC:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("pc");
                    break;
                case CSSPrimitiveType.CSS_DEG:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("deg");
                    break;
                case CSSPrimitiveType.CSS_RAD:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("rad");
                    break;
                case CSSPrimitiveType.CSS_GRAD:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("grad");
                    break;
                case CSSPrimitiveType.CSS_MS:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("ms");
                    break;
                case CSSPrimitiveType.CSS_S:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("s");
                    break;
                case CSSPrimitiveType.CSS_HZ:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("hz");
                    break;
                case CSSPrimitiveType.CSS_KHZ:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    writer.Write("khz");
                    break;
                case CSSPrimitiveType.CSS_DIMENSION:
                    writer.Write(value.GetFloatValue(value.PrimitiveType));
                    break;
                case CSSPrimitiveType.CSS_STRING:
                    writer.Write('\'');
                    writer.Write(value.GetStringValue());
                    writer.Write('\'');
                    break;
                case CSSPrimitiveType.CSS_URI:
                    writer.Write("url(");
                    writer.Write(value.GetStringValue());
                    writer.Write(")");
                    break;
                case CSSPrimitiveType.CSS_IDENT:
                    writer.Write(value.GetStringValue());
                    break;
                case CSSPrimitiveType.CSS_ATTR:
                    writer.Write("attr(");
                    writer.Write(value.GetStringValue());
                    writer.Write(")");
                    break;
                case CSSPrimitiveType.CSS_COUNTER:
                    // TODO
                    break;
                case CSSPrimitiveType.CSS_RECT:
                    FormatRect(writer, value.GetRectValue());
                    break;
                case CSSPrimitiveType.CSS_RGBCOLOR:
                    FormatColor(writer, value.GetRGBColorValue(), true);
                    break;
                default:
                    break;
            }
        }

        public static string FormatRect(CSSRect rect)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatRect(writer, rect);
                return writer.ToString();
            }
        }

        public static void FormatRect(TextWriter writer, CSSRect rect)
        {
            writer.Write("rect(");
            FormatPrimitiveValue(writer, rect.Top);
            writer.Write(',');
            FormatPrimitiveValue(writer, rect.Right);
            writer.Write(',');
            FormatPrimitiveValue(writer, rect.Bottom);
            writer.Write(',');
            FormatPrimitiveValue(writer, rect.Left);
            writer.Write(")");
        }

        public static string FormatColor(RGBColor color, bool useKeywords)
        {
            using (var buffer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatColor(buffer, color, useKeywords);
                return buffer.ToString();
            }
        }

        public static void FormatColor(TextWriter writer, RGBColor color, bool useKeywords)
        {
            if (useKeywords)
            {
                string rgb = FormatColor(color, false);
                string keyword = ToKeyword(rgb);

                if (!String.IsNullOrEmpty(keyword))
                {
                    writer.Write(keyword);
                }
                else
                {
                    writer.Write(rgb);
                }
            }
            else
            {
                writer.Write("rgb(");
                FormatPrimitiveValue(writer, color.Red);
                writer.Write(',');
                FormatPrimitiveValue(writer, color.Green);
                writer.Write(',');
                FormatPrimitiveValue(writer, color.Blue);
                writer.Write(")");
            }
        }

        private static string ToKeyword(string str)
        {
            switch (str)
            {
                case "rgb(240,248,255)":
                    return "aliceblue";
                case "rgb(250,235,215)":
                    return "antiquewhite";
                case "rgb(0,255,255)":
                    return "aqua";
                case "rgb(127,255,212)":
                    return "aquamarine";
                case "rgb(240,255,255)":
                    return "azure";
                case "rgb(245,245,220)":
                    return "beige";
                case "rgb(255,228,196)":
                    return "bisque";
                case "rgb(0,0,0)":
                    return "black";
                case "rgb(255,235,205)":
                    return "blanchedalmond";
                case "rgb(0,0,255)":
                    return "blue";
                case "rgb(138,43,226)":
                    return "blueviolet";
                case "rgb(165,42,42)":
                    return "brown";
                case "rgb(222,184,135)":
                    return "burlywood";
                case "rgb(95,158,160)":
                    return "cadetblue";
                case "rgb(127,255,0)":
                    return "chartreuse";
                case "rgb(210,105,30)":
                    return "chocolate";
                case "rgb(255,127,80)":
                    return "coral";
                case "rgb(100,149,237)":
                    return "cornflowerblue";
                case "rgb(255,248,220)":
                    return "cornsilk";
                case "rgb(220,20,60)":
                    return "crimson";
                case "rgb(0,0,139)":
                    return "darkblue";
                case "rgb(0,139,139)":
                    return "darkcyan";
                case "rgb(184,134,11)":
                    return "darkgoldenrod";
                case "rgb(169,169,169)":
                    return "darkgray";
                case "rgb(0,100,0)":
                    return "darkgreen";
                case "rgb(189,183,107)":
                    return "darkkhaki";
                case "rgb(139,0,139)":
                    return "darkmagenta";
                case "rgb(85,107,47)":
                    return "darkolivegreen";
                case "rgb(255,140,0)":
                    return "darkorange";
                case "rgb(153,50,204)":
                    return "darkorchid";
                case "rgb(139,0,0)":
                    return "darkred";
                case "rgb(233,150,122)":
                    return "darksalmon";
                case "rgb(143,188,143)":
                    return "darkseagreen";
                case "rgb(72,61,139)":
                    return "darkslateblue";
                case "rgb(47,79,79)":
                    return "darkslategray";
                case "rgb(0,206,209)":
                    return "darkturquoise";
                case "rgb(148,0,211)":
                    return "darkviolet";
                case "rgb(255,20,147)":
                    return "deeppink";
                case "rgb(0,191,255)":
                    return "deepskyblue";
                case "rgb(105,105,105)":
                    return "dimgray";
                case "rgb(30,144,255)":
                    return "dodgerblue";
                case "rgb(178,34,34)":
                    return "firebrick";
                case "rgb(255,250,240)":
                    return "floralwhite";
                case "rgb(34,139,34)":
                    return "forestgreen";
                case "rgb(220,220,220)":
                    return "gainsboro";
                case "rgb(248,248,255)":
                    return "ghostwhite";
                case "rgb(255,215,0)":
                    return "gold";
                case "rgb(218,165,32)":
                    return "goldenrod";
                case "rgb(128,128,128)":
                    return "gray";
                case "rgb(0,128,0)":
                    return "green";
                case "rgb(173,255,47)":
                    return "greenyellow";
                case "rgb(240,255,240)":
                    return "honeydew";
                case "rgb(255,105,180)":
                    return "hotpink";
                case "rgb(205,92,92)":
                    return "indianred";
                case "rgb(75,0,130)":
                    return "indigo";
                case "rgb(255,255,240)":
                    return "ivory";
                case "rgb(240,230,140)":
                    return "khaki";
                case "rgb(230,230,250)":
                    return "lavender";
                case "rgb(255,240,245)":
                    return "lavenderblush";
                case "rgb(124,252,0)":
                    return "lawngreen";
                case "rgb(255,250,205)":
                    return "lemonchiffon";
                case "rgb(173,216,230)":
                    return "lightblue";
                case "rgb(240,128,128)":
                    return "lightcoral";
                case "rgb(224,255,255)":
                    return "lightcyan";
                case "rgb(250,250,210)":
                    return "lightgoldenrodyellow";
                case "rgb(211,211,211)":
                    return "lightgray";
                case "rgb(144,238,144)":
                    return "lightgreen";
                case "rgb(255,182,193)":
                    return "lightpink";
                case "rgb(255,160,122)":
                    return "lightsalmon";
                case "rgb(32,178,170)":
                    return "lightseagreen";
                case "rgb(135,206,250)":
                    return "lightskyblue";
                case "rgb(119,136,153)":
                    return "lightslategray";
                case "rgb(176,196,222)":
                    return "lightsteelblue";
                case "rgb(255,255,224)":
                    return "lightyellow";
                case "rgb(0,255,0)":
                    return "lime";
                case "rgb(50,205,50)":
                    return "limegreen";
                case "rgb(250,240,230)":
                    return "linen";
                case "rgb(255,0,255)":
                    return "magenta";
                case "rgb(128,0,0)":
                    return "maroon";
                case "rgb(102,205,170)":
                    return "mediumaquamarine";
                case "rgb(0,0,205)":
                    return "mediumblue";
                case "rgb(186,85,211)":
                    return "mediumorchid";
                case "rgb(147,112,219)":
                    return "mediumpurple";
                case "rgb(60,179,113)":
                    return "mediumseagreen";
                case "rgb(123,104,238)":
                    return "mediumslateblue";
                case "rgb(0,250,154)":
                    return "mediumspringgreen";
                case "rgb(72,209,204)":
                    return "mediumturquoise";
                case "rgb(199,21,133)":
                    return "mediumvioletred";
                case "rgb(25,25,112)":
                    return "midnightblue";
                case "rgb(245,255,250)":
                    return "mintcream";
                case "rgb(255,228,225)":
                    return "mistyrose";
                case "rgb(255,228,181)":
                    return "moccasin";
                case "rgb(255,222,173)":
                    return "navajowhite";
                case "rgb(0,0,128)":
                    return "navy";
                case "rgb(253,245,230)":
                    return "oldlace";
                case "rgb(128,128,0)":
                    return "olive";
                case "rgb(107,142,35)":
                    return "olivedrab";
                case "rgb(255,165,0)":
                    return "orange";
                case "rgb(255,69,0)":
                    return "orangered";
                case "rgb(218,112,214)":
                    return "orchid";
                case "rgb(238,232,170)":
                    return "palegoldenrod";
                case "rgb(152,251,152)":
                    return "palegreen";
                case "rgb(175,238,238)":
                    return "paleturquoise";
                case "rgb(219,112,147)":
                    return "palevioletred";
                case "rgb(255,239,213)":
                    return "papayawhip";
                case "rgb(255,218,185)":
                    return "peachpuff";
                case "rgb(205,133,63)":
                    return "peru";
                case "rgb(255,192,203)":
                    return "pink";
                case "rgb(221,160,221)":
                    return "plum";
                case "rgb(176,224,230)":
                    return "powderblue";
                case "rgb(128,0,128)":
                    return "purple";
                case "rgb(255,0,0)":
                    return "red";
                case "rgb(188,143,143)":
                    return "rosybrown";
                case "rgb(65,105,225)":
                    return "royalblue";
                case "rgb(139,69,19)":
                    return "saddlebrown";
                case "rgb(250,128,114)":
                    return "salmon";
                case "rgb(244,164,96)":
                    return "sandybrown";
                case "rgb(46,139,87)":
                    return "seagreen";
                case "rgb(255,245,238)":
                    return "seashell";
                case "rgb(160,82,45)":
                    return "sienna";
                case "rgb(192,192,192)":
                    return "silver";
                case "rgb(135,206,235)":
                    return "skyblue";
                case "rgb(106,90,205)":
                    return "slateblue";
                case "rgb(112,128,144)":
                    return "slategray";
                case "rgb(255,250,250)":
                    return "snow";
                case "rgb(0,255,127)":
                    return "springgreen";
                case "rgb(70,130,180)":
                    return "steelblue";
                case "rgb(210,180,140)":
                    return "tan";
                case "rgb(0,128,128)":
                    return "teal";
                case "rgb(216,191,216)":
                    return "thistle";
                case "rgb(255,99,71)":
                    return "tomato";
                case "rgb(64,224,208)":
                    return "turquoise";
                case "rgb(238,130,238)":
                    return "violet";
                case "rgb(245,222,179)":
                    return "wheat";
                case "rgb(255,255,255)":
                    return "white";
                case "rgb(245,245,245)":
                    return "whitesmoke";
                case "rgb(255,255,0)":
                    return "yellow";
                case "rgb(154,205,50)":
                    return "yellowgreen";
                default:
                    return null;
            }
        }

    }
}
