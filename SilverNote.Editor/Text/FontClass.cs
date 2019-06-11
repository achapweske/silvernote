/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SilverNote.Editor
{
    public class FontClass
    {
        #region Static Fields

        static FontClass _Heading2, _Heading3;
        static FontClass _Normal, _Serif, _Cursive, _Monospace;
        static FontClass _SourceCode;
        static ObservableCollection<FontClass> _CommonStyles;
        static ObservableCollection<FontClass> _SystemStyles;

        #endregion

        #region Static Constructors

        static FontClass()
        {
            _CommonStyles = new ObservableCollection<FontClass>(CreateCommonStyles());
            _SystemStyles = new ObservableCollection<FontClass>(CreateSystemStyles());
        }

        static IEnumerable<FontClass> CreateCommonStyles()
        {
            _Normal = new FontClass { ID = "sans-serif", Name = "Normal Text", FontFamily = new FontFamily("Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif") };
            _Serif = new FontClass { ID = "serif", Name = "Printed Text", FontFamily = new FontFamily("Times New Roman, Times, Serif") };
            _Cursive = new FontClass { ID = "cursive", Name = "Cursive", FontFamily = new FontFamily("Comic Sans MS, Cursive") };
            _Monospace = new FontClass { ID = "monospace", Name = "Fixed-width", FontFamily = new FontFamily("Consolas, Lucida Console, Lucida Sans Typewriter, Monaco, Courier New, Courier, Monospace") };
            _SourceCode = new FontClass { ID = "code", Name = "Source Code", FontFamily = _Monospace.FontFamily, FontSize = 10 };
            _Heading2 = new FontClass { ID = "h2", Name = "Section Title", FontFamily = _Normal.FontFamily, FontSize = 14, FontWeight = FontWeights.Bold };
            _Heading3 = new FontClass { ID = "h3", Name = "Sub-section Title", FontFamily = _Normal.FontFamily, FontSize = 12, FontWeight = FontWeights.Bold };

            return new[] { _Heading2, _Heading3, _Normal, _Serif, _Cursive, _Monospace, _SourceCode };
        }

        static IEnumerable<FontClass> CreateSystemStyles()
        {
            var systemStyles = new List<FontClass>();

            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                switch (font.Source)
                {
                    case "Arial":
                    case "Lucida Sans":
                    case "Tahoma":
                    case "Verdana":
                    case "Microsoft Sans Serif":
                    case "Trebuchet MS":
                    case "Courier New":
                    case "Times New Roman":
                    case "Comic Sans MS":
                    case "Georgia":
                    case "Palatino Linotype":
                    case "Lucida Sans Unicode":
                    case "Franklin Gothic Medium":
                    case "Lucida Console":
                    case "Impact":
                    case "Arial Black":
                    case "Sylfaen":
                    case "MV Boli":
                    case "Estrangelo Edessa":
                    case "Tunga":
                    case "Raavi":
                    case "Gautami":
                    case "Mangal":
                    case "Shruti":
                    case "Latha":
                    case "Kartika":
                    case "Vrinda":
                    case "Arial Narrow":
                    case "Century Gothic":
                    case "Garamond":
                    case "Book Antiqua":
                    case "Bookman Old Style":
                    case "Calibri":
                    case "Cambria":
                    case "Candara":
                    case "Corbel":
                    case "Cambria Math":
                    case "Constantia":
                    case "Consolas":
                    case "Monotype Corsiva":
                        systemStyles.Add(new FontClass { Name = font.Source, FontFamily = font });
                        break;
                    default:
                        break;
                }
            }

            systemStyles.Sort((a, b) => String.Compare(a.Name, b.Name));

            return systemStyles;
        }

        #endregion

        #region Static Properties

        public static FontClass Heading2
        {
            get { return _Heading2; }
        }

        public static FontClass Heading3
        {
            get { return _Heading3; }
        }

        public static FontClass Normal
        {
            get { return _Normal; }
        }

        public static FontClass Serif
        {
            get { return _Serif; }
        }

        public static FontClass Cursive
        {
            get { return _Cursive; }
        }

        public static FontClass Monospace
        {
            get { return _Monospace; }
        }

        public static FontClass SourceCode
        {
            get { return _SourceCode; }
        }

        public static ObservableCollection<FontClass> CommonStyles
        {
            get { return _CommonStyles; }
        }

        public static ObservableCollection<FontClass> SystemStyles
        {
            get { return _SystemStyles; }
        }

        #endregion

        #region Static Methods

        static IEnumerable<FontClass> AllStyles
        {
            get { return CommonStyles.Concat(SystemStyles); }
        }

        static public FontClass FromFont(FontFamily font)
        {
            if (Normal.FontFamily.Source == font.Source)
            {
                return Normal;
            }
            else
            {
                return AllStyles.FirstOrDefault(style => style.FontFamily != null && style.FontFamily.Source == font.Source);
            }
        }

        static public FontClass FromID(string id)
        {
            return AllStyles.FirstOrDefault(style => style.ID == id);
        }

        #endregion

        #region Constructors

        #endregion

        #region Properties

        public string ID { get; set; }

        public string Name { get; set; }

        public FontFamily FontFamily { get; set; }

        public int? FontSize { get; set; }

        public FontWeight? FontWeight { get; set; }

        public FontStyle? FontStyle { get; set; }

        #endregion

        #region Object

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
