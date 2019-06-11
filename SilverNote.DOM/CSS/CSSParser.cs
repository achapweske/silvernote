/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DOM.CSS.Internal;

namespace DOM.CSS
{
    /// <summary>
    /// http://www.w3.org/TR/1998/REC-CSS2-19980512/syndata.html
    /// http://www.w3.org/TR/css3-selectors/#w3cselgrammar
    /// </summary>
    public static class CSSParser
    {
        #region Tokenizer

        [ThreadStatic]
        private static StringBuilder _Buffer;

        private static StringBuilder Buffer
        {
            get
            {
                if (_Buffer == null)
                {
                    _Buffer = new StringBuilder();
                }
                _Buffer.Clear();
                return _Buffer;
            }
        }

        // ATKEYWORD	@{ident}

        public static bool TryReadAtKeyword(TextReader reader, StringBuilder result)
        {
            if (!TryReadChar(reader, result, '@'))
            {
                return false;
            }

            if (!TryReadIdent(reader, result))
            {
                return false;
            }

            return true;
        }

        // HASH	#{name}

        public static string ReadHash(TextReader reader)
        {
            var result = Buffer;

            if (!TryReadHash(reader, result))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            return result.ToString();
        }

        public static bool TryReadHash(TextReader reader, StringBuilder result)
        {
            if (!TryReadChar(reader, result, '#'))
            {
                return false;
            }

            if (!TryReadName(reader, result))
            {
                return false;
            }

            return true;
        }

        // PERCENTAGE	{num}%

        public static bool TryReadPercentage(TextReader reader, StringBuilder result)
        {
            if (!TryReadNumber(reader, result))
            {
                return false;
            }

            if (!TryReadChar(reader, result, '%'))
            {
                return false;
            }

            return true;
        }

        // DIMENSION	{num}{ident}

        public static bool TryReadDimension(TextReader reader, StringBuilder result)
        {
            if (!TryReadNumber(reader, result))
            {
                return false;
            }

            if (!TryReadIdent(reader, result))
            {
                return false;
            }

            return true;
        }

        // URI	url\({w}{string}{w}\)
        //     |url\({w}([!#$%&*-~]|{nonascii}|{escape})*{w}\)

        public static bool TryReadUri(TextReader reader, StringBuilder result)
        {
            // url
            if (!TryReadChar(reader, result, 'u') ||
                !TryReadChar(reader, result, 'r') ||
                !TryReadChar(reader, result, 'l'))
            {
                return false;
            }

            // \(
            if (!TryReadChar(reader, result, '('))
            {
                return false;
            }

            // {w}
            SkipWhitespace(reader);

            // {string}
            if (!TryReadString(reader, result))
            {
                // ([!#$%&*-~]|{nonascii}|{escape})*

                while (true)
                {
                    char c = (char)reader.Peek();

                    if ("!#$%&*-~".Contains(c) || IsNonAscii(c))
                    {
                        reader.Read();
                        result.Append(c);
                        continue;
                    }

                    if (TryReadEscape(reader, result))
                    {
                        continue;
                    }

                    return false;
                }
            }

            // {w}
            SkipWhitespace(reader);

            // \)
            if (!TryReadChar(reader, result, ')'))
            {
                return false;
            }

            return true;
        }

        // CDO	<!--

        public static bool SkipCDO(TextReader reader)
        {
            if (SkipChar(reader, '<') &&
                SkipChar(reader, '!') &&
                SkipChar(reader, '-') &&
                SkipChar(reader, '-'))
            {
                return true;
            }

            return false;
        }

        // CDC	-->

        public static bool SkipCDC(TextReader reader)
        {
            if (SkipChar(reader, '-') &&
                SkipChar(reader, '-') &&
                SkipChar(reader, '>'))
            {
                return true;
            }

            return false;
        }

        // ident	-?{nmstart}{nmchar}*

        public static string ReadIdent(TextReader reader)
        {
            var result = Buffer;

            if (!TryReadIdent(reader, result))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            return result.ToString();
        }

        public static bool TryReadIdent(TextReader reader, StringBuilder result)
        {
            // -?
            char c = (char)reader.Peek();
            if (c == '-')
            {
                result.Append(c);
                reader.Read();
            }

            // {nmstart}
            if (!TryReadNMStart(reader, result))
            {
                return false;
            }

            // {nmchar}*
            while (TryReadNMChar(reader, result))
            {
                ;
            }

            return true;
        }

