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
using DOM;

namespace SilverNote.Editor
{
    /// <summary>
    /// Text renderer
    /// </summary>
    public class TextRenderer : ContainerVisual, IDisposable
    {
        #region Fields
        
        readonly TextSource _Source;
        readonly TextParagraphProperties _Properties;
        readonly TextRunCache _Cache;
        readonly List<TextLineVisual> _TextLines;
        MinMaxParagraphWidth? _MinMaxParagraphWidth;
        TextFormattingMode _TextFormattingMode;
        double _LineSpacing;
        double _Width;
        double _Height;
        bool _IsRenderValid;

        #endregion

        #region Constructors

        public TextRenderer(TextSource source, TextParagraphProperties properties)
        {
            _Source = source;
            _Properties = properties;
            _Cache = new TextRunCache();
            _TextLines = new List<TextLineVisual>();
            _MinMaxParagraphWidth = null;
            _TextFormattingMode = TextFormattingMode.Display;
            _LineSpacing = 1.0;
            _Width = 0;
            _Height = 0;
            _IsRenderValid = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Text source as set in the constructor
        /// </summary>
        public TextSource Source
        {
            get { return _Source; }
        }

        /// <summary>
        /// Paragraph properties as set in the constructor
        /// </summary>
        public TextParagraphProperties Properties
        {
            get {  return _Properties; }
        }

        /// <summary>
        /// Text formatting mode. 
        /// </summary>
        public TextFormattingMode TextFormattingMode
        {
            get
            {
                return _TextFormattingMode;
            }
            set
            {
                if (value != _TextFormattingMode)
                {
                    _TextFormattingMode = value;
                    _IsRenderValid = false;
                }
            }
        }

        /// <summary>
        /// Line spacing (single-spacing = 1.0, double-spacing = 2.0, etc.)
        /// </summary>
        public double LineSpacing
        {
            get
            {
                return _LineSpacing;
            }
            set
            {
                if (value != _LineSpacing)
                {
                    _LineSpacing = value;
                    _IsRenderValid = false;
                }
            }
        }

        /// <summary>
        /// Desired width (set prior to calling Draw())
        /// </summary>
        public double Width
        {
            get
            {
                return _Width;
            }
            set
            {
                if (value != _Width)
                {
                    _IsRenderValid = false;
                    _Width = value;
                }
            }
        }

        /// <summary>
        /// Actual height (valid after Draw() returns)
        /// </summary>
        public double Height
        {
            get
            {
                return _Height;
            }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    RaiseHeightChanged();
                }
            }
        }

        public MinMaxParagraphWidth MinMaxParagraphWidth
        {
            get
            {
                if (!_MinMaxParagraphWidth.HasValue)
                {
                    using (var formatter = TextFormatter.Create(TextFormattingMode))
                    {
                        _MinMaxParagraphWidth = formatter.FormatMinMaxParagraphWidth(Source, 0, Properties);
                    }
                }
                return _MinMaxParagraphWidth.Value;
            }
        }

        /// <summary>
        /// False if needs to be re-rendered
        /// </summary>
        public bool IsRenderValid
        {
            get { return _IsRenderValid; }
        }

        #endregion

        #region Events

        public event EventHandler TextRendered;

        protected void RaiseTextRendered()
        {
            if (TextRendered != null)
            {
                TextRendered(this, EventArgs.Empty);
            }
        }

        public event EventHandler HeightChanged;

