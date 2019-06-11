/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Xml;

namespace SilverNote.Editor
{
    /// <summary>
    /// A DrawingVisual to which a single line of text is rendered
    /// </summary>
    public class TextLineVisual : DrawingVisual, IComparable<TextLineVisual>, IDisposable
    {
        #region Fields

        TextLine _TextLine;

        #endregion

        #region Constructors

        public TextLineVisual()
        { }

        #endregion

        #region Properties

        /// <summary>
        /// The associated TextLine properties
        /// </summary>
        public TextLine TextLine
        {
            get
            {
                return _TextLine;
            }
            set
            {
                if (_TextLine != null)
                {
                    _TextLine.Dispose();
                }
                _TextLine = value;
            }
        }

        /// <summary>
        /// Zero-based index of this line relative to its container
        /// </summary>
        public int LineIndex { get; set; }

        /// <summary>
        /// Offset of the first character in this line relative to its container
        /// </summary>
        public int CharOffset { get; set; }

        /// <summary>
        /// Offset of the first character in this line when this line was last drawn
        /// </summary>
        public int CachedOffset { get; set; }

        #endregion

        #region IComparable

        public int CompareTo(TextLineVisual other)
        {
            return CharOffset - other.CharOffset;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (TextLine != null)
            {
                TextLine.Dispose();
            }
        }

        #endregion
    }
}