        // name	{nmchar}+

        public static string ReadName(TextReader reader)
        {
            var result = Buffer;

            if (!TryReadName(reader, result))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            return result.ToString();
        }

        public static bool TryReadName(TextReader reader, StringBuilder result)
        {
            if (!TryReadNMChar(reader, result))
            {
                return false;
            }

            while (TryReadNMChar(reader, result))
            {
                ;
            }

            return true;
        }

        // nmstart	[_a-z]|{nonascii}|{escape}

        public static bool TryReadNMStart(TextReader reader, StringBuilder result)
        {
            char c;
            if (!TryReadChar(reader, out c))
            {
                return false;   // end-of-stream
            }

            if (c == '_' || Char.IsLetter(c) || IsNonAscii(c))
            {
                reader.Read();
                result.Append(c);
                return true;
            }
            else if (TryReadEscape(reader, result))
            {
                return true;
            }

            return false;
        }

        // nonascii	[^\0-\177]

        public static bool IsNonAscii(char c)
        {
            // (177 octal = 7F hex)

            return !(c >= 0 && c <= 0x7F);
        }

        // escape	{unicode}|\\[ -~\200-\4177777]

        public static bool TryReadEscape(TextReader reader, StringBuilder result, bool skipEscapeChar = false)
        {
            // (200 octal = 80 hex, 4177777 octal = 10FFFF hex)

            char c;
            if (!TryReadChar(reader, out c))
            {
                return false;   // end-of-stream
            }

            if (!skipEscapeChar)
            {
                if (c != '\\')
                {
                    return false;
                }

                reader.Read();
                result.Append(c);

                if (!TryReadChar(reader, out c))
                {
                    return false;   // end-of-stream
                }
            }

            if (Char.IsLetterOrDigit(c))
            {
                // [0-9a-f]{1,6}[ \n\r\t\f]?

                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();

                for (int i = 1; i <= 6; i++)
                {
                    if (!Char.IsLetterOrDigit(c))
                    {
                        break;
                    }
                    reader.Read();
                    result.Append(c);
                    c = (char)reader.Peek();
                }

                if (c == ' ' || c == '\n' || c == '\r' || c == '\t' || c == '\f')
                {
                    reader.Read();
                    result.Append(c);
                }

                return true;
            }
            else if (c >= ' ' && c <= '~' || c >= 0x80)
            {
                // [ -~\200-\4177777]

                reader.Read();
                result.Append(c);
                return true;
            }
            else
            {
                return false;
            }
        }

        // nmchar	[_a-z0-9-]|{nonascii}|{escape}

        public static bool TryReadNMChar(TextReader reader, StringBuilder result)
        {
            char c;
            if (!TryReadChar(reader, out c))
            {
                return false;   // end of stream
            }

            if (c == '_' || Char.IsLetterOrDigit(c) || c == '-' || IsNonAscii(c))
            {
                reader.Read();
                result.Append(c);
                return true;
            }
            else if (TryReadEscape(reader, result))
            {
                return true;
            }

            return false;
        }

        // num	[0-9]+|[0-9]*\.[0-9]+

        public static bool TryReadNumber(TextReader reader, StringBuilder result)
        {
            char c = (char)reader.Peek();

            // [0-9]+|[0-9]*\.

            if (Char.IsDigit(c))
            {
                do
                {
                    reader.Read();
                    result.Append(c);
                    c = (char)reader.Peek();

                } while (Char.IsDigit(c));

                if (c != '.')
                {
                    return true;
                }
            }
            else if (c != '.')
            {
                return false;
            }

            // \.
            reader.Read();
            result.Append(c);

            // [0-9]+

            c = (char)reader.Peek();
            if (!Char.IsDigit(c))
            {
                return false;
            }

            do
            {
                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();

            } while (Char.IsDigit(c));

            return true;
        }

        // string	{string1}|{string2}
        // string1	\"([\t !#$%&(-~]|\\{nl}|\'|{nonascii}|{escape})*\"
        // string2	\'([\t !#$%&(-~]|\\{nl}|\"|{nonascii}|{escape})*\'