        protected void RaiseHeightChanged()
        {
            if (HeightChanged != null)
            {
                HeightChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Render text.
        /// 
        /// For an optimization, you can (optionally) specify the range
        /// of text that has been modified since the last call to this method.
        /// This helps the renderer to determine what regions need to be drawn.
        /// </summary>
        public void Draw(int offset = 0, int added = 0, int removed = 0)
        {
            // Get the index of the first line to be rendered
            //
            // Begin rendering on the line preceding the first modified line
            // (adding/removing whitespace at the beginning of a line can 
            // affect line breaking, thereby affecting the previous line)

            int lineIndex = 0;

            if (offset > 0)
            {
                lineIndex = LineAt(offset) - 1;

                lineIndex = Math.Max(lineIndex, 0);
            }

            // Character offset of the first character in the next line to be rendered
            int charOffset = 0;
            // Position of the top-left corner of the next line to be rendered
            Point position = new Point(0, 0);
            // Line breaking state
            TextLineBreak textLineBreak = null;

            if (lineIndex < _TextLines.Count)
            {
                TextLineVisual firstLine = _TextLines[lineIndex];
                charOffset = firstLine.CharOffset;
                position.X = firstLine.Offset.X;
                position.Y = firstLine.Offset.Y;

                if (lineIndex > 0)
                {
                    TextLineVisual previousLine = _TextLines[lineIndex - 1];
                    textLineBreak = previousLine.TextLine.GetTextLineBreak();
                }
            }

            // If 'added' or 'removed' are specified, then a change has been made to 
            // a single contiguous block of text. In this case, if we find that
            // the rendered content of any line following that block of text has
            // not changed, then we stop rendering and set cacheHit = true.

            bool cacheHit = false;

            using (var formatter = TextFormatter.Create(TextFormattingMode))
            {
                do
                {
                    // Get the TextLine that needs to be rendered
                    
                    TextLineVisual textLine;

                    if (lineIndex < _TextLines.Count)
                    {
                        textLine = _TextLines[lineIndex];
                    }
                    else
                    {
                        textLine = new TextLineVisual();
                        AppendTextLine(textLine);
                    }

                    // If 'added' or 'remove' are specified, then a single contiguous
                    // range of text has been modified since the last call to this method.
                    // In this case, if we enounter a text line following that block of
                    // text whose contents have not shifted, then we can stop rendering.

                    if (added != 0 || removed != 0)
                    {
                        // Is charOffset past the modified range of text?
                        if (charOffset >= offset + Math.Max(added, removed))
                        {
                            // Is the old charOffset of this line the same as its new charOffset?
                            if (textLine.CharOffset == charOffset - added + removed &&
                                textLine.TextLine != null)
                            {
                                cacheHit = true;
                                break;
                            }
                        }
                    }

                    // Certain properties apply only to the first line in the paragraph

                    var proxy = _Properties as TextParagraphPropertiesProxy;
                    if (proxy != null)
                    {
                        proxy.SetFirstLineInParagraph(lineIndex == 0);
                    }

                    // Compute the text's geometry, etc.

                    textLine.TextLine = formatter.FormatLine ( 
                        Source, 
                        charOffset, 
                        Width, 
                        Properties, 
                        textLineBreak, 
                        _Cache
                    );
                    textLineBreak = textLine.TextLine.GetTextLineBreak();

                    // Now actually draw the text to a DrawingContext

                    Size renderSize = new Size(Width, textLine.TextLine.Height * LineSpacing);

                    using (var dc = textLine.RenderOpen())
                    {
                        dc.DrawRectangle(Brushes.Transparent, null, new Rect(renderSize));
                        textLine.TextLine.Draw(dc, new Point(0, 0), InvertAxes.None);
                    }

                    // Update line position

                    textLine.Offset = new Vector(position.X, position.Y);
                    position.Y += renderSize.Height;

                    // Update char offset

                    textLine.CachedOffset = textLine.CharOffset = charOffset;
                    charOffset += textLine.TextLine.Length;

                    // Update line index

                    textLine.LineIndex = lineIndex;
                    lineIndex++;

                } while (!(Source.GetTextRun(charOffset) is TextEndOfParagraph));
            }

            if (cacheHit)
            {
                // Rendering stopped due to a cache hit - we still need to 
                // update the position and char offset of the remaining lines

                while (lineIndex < _TextLines.Count)
                {
                    TextLineVisual line = _TextLines[lineIndex];
                    
                    // Update position
                    line.Offset = new Vector(position.X, position.Y);
                    position.Y += line.TextLine.Height * LineSpacing;

                    // Update char offset
                    line.CharOffset = charOffset;
                    charOffset += line.TextLine.Length;

                    lineIndex++;
                }
            }
            else
            {
                // All lines were rendered, and any remaining lines are
                // not needed and should be removed

                RemoveLines(lineIndex);
            }

            _IsRenderValid = true;
            Height = position.Y;
            RaiseTextRendered();
        }

        /// <summary>
        /// Invalidate the rendering cache
        /// </summary>
        public void Invalidate(int charOffset, int numAdded, int numRemoved)
        {
            _Cache.Change(charOffset, numAdded, numRemoved);
            _IsRenderValid = false;
            _MinMaxParagraphWidth = null;
        }

        /// <summary>
        /// Invalidate the rendering cache
        /// </summary>
        public void Invalidate()
        {
            _Cache.Invalidate();
            _IsRenderValid = false;
            _MinMaxParagraphWidth = null;
        }

        #endregion

        #region TextLines

        /// <summary>
        /// Get the number of lines of text
        /// </summary>
        public int LineCount
        {
            get { return _TextLines.Count; }
        }

        /// <summary>
        /// Create a BaseTextLine and add it to our children
        /// </summary>
        /// <returns></returns>
        private void AppendTextLine(TextLineVisual textLine)
        {
            _TextLines.Add(textLine);
            Children.Add(textLine);
        }

        /// <summary>
        /// Remove all lines starting at the given index
        /// </summary>
        private void RemoveLines(int startIndex = 0)
        {
            for (int i = startIndex; i < _TextLines.Count; i++)
            {
                _TextLines[i].Dispose();
            }

            _TextLines.RemoveRange(startIndex, _TextLines.Count - startIndex);
            Children.RemoveRange(startIndex, Children.Count - startIndex);
        }

        /// <summary>
        /// Get the position of the given text line
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <returns></returns>
        public Point GetTextLinePosition(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= _TextLines.Count)
            {
                return new Point(0, 0);
            }

            TextLineVisual textLine = _TextLines[lineIndex];

            if (textLine.TextLine != null)
            {
                return new Point(textLine.Offset.X, textLine.Offset.Y);
            }
            else
            {
                return new Point(0, 0);
            }
        }

        /// <summary>
        /// Get the width of the given text line
        /// </summary>
        public double GetTextLineWidth(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= _TextLines.Count)
            {
                return 0;
            }

            TextLineVisual textLine = _TextLines[lineIndex];

            if (textLine.TextLine != null)
            {
                return textLine.TextLine.Width;
            }
            else
            {
                return 0;
            }
        }

