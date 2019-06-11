/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using SilverNote.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace SilverNote.Editor
{
    public class TextField : TextElement, ITextElement, ISearchable
    {
        #region Fields

        Caret _Caret;
        int _CaretIndex;
        int _SelectionBegin;
        int _SelectionEnd;
        DrawingVisual _WatermarkDrawing;
        string _DropData;
        DrawingVisual _DropVisual;
        bool _IsMouseLeftButtonHandled;

        #endregion

        #region Constructors

        public TextField()
            : base()
        {
            Initialize();
        }

        public TextField(TextBuffer buffer)
            : base(buffer)
        {
            Initialize();
        }

        public TextField(TextField copy)
            : base(copy)
        {
            Initialize();

            AcceptsReturn = copy.AcceptsReturn;
            AutoIndent = copy.AutoIndent;
            CapturesMouse = copy.CapturesMouse;

            if (copy.ReadLocalValue(HighlightBrushProperty) != DependencyProperty.UnsetValue)
            {
                HighlightBrush = copy.HighlightBrush;
            }

            if (copy.ReadLocalValue(IsHighlightingProperty) != DependencyProperty.UnsetValue)
            {
                IsHighlighting = copy.IsHighlighting;
            }

            SelectionBrush = copy.SelectionBrush;
            SearchResultsBrush = copy.SearchResultsBrush;
            Watermark = copy.Watermark;

            CaretBrush = copy.CaretBrush;
        }

        private void Initialize()
        {
            _Caret = new Caret();

            AllowDrop = true;
            Cursor = Cursors.IBeam;
            IsEnabled = true;
            Focusable = true;
            FocusVisualStyle = null;

            GotKeyboardFocus += UpdateCaretVisibility;
            LostKeyboardFocus += UpdateCaretVisibility;
            KeyDown += KeyDown_Caret;

            TextRendered += UpdateCaretPosition;

            GotFocus += UpdateSelection;
            LostFocus += UpdateSelection;
            TextRendered += UpdateSelection;

            GotFocus += UpdateWatermarkVisibility;
            LostFocus += UpdateWatermarkVisibility;
            TextChanged += UpdateWatermarkVisibility;

            TextRendered += UpdateSearchResults;

            UpdateWatermarkVisibility(this, EventArgs.Empty);

            TextCompositionManager.AddTextInputHandler(this, OnTextInput);
        }

        #endregion

        #region Dependency Properties

        #region AcceptsReturn

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(
            "AcceptsReturn",
            typeof(bool),
            typeof(TextField),
            new FrameworkPropertyMetadata(false)
        );

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        #endregion

        #region AutoIndent

        public static readonly DependencyProperty AutoIndentProperty = DependencyProperty.Register(
            "AutoIndent",
            typeof(bool),
            typeof(TextField),
            new FrameworkPropertyMetadata(false)
        );

        public bool AutoIndent
        {
            get { return (bool)GetValue(AutoIndentProperty); }
            set { SetValue(AutoIndentProperty, value); }
        }

        #endregion

        #region CapturesMouse

        public static readonly DependencyProperty CapturesMouseProperty = DependencyProperty.Register(
            "CapturesMouse",
            typeof(bool),
            typeof(TextField),
            new FrameworkPropertyMetadata(false)
        );

        public bool CapturesMouse
        {
            get { return (bool)GetValue(CapturesMouseProperty); }
            set { SetValue(CapturesMouseProperty, value); }
        }

        #endregion

        #region HighlightBrush

        public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.RegisterAttached(
            "HighlightBrush",
            typeof(Brush),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(Brushes.Yellow, FrameworkPropertyMetadataOptions.Inherits)
        );

        public static void SetHighlightBrush(DependencyObject target, Brush value)
        {
            target.SetValue(HighlightBrushProperty, value);
        }

        public static Brush GetHighlightBrush(DependencyObject target)
        {
            return (Brush)target.GetValue(HighlightBrushProperty);
        }

        public Brush HighlightBrush
        {
            get { return GetHighlightBrush(this); }
            set { SetHighlightBrush(this, value); }
        }

        #endregion

        #region IsHighlighting

        public static readonly DependencyProperty IsHighlightingProperty = DependencyProperty.RegisterAttached(
            "IsHighlighting",
            typeof(bool),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits)
        );

        public static void SetIsHighlighting(DependencyObject target, bool value)
        {
            target.SetValue(IsHighlightingProperty, value);
        }

        public static bool GetIsHighlighting(DependencyObject target)
        {
            return (bool)target.GetValue(IsHighlightingProperty);
        }

        public bool IsHighlighting
        {
            get { return GetIsHighlighting(this); }
            set { SetIsHighlighting(this, value); }
        }
        
        #endregion

        #region SelectionBrush

        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(
            "SelectionBrush",
            typeof(Brush),
            typeof(TextField),
            new FrameworkPropertyMetadata(Brushes.LightSteelBlue, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }

        #endregion

        #region SearchResultsBrush

        public static readonly DependencyProperty SearchResultsBrushProperty = DependencyProperty.Register(
            "SearchResultsBrush",
            typeof(Brush),
            typeof(TextField),
            new FrameworkPropertyMetadata(Brushes.Khaki, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Brush SearchResultsBrush
        {
            get { return (Brush)GetValue(SearchResultsBrushProperty); }
            set { SetValue(SearchResultsBrushProperty, value); }
        }

        #endregion

        #region Watermark

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            "Watermark",
            typeof(string),
            typeof(TextField),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, WatermarkProperty_PropertyChanged)
        );

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        static void WatermarkProperty_PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((TextField)sender).OnWatermarkChanged();
        }

        void OnWatermarkChanged()
        {
            if (IsArrangeValid)
            {
                DrawWatermark(RenderSize);
            }
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Get/set the brush used to paint the caret
        /// </summary>
        public Brush CaretBrush
        {
            get { return _Caret.Brush; }
            set { _Caret.Brush = value; }
        }

        /// <summary>
        /// Get/set the current character offset of the caret.
        /// 
        /// The caret is drawn immediately before the character at this offset,
        /// or immediately after the last character if value = Length.
        /// </summary>
        public int CaretIndex
        {
            get
            {
                return _CaretIndex;
            }
            set
            {
                if (value != _CaretIndex)
                {
                    _CaretIndex = value;
                    UpdateCaretPosition(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Character offset of the first selected character
        /// </summary>
        public int SelectionBegin
        {
            get { return _SelectionBegin; }
            set { SetSelection(value, _SelectionEnd); }
        }

        /// <summary>
        /// Character offset just beyond the last selected character
        /// </summary>
        public int SelectionEnd
        {
            get { return _SelectionEnd; }
            set { SetSelection(_SelectionBegin, value); }
        }

        /// <summary>
        /// Get/set the character offset of the "ordinal" first selected character
        /// </summary>
        public int SelectionOffset
        {
            get
            {
                return Math.Min(SelectionBegin, SelectionEnd);
            }
            set
            {
                int selectionLength = SelectionLength;

                if (SelectionBegin <= SelectionEnd)
                {
                    SetSelection(value, value + selectionLength);
                }
                else
                {
                    SetSelection(value + selectionLength, value);
                }
            }
        }

        /// <summary>
        /// Get/set the number of selected characters
        /// 
        /// When setting this value, the ordinal first selected character
        /// remains the same and the ordinal last selected character is changed
        /// </summary>
        public int SelectionLength
        {
            get
            {
                return Math.Abs(SelectionEnd - SelectionBegin);
            }
            set
            {
                if (SelectionBegin <= SelectionEnd)
                {
                    SelectionEnd = SelectionBegin + value;
                }
                else
                {
                    SelectionBegin = SelectionEnd + value;
                }
            }
        }

        #region DefaultTextRunProperties

        public override TextRunProperties DefaultTextRunProperties
        {
            get
            {
                return Buffer.GetProperties(CaretIndex);
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        #endregion

        #endregion

        #region Routed Events

        #region FormatChanged

        /// <summary>
        /// Invoked when a format property changes
        /// </summary>
        public static RoutedEvent FormatChanged = Formattable.FormatChanged.AddOwner(typeof(TextField));

        private void RaiseFormatChanged()
        {
            RaiseEvent(new RoutedEventArgs(FormatChanged));
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the range of selected text changes
        /// </summary>
        public event EventHandler SelectionChanged;

        private void RaiseSelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get selected text's properties (font family, size, color, etc.)
        /// </summary>
        public GenericTextRunProperties GetTextProperties()
        {
            if (HasPendingProperties)
            {
                return GetPendingProperties();
            }

            int index = Math.Max(SelectionBegin, SelectionEnd) - 1;
            index = Math.Min(index, Length - 1);
            index = Math.Max(index, 0);

            return GetTextProperties(index);
        }

        /// <summary>
        /// Set selected text's properties (font family, size, color, etc.)
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void SetTextProperties(GenericTextRunProperties properties)
        {
            if (SelectionLength > 0 || Buffer.Length == 0)
            {
                SetTextProperties(SelectionOffset, SelectionLength, properties);
            }
            else
            {   // No text is selected - save properties to be applied to next typed character
                SetPendingProperties((GenericTextRunProperties)properties.Clone());
            }
        }

        /// <summary>
        /// Get a property of the currently-selected text
        /// </summary>
        public object GetTextProperty(string name)
        {
            if (HasPendingProperties)
            {
                return GetPendingProperties().GetProperty(name);
            }

            int index = Math.Max(SelectionBegin, SelectionEnd) - 1;
            index = Math.Min(index, Length - 1);
            index = Math.Max(index, 0);

            return GetTextProperty(index, name);
        }

        /// <summary>
        /// Set selected text's properties (font family, size, color, etc.)
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void SetTextProperty(string name, object newValue)
        {
            if (!TextProperties.Default.HasProperty(name))
            {
                return;
            }

            if (SelectionLength > 0 || Buffer.Length == 0)
            {
                SetTextProperty(SelectionOffset, SelectionLength, name, newValue);
            }
            else
            {
                SetPendingProperty(name, newValue);
            }
        }

        /// <summary>
        /// Set selected text's properties (font family, size, color, etc.)
        /// 
        /// This operation is undo-able.
        /// </summary>
        /// <param name="property">The property to be set</param>
        /// <param name="newValues">An array where each item contains the new 
        /// property value and the number of characters to which it applies</param>
        public void SetTextProperty(string name, IList<TextPropertyValue> newValues)
        {
            SetTextProperty(SelectionOffset, SelectionLength, name, newValues);
        }

        /// <summary>
        /// Reset all text properties to their default values for the currently selected text
        /// </summary>
        public void ResetTextProperties()
        {
            if (SelectionLength > 0 || Buffer.Length == 0)
            {
                ResetTextProperties(SelectionOffset, SelectionLength);
            }
            else
            {   // No text is selected - save properties to be applied to next typed character
                SetPendingProperties((GenericTextRunProperties)TextProperties.Default.Clone());
            }
        }

        #endregion

        #region SVG

        string[] _SVGAttributes = new[] {
            HTMLAttributes.PLACEHOLDER
        };

        public override IList<string> GetSVGAttributes(ElementContext context)
        {
            return _SVGAttributes.Concat(base.GetSVGAttributes(context)).ToList();
        }

        public override string GetSVGAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.PLACEHOLDER:
                    return Watermark;
                default:
                    return base.GetSVGAttribute(context, name);
            }
        }

        public override void SetSVGAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case HTMLAttributes.PLACEHOLDER:
                    Watermark = value;
                    break;
                default:
                    base.SetSVGAttribute(context, name, value);
                    break;
            }
        }

        public override void ResetSVGAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.PLACEHOLDER:
                    Watermark = null;
                    break;
                default:
                    base.ResetSVGAttribute(context, name);
                    break;
            }
        }

        string[] _SVGStyles = new[] {
            "label" // for backward compatibility; use "placeholder" attribute instead
        };

        public override IList<string> GetSVGStyles(ElementContext context)
        {
            return _SVGStyles.Concat(base.GetSVGStyles(context)).ToList();
        }

        public override CSSValue GetSVGStyle(ElementContext context, string name)
        {
            return base.GetSVGStyle(context, name);
        }

        public override void SetSVGStyle(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case "label": // For backward compatibility; we now use the "placeholder" attribute
                    var label = (CSSPrimitiveValue)value;
                    if (label == null)
                        Watermark = "";
                    else if (label.PrimitiveType == CSSPrimitiveType.CSS_STRING ||
                        label.PrimitiveType == CSSPrimitiveType.CSS_IDENT)
                        Watermark = label.GetStringValue();
                    break;
                default:
                    base.SetSVGStyle(context, name, value);
                    break;
            }
        }

        #endregion

        #region INavigable

        /// <summary>
        /// If you place the caret at the end of "fox" in the first line below
        /// and then press the down arrow on your keyboard, the caret moves to
        /// the end of "jumped". Now, if you press the down arrow again, the 
        /// caret does not move directly down (to after the 't'), but rather it
        /// moves to the end of the word "dog", keeping the X-position of the 
        /// caret as close as possible to the original X-position when you 
        /// started pressing the up/down arrows:
        /// 
        /// The quick brown fox
        /// jumped
        /// over the lazy dog
        /// 
        /// We implement that here by keeping track of that target X-coordinate
        /// as NavigationOffset.X. Every time a non-arrow key is pressed, we
        /// reset NavigationOffset.X to -1, and the FIRST time the up or down
        /// arrow is subsequently pressed, we set NavigationOffset.X to the x-
        /// offset of the current caret position, then each subsequent time
        /// up/down is pressed, we use NavigationOffset.X as the target 
        /// x-offset.
        /// 
        /// NavigationOffset is public so that our parent container can pass
        /// its value from one paragraph to the next when navigating between
        /// paragraphs
        /// </summary>
        public Point NavigationOffset { get; set; }

        /// <summary>
        /// Move the caret to the specified character offset.
        /// </summary>
        protected void MoveTo(int offset)
        {
            NavigationOffset = new Point(-1, -1);

            Selection = offset;
        }

        /// <summary>
        /// Move the caret up one line
        /// </summary>
        /// <returns>true on success, or false if we're already on the top line</returns>
        public virtual bool MoveUp()
        {
            if (NavigationOffset.X < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            // Move the caret up one line

            int line = Renderer.LineAt(SelectionEnd);
            if (line == 0)
            {
                return false;   // already at the top line
            }

            Selection = Renderer.CharFromDistance(line - 1, NavigationOffset.X);
            return true;
        }

        /// <summary>
        /// Move the caret down one line
        /// </summary>
        /// <returns>true on success, or false if we're already on the bottom line</returns>
        public virtual bool MoveDown()
        {
            if (NavigationOffset.X < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            // Move the caret down one line

            int line = Renderer.LineAt(SelectionEnd);
            if (line == Renderer.LineCount - 1)
            {
                return false;   // already at the bottom line
            }

            Selection = Renderer.CharFromDistance(line + 1, NavigationOffset.X);
            return true;
        }

        /// <summary>
        /// Move the caret left one character
        /// </summary>
        /// <returns>true on success or false if we're already at the start of the paragraph</returns>
        public virtual bool MoveLeft()
        {
            NavigationOffset = new Point(-1, -1);

            // If text is selected, unselect the text moving the caret to the beginning of the selection

            if (SelectionBegin != SelectionEnd)
            {
                Selection = Math.Min(SelectionBegin, SelectionEnd);
                return true;
            }

            // Move the caret left one character

            if (SelectionEnd == 0)
            {
                return false;   // already at the first character
            }

            Selection = SelectionEnd - 1;

            return true;
        }

        /// <summary>
        /// Move the caret right one character
        /// </summary>
        /// <returns>true on success or false if we're already at the end of the paragraph</returns>
        public virtual bool MoveRight()
        {
            NavigationOffset = new Point(-1, -1);

            // If text is selected, unselect the text moving the caret to the end of the selection

            if (SelectionBegin != SelectionEnd)
            {
                Selection = Math.Max(SelectionBegin, SelectionEnd);
                return true;
            }

            // Move the caret right one character

            if (SelectionEnd == Length)
            {
                return false;   // already at the last character
            }

            Selection = SelectionEnd + 1;

            return true;
        }

        /// <summary>
        /// Move the caret to the start of the paragraph
        /// </summary>
        public void MoveToStart()
        {
            NavigationOffset = new Point(-1, -1);

            Selection = 0;
        }

        /// <summary>
        /// Move the caret to the end of the paragraph
        /// </summary>
        public void MoveToEnd()
        {
            NavigationOffset = new Point(-1, -1);

            Selection = Length;
        }

        /// <summary>
        /// Move the caret to the top of the paragraph
        /// </summary>
        public void MoveToTop()
        {
            if (NavigationOffset.X < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            var point = TransformToDescendant(Renderer).Transform(NavigationOffset);

            Selection = Renderer.CharFromDistance(0, point.X);
        }

        /// <summary>
        /// Move the caret to the bottom of the paragraph
        /// </summary>
        public void MoveToBottom()
        {
            if (NavigationOffset.X < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            var point = TransformToDescendant(Renderer).Transform(NavigationOffset);

            Selection = Renderer.CharFromDistance(Renderer.LineCount - 1, point.X);
        }

        /// <summary>
        /// Move the caret to the left edge of the paragraph
        /// </summary>
        public virtual void MoveToLeft()
        {
            if (NavigationOffset.Y < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            var point = TransformToDescendant(Renderer).Transform(NavigationOffset);
            point = new Point(0, point.Y);

            int charOffset = Renderer.CharFromPoint(point);
            if (charOffset >= 0 && charOffset <= Length)
            {
                Selection = charOffset;
            }
        }

        /// <summary>
        /// Move the caret to the left edge of the paragraph
        /// </summary>
        public virtual void MoveToRight()
        {
            if (NavigationOffset.Y < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            var point = TransformToDescendant(Renderer).Transform(NavigationOffset);
            point = new Point(Renderer.Width - 1, point.Y);

            int charOffset = Renderer.CharFromPoint(point);
            if (charOffset >= 0 && charOffset <= Length)
            {
                Selection = charOffset;
            }
        }

        /// <summary>
        /// Set the end of the selection range to the specified character offset
        /// </summary>
        /// <param name="offset"></param>
        protected void SelectTo(int offset)
        {
            NavigationOffset = new Point(-1, -1);

            SelectionEnd = offset;
        }

        /// <summary>
        /// Move SelectionEnd up one line
        /// </summary>
        /// <returns></returns>
        public virtual bool SelectUp()
        {
            if (NavigationOffset.X < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            int line = Renderer.LineAt(SelectionEnd);
            if (line == 0)
            {
                return false;
            }

            SelectionEnd = Renderer.CharFromDistance(line - 1, NavigationOffset.X);
            return true;
        }

        /// <summary>
        /// Move SelectionEnd down one line
        /// </summary>
        /// <returns></returns>
        public virtual bool SelectDown()
        {
            if (NavigationOffset.X < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            int line = Renderer.LineAt(SelectionEnd);
            if (line == Renderer.LineCount - 1)
            {
                return false;
            }

            SelectionEnd = Renderer.CharFromDistance(line + 1, NavigationOffset.X);
            return true;
        }

        /// <summary>
        /// Move SelectionEnd left one character
        /// </summary>
        /// <returns></returns>
        public virtual bool SelectLeft()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionEnd == 0)
            {
                return false;
            }

            SelectionEnd = SelectionEnd - 1;

            return true;

        }

        /// <summary>
        /// Move SelectionEnd right one character
        /// </summary>
        /// <returns></returns>
        public virtual bool SelectRight()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionEnd == Length)
            {
                return false;
            }

            SelectionEnd = SelectionEnd + 1;

            return true;
        }

        /// <summary>
        /// Move SelectionEnd to the start of the paragraph
        /// </summary>
        public void SelectToStart()
        {
            SelectionEnd = 0;
        }

        /// <summary>
        /// Move SelectionEnd to the end of the paragraph
        /// </summary>
        public void SelectToEnd()
        {
            SelectionEnd = Length;
        }

        /// <summary>
        /// Move SelectionEnd to the top of the paragraph
        /// </summary>
        public void SelectToTop()
        {
            if (NavigationOffset.X < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            var point = TransformToDescendant(Renderer).Transform(NavigationOffset);

            SelectionEnd = Renderer.CharFromDistance(0, point.X);
        }

        /// <summary>
        /// Move SelectionEnd to the bottom of the paragraph
        /// </summary>
        public void SelectToBottom()
        {
            if (NavigationOffset.X < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            var point = TransformToDescendant(Renderer).Transform(NavigationOffset);

            SelectionEnd = Renderer.CharFromDistance(Renderer.LineCount - 1, point.X);
        }

        /// <summary>
        /// Move SelectionEnd to the left edge of the paragraph
        /// </summary>
        public virtual void SelectToLeft()
        {
            if (NavigationOffset.Y < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            var point = TransformToDescendant(Renderer).Transform(NavigationOffset);
            point = new Point(0, point.Y);

            int charOffset = Renderer.CharFromPoint(point);
            if (charOffset >= 0 && charOffset <= Length)
            {
                SelectionEnd = charOffset;
            }
        }

        /// <summary>
        /// Move SelectionEnd to the right edge of the paragraph
        /// </summary>
        public virtual void SelectToRight()
        {
            if (NavigationOffset.Y < 0)
            {
                NavigationOffset = Renderer.GetCharPosition(SelectionEnd);
            }

            var point = TransformToDescendant(Renderer).Transform(NavigationOffset);
            point = new Point(Renderer.Width - 1, point.Y);

            int charOffset = Renderer.CharFromPoint(point);
            if (charOffset >= 0 && charOffset <= Length)
            {
                SelectionEnd = charOffset;
            }
        }

        /// <summary>
        /// Handle a Tab keypress
        /// </summary>
        /// <returns></returns>
        public bool TabForward()
        {
            using (new UndoScope(UndoStack, "Tab forward"))
            {
                if (SelectionOffset == 0)
                {
                    // If tab pressed at start of paragraph, indent entire paragraph
                    NFormattingCommands.IncreaseIndentation.Execute(null, this);
                }
                else if (SelectedText.Contains('\n'))
                {
                    IndentText(SelectionOffset, SelectionLength);
                }
                else
                {
                    Delete();
                    Insert(SelectionOffset, "\t");
                }
            }

            return true;
        }

        /// <summary>
        /// Handle a Shift+Tab keypress
        /// </summary>
        /// <returns></returns>
        public bool TabBackward()
        {
            using (new UndoScope(UndoStack, "Tab backward"))
            {
                if (SelectionOffset == 0)
                {
                    // If Shift+Tab pressed at start of paragraph, outdent entire paragraph
                    NFormattingCommands.DecreaseIndentation.Execute(null, this);
                }
                else if (SelectedText.Contains('\n'))
                {
                    OutdentText(SelectionOffset, SelectionLength);
                }
                else
                {
                    Delete();
                    Insert(SelectionOffset, "\t");
                }
            }

            return true;
        }

        private void IndentText(int offset, int length)
        {
            string indent = "\t";

            int lineStart = Buffer.LastIndexOf('\n', offset) + 1;
            while (lineStart < offset + length)
            {
                Insert(lineStart, indent);
                if (lineStart >= offset)
                {
                    length += indent.Length;
                }

                lineStart = Buffer.IndexOf('\n', lineStart) + 1;
                if (lineStart == 0)
                {
                    break;
                }
            }
        }

        private void OutdentText(int offset, int length)
        {
            int lineStart = Buffer.LastIndexOf('\n', offset) + 1;
            while (lineStart < offset + length)
            {
                // Remove tabs and spaces from start of line until we've removed
                // at least TabSize equivalent of whitespace.

                string lineStartText = Buffer.Substring(lineStart, Math.Min(TabSize, Buffer.Length - lineStart));

                int deleteCount = 0;    // Number of characters to delete
                int spaceCount = 0;     // Whitespace equivalent

                for (int i = 0; i < lineStartText.Length && spaceCount < TabSize; i++)
                {
                    if (lineStartText[i] == '\t')
                    {
                        deleteCount += 1;
                        spaceCount += TabSize;
                    }
                    else if (lineStartText[i] == ' ')
                    {
                        deleteCount += 1;
                        spaceCount += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                Delete(lineStart, deleteCount);
                if (lineStart >= offset)
                {
                    length -= deleteCount;
                }

                // If we've deleted too much whitespace, add some spaces

                if (spaceCount > TabSize)
                {
                    string spaces = new string(' ', spaceCount - TabSize);

                    Insert(lineStart, spaces);
                    if (lineStart >= offset)
                    {
                        length += spaces.Length;
                    }
                }

                lineStart = Buffer.IndexOf('\n', lineStart) + 1;
                if (lineStart == 0)
                {
                    break;
                }
            }
        }

        #endregion

        #region IFormattable

        /// <summary>
        /// Determine if this element has a format property with the given name
        /// </summary>
        public override bool HasProperty(string name)
        {
            return HasParagraphProperty(name) || TextProperties.Default.HasProperty(name);
        }

        /// <summary>
        /// Set a format property value for this element
        /// 
        /// This operation is undo-able.
        /// </summary>
        public override void SetProperty(string name, object value)
        {
            if (HasParagraphProperty(name))
            {
                SetParagraphProperty(name, value);
            }

            if (TextProperties.Default.HasProperty(name))
            {
                SetTextProperty(name, value);
            }
        }

        /// <summary>
        /// Get a format property value for this element
        /// </summary>
        public override object GetProperty(string name)
        {
            if (HasParagraphProperty(name))
            {
                return GetParagraphProperty(name);
            }

            if (TextProperties.Default.HasProperty(name))
            {
                return GetTextProperty(name);
            }

            return null;
        }

        /// <summary>
        /// Reset all format properties.
        /// 
        /// This operation is undo-able.
        /// </summary>
        public override void ResetProperties()
        {
            // *Any* of the paragraph can be selected to *set* its 
            // properties, but *all* of the paragraph must be selected to 
            // *clear* its properties. This results in a more
            // natural behavior from the user's perspective.

            ResetTextProperties();

            if (SelectionLength >= Length)
            {
                ResetParagraphProperties();
            }
        }

        /// <summary>
        /// Change the value for a given property.
        /// 
        /// This operation is undo-able.
        /// </summary>
        public override int ChangeProperty(string name, object oldValue, object newValue)
        {
            int result = 0;

            if (HasParagraphProperty(name))
            {
                result += ChangeParagraphProperty(name, oldValue, newValue);
            }

            if (TextProperties.Default.HasProperty(name))
            {
                result += ChangeTextProperty(name, oldValue, newValue);
            }

            return result;
        }

        public virtual TextParagraphProperties GetParagraphProperties()
        {
            var result = new GenericTextParagraphProperties();
            result.SetAlwaysCollapsible(AlwaysCollapsible);
            result.SetDefaultIncrementalTab(DefaultIncrementalTab);
            result.SetDefaultTextRunProperties(DefaultTextRunProperties);
            result.SetFlowDirection(FlowDirection);
            result.SetIndent(Indent);
            result.SetLineHeight(LineHeight);
            result.SetParagraphIndent(ParagraphIndent);
            result.SetTabs(Tabs);
            result.SetTextAlignment(TextAlignment);
            result.SetTextDecorations(TextDecorations);
            result.SetTextMarkerProperties(TextMarkerProperties);
            result.SetTextWrapping(TextWrapping);
            return result;
        }

        public virtual void SetParagraphProperties(TextParagraphProperties properties)
        {
            AlwaysCollapsible = properties.AlwaysCollapsible;
            DefaultIncrementalTab = properties.DefaultIncrementalTab;
            FlowDirection = properties.FlowDirection;
            Indent = properties.Indent;
            LineHeight = properties.LineHeight;
            ParagraphIndent = properties.ParagraphIndent;
            Tabs = properties.Tabs;
            TextAlignment = properties.TextAlignment;
            TextDecorations = properties.TextDecorations;
            TextMarkerProperties = properties.TextMarkerProperties;
            TextWrapping = properties.TextWrapping;
        }

        public const string AlwaysCollapsiblePropertyName = "AlwaysCollapsible";
        public const string DefaultIncrementalTabPropertyName = "DefaultIncrementalTab";
        public const string DefaultTextRunPropertiesPropertyName = "DefaultTextRunProperties";
        public const string FlowDirectionPropertyName = "FlowDirection";
        public const string IndentPropertyName = "Indent";
        public const string LineHeightPropertyName = "LineHeight";
        public const string LineSpacingPropertyName = "LineSpacing";
        public const string ParagraphIndentPropertyName = "ParagraphIndent";
        public const string TabsPropertyName = "Tabs";
        public const string TextAlignmentPropertyName = "TextAlignment";
        public const string TextDecorationsPropertyName = "TextDecorations";
        public const string TextMarkerPropertiesPropertyName = "TextMarkerProperties";
        public const string TextWrappingPropertyName = "TextWrapping";

        protected bool HasParagraphProperty(string name)
        {
            switch (name)
            {
                case AlwaysCollapsiblePropertyName:
                case DefaultIncrementalTabPropertyName:
                case DefaultTextRunPropertiesPropertyName:
                case FlowDirectionPropertyName:
                case IndentPropertyName:
                case LineHeightPropertyName:
                case LineSpacingPropertyName:
                case ParagraphIndentPropertyName:
                case TabsPropertyName:
                case TextAlignmentPropertyName:
                case TextDecorationsPropertyName:
                case TextMarkerPropertiesPropertyName:
                case TextWrappingPropertyName:
                    return true;
                default:
                    return false;
            }
        }

        protected void SetParagraphProperty(string name, object value)
        {
            switch (name)
            {
                case AlwaysCollapsiblePropertyName:
                    AlwaysCollapsible = SafeConvert.ToBool(value, false).Value;
                    break;
                case DefaultIncrementalTabPropertyName:
                    DefaultIncrementalTab = SafeConvert.ToDouble(value);
                    break;
                case DefaultTextRunPropertiesPropertyName:
                    DefaultTextRunProperties = value as TextRunProperties;
                    break;
                case FlowDirectionPropertyName:
                    FlowDirection = SafeConvert.ToFlowDirection(value);
                    break;
                case IndentPropertyName:
                    Indent = SafeConvert.ToDouble(value);
                    break;
                case LineHeightPropertyName:
                    LineHeight = SafeConvert.ToDouble(value);
                    break;
                case LineSpacingPropertyName:
                    LineSpacing = SafeConvert.ToDouble(value, 1.0);
                    break;
                case ParagraphIndentPropertyName:
                    ParagraphIndent = SafeConvert.ToDouble(value);
                    break;
                case TabsPropertyName:
                    Tabs = value as IList<TextTabProperties>;
                    break;
                case TextAlignmentPropertyName:
                    TextAlignment = SafeConvert.ToTextAlignment(value);
                    break;
                case TextDecorationsPropertyName:
                    TextDecorations = SafeConvert.ToTextDecorations(value);
                    break;
                case TextMarkerPropertiesPropertyName:
                    TextMarkerProperties = value as TextMarkerProperties;
                    break;
                case TextWrappingPropertyName:
                    TextWrapping = SafeConvert.ToTextWrapping(value);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Get a format property value for this element
        /// </summary>
        protected object GetParagraphProperty(string name)
        {
            switch (name)
            {
                case AlwaysCollapsiblePropertyName:
                    return AlwaysCollapsible;
                case DefaultIncrementalTabPropertyName:
                    return DefaultIncrementalTab;
                case DefaultTextRunPropertiesPropertyName:
                    return DefaultTextRunProperties;
                case FlowDirectionPropertyName:
                    return FlowDirection;
                case IndentPropertyName:
                    return Indent;
                case LineHeightPropertyName:
                    return LineHeight;
                case LineSpacingPropertyName:
                    return LineSpacing;
                case ParagraphIndentPropertyName:
                    return ParagraphIndent;
                case TabsPropertyName:
                    return Tabs;
                case TextAlignmentPropertyName:
                    return TextAlignment;
                case TextDecorationsPropertyName:
                    return TextDecorations;
                case TextMarkerPropertiesPropertyName:
                    return TextMarkerProperties;
                case TextWrappingPropertyName:
                    return TextWrapping;
                default:
                    return null;
            }
        }

        protected void ResetParagraphProperties()
        {

        }

        /// <summary>
        /// Change the value for a given property.
        /// 
        /// This operation is undo-able.
        /// </summary>
        protected int ChangeParagraphProperty(string name, object oldValue, object newValue)
        {
            return 0;
        }

        #endregion

        #region IEditable

        /// <summary>
        /// See IEditable.cs
        /// </summary>

        public override IList<object> Cut()
        {
            if (SelectionLength == Length)
            {
                return new object[] { this };
            }

            var text = Cut(SelectionOffset, SelectionLength);
            var fragment = new TextFragment(text);

            return new object[] { fragment };
        }

        public override IList<object> Copy()
        {
            if (SelectionLength == Length)
            {
                return new object[] { Clone() };
            }

            var text = Copy(SelectionOffset, SelectionLength);
            var fragment = new TextFragment(text);

            return new object[] { fragment };
        }

        public override IList<object> Paste(IList<object> items)
        {
            var results = new List<object>(items);

            foreach (object item in items)
            {
                if (item is string)
                {
                    string text = (string)item;
                    results.Remove(text);
                    Replace(text);
                }

                if (item is TextRunSource)
                {
                    var run = (TextRunSource)item;
                    results.Remove(run);
                    Replace(run.Text);
                    SetTextProperties(run.Properties);
                }
                else if (item is TextBuffer)
                {
                    var text = (TextBuffer)item;
                    results.Remove(text);
                    Paste(SelectionOffset, text);
                }
                else if (item is TextFragment)
                {
                    var fragment = (TextFragment)item;
                    results.Remove(item);
                    Paste(SelectionOffset, fragment.Buffer);
                }
                else if (item is TextField)
                {
                    var paragraph = (TextField)item;
                    results.Remove(paragraph);
                    Paste(SelectionOffset, paragraph.Buffer);
                    SetParagraphProperties(paragraph.GetParagraphProperties());
                    break;
                }
                else
                {
                    break;
                }
            }

            // If we pasted into the middle of the paragraph, we need to
            // split the paragraph and move the latter portion to the end
            // of the paste buffer

            if (SelectionOffset < Length && results.Count > 0)
            {
                var remainder = Split();
                var last = results.Last() as ITextElement;
                if (last == null || !last.Merge(remainder))
                {
                    results.Add(remainder);
                }
            }

            return results.ToArray();
        }

        public override bool Delete()
        {
            Delete(SelectionOffset, SelectionLength);

            return (Length != 0);
        }

        #endregion

        #region ISearchable

        /// <summary>
        ///  Text that should be highlighted for the current Find() operation
        /// </summary>
        string _FindText = "";

        /// <summary>
        /// This contains the list of offsets of all strings found by the
        /// most recent call to the Find function. These are passed to
        /// DrawFindResults() to draw a yellow background highlighting them
        /// </summary>
        LinkedList<int> _SearchResults;

        /// <summary>
        /// This is the background highlighting search results
        /// </summary>
        DrawingVisual _SearchResultsDrawing;

        /// <summary>
        /// Select the first instance of the given pattern
        /// </summary>
        public bool FindFirst(string pattern, RegexOptions options)
        {
            var result = Buffer.FindFirst(pattern, options);
            if (result.Item1 == -1)
            {
                return false;
            }

            SetSelection(result.Item1, result.Item1 + result.Item2);

            return true;
        }

        /// <summary>
        /// Select the last instance of the given pattern
        /// </summary>
        public bool FindLast(string pattern, RegexOptions options)
        {
            var result = Buffer.FindLast(pattern, options);
            if (result.Item1 == -1)
            {
                return false;
            }

            SetSelection(result.Item1 + result.Item2, result.Item1);

            return true;
        }

        /// <summary>
        /// Select the next instance of the given pattern
        /// </summary>
        public bool FindNext(string pattern, RegexOptions options)
        {
            int startIndex = Math.Max(SelectionBegin, SelectionEnd);
            if (startIndex == Length)
            {
                return false;
            }

            var result = Buffer.FindFirst(pattern, options, startIndex);
            if (result.Item1 == -1)
            {
                return false;
            }

            SetSelection(result.Item1, result.Item1 + result.Item2);

            return true;
        }

        /// <summary>
        /// Select the previous instance of the given pattern
        /// </summary>
        public bool FindPrevious(string pattern, RegexOptions options)
        {
            int startIndex = Math.Min(SelectionBegin, SelectionEnd) - 1;
            if (startIndex < 0)
            {
                return false;
            }

            var result = Buffer.FindLast(pattern, options, startIndex);
            if (result.Item1 == -1)
            {
                return false;
            }

            SetSelection(result.Item1 + result.Item2, result.Item1);

            return true;
        }

        ///<summary>
        /// Determine the string comparieson type we should use for find operations:
        /// 
        /// If the target string contains any upper-case characters, do a case-
        /// sensitive search; otherwise do a case-insensitive search.
        ///</summary>
        StringComparison AutoComparisonType(string s)
        {
            if (s == s.ToLower())
            {
                return StringComparison.CurrentCultureIgnoreCase;
            }
            else
            {
                return StringComparison.CurrentCulture;
            }
        }

        /// <summary>
        /// Highlight all sub-strings matching the given string
        /// </summary>
        /// <param name="text"></param>
        /// <returns>The number of matched strings</returns>
        public int Find(string text)
        {
            // If new string starts with the previously string, do a (faster) incremental search

            if (!String.IsNullOrEmpty(_FindText) &&
                text.StartsWith(_FindText) &&
                text.Length > _FindText.Length)
            {
                return IncrementalFind(text);
            }

            if (_SearchResults == null)
            {
                _SearchResults = new LinkedList<int>();
            }

            _SearchResults.Clear();

            if (!String.IsNullOrEmpty(text))
            {
                // Determine comparison type (case-sensitive vs. insensitive)

                StringComparison comparisonType = AutoComparisonType(text);

                // Now actually find all matches and save them in findResults

                int index = Buffer.IndexOf(text, comparisonType);
                while (index != -1)
                {
                    _SearchResults.AddLast(index);

                    index += text.Length;
                    if (index < Buffer.Length)
                    {
                        index = Buffer.IndexOf(text, index, comparisonType);
                    }
                    else
                    {
                        index = -1;
                    }
                }
            }

            // Save the text for incremental searching

            _FindText = text;

            // Draw a background highlighting the results

            UpdateSearchResults(this, EventArgs.Empty);

            return _SearchResults.Count;
        }

        /// <summary>
        /// Find all sub-strings that match the given string.
        /// 
        /// This is called by Find() when then newly-specified text is
        /// simply the previously-specified text with characters appended.
        /// </summary>
        /// <param name="matchText">The string to match against</param>
        /// <returns>The number of matched strings</returns>
        private int IncrementalFind(string matchText)
        {
            // Determine comparison type (case-sensitive vs. insensitive)

            var comparisonType = AutoComparisonType(matchText);

            // Update _FindResults

            IncrementalFind(Buffer, matchText, comparisonType, _SearchResults);

            // Save matchText for incremental searching

            _FindText = matchText;

            // Draw a background highlighting the results

            UpdateSearchResults(this, EventArgs.Empty);

            return _SearchResults.Count;
        }

        /// <summary>
        /// Incrementally find all sub-strings that match the given string.
        /// 
        /// For each index in findResults, we determine if the substring at
        /// that index matches the given text and if not we remove its index
        /// from findResults.
        /// </summary>
        /// <param name="buffer">Text buffer to be searched</param>
        /// <param name="matchText">Substring to match against</param>
        /// <param name="comparisonType">String comparison type</param>
        /// <param name="findResults">
        ///     Indices of the matching substrings from the previous call to Find() or IncrementalFind()
        /// </param>
        private static void IncrementalFind(
            TextBuffer buffer,
            string matchText,
            StringComparison comparisonType,
            LinkedList<int> findResults)
        {
            var currentResult = findResults.First;

            while (currentResult != null)
            {
                var nextResult = currentResult.Next;

                int startIndex = currentResult.Value;
                int length = Math.Min(matchText.Length, buffer.Length - startIndex);
                string substr = buffer.Substring(startIndex, length);

                if (!substr.Equals(matchText, comparisonType))
                {
                    findResults.Remove(currentResult);
                }

                currentResult = nextResult;
            }
        }

        /// <summary>
        /// Select the first result of the most recent call to Find()
        /// </summary>
        /// <returns></returns>
        public bool FindFirst()
        {
            var comparisonType = AutoComparisonType(_FindText);

            int index = Buffer.IndexOf(_FindText, comparisonType);
            if (index == -1)
            {
                return false;
            }

            SetSelection(index, index + _FindText.Length);

            return true;
        }

        /// <summary>
        /// Select the last result of the most recent call to Find()
        /// </summary>
        /// <returns></returns>
        public bool FindLast()
        {
            var comparisonType = AutoComparisonType(_FindText);

            int index = Buffer.Text.LastIndexOf(_FindText, comparisonType);
            if (index == -1)
            {
                return false;
            }

            SetSelection(index, index + _FindText.Length);

            return true;
        }

        /// <summary>
        /// Select the next result of the most recent call to Find()
        /// </summary>
        /// <returns></returns>
        public bool FindNext()
        {
            var comparisonType = AutoComparisonType(_FindText);

            int startIndex = SelectionOffset + SelectionLength;
            startIndex = Math.Min(startIndex, Length);
            startIndex = Math.Max(startIndex, 0);

            int index = Buffer.IndexOf(_FindText, startIndex, comparisonType);
            if (index == -1)
            {
                return false;
            }

            SetSelection(index, index + _FindText.Length);

            return true;
        }

        /// <summary>
        /// Select the previous result of the most recent call to Find()
        /// </summary>
        /// <returns></returns>
        public bool FindPrevious()
        {
            int startIndex = SelectionOffset;
            startIndex = Math.Min(startIndex, Length - 1);
            startIndex = Math.Max(startIndex, 0);

            if (--startIndex == -1)
            {
                return false;
            }

            var comparisonType = AutoComparisonType(_FindText);

            int index = Buffer.Text.LastIndexOf(_FindText, startIndex, comparisonType);
            if (index == -1)
            {
                return false;
            }

            SetSelection(index, index + _FindText.Length);

            return true;
        }

        /// <summary>
        /// Redraw the background highlighting search results
        /// </summary>
        protected void UpdateSearchResults(object sender, EventArgs e)
        {
            if (_SearchResults == null || _SearchResults.Count == 0)
            {
                // No results - remove the visual

                if (_SearchResultsDrawing != null)
                {
                    RemoveVisual(_SearchResultsDrawing);
                    _SearchResultsDrawing = null;
                }
                return;
            }

            // Create a drawing visual if needed

            if (_SearchResultsDrawing == null)
            {
                _SearchResultsDrawing = new DrawingVisual();
                AddVisual(_SearchResultsDrawing, -1000);
            }

            // Now actually draw the highlighting

            using (var dc = _SearchResultsDrawing.RenderOpen())
            {
                dc.PushTransform(new TranslateTransform(Renderer.Offset.X, Renderer.Offset.Y));

                foreach (int findResult in _SearchResults)
                {
                    var geometry = Renderer.GetSelectionBounds(findResult, findResult + _FindText.Length);
                    dc.DrawGeometry(SearchResultsBrush, null, geometry);
                }
            }
        }

        #endregion

        #region ITextElement

        /// <summary>
        ///  Determine if the selection is not empty
        /// </summary>
        public virtual bool IsTextSelected
        {
            get { return (SelectionLength > 0); }
        }

        /// <summary>
        /// Get the selected text if any
        /// </summary>
        public virtual string SelectedText
        {
            get { return Text.Substring(SelectionOffset, SelectionLength); }
        }

        /// <summary>
        /// Insert a string at the end of the current selection
        /// </summary>
        /// <param name="value"></param>
        public void Insert(string value)
        {
            int offset = SelectionEnd;
            int length = value.Length;
            var properties = GetPendingProperties();

            Insert(SelectionEnd, value);

            // Apply any pending properties
            if (properties != null)
            {
                SetTextProperties(offset, length, properties);
                SetPendingProperties(null);
            }
        }

        /// <summary>
        /// Replace the currently-selected text with the given string
        /// </summary>
        /// <param name="value"></param>
        public void Replace(string value)
        {
            int selectionOffset = SelectionOffset;

            Delete();
            Insert(value);

            SelectionBegin = selectionOffset;
            SelectionEnd = selectionOffset + value.Length;
        }

        /// <summary>
        /// Replace all instances of oldValue with newValue within the
        /// currently-selected text
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public int Replace(string oldValue, string newValue)
        {
            int count = 0;

            int index = Text.IndexOf(oldValue, SelectionOffset, SelectionLength);
            while (index != -1)
            {
                Delete(index, oldValue.Length);
                Insert(index, newValue);

                index += newValue.Length;
                count += 1;

                int remainingLength = SelectionLength - (index - SelectionOffset);
                remainingLength = Math.Min(remainingLength, Text.Length - index);
                remainingLength = Math.Max(remainingLength, 0);

                index = Text.IndexOf(oldValue, index, remainingLength);
            }

            return count;
        }

        /// <summary>
        /// Move the caret left one word
        /// </summary>
        /// <returns>true on success or false if we're at the start of the paragraph</returns>
        public virtual bool MoveLeftByWord()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionBegin != SelectionEnd)
            {
                Selection = Math.Min(SelectionBegin, SelectionEnd);
                return true;
            }

            if (SelectionEnd == 0)
            {
                return false;
            }

            Selection = Buffer.GetWordStart(SelectionEnd - 1);

            return true;
        }

        /// <summary>
        /// Move the caret right one word
        /// </summary>
        /// <returns>true on success or false if we're at the end of the paragraph</returns>
        public virtual bool MoveRightByWord()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionBegin != SelectionEnd)
            {
                Selection = Math.Max(SelectionBegin, SelectionEnd);
                return true;
            }

            if (SelectionEnd == Length)
            {
                return false;
            }

            Selection = Buffer.GetWordEnd(SelectionEnd + 1);

            return true;
        }

        /// <summary>
        /// Move caret to the start of the current line
        /// </summary>
        /// <returns>true on success or false if we're at the start of a paragraph</returns>
        public virtual bool MoveToLineStart()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionBegin != SelectionEnd)
            {
                Selection = Math.Min(SelectionBegin, SelectionEnd);
                return true;
            }

            if (SelectionEnd == 0)
            {
                return false;
            }

            int lineIndex = Renderer.LineAt(SelectionEnd - 1);

            Selection = Renderer.LineOffset(lineIndex);

            return true;
        }

        /// <summary>
        /// Move caret to the end of the current line
        /// </summary>
        /// /// <returns>true on success or false if we're at the end of the paragraph</returns>
        public virtual bool MoveToLineEnd()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionBegin != SelectionEnd)
            {
                Selection = Math.Max(SelectionBegin, SelectionEnd);
                return true;
            }

            if (SelectionEnd == Length)
            {
                return false;
            }

            int lineIndex = Renderer.LineAt(SelectionEnd + 1);
            int lineEnd = Renderer.LineOffset(lineIndex) + Renderer.LineMetrics(lineIndex).Length - 1;

            Selection = Math.Max(lineEnd, 0);

            return true;
        }

        /// <summary>
        /// Move caret to the start of the current paragraph
        /// </summary>
        /// /// <returns>true on success or false if we're at the start of the paragraph</returns>
        public virtual bool MoveToParagraphStart()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionBegin != SelectionEnd)
            {
                Selection = Math.Min(SelectionBegin, SelectionEnd);
                return true;
            }

            if (SelectionEnd == 0)
            {
                return false;
            }

            Selection = 0;

            return true;
        }

        /// <summary>
        /// Move caret to the end of the current paragraph
        /// </summary>
        /// <returns>true on success or false if we're at the end of the paragraph</returns>
        public virtual bool MoveToParagraphEnd()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionBegin != SelectionEnd)
            {
                Selection = Math.Max(SelectionBegin, SelectionEnd);
                return true;
            }

            if (SelectionEnd == Length)
            {
                return false;
            }

            Selection = Length;

            return true;
        }

        /// <summary>
        /// Move SelectionEnd left one word
        /// </summary>
        /// <returns></returns>
        /// <returns>true on success or false if we're at the start of the paragraph</returns>
        public virtual bool SelectLeftByWord()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionEnd == 0)
            {
                return false;
            }

            SelectionEnd = Buffer.GetWordStart(SelectionEnd - 1);

            return true;
        }

        /// <summary>
        /// Move SelectionEnd right one word
        /// </summary>
        /// <returns>true on success or false if we're at the end of the paragraph</returns>
        public virtual bool SelectRightByWord()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionEnd == Length)
            {
                return false;
            }

            SelectionEnd = Buffer.GetWordEnd(SelectionEnd + 1);

            return true;
        }

        /// <summary>
        /// Move SelectionEnd to the start of the current line
        /// </summary>
        /// <returns>true on success or false if we're at the start of the paragraph</returns>
        public virtual bool SelectToLineStart()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionEnd == 0)
            {
                return false;
            }

            int lineIndex = Renderer.LineAt(SelectionEnd - 1);

            SelectionEnd = Renderer.LineOffset(lineIndex);

            return true;
        }

        /// <summary>
        /// Move SelectionEnd to the end of the current line
        /// </summary>
        /// <returns>true on success or false if we're at the end of the paragraph</returns>
        public virtual bool SelectToLineEnd()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionEnd == Length)
            {
                return false;
            }

            int lineIndex = Renderer.LineAt(SelectionEnd + 1);
            int lineEnd = Renderer.LineOffset(lineIndex) + Renderer.LineMetrics(lineIndex).Length;

            // Note: CharRangeFromLine() counts end-of-paragraph as a character

            SelectionEnd = Math.Min(lineEnd, this.Length);

            return true;
        }

        /// <summary>
        /// Move SelectionEnd to the start of the current paragraph
        /// </summary>
        public virtual bool SelectToParagraphStart()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionEnd == 0)
            {
                return false;
            }

            SelectionEnd = 0;

            return true;
        }

        /// <summary>
        /// Move SelectionEnd to the end of the current paragraph
        /// </summary>
        public virtual bool SelectToParagraphEnd()
        {
            NavigationOffset = new Point(-1, -1);

            if (SelectionEnd == Length)
            {
                return false;
            }

            SelectionEnd = Length;

            return true;
        }

        /// <summary>
        /// Delete one character backward (i.e., handle backspace keystroke)
        /// </summary>
        /// <returns>
        /// true on success, or false if caret is at beginning of paragraph
        /// </returns>
        public virtual bool DeleteBack()
        {
            if (SelectionLength > 0)
            {
                // backspace pressed while text selected
                Delete(SelectionOffset, SelectionLength);
                return true;
            }
            else if (CaretIndex > 0)
            {
                // backspace pressed with a character to delete
                Delete(CaretIndex - 1, 1);
                return true;
            }
            else
            {
                // backspace pressed at beginning of paragraph
                return false;
            }
        }

        /// <summary>
        /// Delete one word backward
        /// </summary>
        /// <returns></returns>
        public virtual bool DeleteBackByWord()
        {
            if (SelectionBegin != SelectionEnd)
            {
                // text selected
                Delete(SelectionOffset, SelectionLength);
                return true;
            }
            else if (SelectionEnd == 0)
            {
                // beginning of paragraph
                return false;
            }

            int index = Buffer.GetWordStart(SelectionEnd - 1);

            Delete(index, SelectionEnd - index);

            return true;
        }

        /// <summary>
        /// Delete one pagragraph backward
        /// </summary>
        /// <returns></returns>
        public virtual bool DeleteBackByParagraph()
        {
            if (SelectionBegin != SelectionEnd)
            {
                // text selected
                Delete(SelectionOffset, SelectionLength);
                return true;
            }
            else if (SelectionEnd == 0)
            {
                // beginning of paragraph
                return false;
            }

            Delete(0, SelectionEnd);

            return true;
        }

        /// <summary>
        /// Delete one character forward
        /// </summary>
        /// <returns></returns>
        public virtual bool DeleteForward()
        {
            if (SelectionBegin != SelectionEnd)
            {
                // delete pressed while text selected
                Delete(SelectionOffset, SelectionLength);
                return true;
            }
            else if (SelectionEnd < Buffer.Length)
            {
                // delete pressed with a character to delete
                Delete(SelectionEnd, 1);
                return true;
            }
            else
            {
                // delete pressed at end of paragraph
                return false;
            }
        }

        /// <summary>
        /// Delete one word forward
        /// </summary>
        /// <returns></returns>
        public virtual bool DeleteForwardByWord()
        {
            if (SelectionBegin != SelectionEnd)
            {
                // text selected
                Delete(SelectionOffset, SelectionLength);
                return true;
            }
            else if (SelectionEnd == Length)
            {
                // end of paragraph
                return false;
            }

            int index = Buffer.GetWordEnd(SelectionEnd + 1);

            Delete(SelectionEnd, index - SelectionEnd);

            return true;
        }

        /// <summary>
        /// Delete one paragraph forward
        /// </summary>
        /// <returns></returns>
        public virtual bool DeleteForwardByParagraph()
        {
            if (SelectionBegin != SelectionEnd)
            {
                // text selected
                Delete(SelectionOffset, SelectionLength);
                return true;
            }
            else if (SelectionEnd == Buffer.Length)
            {
                // end of paragraph
                return false;
            }

            Delete(SelectionEnd, Length - SelectionEnd);

            return true;
        }

        /// <summary>
        /// Insert a line break
        /// </summary>
        /// <returns></returns>
        public virtual bool EnterLineBreak()
        {
            using (new UndoScope(UndoStack, "Enter line break"))
            {
                Insert(SelectionOffset, "\n");

                if (AutoIndent && SelectionOffset > 1)
                {
                    int lineEnd = SelectionOffset - 2;
                    int lineBegin = Buffer.LastIndexOf('\n', lineEnd) + 1;
                    if (lineBegin != 0 && lineBegin <= lineEnd)
                    {
                        string lineText = Buffer.Substring(lineBegin, 1 + lineEnd - lineBegin);
                        var leadingWhitespace = lineText.TakeWhile(Char.IsWhiteSpace).ToArray();
                        Insert(SelectionOffset, new String(leadingWhitespace));
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Insert a paragraph break
        /// </summary>
        /// <returns></returns>
        public virtual bool EnterParagraphBreak()
        {
            return false;
        }

        /// <summary>
        /// Split this paragraph in two at the current selection offset
        /// </summary>
        /// <returns></returns>
        public virtual ITextElement Split()
        {
            var result = (TextField)this.Clone();
            Delete(SelectionOffset, Length - SelectionOffset);
            result.Delete(0, SelectionOffset);
            return result;
        }

        /// <summary>
        /// Merge two ITextElement objects
        /// </summary>
        /// <param name="_other"></param>
        /// <returns></returns>
        public virtual bool Merge(ITextElement other)
        {
            if (other is TextField)
            {
                return MergeTextField((TextField)other);
            }
            else
            {
                return false;
            }
        }

        private bool MergeTextField(TextField other)
        {
            Paste(Length, other.Buffer);
            Selection = Length - other.Length;
            return true;
        }


        #endregion

        #region ISelectable

        /// <summary>
        /// Select this element
        /// </summary>
        public override void Select()
        {
            base.Select();

            IsSelectionVisible = true;
        }

        /// <summary>
        /// Unselect this element
        /// </summary>
        public override void Unselect()
        {
            base.Unselect();

            SelectionLength = 0;

            IsSelectionVisible = false;
        }

        #endregion

        #region IMovable

        public override void MoveCompleted()
        {
            Positioning = Positioning.Static;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Called when the range of selected text changes
        /// </summary>
        protected virtual void OnSelectionChanged()
        {
            UpdateSelection(this, EventArgs.Empty);

            RaiseSelectionChanged();
        }

        /// <summary>
        /// Called when a format property changes
        /// </summary>
        protected virtual void OnSelectionFormatChanged()
        {
            RaiseFormatChanged();
        }

        #endregion

        #region Implementation

        #region Caret

        /// <summary>
        /// Update caret's visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateCaretVisibility(object sender, EventArgs e)
        {
            if (IsKeyboardFocused)
            {
                // Show the caret
                AddVisual(_Caret, 1);
                _Caret.IsEnabled = true;
            }
            else
            {
                // Hide the caret
                _Caret.IsEnabled = false;
                RemoveVisual(_Caret);
            }
        }

        /// <summary>
        /// Update caret's size/position
        /// </summary>
        private void UpdateCaretPosition(object sender, EventArgs e)
        {
            Rect rect = Renderer.GetCaretBounds(CaretIndex);

            if (!rect.IsEmpty)
            {
                rect = Renderer.TransformToAncestor(this).TransformBounds(rect);
                _Caret.Offset = new Vector(rect.X, rect.Y);
                _Caret.Height = rect.Height;
            }
        }

        private void KeyDown_Caret(object sender, KeyEventArgs e)
        {
            if (_Caret.IsEnabled &&
                e.Key != Key.LeftShift && e.Key != Key.RightShift &&
                e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
                e.Key != Key.LeftAlt && e.Key != Key.RightAlt)
            {
                _Caret.Opacity = 1;
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// Set SelectionBegin and SelectionEnd to the same value
        /// </summary>
        protected int Selection
        {
            set { SetSelection(value, value); }
        }

        /// <summary>
        /// Set the range of currently-selected characters.
        /// 
        /// This operation is undo-able.
        /// </summary>
        protected void SetSelection(int newSelectionBegin, int newSelectionEnd)
        {
            if (newSelectionBegin == _SelectionBegin && newSelectionEnd == _SelectionEnd)
            {
                return;
            }

            // Update undo stack

            using (var undo = new UndoScope(UndoStack, "Select Text", isEditing: false))
            {
                int oldSelectionBegin = _SelectionBegin;
                int oldSelectionEnd = _SelectionEnd;
                undo.Push(() => SetSelection(oldSelectionBegin, oldSelectionEnd));
            }

            // Get text properties of the previously-selected text

            var properties = GetTextProperties();

            // Update selection

            _SelectionBegin = newSelectionBegin;
            _SelectionEnd = newSelectionEnd;

            // Update caret

            CaretIndex = newSelectionEnd;

            OnSelectionChangedInternal();

            // Notify others if the properties of the selected text have changed.

            if (!Object.Equals(GetTextProperties(), properties))
            {
                OnSelectionFormatChanged();
            }
        }

        /// <summary>
        /// This is the blue background that appears behind selected text
        /// </summary>
        DrawingVisual _SelectionDrawing;

        private bool _IsSelectionvisible;

        /// <summary>
        /// Get/set whether or not the selection visual is visible
        /// </summary>
        public bool IsSelectionVisible
        {
            get
            {
                return _IsSelectionvisible;
            }
            set
            {
                if (value)
                {
                    ShowSelectionDrawing();
                }
                else
                {
                    HideSelectionDrawing();
                }

                _IsSelectionvisible = value;
            }
        }

        /// <summary>
        /// Make the selection visual visible
        /// </summary>
        void ShowSelectionDrawing()
        {
            if (_SelectionDrawing != null && !HasVisual(_SelectionDrawing))
            {
                // Draw the selection behind everything else except for the
                // "find" results highlighting if present

                AddVisual(_SelectionDrawing, -100);
            }
        }

        /// <summary>
        /// Make the selection drawing invisible
        /// </summary>
        void HideSelectionDrawing()
        {
            if (_SelectionDrawing != null)
            {
                RemoveVisual(_SelectionDrawing);
            }
        }

        /// <summary>
        /// Draw the selection background visual
        /// </summary>
        protected void UpdateSelection(object sender, EventArgs e)
        {
            // If nothing selected, delete the unneeded DrawingVisual
            // 
            // Note that if the paragraph is empty we still need to
            // draw a selection visual even though SelectionLength 
            // will be 0.

            if (SelectionLength == 0 && (Length > 0 || IsFocused))
            {
                if (_SelectionDrawing != null)
                {
                    HideSelectionDrawing();
                    _SelectionDrawing = null;
                }
                return;
            }

            // Otherwise, create a DrawingVisual and make it visible

            if (_SelectionDrawing == null)
            {
                _SelectionDrawing = new DrawingVisual();
                if (IsSelectionVisible)
                {
                    ShowSelectionDrawing();
                }
            }

            // Now actually draw the selection

            using (var dc = _SelectionDrawing.RenderOpen())
            {
                Geometry geometry = Renderer.GetSelectionBounds(SelectionOffset, SelectionOffset + SelectionLength);

                // if the user selects empty paragraphs, we need to draw some indication
                // that the paragraph is selected even though its text geometry is empty
                Rect bounds = geometry.Bounds;
                if (bounds.Width == 0)
                {
                    bounds.Width = 5;
                    geometry = new RectangleGeometry(bounds);
                }

                var offset = new TranslateTransform(Renderer.Offset.X, Renderer.Offset.Y);
                dc.PushTransform(offset);
                dc.DrawGeometry(SelectionBrush, null, geometry);
            }
        }

        #endregion

        #region Pending Properties

        /// <summary>
        /// When not null, this specifies any properties that will be applied
        /// to the next string that is inserted. This is useful for the case 
        /// where the user changes a text property when no text is selected.
        /// They expect that the next character they type to contain the
        /// newly-set property, and this is how we make that happen.
        /// </summary>
        private GenericTextRunProperties _PendingProperties;

        protected virtual bool HasPendingProperties
        {
            get { return _PendingProperties != null; }
        }

        protected virtual GenericTextRunProperties GetPendingProperties()
        {
            return _PendingProperties;
        }

        protected virtual void SetPendingProperties(GenericTextRunProperties newProperties)
        {
            if (!Object.ReferenceEquals(newProperties, _PendingProperties))
            {
                if (_PendingProperties != null)
                {
                    _PendingProperties.FormatChanged -= PendingProperties_FormatChanged;
                }

                _PendingProperties = newProperties;

                if (_PendingProperties != null)
                {
                    _PendingProperties.FormatChanged += PendingProperties_FormatChanged;
                }

                OnSelectionFormatChanged();
            }
        }

        protected virtual void SetPendingProperty(string name, object newValue)
        {
            if (!HasPendingProperties)
            {
                SetPendingProperties((GenericTextRunProperties)GetTextProperties().Clone());
            }

            GetPendingProperties().SetProperty(name, newValue);
        }

        protected void PendingProperties_FormatChanged(object sender, EventArgs e)
        {
            OnSelectionFormatChanged();
        }

        #endregion

        #region Highlighting

        private void OnSelectionChangedInternal()
        {
            // If we're in highlighting mode, highlight text as it's selected

            if (UndoStack == null || (!UndoStack.IsUndoing && !UndoStack.IsRedoing))
            {
                if (SelectionLength > 0 && IsHighlighting)
                {
                    SetTextProperty(GenericTextRunProperties.BackgroundBrushProperty, HighlightBrush);

                    MoveTo(SelectionEnd);

                    // Note: MoveTo() triggers another call to OnSelectionChanged()
                    return;
                }
            }

            SetPendingProperties(null);

            OnSelectionChanged();
        }

        #endregion

        #region Watermark

        private void DrawWatermark(Size size)
        {
            if (_WatermarkDrawing == null)
            {
                _WatermarkDrawing = new DrawingVisual();
            }

            _WatermarkDrawing.Clip = new RectangleGeometry(new Rect(size));

            using (var dc = _WatermarkDrawing.RenderOpen())
            {
                if (String.IsNullOrEmpty(Watermark))
                {
                    return;
                }

                var text = new FormattedText(
                    Watermark,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(
                        DefaultTextRunProperties.Typeface.FontFamily,
                        FontStyles.Italic,
                        DefaultTextRunProperties.Typeface.Weight,
                        DefaultTextRunProperties.Typeface.Stretch
                    ),
                    DefaultTextRunProperties.FontRenderingEmSize,
                    Brushes.Gray
                );

                double x;
                switch (TextAlignment)
                {
                    case TextAlignment.Left:
                        x = Padding.Left;
                        break;
                    case TextAlignment.Right:
                        x = size.Width - text.Width - Padding.Right;
                        break;
                    case TextAlignment.Center:
                        x = (size.Width - text.Width) / 2;
                        break;
                    default:
                        x = 0;
                        break;
                }

                double y;
                switch (VerticalContentAlignment)
                {
                    case VerticalAlignment.Top:
                        y = Padding.Top;
                        break;
                    case VerticalAlignment.Bottom:
                        y = size.Height - text.Height - Padding.Bottom;
                        break;
                    case VerticalAlignment.Center:
                        y = (size.Height - text.Height) / 2;
                        break;
                    default:
                        y = 0;
                        break;
                }

                dc.DrawText(text, new Point(x, y));
            }
        }

        private void UpdateWatermarkVisibility(object sender, EventArgs e)
        {
            if (!IsFocused && (Length == 0))
            {
                // Show the watermark
                if (_WatermarkDrawing == null)
                {
                    _WatermarkDrawing = new DrawingVisual();
                }
                AddVisual(_WatermarkDrawing);
            }
            else
            {
                // Hide the watermark
                if (_WatermarkDrawing != null)
                {
                    RemoveVisual(_WatermarkDrawing);
                }
            }
        }

        #endregion

        #region Layout

        protected override Size ArrangeOverride(Size finalSize)
        {
            finalSize = base.ArrangeOverride(finalSize);

            if (!String.IsNullOrEmpty(Watermark))
            {
                DrawWatermark(finalSize);
            }

            UpdateCaretPosition(this, EventArgs.Empty);
            UpdateSelection(this, EventArgs.Empty);
            UpdateSearchResults(this, EventArgs.Empty);

            return finalSize;
        }

        #endregion

        #region Keyboard Input

        protected void OnTextInput(object sender, TextCompositionEventArgs e)
        {
            // Filter-out any non-printable characters

            StringBuilder buffer = new StringBuilder();
            foreach (char c in e.Text)
            {
                if (!Char.IsControl(c))
                {
                    buffer.Append(c);
                }
            }
            string value = buffer.ToString();

            // Replace any currently-selected text with the newly-typed text

            if (value.Length > 0)
            {
                if (SelectionLength > 0)
                {
                    Delete(SelectionOffset, SelectionLength);
                }

                Insert(value);

                NavigationOffset = new Point(-1, -1);
            }

            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Enter:
                    OnEnterKeyDown(e);
                    break;
            }
        }

        private void OnEnterKeyDown(KeyEventArgs e)
        {
            if (AcceptsReturn)
            {
                EnterLineBreak();
                e.Handled = true;
            }
        }

        #endregion

        #region Mouse Input

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            _IsMouseLeftButtonHandled = e.MouseDevice.LeftButton.HasFlag(MouseButtonState.Pressed);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (CapturesMouse)
            {
                CaptureMouse();
            }

            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();

            base.OnPreviewMouseUp(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(this);

            // Get cursor position relative to the text area

            point = TransformToDescendant(Renderer).Transform(point);

            // Constrain the cursor to lie inside the text area

            point.X = Math.Min(Math.Max(point.X, 0), Renderer.Width - 1);
            point.Y = Math.Min(Math.Max(point.Y, 0), Renderer.Height - 1);

            // Get the offset of the character at 'cursor'

            int offset = Renderer.CharFromPoint(point);

            if (offset >= 0 && offset <= Length)
            {
                if (e.ClickCount == 1)
                {
                    // Single-click: Move the caret to the new location

                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        SelectTo(offset);
                    }
                    else
                    {
                        MoveTo(offset);
                    }
                }
                else if (e.ClickCount == 2)
                {
                    // Double-click: Select the entire word

                    if (Buffer.Length > 0)
                    {
                        offset = Math.Max(Math.Min(offset, Buffer.Length - 1), 0);
                        int startIndex = Buffer.GetWordStart(offset);
                        int endIndex = Buffer.GetWordEnd(offset);
                        SetSelection(startIndex, endIndex);
                    }
                }
                else if (e.ClickCount == 3)
                {
                    // Triple-click: Select the entire paragraph

                    SetSelection(0, Length);
                }

                this.Focus();

                e.Handled = true;
            }

            _IsMouseLeftButtonHandled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!_IsMouseLeftButtonHandled)
            {
                // Get the position of the cursor relative to the text area

                Point cursor = e.GetPosition(this);
                cursor = TransformToDescendant(Renderer).Transform(cursor);

                // Constrain the cursor to lie inside the text area

                cursor.X = Math.Min(Math.Max(cursor.X, 0), Renderer.Width - 1);
                cursor.Y = Math.Min(Math.Max(cursor.Y, 0), Renderer.Height - 1);

                // Get the offset of the character at 'cursor'

                int offset = Renderer.CharFromPoint(cursor);
                if (offset >= 0 && offset <= Length)
                {
                    // Single-click: Move the caret to the new location

                    if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        MoveTo(offset);
                    }

                    this.Focus();

                    e.Handled = true;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                // MouseMove + LeftButton = Drag-selection

                // Get the position of the cursor relative to the text area

                Point point = e.GetPosition(this);

                point = TransformToDescendant(Renderer).Transform(point);

                // Constrain the cursor to lie within the text area

                point.X = Math.Min(Math.Max(point.X, 0), Renderer.Width - 1);
                point.Y = Math.Min(Math.Max(point.Y, 0), Renderer.Height - 1);

                // Get the offset of the character at 'cursor'

                int offset = Renderer.CharFromPoint(point);
                if (offset >= 0 && offset <= Length)
                {
                    // Select to the new location

                    if (_IsMouseLeftButtonHandled)
                    {
                        SelectTo(offset);
                    }
                    else
                    {
                        MoveTo(offset);
                    }

                    this.Focus();

                    e.Handled = true;
                }

                _IsMouseLeftButtonHandled = true;
            }
        }

        #endregion

        #region Drag and Drop

        protected override void OnDragEnter(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                base.OnDragEnter(e);
                return;
            }

            foreach (var format in e.Data.GetFormats())
            {
                if (format != DataFormats.StringFormat &&
                    format != DataFormats.Text &&
                    format != DataFormats.UnicodeText)
                {
                    base.OnDragEnter(e);
                    return;
                }
            }

            string data = (string)e.Data.GetData(DataFormats.StringFormat);
            if (data.Contains('\r') || data.Contains('\n'))
            {
                base.OnDragEnter(e);
                return;
            }

            _DropData = data;
            e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
            e.Handled = true;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            if (_DropData != null)
            {
                _DropData = null;
                HideDropVisual();
                e.Handled = true;
            }
            else
            {
                base.OnDragLeave(e);
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            if (_DropData != null)
            {
                Point dropPoint = e.GetPosition(this);
                ShowDropVisual(dropPoint);
                e.Handled = true;
            }
            else
            {
                base.OnDrop(e);
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (_DropData != null)
            {
                Point dropPoint = e.GetPosition(this);
                DoDrop(_DropData, dropPoint);
                HideDropVisual();
                e.Handled = true;
            }
            else
            {
                base.OnDrop(e);
            }
        }

        private void DoDrop(string dropData, Point dropPoint)
        {
            dropPoint = TransformToDescendant(Renderer).Transform(dropPoint);
            int offset = Renderer.CharFromPoint(dropPoint);
            if (offset != -1)
            {
                Insert(offset, dropData);
            }
        }

        private void ShowDropVisual(Point dropPoint)
        {
            dropPoint = TransformToDescendant(Renderer).Transform(dropPoint);
            int charOffset = Renderer.CharFromPoint(dropPoint);
            if (charOffset != -1)
            {
                ShowDropVisual(charOffset);
            }
            else
            {
                HideDropVisual();
            }
        }

        private void ShowDropVisual(int charOffset)
        {
            Rect caretBounds = Renderer.GetCaretBounds(charOffset);
            caretBounds = Renderer.TransformToAncestor(this).TransformBounds(caretBounds);
            DrawDropVisual(caretBounds);
            ShowDropVisual();
        }

        private void DrawDropVisual(Rect bounds)
        {
            if (_DropVisual == null)
            {
                _DropVisual = new DrawingVisual();
            }

            using (var dc = _DropVisual.RenderOpen())
            {
                double x = bounds.X + 0.5;
                double y1 = bounds.Top;
                double y2 = bounds.Bottom;
                Pen pen = new Pen(Brushes.Gray, 1);
                pen.Freeze();
                dc.DrawLine(pen, new Point(x, y1), new Point(x, y2));
            }
        }

        private void ShowDropVisual()
        {
            if (_DropVisual != null && !HasVisual(_DropVisual))
            {
                AddVisual(_DropVisual, 2);
            }
        }

        private void HideDropVisual()
        {
            if (_DropVisual != null)
            {
                RemoveVisual(_DropVisual);
            }
        }

        #endregion

        protected override void OnTextChanged(int offset, int numAdded, int numRemoved)
        {
            base.OnTextChanged(offset, numAdded, numRemoved);

            if (!String.IsNullOrEmpty(_FindText))
            {
                Find(_FindText);
            }

            int delta = numAdded - numRemoved;
            if (delta > 0)
            {
                if (offset <= SelectionOffset)
                {
                    SelectionOffset += delta;
                }

                if (offset >= SelectionOffset && offset < SelectionOffset + SelectionLength)
                {
                    SelectionLength += delta;
                }
            }
            else if (delta < 0)
            {
                delta = -delta;

                if (offset >= SelectionOffset && offset < SelectionOffset + SelectionLength)
                {
                    SelectionLength -= Math.Min(delta, SelectionOffset + SelectionLength - offset);
                }

                if (offset <= SelectionOffset)
                {
                    SelectionOffset -= Math.Min(delta, SelectionOffset - offset);
                }
            }
        }

        protected override void OnTextPropertyChanged(int offset, int length, string propertyName)
        {
            base.OnTextPropertyChanged(offset, length, propertyName);

            OnSelectionFormatChanged();
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new TextField(this);
        }

        #endregion
    }
}