        public static string ReadString(TextReader reader)
        {
            var result = Buffer;

            if (!TryReadString(reader, result))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            return result.ToString();
        }

        public static bool TryReadString(TextReader reader, StringBuilder result)
        {
            char delimiter = (char)reader.Peek();

            if (delimiter != '\"' && delimiter != '\'')
            {
                return false;
            }

            reader.Read();
            result.Append(delimiter);

            while (true)
            {
                char c;
                if (!TryReadChar(reader, out c))
                {
                    return false;
                }

                if (c == delimiter)
                {
                    reader.Read();
                    result.Append(c);
                    return true;
                }

                // [\t !#$%&(-~]

                if (c >= '(' && c <= '~' || "\t !#$%&".Contains(c))
                {
                    reader.Read();
                    result.Append(c);
                    continue;
                }

                // \'

                if (c == '\'' || c == '\"')
                {
                    reader.Read();
                    result.Append(c);
                    continue;
                }

                // {nonascii}

                if (IsNonAscii(c))
                {
                    reader.Read();
                    result.Append(c);
                    continue;
                }

                // \\{nl}|{escape}

                if (c == '\\')
                {
                    reader.Read();
                    result.Append(c);

                    if (!TryReadNewline(reader, result) && !TryReadEscape(reader, result, true))
                    {
                        return false;
                    }
                    continue;
                }

                return false;
            }
        }

        // nl	\n|\r\n|\r|\f

        public static bool TryReadNewline(TextReader reader, StringBuilder buffer)
        {
            char c = (char)reader.Peek();

            if (c != '\r' && c != '\n' && c != '\f')
            {
                return false;
            }

            reader.Read();
            buffer.Append(c);

            if (c == '\r')
            {
                c = (char)reader.Peek();
                if (c == '\n')
                {
                    reader.Read();
                    buffer.Append(c);
                }
            }

            return true;
        }

        // w	[ \t\r\n\f]*

        public static bool TryReadWhitespace(TextReader reader, StringBuilder buffer)
        {
            char c = (char)reader.Peek();
            if (!IsWhitespace(c))
            {
                return false;
            }

            do
            {
                reader.Read();
                buffer.Append(c);
                c = (char)reader.Peek();

            } while (IsWhitespace(c));

            return true;
        }

        public static bool SkipWhitespace(TextReader reader)
        {
            char c = (char)reader.Peek();
            if (!IsWhitespace(c))
            {
                return false;
            }

            do
            {
                reader.Read();
                c = (char)reader.Peek();

            } while (IsWhitespace(c));

            return true;
        }

        // [ \t\r\n\f]

