/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.Views;
using SilverNote;
using SilverNote.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FileFormats.RTF
{
    /// <summary>
    /// Convert an HTML document to an RTF document
    /// </summary>
    public static class RTFDocument
    {
        public static string FromHTML(HTMLDocument htmlDocument)
        {
            HTMLFilters.FlattenLists(htmlDocument.Body);

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                ConvertDocument(htmlDocument, writer, new ConverterState());
                return writer.ToString();
            }
        }

        private class ConverterState
        {  }

        private static void ConvertDocument(HTMLDocument document, TextWriter writer, ConverterState state)
        {
            writer.Write('{');
            WriteHeader(writer);
            WriteFontTable(writer);
            var listTable = GenerateListTable();
            WriteListTable(listTable, writer);
            writer.Write("{\\*\\listoverridetable{\\listoverride\\listid2142453657\\listoverridecount0\\ls1}}");

            ConvertElement(document.Body, writer, state);
            
            writer.Write('}');
        }

        private static void WriteChildren(HTMLElement element, TextWriter writer, ConverterState state)
        {
            for (var child = element.FirstChild; child != null; child = child.NextSibling)
            {
                if (child is HTMLElement)
                {
                    ConvertElement((HTMLElement)child, writer, state);
                }
                else if (child is Text)
                {
                    WriteText((Text)child, writer, state);
                }
            }
        }

        private static void ConvertElement(HTMLElement element, TextWriter writer, ConverterState state)
        {
            if (element.TagName == HTMLElements.H1 ||
                element.TagName == HTMLElements.H2 ||
                element.TagName == HTMLElements.H3 ||
                element.TagName == HTMLElements.H4 ||
                element.TagName == HTMLElements.H5 ||
                element.TagName == HTMLElements.H6)
            {
                WriteParagraph(element, writer, state);
            }
            else if (element.TagName == HTMLElements.P || element.TagName == HTMLElements.BR || element.TagName == HTMLElements.PRE)
            {
                WriteParagraph(element, writer, state);
            }
            else if (element.TagName == HTMLElements.LI)
            {
                WriteParagraph(element, writer, state);
            }
            else if (element.TagName == HTMLElements.SPAN ||
                     element.TagName == HTMLElements.A ||
                     element.TagName == HTMLElements.B ||
                     element.TagName == HTMLElements.I ||
                     element.TagName == HTMLElements.U ||
                     element.TagName == HTMLElements.SUP ||
                     element.TagName == HTMLElements.SUB)
            {
                WriteSpan(element, writer, state);
            }
            else
            {
                WriteChildren(element, writer, state);
            }
        }

        private static void WriteText(Text text, TextWriter writer, ConverterState state)
        {
            string str = Escape(text.Data);
            writer.Write(str);
        }

        private static string Escape(string str)
        {
            var result = new StringBuilder();
            foreach (char c in str)
            {
                result.Append(Escape(c));
            }
            return result.ToString();
        }

        private static string Escape(char c)
        {
            switch (c)
            {
                case '\\':
                    return "\\\\";
                case '{':
                    return "\\{";
                case '}':
                    return "\\}";
                case '~':
                    return "\\~";
                case '-':
                    return "\\-";
                case '_':
                    return "\\_";
                default:
                    return c.ToString();
            }
        }

        private static void WriteHeader(TextWriter writer)
        {
            // RTF version: \rtf<N>
            writer.Write("\\rtf1");
            // Character set: \ansi | \mac | \pc | \pca
            writer.Write("\\ansi");
            // Default font: \deff<N>
            writer.Write("\\deff0");
            writer.Write(' ');
        }

        private static void WriteFontTable(TextWriter writer)
        {
            writer.Write('{');
            // \fonttbl
            writer.Write("\\fonttbl");
            WriteFontTableEntry(0, "nil", "Calibri", writer);
            writer.Write("{\\f2\\fbidi \\fmodern\\fcharset0\\fprq1{\\*\\panose 02070309020205020404}Courier New;}");
            writer.Write("{\\f3\\fbidi \\froman\\fcharset2\\fprq2{\\*\\panose 05050102010706020507}Symbol;}");
            writer.Write("{\\f10\\fbidi \\fnil\\fcharset2\\fprq2{\\*\\panose 05000000000000000000}Wingdings;}");

            writer.Write('}');
        }

        private static void WriteFontTableEntry(int fontNumber, string fontFamily, string fontName, TextWriter writer)
        {
            // {\f0\fswiss Chicago;}
            writer.Write('{');
            writer.Write("\\f");
            writer.Write(fontNumber.ToString(CultureInfo.InvariantCulture));
            writer.Write("\\f");
            writer.Write(fontFamily);
            writer.Write(' ');
            writer.Write(fontName);
            writer.Write(';');
            writer.Write('}');
        }

        private static void WriteListTable(ListTable listTable, TextWriter writer)
        {
            // {\*\listtable
            writer.Write("{\\*\\listtable");

            foreach (var list in listTable.Lists)
            {
                WriteList(list, writer);
            }

            // }
            writer.Write("}");
        }

        private static void WriteList(ListTableEntry list, TextWriter writer)
        {
            // {
            writer.Write('{');

            // \list
            WriteControlWord("list", writer);

            // \listtemplateid740315204
            WriteControlWord("listtemplateid", list.ListTemplateID, writer);

            // \listhybrid
            if (list.IsHybrid)
            {
                WriteControlWord("listhybrid", writer);
            }

            foreach (var listLevel in list.ListLevels)
            {
                WriteListLevel(listLevel, writer);
            }

            // {\listname ;}
            writer.Write("{\\listname ;}");

		    // \listid2142453657
            WriteControlWord("listid", list.ListID, writer);

            // }
            writer.Write("}");
        }

        private static void WriteListLevel(ListLevel listLevel, TextWriter writer)
        {
            // {
            writer.Write('{');

            // \listlevel
            WriteControlWord("listlevel", writer);

            // \levelnfc<N>
            WriteControlWord("levelnfc", listLevel.ListType, writer);

            // \levelnfcn<N>
            WriteControlWord("levelnfcn", listLevel.ListType, writer);

            // \leveljc<N>
            WriteControlWord("leveljc", listLevel.Justify, writer);

            // \leveljcn<N>
            WriteControlWord("leveljcn", listLevel.Justify, writer);

            // \levelfollow<N>
            WriteControlWord("levelfollow", listLevel.Follow, writer);

            // \levelstartat<N>
            WriteControlWord("levelstartat", listLevel.StartAt, writer);

            // \levelspace<N>
            WriteControlWord("levelspace", listLevel.Space, writer);

            // \levelindent<N>
            WriteControlWord("levelindent", listLevel.Indent, writer);

            WriteLevelText(listLevel.Text, writer);

			// {\levelnumbers;}
            writer.Write("{\\levelnumbers;}");

            // chrfmt
            if (listLevel.CharFormat != null)
            {
                writer.Write(listLevel.CharFormat);
            }

            // }
            writer.Write('}');
        }

        private static void WriteLevelText(LevelText text, TextWriter writer)
        {
            // {
            writer.Write('{');

            // \leveltext
            WriteControlWord("leveltext", writer);

            // \leveltemplateid67698689
            WriteControlWord("leveltemplateid", text.TemplateID, writer);

            // \'01\u-3913 ?;
            writer.Write('\\');
            writer.Write(text.Value);

            // }
            writer.Write('}');
        }

        private static ListTable GenerateListTable()
        {
            return new ListTable
            {
                Lists = new ListTableEntry[] 
                {
                    new ListTableEntry 
                    {
                        ListID = 2142453657,
                        ListName = "",
                        ListTemplateID = 740315204,
                        IsHybrid = true,
                        ListLevels = new ListLevel[]
                        {
                            // 1
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 1,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698601,
                                    Value = "\'01\\u-3913 ?;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f3\\fbias0\\fi-360\\li720\\lin720"
                            },
                            // 2
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 0,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698602,
                                    Value = "\'01o;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f2\\fbias0\\fi-360\\li1440\\lin1440"
                            },
                            // 3
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 0,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698603,
                                    Value = "\'01\\u-3929 ?;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f10\\fbias0 \\fi-360\\li2160\\lin2160 "
                            },
                            // 4
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 0,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698604,
                                    Value = "\'01\\u-3913 ?;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f3\\fbias0 \\fi-360\\li2880\\lin2880 "
                            },
                            // 5
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 0,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698605,
                                    Value = "\'01o;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f2\\fbias0 \\fi-360\\li3600\\lin3600 "
                            },
                            // 6
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 0,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698606,
                                    Value = "\'01\\u-3929 ?;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f10\\fbias0 \\fi-360\\li4320\\lin4320 "
                            },
                            // 7
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 0,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698607,
                                    Value = "\'01\\u-3913 ?;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f3\\fbias0 \\fi-360\\li5040\\lin5040 "
                            },
                            // 8
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 0,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698608,
                                    Value = "\'01o;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f2\\fbias0 \\fi-360\\li5760\\lin5760 "
                            },
                            // 9
                            new ListLevel
                            {
                                ListType = 23,
                                Justify = 0,
                                Follow = 0,
                                StartAt = 1,
                                Space = 360,
                                Indent = 0,
                                Text = new LevelText
                                {
                                    TemplateID = 67698609,
                                    Value = "\'01\\u-3929 ?;"
                                }, 
                                LevelNumbers = "",
                                CharFormat = "\\f10\\fbias0 \\fi-360\\li6480\\lin6480 "
                            }
                        }
                    }
                }
            };
        }

        private static void WriteControlWord(string name, TextWriter writer)
        {
            writer.Write('\\');
            writer.Write(name);
        }

        private static void WriteControlWord(string name, int value, TextWriter writer)
        {
            writer.Write('\\');
            writer.Write(name);
            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void WriteNumber(int value, TextWriter writer)
        {
            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void WriteParagraph(HTMLElement element, TextWriter writer, ConverterState state)
        {
            var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, "");

            writer.Write('{');
            WriteControlWord("pard", writer);
            
            // paragraph alignment
            var textAlign = style.GetPropertyCSSValue(CSSProperties.TextAlign);
            if (textAlign == CSSValues.Left)
            {
                WriteControlWord("ql", writer);
            }
            else if (textAlign == CSSValues.Center)
            {
                WriteControlWord("qc", writer);
            }
            else if (textAlign == CSSValues.Right)
            {
                WriteControlWord("qr", writer);
            }
            else if (textAlign == CSSValues.Justify)
            {
                WriteControlWord("qj", writer);
            }
            // top margin
            var marginTopCSS = style.GetPropertyCSSValue(CSSProperties.MarginTop);
            double marginTop = CSSConverter.ToLength(marginTopCSS, CSSPrimitiveType.CSS_PT);
            if (marginTop > 0)
            {
                marginTop = Math.Floor(marginTop * 20.0);   // convert to twips
                WriteControlWord("sb", (int)marginTop, writer);
            }
            // bottom margin
            var marginBottomCSS = style.GetPropertyCSSValue(CSSProperties.MarginBottom);
            double marginBottom = CSSConverter.ToLength(marginBottomCSS, CSSPrimitiveType.CSS_PT);
            if (marginBottom > 0)
            {
                marginBottom = Math.Floor(marginBottom * 20.0);   // convert to twips
                WriteControlWord("sa", (int)marginBottom, writer);
            }
            // left margin
            var marginLeftCSS = style.GetPropertyCSSValue(CSSProperties.MarginLeft);
            double marginLeft = CSSConverter.ToLength(marginLeftCSS, CSSPrimitiveType.CSS_PT);
            marginLeft = Math.Floor(marginLeft * 20.0);   // convert to twips
            // list style
            if (style.GetPropertyCSSValue(CSSProperties.Display) == CSSValues.ListItem)
            {
                var listStyleCSS = style.GetPropertyCSSValue(CSSProperties.ListStyleType);
                if (CSSValues.NumericListStyleTypes.Contains(listStyleCSS))
                {
                    WriteControlWord("ls", 1, writer);
                }
                else
                {
                    WriteControlWord("ls", 1, writer);
                }
                int listLevel = SafeConvert.ToInt32(style.GetPropertyValue("list-level"));
                WriteControlWord("ilvl", listLevel, writer);
                WriteControlWord("li", 360 + (int)marginLeft, writer);
                WriteControlWord("fi", -360, writer);
            }
            else if (marginLeft > 0)
            {
                WriteControlWord("li", (int)marginLeft, writer);
            }

            writer.Write(' ');
            WriteChildren(element, writer, state);
            WriteControlWord("par", writer);
            writer.Write('}');
        }

        private static void WriteSpan(HTMLElement element, TextWriter writer, ConverterState state)
        {
            var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, "");

            writer.Write('{');
            writer.Write("\\plain");

            double fontSize = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.FontSize));
            if (fontSize > 0 && fontSize != 16.0)
            {
                fontSize = 2.0 * fontSize * 72.0 / 96.0;  // Convert to half-points
                writer.Write("\\fs");
                writer.Write(fontSize.ToString(CultureInfo.InvariantCulture));
            }

            if (style.GetPropertyCSSValue(CSSProperties.FontWeight) == CSSValues.Bold)
            {
                writer.Write("\\b");
            }
            if (style.GetPropertyCSSValue(CSSProperties.FontStyle) == CSSValues.Italic)
            {
                writer.Write("\\i");
            }
            var decorations = (CSSValueList)style.GetPropertyCSSValue(CSSProperties.TextDecoration);
            if (decorations.Contains(CSSValues.Underline))
            {
                writer.Write("\\ul");
            }
            var verticalAlign = style.GetPropertyCSSValue(CSSProperties.VerticalAlign);
            if (verticalAlign == CSSValues.Super)
            {
                writer.Write("\\super");
            }
            if (verticalAlign == CSSValues.Sub)
            {
                writer.Write("\\sub");
            }

            writer.Write(' ');
            WriteChildren(element, writer, state);

            writer.Write('}');
       }
    }
}