        private int _CachedLineFromCharResult = 0;

        /// <summary>
        /// Get the line containing the specified character offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int LineAt(int offset)
        {
            // For improved performance, begin our search at the index
            // that was returned by the previous call to this method

            if (_CachedLineFromCharResult >= 0 && _CachedLineFromCharResult < _TextLines.Count)
            {
                TextLineVisual textLine = _TextLines[_CachedLineFromCharResult];

                if (offset >= textLine.CharOffset && offset < textLine.CharOffset + textLine.TextLine.Length)
                {
                    return _CachedLineFromCharResult;
                }
            }

            int result = _TextLines.BinarySearch(new TextLineVisual { CharOffset = offset });
            if (result < 0)
            {
                result = ~result - 1;
            }

            _CachedLineFromCharResult = result;

            return result;
        }

        /// <summary>
        /// Get the character offset of the specified line
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <returns></returns>
        public int LineOffset(int lineIndex)
        {
            return _TextLines[lineIndex].CharOffset;
        }


        public TextLine LineMetrics(int lineIndex)
        {
            return _TextLines[lineIndex].TextLine;
        }

        /// <summary>
        /// Get the line containing the specified geometric offset
        /// </summary>
        public int LineFromPoint(Point point)
        {
            var hit = VisualTreeHelper.HitTest(this, point);

            if (hit != null && hit.VisualHit is TextLineVisual)
            {
                return ((TextLineVisual)hit.VisualHit).LineIndex;
            }

            return -1;
        }

        /// <summary>
        /// Get the character offset from the specified geometric offset
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public int CharFromPoint(Point point)
        {
            var lineHit = VisualTreeHelper.HitTest(this, point);
            if (lineHit == null)
            {
                return -1;  // point did not hit anything
            }

            var textLine = lineHit.VisualHit as TextLineVisual;
            if (textLine == null)
            {
                return -1;  // point did not hit a line
            }

            point = TransformToDescendant(textLine).Transform(point);
            var charHit = textLine.TextLine.GetCharacterHitFromDistance(point.X);
            if (charHit == null)
            {
                return -1;  // point did not hit a character
            }

            int charOffset = charHit.FirstCharacterIndex + charHit.TrailingLength;
            return charOffset + textLine.CharOffset - textLine.CachedOffset;
        }

        /// <summary>
        /// Get the horizontal distance from the left edge of our container to the given character offset
        /// </summary>
        public Point GetCharPosition(int offset)
        {
            int lineIndex = LineAt(offset);
            if (lineIndex == -1)
            {
                return new Point(0, 0);
            }

            TextLineVisual textLine = _TextLines[lineIndex];
            double y = textLine.Offset.Y + textLine.TextLine.Height / 2;

            offset = Math.Min(offset, textLine.CharOffset + textLine.TextLine.Length);
            var charHit = new CharacterHit(offset + textLine.CachedOffset - textLine.CharOffset, 0);
            double x = textLine.Offset.X + textLine.TextLine.GetDistanceFromCharacterHit(charHit);

            return new Point(x, y);
        }

        /// <summary>
        /// Get the character offset at the given horizontal distance from the left edge of the given line
        /// </summary>
        public int CharFromDistance(int lineIndex, double distance)
        {
            if (lineIndex < 0 || lineIndex >= _TextLines.Count)
            {
                return 0;
            }

            TextLineVisual textLine = _TextLines[lineIndex];

            var charHit = textLine.TextLine.GetCharacterHitFromDistance(distance);
            if (charHit == null)
            {
                return 0;
            }

            int charOffset = charHit.FirstCharacterIndex + charHit.TrailingLength;
            charOffset += textLine.CharOffset - textLine.CachedOffset;
            return charOffset;
        }