        public static bool IsWhitespace(char c)
        {
            return (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f');
        }

        private static char ReadChar(TextReader reader, char c)
        {
            if ((char)reader.Peek() != c)
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            return (char)reader.Read();
        }

        private static bool TryReadChar(TextReader reader, out char c)
        {
            int i = reader.Peek();
            if (i != -1)
            {
                c = (char)i;
                return true;
            }
            else
            {
                c = default(char);
                return false;
            }
        }

        private static bool TryReadChar(TextReader reader, StringBuilder result, char c)
        {
            if ((char)reader.Peek() == c)
            {
                result.Append((char)reader.Read());
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool SkipChar(TextReader reader, char c)
        {
            if ((char)reader.Peek() == c)
            {
                reader.Read();
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Grammer

        // stylesheet  : [ CDO | CDC | S | statement ]*;

        public static CSSStyleSheetImpl ParseStylesheet(string text, CSSStyleSheetImpl result = null)
        {
            text = StripComments(text);

            using (var reader = new StringReader(text))
            {
                return ParseStylesheet(reader, result);
            }
        }

        public static CSSStyleSheetImpl ParseStylesheet(TextReader reader, CSSStyleSheetImpl result = null)
        {
            if (result == null)
            {
                result = new CSSStyleSheetImpl();
            }

            while (reader.Peek() != -1)
            {
                if (SkipCDO(reader) || SkipCDC(reader) || SkipWhitespace(reader))
                {
                    continue;
                }

                CSSRuleBase rule = ParseStatement(reader);

                result.AppendRule(rule);
            }

            return result;
        }

        // statement   : ruleset | at-rule;

        public static CSSRuleBase ParseStatement(string text)
        {
            using (var reader = new StringReader(text))
            {
                return ParseStatement(reader);
            }
        }

        public static CSSRuleBase ParseStatement(TextReader reader)
        {
            char c = (char)reader.Peek();

            if (c == '@')
            {
                return ParseAtRule(reader);
            }
            else
            {
                return ParseRuleSet(reader);
            }
        }

        // at-rule     : ATKEYWORD S* any* [ block | ';' S* ];

        public static CSSRuleBase ParseAtRule(TextReader reader)
        {
            StringBuilder buffer = new StringBuilder();

            if (!TryReadAtKeyword(reader, buffer))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            string keyword = buffer.ToString();

            bool done = false;
            bool inQuotes = false;
            char quoteChar = '\0';
            int blockLevel = 0;

            while (!done)
            {
                int i = reader.Peek();
                if (i == -1)
                {
                    // throw new DOMException(DOMException.SYNTAX_ERR);
                    break;  // css3-syntax 5.4.7
                }

                char c = (char)i;

                if (!inQuotes && (c == '\'' || c == '\"'))
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (inQuotes)
                {
                    inQuotes = (c == quoteChar);
                }
                else if (c == '{')
                {
                    blockLevel++;
                }
                else if (blockLevel > 0 && c == '}')
                {
                    blockLevel--;
                    done = blockLevel == 0;
                }
                else if (blockLevel == 0 && c == ';')
                {
                    done = true;
                }

                buffer.Append(c);
                reader.Read();
            }

            TryReadWhitespace(reader, buffer);

            CSSRuleBase result = null;

            switch (keyword)
            {
                case "@import":
                    result = new CSSImportRuleBase();
                    break;
                case "@page":
                    result = new CSSPageRuleBase();
                    break;
                case "@media":
                    result = new CSSMediaRuleBase();
                    break;
                case "@font-face":
                    result = new CSSFontFaceRuleBase();
                    break;
                case "@charset":
                    result = new CSSCharsetRuleBase();
                    break;
                default:
                    result = new CSSUnknownRuleBase();
                    break;
            }

            try
            {
                result.CssText = buffer.ToString();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                result = new CSSUnknownRuleBase();
            }

            return result;
        }

        // ruleset     : selector? '{' S* declaration? [ ';' S* declaration? ]* '}' S*;

        public static CSSStyleRuleBase ParseRuleSet(string text, CSSStyleRuleBase result = null)
        {
            using (var reader = new StringReader(text))
            {
                return ParseRuleSet(reader, result);
            }
        }

        public static CSSStyleRuleBase ParseRuleSet(TextReader reader, CSSStyleRuleBase result = null)
        {
            StringBuilder buffer = new StringBuilder();

            if (!TryReadSelector(reader, buffer))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            string selector = buffer.ToString();

            buffer.Clear();

            if (!TryReadBlock(reader, buffer))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            string style = buffer.ToString();

            if (result == null)
            {
                result = new CSSStyleRuleBase();
            }

            try
            {
                ParseSelectorGroup(selector.Trim(), result.Selector);
                style = style.TrimStart('{', ' ', '\t', '\r', '\n', '\f');
                style = style.TrimEnd('}', ' ', '\t', '\r', '\n', '\f');
                result.Style.CssText = style;
            }
            catch (Exception e)
            {
                Debug.Write(e.Message + "\n\n" + e.StackTrace);
            }

            return result;
        }

        // selector    : any+;

        public static bool TryReadSelector(TextReader reader, StringBuilder result)
        {
            // any         : [ IDENT | NUMBER | PERCENTAGE | DIMENSION | STRING
            //                 | DELIM | URI | HASH | UNICODE-RANGE | INCLUDES
            //                 | FUNCTION | DASHMATCH | '(' any* ')' | '[' any* ']' ] S*;

            bool inQuotes = false;
            char quoteChar = '\0';

            while (true)
            {
                int i = reader.Peek();
                if (i == -1)
                {
                    break;
                }

                char c = (char)i;

                if (!inQuotes && (c == '\'' || c == '\"'))
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (inQuotes)
                {
                    inQuotes = (c == quoteChar);
                }
                else if (c == '{')
                {
                    break;
                }

                result.Append(c);
                reader.Read();
            }

            return true;
        }

        // block : '{' S* [ any | block | ATKEYWORD S* | ';' ]* '}' S*;

        public static bool TryReadBlock(TextReader reader, StringBuilder result)
        {
            if (!TryReadChar(reader, result, '{'))
            {
                return false;
            }

            bool done = false;
            bool inQuotes = false;
            char quoteChar = '\0';
            int blockLevel = 1;

            while (!done)
            {
                int i = reader.Peek();
                if (i == -1)
                {
                    return false;
                }

                char c = (char)i;

                if (!inQuotes && (c == '\'' || c == '\"'))
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (inQuotes)
                {
                    inQuotes = (c != quoteChar);
                }
                else if (c == '{')
                {
                    blockLevel++;
                }
                else if (blockLevel > 0 && c == '}')
                {
                    blockLevel--;
                    done = blockLevel == 0;
                }

                result.Append(c);
                reader.Read();
            }

            TryReadWhitespace(reader, result);

            return true;
        }

        public static string StripComments(string text)
        {
            int startIndex = text.IndexOf("/*");
            while (startIndex != -1)
            {
                int endIndex = text.IndexOf("*/", startIndex + 2);
                if (endIndex == -1)
                {
                    text = text.Remove(startIndex);
                    break;
                }

                int length = (endIndex - startIndex) + 2;
                text = text.Remove(startIndex, length);
                endIndex = startIndex;

                startIndex = text.IndexOf("/*", endIndex);
            }

            return text;
        }

        // selectors_group
        //   : selector [ COMMA S* selector ]*
        //   ;

        public static CSSSelectorGroup ParseSelectorGroup(string text, CSSSelectorGroup result = null)
        {
            using (var reader = new StringReader(text))
            {
                return ParseSelectorGroup(reader, result);
            }
        }

        public static CSSSelectorGroup ParseSelectorGroup(TextReader reader, CSSSelectorGroup result = null)
        {
            if (result == null)
            {
                result = new CSSSelectorGroup();
            }

            // selector
            result.Add(CSSParser.ParseSelector(reader));

            // [ COMMA S* selector ]*
            while (SkipChar(reader, ','))
            {
                // S*
                CSSParser.SkipWhitespace(reader);

                // selector
                result.Add(CSSParser.ParseSelector(reader));
            }

            return result;
        }

        // selector
        //   /* combinators can be surrounded by whitespace */
        //   : simple_selector_sequence [ combinator simple_selector_sequence ]*
        //   ;

        public static CSSSelector ParseSelector(string text, CSSSelector result = null)
        {
            using (var reader = new StringReader(text))
            {
                return ParseSelector(reader, result);
            }
        }

        public static CSSSelector ParseSelector(TextReader reader, CSSSelector result = null)
        {
            if (result == null)
            {
                result = new CSSSelector();
            }

            // Note: leading whitespace is signficant for a combinator

            var item = CSSParser.ParseSelectorItem(reader);

            result.Add(item);

            while (item.Combinator != CSSCombinator.None)
            {
                CSSParser.SkipWhitespace(reader);

                item = CSSParser.ParseSelectorItem(reader);

                result.Add(item);
            }

            result.Optimize();

            return result;
        }

        public static CSSSelectorItem ParseSelectorItem(TextReader reader, CSSSelectorItem result = null)
        {
            // A CSSSelectorItem is a simple sequence followed by a combinator
            //
            // Note: leading whitespace is signficant for a combinator

            if (result == null)
            {
                result = new CSSSelectorItem();
            }

            result.Sequence = CSSParser.ParseSimpleSelectorSequence(reader);
            result.Combinator = CSSParser.ParseCombinator(reader);

            return result;
        }

        // combinator
        //   /* combinators can be surrounded by whitespace */
        //   : PLUS S* | GREATER S* | TILDE S* | S+
        //   ;

        public static CSSCombinator ParseCombinator(TextReader reader)
        {
            CSSCombinator result = CSSCombinator.None;

            char c = (char)reader.Peek();

            while (Char.IsWhiteSpace(c))
            {
                result = CSSCombinator.Descendant;
                reader.Read();
                c = (char)reader.Peek();
            }

            switch (c)
            {
                case '+':
                    reader.Read();
                    return CSSCombinator.Adjacent;
                case '>':
                    reader.Read();
                    return CSSCombinator.Child;
                case '~':
                    reader.Read();
                    return CSSCombinator.Sibling;
                case ',':   
                    // end-of-selector
                    return CSSCombinator.None;
                default:
                    return result;
            }
        }

        // simple_selector_sequence
        //   : [ type_selector | universal ]
        //     [ HASH | class | attrib | pseudo | negation ]*
        //   | [ HASH | class | attrib | pseudo | negation ]+
        //   ;

        public static CSSSimpleSelectorSequence ParseSimpleSelectorSequence(TextReader reader, CSSSimpleSelectorSequence result = null)
        {
            if (result == null)
            {
                result = new CSSSimpleSelectorSequence();
            }

            var selector = ParseSimpleSelector(reader);

            result.Add(selector);

            while (IsSimpleSelectorStartChar((char)reader.Peek()))
            {
                selector = ParseSimpleSelector(reader);

                result.Add(selector);
            }

            return result;
        }

        private static bool IsSimpleSelectorStartChar(char c)
        {
            return c == '#' || c == '.' || c == '[' || c == ':' || Char.IsLetterOrDigit(c);
        }

        public static CSSSimpleSelector ParseSimpleSelector(TextReader reader, CSSSimpleSelector result = null)
        {
            if (result == null)
            {
                result = new CSSSimpleSelector();
            }

            char c = (char)reader.Peek();

            switch (c)
            {
                case '*':
                    return ParseUniversalSelector(reader, result);
                case '#':
                    return ParseIDSelector(reader, result);
                case '.':
                    return ParseClassSelector(reader, result);
                case '[':
                    return ParseAttributeSelector(reader, result);
                case ':':
                    return ParsePseudoSelector(reader, result);
                default:
                    if (Char.IsLetterOrDigit(c))
                    {
                        return ParseTypeSelector(reader, result);
                    }
                    else
                    {
                        throw new DOMException(DOMException.SYNTAX_ERR);
                    }
            }
        }

        // universal
        //  : [ namespace_prefix ]? '*'
        //  ;

        private static CSSSimpleSelector ParseUniversalSelector(TextReader reader, CSSSimpleSelector result)
        {
            // TODO: namespace_prefix

            ReadChar(reader, '*');

            result.Type = CSSSimpleSelectorType.Universal;

            return result;
        }

        // id_selector
        //   : HASH
        //   ;

        private static CSSSimpleSelector ParseIDSelector(TextReader reader, CSSSimpleSelector result)
        {
            string hash = ReadHash(reader);

            result.Type = CSSSimpleSelectorType.ID;
            result.Name = hash.Remove(0, 1);

            return result;
        }

        // class
        //   : '.' IDENT
        //   ;

        private static CSSSimpleSelector ParseClassSelector(TextReader reader, CSSSimpleSelector result)
        {
            ReadChar(reader, '.');

            result.Type = CSSSimpleSelectorType.Class;
            result.Name = ReadIdent(reader);

            return result;
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

        private static CSSSimpleSelector ParseAttributeSelector(TextReader reader, CSSSimpleSelector result)
        {
            // '['
            ReadChar(reader, '[');

            result.Type = CSSSimpleSelectorType.Attribute;

            // S*
            SkipWhitespace(reader);

            // TODO: namespace_prefix

            // IDENT
            result.Name = ReadIdent(reader);

            // S*
            SkipWhitespace(reader);

            // [ [ PREFIXMATCH | SUFFIXMATCH | SUBSTRINGMATCH | '=' | INCLUDES | DASHMATCH ] S* [ IDENT | STRING ] S*]?
            if ((char)reader.Peek() != ']')
            {
                // [ PREFIXMATCH | ... ]
                result.Operation = ParseSimpleSelectorOperation(reader);

                // S*
                SkipWhitespace(reader);

                // [ IDENT | STRING ]
                if ((char)reader.Peek() == '\'' || (char)reader.Peek() == '\"')
                {
                    result.Operand = ReadString(reader);
                }
                else
                {
                    result.Operand = ReadIdent(reader);
                }

                // S*
                SkipWhitespace(reader);
            }

            ReadChar(reader, ']');

            return result;
        }

        private static CSSSimpleSelectorOperation ParseSimpleSelectorOperation(TextReader reader)
        {
            char c = (char)reader.Peek();

            switch (c)
            {
                case '=':
                    reader.Read();
                    return CSSSimpleSelectorOperation.Equals;
                case '~':
                    reader.Read();
                    ReadChar(reader, '=');
                    return CSSSimpleSelectorOperation.Includes;
                case '^':
                    reader.Read();
                    ReadChar(reader, '=');
                    return CSSSimpleSelectorOperation.PrefixMatch;
                case '$':
                    reader.Read();
                    ReadChar(reader, '=');
                    return CSSSimpleSelectorOperation.SuffixMatch;
                case '*':
                    reader.Read();
                    ReadChar(reader, '=');
                    return CSSSimpleSelectorOperation.SubstringMatch;
                case '|':
                    reader.Read();
                    ReadChar(reader, '=');
                    return CSSSimpleSelectorOperation.DashMatch;
                default:
                    return CSSSimpleSelectorOperation.None;
            }
        }

        // pseudo
        //   /* '::' starts a pseudo-element, ':' a pseudo-class */
        //   /* Exceptions: :first-line, :first-letter, :before and :after. */
        //   /* Note that pseudo-elements are restricted to one per selector and */
        //   /* occur only in the last simple_selector_sequence. */
        //   : ':' ':'? [ IDENT | functional_pseudo ]
        //   ;

        private static CSSSimpleSelector ParsePseudoSelector(TextReader reader, CSSSimpleSelector result)
        {
            // ':'
            ReadChar(reader, ':');

            // ':'?
            if (SkipChar(reader, ':'))
            {
                result.Type = CSSSimpleSelectorType.PseudoElement;
            }
            else
            {
                result.Type = CSSSimpleSelectorType.PseudoClass;
            }

            // [ IDENT | functional_pseudo ]
            // TODO: support functional_pseudo
            result.Name = ReadIdent(reader);

            switch (result.Name)
            {
                case "first-line":
                case "first-letter":
                case "before":
                case "after":
                    result.Type = CSSSimpleSelectorType.PseudoElement;
                    break;
            }

            return result;
        }

        // type_selector
        //   : [ namespace_prefix ]? element_name
        //   ;

        private static CSSSimpleSelector ParseTypeSelector(TextReader reader, CSSSimpleSelector result)
        {
            // element_name
            //   : IDENT
            //   ;

            // TODO: support namespace_prefix

            result.Type = CSSSimpleSelectorType.Type;
            result.Name = ReadIdent(reader);

            return result;
        }

        // Note: a CSSStyleDeclaration is a collection of one or more individual declarations
        //
        // declaration? [ ';' S* declaration? ]*

        public static CSSStyleDeclarationBase ParseStyleDeclaration(string text, CSSStyleDeclarationBase result = null)
        {
            using (var reader = new StringReader(text))
            {
                return ParseStyleDeclaration(reader, result);
            }
        }

        public static CSSStyleDeclarationBase ParseStyleDeclaration(TextReader reader, CSSStyleDeclarationBase result = null)
        {
            // declaration : property ':' S* value;
            // property    : IDENT S*;
            // value       : [ any | block | ATKEYWORD S* ]+;

            if (result == null)
            {
                result = new CSSStyleDeclarationBase();
            }

            var property = new CSSStyleProperty();

            StringBuilder buffer = new StringBuilder();

            while (TryReadDeclaration(reader, buffer))
            {
                string declaration = buffer.ToString();

                if (!String.IsNullOrWhiteSpace(declaration))
                {
                    try
                    {
                        ParseStyleProperty(declaration.Trim(), property);

                        result.SetProperty(property.Name, property.Value, property.Priority);
                    }
                    catch
                    {
                        Debug.WriteLine("Warning: invalid CSS declaration \"" + declaration + "\"");
                    }
                }

                buffer.Clear();
            }

            return result;
        }

        private static bool TryReadDeclaration(TextReader reader, StringBuilder result)
        {
            int i = reader.Peek();
            if (i == -1)
            {
                return false;
            }

            do
            {
                char ch = (char)i;
                if (ch == ';')
                {
                    reader.Read();
                    break;
                }
                else if (ch == '\'' || ch == '\"')
                {
                    TryReadString(reader, result);
                }
                else if (ch == '(')
                {
                    TryReadGroup(reader, result, ')');
                }
                else if (ch == '[')
                {
                    TryReadGroup(reader, result, ']');
                }
                else
                {
                    reader.Read();
                    result.Append(ch);
                }

                i = reader.Peek();

            } while (i != -1);

            return true;
        }

        private static bool TryReadGroup(TextReader reader, StringBuilder result, char delimiter)
        {
            int i = reader.Read();
            if (i == -1)
            {
                return false;
            }

            char ch = (char)i;
            result.Append(ch);

            while (true)
            {
                i = reader.Read();
                if (i == -1)
                {
                    return false;
                }

                ch = (char)i;
                if (ch == delimiter)
                {
                    result.Append(ch);
                    return true;
                }
                else if (ch == '\'' || ch == '\"')
                {
                    TryReadString(reader, result);
                }
                else if (ch == '(')
                {
                    TryReadGroup(reader, result, ')');
                }
                else if (ch == '[')
                {
                    TryReadGroup(reader, result, ']');
                }
                else
                {
                    result.Append(ch);
                }
            };
        }

        // declaration : name ':' S* value;

        internal static CSSStyleProperty ParseStyleProperty(string text, CSSStyleProperty result = null)
        {
            using (var reader = new StringReader(text))
            {
                return ParseStyleProperty(reader, result);
            }
        }

        internal static CSSStyleProperty ParseStyleProperty(TextReader reader, CSSStyleProperty result = null)
        {
            if (result == null)
            {
                result = new CSSStyleProperty();
            }

            /* Optimized below
            // name
            result.Name = ReadName(reader);

            // ':'
            ReadChar(reader, ':');
            */

            var buffer = Buffer;

            while (true)
            {
                int i = reader.Read();
                if (i == -1)
                {
                    throw new DOMException(DOMException.SYNTAX_ERR);
                }

                char ch = (char)i;
                if (ch == ':')
                {
                    break;
                }

                buffer.Append(ch);
            }

            result.Name = buffer.ToString();

            // S*
            SkipWhitespace(reader);

            // value

            string value = reader.ReadToEnd();
            string priority = String.Empty;

            // [S+ priority]
            if (value.EndsWith("!important"))
            {
                value = value.Remove(value.Length - "!important".Length).TrimEnd();
                priority = "!important";
            }

            result.Value = value;
            result.Priority = priority;

            return result;
        }

        // CSSValueList

        public static CSSValueListBase ParseValueList(string text, char separator = ' ')
        {
            using (var reader = new StringReader(text))
            {
                return ParseValueList(reader, separator);
            }
        }

        public static CSSValueListBase ParseValueList(TextReader reader, char separator = ' ')
        {
            var result = new List<CSSValueBase>();

            var buffer = new StringBuilder();
            bool inQuotes = false;
            char quoteChar = '\0';

            int i;
            while ((i = reader.Read()) != -1)
            {
                char ch = (char)i;

                if (inQuotes)
                {
                    if (ch == quoteChar)
                    {
                        inQuotes = false;
                    }

                    buffer.Append(ch);
                }
                else if (ch == '\'' || ch == '\"')
                {
                    inQuotes = true;
                    quoteChar = ch;

                    buffer.Append(ch);
                }
                else if (ch == '(')
                {
                    buffer.Append(ch);
                    TryReadGroup(reader, buffer, ')');
                }
                else if (separator == ' ' && IsWhitespace(ch) || ch == separator)
                {
                    var value = CSSPrimitiveValueBase.Parse(buffer.ToString());
                    result.Add(value);
                    SkipWhitespace(reader);
                    buffer.Clear();
                }
                else
                {
                    buffer.Append(ch);
                }
            }

            if (buffer.Length > 0)
            {
                var value = CSSPrimitiveValueBase.Parse(buffer.ToString());
                result.Add(value);
            }

            if (result.Count == 0)
            {
                return CSSValueListBase.Empty;
            }

            if (result.Count == 1 && result[0].IsInherit)
            {
                return CSSValueListBase.Inherit;
            }

            return new CSSValueListBase(result, separator);
        }


        #endregion
    }
}