        /// <summary>
        /// Get a rectangular outline of the given range of text
        /// 
        /// This is used for drawing a selection background
        /// </summary>
        public Geometry GetSelectionBounds(int beginOffset, int endOffset)
        {
            var geometry = new GeometryGroup();

            int lineIndex = LineAt(beginOffset);
            if (lineIndex < 0)
            {
                return geometry;
            }

            // Add a RectangleGeometry for each line included in the selection

            TextLineVisual textLine;
            do
            {
                if (lineIndex >= _TextLines.Count)
                {
                    break;
                }
                textLine = _TextLines[lineIndex++];

                // Get the x-coordinate of the left edge of region of the
                // current text line that is included in the given range

                double left = textLine.Offset.X;

                if (beginOffset >= textLine.CharOffset && 
                    beginOffset < textLine.CharOffset + textLine.TextLine.Length)
                {
                    int charIndex = beginOffset + textLine.CachedOffset - textLine.CharOffset;
                    var hit = new CharacterHit(charIndex, 0);
                    left += textLine.TextLine.GetDistanceFromCharacterHit(hit);
                }
                else
                {
                    left += textLine.TextLine.Start;
                }

                // Get the x-coordinate of the right edge of region of the
                // current text line that is included in the given range

                double right = textLine.Offset.X;

                if (endOffset >= textLine.CharOffset && 
                    endOffset < textLine.CharOffset + textLine.TextLine.Length)
                {
                    int charIndex = endOffset + textLine.CachedOffset - textLine.CharOffset;
                    var hit = new CharacterHit(charIndex, 0);
                    right += textLine.TextLine.GetDistanceFromCharacterHit(hit);
                }
                else
                {
                    right += textLine.TextLine.Start + textLine.TextLine.WidthIncludingTrailingWhitespace;

                    // WidthIncludingTrailingWhitespace sometimes evaluates to 1 pixel less than
                    // the value returned by GetDistanceFromCharacterHit() used in computing 'left'.
                    // Seems like a bug on Microsoft's end.
                    right = Math.Max(right, left);
                }

                double x = Math.Min(left, right);
                double y = textLine.Offset.Y;
                double width = Math.Abs(right - left);
                double height = textLine.TextLine.Height * LineSpacing;

                // Construct a rectangle representing the geometric region
                // in the current text line that is included in the given range

                Rect rect = new Rect(x, y, width, height);

                // Add this to our result

                geometry.Children.Add(new RectangleGeometry(rect));
            
            } while (textLine.CharOffset + textLine.TextLine.Length <= endOffset);

            return geometry;
        }

        /// <summary>
        /// Get the geometry of the caret at the given character offset
        /// 
        /// This is used for drawing the caret
        /// </summary>
        public Rect GetCaretBounds(int charOffset)
        {
            int lineIndex = LineAt(charOffset);
            if (lineIndex == -1)
            {
                return Rect.Empty;
            }

            TextLineVisual textLine = _TextLines[lineIndex];
            charOffset = Math.Min(charOffset, textLine.CharOffset + textLine.TextLine.Length);
            var hit = new CharacterHit(charOffset + textLine.CachedOffset - textLine.CharOffset, 0);
            double distance = textLine.TextLine.GetDistanceFromCharacterHit(hit);

            return new Rect(textLine.Offset.X + distance, textLine.Offset.Y, 0, textLine.TextLine.Height);
        }

        #endregion

        #region SVG

        public IEnumerable<object> GetSVGChildNodes(NodeContext context, Point origin)
        {
            var results = new List<object>();

            var buffer = Source as TextBuffer;
            if (buffer != null)
            {
                foreach (TextLineVisual line in _TextLines)
                {
                    // If a paragraph ends in a newline, an empty NTextLine will
                    // be appended to the set of TextLines. This line will not
                    // contain text and its charOffset will be set to buffer.Length.

                    if (line.CharOffset < buffer.Length)
                    {
                        int charOffset = line.CharOffset;
                        int charLength = Math.Min(line.TextLine.Length, buffer.Length - charOffset);

                        Point position = origin + line.Offset;
                        position.X += line.TextLine.Start;
                        position.Y += line.TextLine.Baseline;

                        var result = buffer.GetSVGChildNodes(context, charOffset, charLength, position);
                        if (result != null)
                        {
                            results.AddRange(result);
                        }
                    }
                }
            }

            return results;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            RemoveLines(startIndex: 0);
        }

        #endregion
    }

    public class ParagraphPropertiesChangedEventArgs : EventArgs
    {
        public ParagraphPropertiesChangedEventArgs(GenericTextParagraphProperties oldValue, GenericTextParagraphProperties newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public GenericTextParagraphProperties OldValue { get; private set; }
        public GenericTextParagraphProperties NewValue { get; private set; }
    }

    public class TextSourceChangedEventArgs : EventArgs
    {
        public TextSourceChangedEventArgs(TextSource oldValue, TextSource newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public TextSource OldValue { get; private set; }
        public TextSource NewValue { get; private set; }
    }
}
