/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.LS;
using DOM.SVG;
using SilverNote.Behaviors;
using SilverNote.Commands;
using SilverNote.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SilverNote.Editor
{
    public class NoteEditor : InteractivePanel, IDocumentPaginatorSource
    {
        #region Fields

        NotePaginator _DocumentPaginator;

        #endregion

        #region Constructors

        public NoteEditor()
        {
            Initialize();

            DrawingBoard = CreateDrawingBoard();
            Append(DrawingBoard);
            Append(new NHeading());

            UndoStack.Clear();
            IsDirty = false;
        }

        public NoteEditor(NoteEditor copy)
            : base(copy)
        {
            Initialize();

            DrawingBoard = InternalChildren.OfType<PrimaryCanvas>().FirstOrDefault();

            foreach (var child in InternalChildren)
            {
                OnChildAdded((UIElement)child);
            }

            UndoStack.Clear();
            IsDirty = false;
        }

        private void Initialize()
        {
            LoadDefaultProperties();
            LoadCommandBindings();

            KeyDown += UpdateAlternativeHighlightBrush;
            KeyUp += UpdateAlternativeHighlightBrush;

            InputElementBehavior.SetIsFocusScope(this, true);
            Formattable.AddFormatChangedHandler(this, Descendant_FormatChanged);
            DropHandlers.Add(new DrawingDropHandler(this));

            OwnerDocument = DOMFactory.CreateHTMLDocument();
            OwnerDocument.SetUserStyleSheet(CSSParser.ParseStylesheet(DEFAULT_STYLESHEET));
            OwnerDocument.Body.Bind(this);

            UndoStack = new UndoStack();

            IdleTimer.Start();
            FlushTimer.Start();
        }

        public const string DEFAULT_STYLESHEET =
@"
        body  { margin:0; padding:0 6px; font-family:Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif; font-size:12pt; } 
        .serif { font-family:Times New Roman, Times, Serif; }
        .cursive { font-family:Comic Sans MS, Cursive; }
        .monospace { font-family:Consolas, Lucida Console, Lucida Sans Typewriter, Monaco, Courier New, Courier, Monospace; }
        .code { font-family:Consolas, Lucida Console, Lucida Sans Typewriter, Monaco, Courier New, Courier, Monospace; font-size:10pt; }
        h1    { margin-top: 0; border-bottom:2px solid gray; font-size:1.5em; font-weight: normal; }
        h2    { margin: 12px 0; padding: 4px; background-color: #ECF5FF; font-size: 14pt; font-weight: bold; font-style: normal; }
        h3    { margin: 12px 0; padding: 4px; font-size: 12pt; font-weight: bold; font-style: normal; border-bottom: 1px solid lightgray; }
        p     { margin:0; } 
        pre  { margin: 5px 20px; padding: 10px; border: 2px solid lightgray; }
        table,th,td   { border:1px solid gray; border-collapse:collapse; }
        table { border-radius: 6px; }
        td    { padding: 4px; }
    ";
        private void LoadDefaultProperties()
        {
            IsEnabled = true;
            Focusable = true;
            AllowDrop = true;
            SelectedLine = new QuadraticCurve { X1 = 0, Y1 = 16, X2 = 8, Y2 = 8, X3 = 16, Y3 = 0, StrokeBrush = Brushes.Black, FillBrush = null };
            SelectedClipart = new Rectangle { Width = 16, Height = 16, StrokeBrush = Brushes.Black, FillBrush = null };
        }

        #endregion

        #region Properties

        #region Url

        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
             "Url",
             typeof(string),
             typeof(NoteEditor),
             new FrameworkPropertyMetadata(null, OnUrlChanged)
         );

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        private static void OnUrlChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            string newValue = (string)e.NewValue;

            editor.OnUrlChanged(newValue);
        }

        private void OnUrlChanged(string newURL)
        {
            OwnerDocument.DocumentURI = newURL;
        }

        #endregion

        #region Title

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private NHeading _TitleElement = null;

        public NHeading TitleElement
        {
            get { return _TitleElement; }
            set
            {
                var oldValue = TitleElement;
                var newValue = value;

                if (newValue != oldValue)
                {
                    _TitleElement = newValue;
                    OnTitleChanged(oldValue, newValue);
                }
            }
        }

        private void OnTitleChanged(NHeading oldValue, NHeading newValue)
        {
            if (oldValue != null)
            {
                oldValue.IsDeletable = true;
                oldValue.TextChanged -= Title_TextChanged;
            }

            if (newValue != null)
            {
                newValue.IsDeletable = false;
                newValue.TextChanged += Title_TextChanged;
                SetValue(TitleProperty, newValue.Text.Trim());
            }
        }

        protected void Title_TextChanged(object sender, EventArgs e)
        {
            SetValue(TitleProperty, TitleElement.Text.Trim());
        }

        #endregion

        #region Content

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content",
            typeof(string),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnContentChanged)
        );

        string _Content = null;

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        private static void OnContentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            string newContent = (string)e.NewValue;

            editor.OnContentChanged(newContent);
        }

        protected void OnContentChanged(string newContent)
        {
            if (newContent != null && newContent != _Content)
            {
                if (IsDirty)
                {
                    string message = String.Format("The note \"{0}\" has been modified outside this application\n\n", Title);
                    message += "Would you like to reload it?";

                    if (MessageBox.Show(message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                _Content = newContent;

                FromHTML(newContent);

                if (_SearchPattern != null)
                {
                    SelectNth(_SearchPattern, _SearchOptions, _SelectCount);
                }
            }

            IsDirty = false;
        }

        #endregion

        #region DocumentPaginator

        public DocumentPaginator DocumentPaginator
        {
            get
            {
                if (_DocumentPaginator == null)
                {
                    _DocumentPaginator = new NotePaginator(this);
                }
                return _DocumentPaginator;
            }
        }

        #endregion

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(null)
        );

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        #region IsDirty

        public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register(
            "IsDirty",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false)
        );

        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        protected void OnEdited()
        {
            IsDirty = true;
            Revised = DateTime.UtcNow;
            ResetIdleTimer();
        }

        public DateTime Revised { get; set; }

        #endregion

        #region FindText

        public static readonly DependencyProperty FindTextProperty = DependencyProperty.Register(
            "FindText",
            typeof(string),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(null)
        );

        public string FindText
        {
            get { return (string)GetValue(FindTextProperty); }
            set { SetValue(FindTextProperty, value); }
        }

        #endregion

        #region ReplaceText

        public static readonly DependencyProperty ReplaceTextProperty = DependencyProperty.Register(
            "ReplaceText",
            typeof(string),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(null)
        );

        public string ReplaceText
        {
            get { return (string)GetValue(ReplaceTextProperty); }
            set { SetValue(ReplaceTextProperty, value); }
        }

        #endregion

        #region HasTextProperties

        public static readonly DependencyProperty HasTextPropertiesProperty = DependencyProperty.Register(
            "HasTextProperties",
            typeof(bool),
            typeof(NoteEditor)
        );

        public bool HasTextProperties
        {
            get { return (bool)GetValue(HasTextPropertiesProperty); }
            set { SetValue(HasTextPropertiesProperty, value); }
        }

        #endregion

        #region HasParagraphProperties

        public static readonly DependencyProperty HasParagraphPropertiesProperty = DependencyProperty.Register(
            "HasParagraphProperties",
            typeof(bool),
            typeof(NoteEditor)
        );

        public bool HasParagraphProperties
        {
            get { return (bool)GetValue(HasParagraphPropertiesProperty); }
            set { SetValue(HasParagraphPropertiesProperty, value); }
        }

        #endregion

        #region FontClass

        public static readonly DependencyProperty FontClassProperty = DependencyProperty.Register(
            "FontClass",
            typeof(FontClass),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(
                null,   // defaultValue 
                (DependencyObject target, DependencyPropertyChangedEventArgs e) =>
                {
                    ((NoteEditor)target).FontClass_PropertyChanged(e);
                })
        );

        public FontClass FontClass
        {
            get { return (FontClass)GetValue(FontClassProperty); }
            set { SetValue(FontClassProperty, value); }
        }

        FontClass _FontClass = null;

        private void FontClass_PropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            FontClass newValue = (FontClass)e.NewValue;

            if (newValue != _FontClass)
            {
                _FontClass = newValue;

                if (newValue == null)
                {
                    SetProperty(TextProperties.FontClassProperty, null);
                }
                else if (newValue.ID != null)
                {
                    SetProperty(TextProperties.FontClassProperty, newValue.ID);
                }
                else
                {
                    FontFamily = newValue.FontFamily;
                }

                RecordFontClass(newValue);
            }
        }

        private static void RecordFontClass(FontClass fontClass)
        {
           
        }

        #endregion

        #region FontFamily

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily",
            typeof(FontFamily),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(
                null,   // defaultValue 
                (DependencyObject target, DependencyPropertyChangedEventArgs e) =>
                {
                    ((NoteEditor)target).FontFamily_PropertyChanged(e);
                })
        );

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        FontFamily _FontFamily;

        private void FontFamily_PropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            FontFamily newValue = (FontFamily)e.NewValue;

            if (newValue != _FontFamily)
            {
                _FontFamily = newValue;

                SetProperty(TextProperties.FontFamilyProperty, newValue);

                // Update FontClass based on FontFamily
                if (FontClass == null || FontClass.FontFamily != newValue)
                {
                    FontClass = _FontClass = FontClass.FromFont(newValue);
                    if (_FontClass != null)
                    {
                        SetProperty(TextProperties.FontClassProperty, _FontClass.ID);
                    }
                    else
                    {
                        SetProperty(TextProperties.FontClassProperty, null);
                    }
                }
            }
        }

        #endregion

        #region FontSize

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize",
            typeof(int),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnFontSizeChanged))
        );

        private static void OnFontSizeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            int newValue = (int)e.NewValue;

            if (newValue != editor._FontSize)
            {
                editor._FontSize = newValue;
                editor.SetProperty(TextProperties.FontSizeProperty, newValue * 96.0 / 72.0);
            }
        }

        public int FontSize
        {
            get { return (int)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        int _FontSize = 0;

        #endregion

        #region Bold

        public static readonly DependencyProperty BoldProperty = DependencyProperty.Register(
            "Bold",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnBoldChanged))
        );

        private static void OnBoldChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            bool newValue = (bool)e.NewValue;

            if (newValue != editor._Bold)
            {
                editor._Bold = newValue;
                if (newValue)
                    editor.SetProperty(TextProperties.FontWeightProperty, FontWeights.Bold);
                else
                    editor.SetProperty(TextProperties.FontWeightProperty, FontWeights.Normal);
            }
        }

        public bool Bold
        {
            get { return (bool)GetValue(BoldProperty); }
            set { SetValue(BoldProperty, value); }
        }

        bool _Bold = false;

        #endregion

        #region Italic

        public static readonly DependencyProperty ItalicProperty = DependencyProperty.Register(
            "Italic",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnItalicChanged))
        );

        private static void OnItalicChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            bool newValue = (bool)e.NewValue;

            if (newValue != editor._Italic)
            {
                editor._Italic = newValue;
                if (newValue)
                    editor.SetProperty(TextProperties.FontStyleProperty, FontStyles.Italic);
                else
                    editor.SetProperty(TextProperties.FontStyleProperty, FontStyles.Normal);
            }
        }

        public bool Italic
        {
            get { return (bool)GetValue(ItalicProperty); }
            set { SetValue(ItalicProperty, value); }
        }

        bool _Italic = false;

        #endregion

        #region Underline

        public static readonly DependencyProperty UnderlineProperty = DependencyProperty.Register(
            "Underline",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnUnderlineChanged))
        );

        private static void OnUnderlineChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            bool newValue = (bool)e.NewValue;

            if (newValue != editor._Underline)
            {
                editor._Underline = newValue;
                if (newValue)
                    editor.SetProperty(TextProperties.TextDecorationsProperty, TextDecorations.Underline);
                else
                    editor.SetProperty(TextProperties.TextDecorationsProperty, null);
            }
        }

        public bool Underline
        {
            get { return (bool)GetValue(UnderlineProperty); }
            set { SetValue(UnderlineProperty, value); }
        }

        bool _Underline = false;

        #endregion

        #region Strikethrough

        public static readonly DependencyProperty StrikethroughProperty = DependencyProperty.Register(
            "Strikethrough",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnStrikethroughChanged))
        );

        private static void OnStrikethroughChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            bool newValue = (bool)e.NewValue;

            if (newValue != editor._Strikethrough)
            {
                editor._Strikethrough = newValue;
                if (newValue)
                    editor.SetProperty(TextProperties.TextDecorationsProperty, TextDecorations.Strikethrough);
                else
                    editor.SetProperty(TextProperties.TextDecorationsProperty, null);
            }
        }

        public bool Strikethrough
        {
            get { return (bool)GetValue(StrikethroughProperty); }
            set { SetValue(StrikethroughProperty, value); }
        }

        bool _Strikethrough = false;

        #endregion

        #region Superscript

        public static readonly DependencyProperty SuperscriptProperty = DependencyProperty.Register(
            "Superscript",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, SuperscriptProperty_Changed)
        );

        private static void SuperscriptProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)sender).SuperscriptProperty_Changed(e);
        }

        private void SuperscriptProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;

            if (newValue != _Superscript)
            {
                _Superscript = newValue;

                if (newValue)
                {
                    if (Subscript)
                    {
                        Subscript = false;
                    }
                    SetProperty(TextProperties.BaselineAlignmentProperty, BaselineAlignment.Superscript);
                }
                else
                {
                    SetProperty(TextProperties.BaselineAlignmentProperty, BaselineAlignment.Baseline);
                }
            }
        }

        public bool Superscript
        {
            get { return (bool)GetValue(SuperscriptProperty); }
            set { SetValue(SuperscriptProperty, value); }
        }

        bool _Superscript = false;

        #endregion

        #region Subscript

        public static readonly DependencyProperty SubscriptProperty = DependencyProperty.Register(
            "Subscript",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, SubscriptProperty_Changed)
        );

        private static void SubscriptProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)sender).SubscriptProperty_Changed(e);
        }

        private void SubscriptProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;

            if (newValue != _Subscript)
            {
                _Subscript = newValue;

                if (newValue)
                {
                    if (Superscript)
                    {
                        Superscript = false;
                    }
                    SetProperty(TextProperties.BaselineAlignmentProperty, BaselineAlignment.Subscript);
                }
                else
                {
                    SetProperty(TextProperties.BaselineAlignmentProperty, BaselineAlignment.Baseline);
                }
            }
        }

        public bool Subscript
        {
            get { return (bool)GetValue(SubscriptProperty); }
            set { SetValue(SubscriptProperty, value); }
        }

        bool _Subscript = false;

        #endregion

        #region Alignment

        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register(
            "Alignment",
            typeof(TextAlignment),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(TextAlignment.Left, new PropertyChangedCallback(OnAlignmentChanged))
        );

        private static void OnAlignmentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            TextAlignment newValue = (TextAlignment)e.NewValue;

            if (newValue != editor._Alignment)
            {
                editor._Alignment = newValue;
                editor.SetProperty(TextParagraph.TextAlignmentPropertyName, newValue);
                RecordAlignment(newValue);
            }
        }

        public TextAlignment Alignment
        {
            get { return (TextAlignment)GetValue(AlignmentProperty); }
            set { SetValue(AlignmentProperty, value); }
        }

        TextAlignment _Alignment = TextAlignment.Left;

        private static void RecordAlignment(TextAlignment alignment)
        {

        }

        #endregion

        #region ListStyle

        public static readonly DependencyProperty ListStyleProperty = DependencyProperty.Register(
            "ListStyle",
            typeof(IListStyle),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ListStyleProperty_Changed))
        );

        private static void ListStyleProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            IListStyle newValue = (IListStyle)e.NewValue;

            editor.OnListStyleChanged(newValue);
        }

        protected void OnListStyleChanged(IListStyle newValue)
        {
            if (newValue != _ListStyle)
            {
                _ListStyle = newValue;
                ApplyListStyle(newValue);
            }
        }

        public IListStyle ListStyle
        {
            get { return (IListStyle)GetValue(ListStyleProperty); }
            set { SetValue(ListStyleProperty, value); }
        }

        IListStyle _ListStyle = null;

        private static void RecordListSTyle(IListStyle listStyle)
        {
        }

        #endregion

        #region DefaultListStyle

        public static readonly DependencyProperty DefaultListStyleProperty = DependencyProperty.Register(
            "DefaultListStyle",
            typeof(IListStyle),
            typeof(NoteEditor),
            new PropertyMetadata(ListStyles.Circle)
        );

        public IListStyle DefaultListStyle
        {
            get { return (IListStyle)GetValue(DefaultListStyleProperty); }
            set { SetValue(DefaultListStyleProperty, value); }
        }

        #endregion

        #region Bulleted

        public static readonly DependencyProperty BulletedProperty = DependencyProperty.Register(
            "Bulleted",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(BulletedProperty_Changed))
        );

        static void BulletedProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).BulletedProperty_Changed(e);
        }

        void BulletedProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;

            if (newValue != _Bulleted)
            {
                _Bulleted = newValue;
                ApplyListStyle(newValue ? DefaultListStyle : null);
            }
        }

        public bool Bulleted
        {
            get { return (bool)GetValue(BulletedProperty); }
            set { SetValue(BulletedProperty, value); }
        }

        bool _Bulleted = false;

        #endregion

        #region TextBrush

        public static readonly DependencyProperty TextBrushProperty = DependencyProperty.Register(
            "TextBrush",
            typeof(Brush),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnTextBrushChanged))
        );

        private static void OnTextBrushChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            editor.ApplyTextBrush();
        }

        public Brush TextBrush
        {
            get { return (Brush)GetValue(TextBrushProperty); }
            set { SetValue(TextBrushProperty, value); }
        }

        public void ApplyTextBrush()
        {
            SetProperty(TextProperties.ForegroundBrushProperty, TextBrush);
        }

        #endregion

        #region HighlightBrush

        public static readonly DependencyProperty HighlightBrushProperty = TextParagraph.HighlightBrushProperty.AddOwner(
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(Brushes.Yellow, HighlightBrushProperty_Changed)
        );

        private static void HighlightBrushProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)sender).HighlightBrushProperty_Changed(e);
        }

        private void HighlightBrushProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            Brush newValue = (Brush)e.NewValue;

            if (IsTextSelected)
            {
                SetProperty(TextProperties.BackgroundBrushProperty, newValue);
                IsHighlighting = false;
            }
            else
            {
                IsHighlighting = true;
                Cursor = GetHighlightCursor(newValue);
            }
        }

        public static Brush GetHighlightBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightBrushProperty);
        }

        public static void SetHighlightBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightBrushProperty, value);
        }

        public Brush HighlightBrush
        {
            get { return GetHighlightBrush(this); }
            set { SetHighlightBrush(this, value); }
        }

        public void ApplyHighlightBrush()
        {
            SetProperty(TextProperties.BackgroundBrushProperty, HighlightBrush);
        }

        Brush _OriginalHighlightBrush;

        void UpdateAlternativeHighlightBrush(object sender, EventArgs e)
        {
            if (IsHighlighting)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (HighlightBrush != Brushes.Transparent)
                    {
                        _OriginalHighlightBrush = HighlightBrush;
                        HighlightBrush = Brushes.Transparent;
                    }
                }
                else if (_OriginalHighlightBrush != null)
                {
                    HighlightBrush = _OriginalHighlightBrush;
                    _OriginalHighlightBrush = null;
                }
            }
            else if (_OriginalHighlightBrush != null)
            {
                HighlightBrush = _OriginalHighlightBrush;
                _OriginalHighlightBrush = null;
            }
        }

        #endregion

        #region IsHighlighting

        public static readonly DependencyProperty IsHighlightingProperty = TextParagraph.IsHighlightingProperty.AddOwner(
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, IsHighlightingProperty_Changed)
        );

        private static void IsHighlightingProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)sender).IsHighlightingProperty_Changed(e);
        }

        private void IsHighlightingProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                EditingMode = EditingModes.Highlighting;
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public static bool GetIsHighlighting(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHighlightingProperty);
        }

        public static void SetIsHighlighting(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHighlightingProperty, value);
        }

        public bool IsHighlighting
        {
            get { return GetIsHighlighting(this); }
            set { SetIsHighlighting(this, value); }
        }

        private static Cursor GetHighlightCursor(Brush brush)
        {
            if (brush == Brushes.Transparent)
            {
                return GetCursor("HighlighterWhite.cur");
            }
            else if (brush == Brushes.LightGray)
            {
                return GetCursor("HighlighterGray.cur");
            }
            else if (brush == Brushes.Red)
            {
                return GetCursor("HighlighterRed.cur");
            }
            else if (brush == Brushes.Yellow)
            {
                return GetCursor("HighlighterYellow.cur");
            }
            else if (brush == Brushes.Aqua)
            {
                return GetCursor("HighlighterAqua.cur");
            }
            else if (brush == Brushes.Violet)
            {
                return GetCursor("HighlighterViolet.cur");
            }

            SolidColorBrush solidColorBrush = brush as SolidColorBrush;

            if (solidColorBrush != null &&
                solidColorBrush.Color.R == 0x7F &&
                solidColorBrush.Color.G == 0xFF &&
                solidColorBrush.Color.B == 0x00)
            {
                return GetCursor("HighlighterGreen.cur");
            }

            return Cursors.IBeam;
        }

        const string CURSORS_PATH = "pack://application:,,,/SilverNote.Editor;component/Cursors/";

        static Cursor GetCursor(string name)
        {
            var uri = new Uri("SilverNote.Editor;component/Cursors/" + name, UriKind.Relative);
            var resource = Application.GetResourceStream(uri);
            return new Cursor(resource.Stream);
        }

        #endregion

        #region LineSpacing

        public static readonly DependencyProperty LineSpacingProperty = DependencyProperty.Register(
            "LineSpacing",
            typeof(double),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(OnLineSpacingChanged))
        );

        private static void OnLineSpacingChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            double newValue = (double)e.NewValue;

            if (newValue != editor._LineSpacing)
            {
                editor._LineSpacing = newValue;
                editor.SetProperty(TextParagraph.LineSpacingPropertyName, newValue);
                RecordLineSpacing(newValue);
            }
        }

        public double LineSpacing
        {
            get { return (double)GetValue(LineSpacingProperty); }
            set { SetValue(LineSpacingProperty, value); }
        }

        double _LineSpacing = 0;

        private static void RecordLineSpacing(double lineSpacing)
        {
        }

        #endregion

        #region IsSelecting

        public static readonly DependencyProperty IsSelectingProperty = DependencyProperty.Register(
            "IsSelecting",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsSelectingChanged))
        );

        private static void OnIsSelectingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (NoteEditor)sender;
            var newValue = (bool)e.NewValue;

            editor.OnIsSelectingChanged(newValue);
        }

        private void OnIsSelectingChanged(bool newValue)
        {
            IsSelectingGeometry = newValue;

            if (newValue)
            {
                EditingMode = EditingModes.Selecting;
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public override bool IsSelectingGeometry
        {
            get
            {
                return (bool)GetValue(IsSelectingProperty);
            }
            set
            {
                if (value != IsSelectingGeometry)
                {
                    SetValue(IsSelectingProperty, value);
                }

                base.IsSelectingGeometry = value;
            }
        }

        #endregion

        #region IsDrawingPath

        public static readonly DependencyProperty IsDrawingPathProperty = DependencyProperty.Register(
            "IsDrawingPath",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsDrawingPathChanged))
        );

        private static void OnIsDrawingPathChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            bool newValue = (bool)e.NewValue;

            editor.OnIsDrawingPathChanged(newValue);
        }

        void OnIsDrawingPathChanged(bool newValue)
        {
            if (newValue)
            {
                InsertDrawing(new NPath { StrokeBrush = Brushes.Black, FillBrush = null, Smoothness = PathSmoothness });
                EditingMode = EditingModes.Drawing;
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public bool IsDrawingPath
        {
            get { return (bool)GetValue(IsDrawingPathProperty); }
            set { SetValue(IsDrawingPathProperty, value); }
        }

        #endregion

        #region PathSmoothness

        public static readonly DependencyProperty PathSmoothnessProperty = DependencyProperty.Register(
            "PathSmoothness",
            typeof(double),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(256.0)
        );

        public double PathSmoothness
        {
            get { return (double)GetValue(PathSmoothnessProperty); }
            set { SetValue(PathSmoothnessProperty, value); }
        }

        #endregion

        #region IsErasing

        public static readonly DependencyProperty IsErasingProperty = DependencyProperty.Register(
            "IsErasing",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsErasingChanged))
        );

        private static void OnIsErasingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (NoteEditor)sender;
            var newValue = (bool)e.NewValue;

            editor.OnIsErasingChanged(newValue);
        }

        private void OnIsErasingChanged(bool newValue)
        {
            IsErasing = newValue;

            if (newValue)
            {
                EditingMode = EditingModes.Erasing;
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public override bool IsErasing
        {
            get
            {
                return (bool)GetValue(IsErasingProperty);
            }
            set
            {
                if (value != IsErasing)
                {
                    SetValue(IsErasingProperty, value);
                }

                base.IsErasing = value;
            }
        }
        #endregion

        #region IsFilling

        public static readonly DependencyProperty IsFillingProperty = DependencyProperty.Register(
            "IsFilling",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(IsFillingProperty_Changed))
        );

        private static void IsFillingProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)sender).IsFillingProperty_Changed(e);
        }

        /// <summary>
        /// This is invoked when the fill button is clicked
        /// </summary>
        /// <param name="e"></param>
        private void IsFillingProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            var newValue = (bool)e.NewValue;

            // If the button is depressed while in "normal" mode, call 
            // Fill() to apply the current fill properties to all
            // selected drawings. If that succeeds (i.e., drawings are 
            // selected), then automatically reset IsFilling to false.

            if (newValue && EditingMode == EditingModes.Normal && Fill())
            {
                IsFilling = false;
                return;
            }

            // Otherwise, enter "filling" mode

            base.IsFilling = newValue;

            if (newValue)
            {
                EditingMode = EditingModes.Filling;
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public override bool IsFilling
        {
            get { return (bool)GetValue(IsFillingProperty); }
            set { SetValue(IsFillingProperty, value); }
        }
        #endregion

        #region IsStroking

        public static readonly DependencyProperty IsStrokingProperty = DependencyProperty.Register(
            "IsStroking",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, IsStrokingProperty_Changed)
        );

        private static void IsStrokingProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)sender).IsStrokingProperty_Changed(e);
        }

        /// <summary>
        /// This is invoked when the stroke button is clicked
        /// </summary>
        /// <param name="e"></param>
        private void IsStrokingProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            var newValue = (bool)e.NewValue;

            // If the button is depressed while in "normal" mode, call 
            // Stroke() to apply the current stroke properties to all
            // selected drawings. If that succeeds (i.e., drawings are 
            // selected), then automatically reset IsStroked to false.

            if (newValue && EditingMode == EditingModes.Normal && Stroke())
            {
                IsStroking = false;
                return;
            }

            // Otherwise, enter "stroking" mode

            base.IsStroking = newValue;

            if (newValue)
            {
                EditingMode = EditingModes.Stroking;
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public override bool IsStroking
        {
            get { return (bool)GetValue(IsStrokingProperty); }
            set { SetValue(IsStrokingProperty, value); }
        }
        #endregion

        #region IsDrawingLine

        public static readonly DependencyProperty IsDrawingLineProperty = DependencyProperty.Register(
            "IsDrawingLine",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsDrawingLineChanged))
        );

        private static void OnIsDrawingLineChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            bool newValue = (bool)e.NewValue;

            editor.OnIsDrawingLineChanged(newValue);
        }

        private void OnIsDrawingLineChanged(bool newValue)
        {
            if (newValue && EditingMode != EditingModes.InsertingLine)
            {
                InsertLine(SelectedLine);
                EditingMode = EditingModes.InsertingLine;
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public bool IsDrawingLine
        {
            get { return (bool)GetValue(IsDrawingLineProperty); }
            set { SetValue(IsDrawingLineProperty, value); }
        }

        #endregion

        #region SelectedLine

        public static readonly DependencyProperty SelectedLineProperty = DependencyProperty.Register(
            "SelectedLine",
            typeof(Shape),
            typeof(NoteEditor)
        );

        public Shape SelectedLine
        {
            get { return (Shape)GetValue(SelectedLineProperty); }
            set { SetValue(SelectedLineProperty, value); }
        }

        #endregion

        #region IsDrawingClipart

        public static readonly DependencyProperty IsDrawingClipartProperty = DependencyProperty.Register(
            "IsDrawingClipart",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsDrawingClipartChanged))
        );

        private static void OnIsDrawingClipartChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            bool newValue = (bool)e.NewValue;

            editor.OnIsDrawingClipartChanged(newValue);
        }

        private void OnIsDrawingClipartChanged(bool newValue)
        {
            if (newValue && EditingMode != EditingModes.InsertingClipart)
            {
                InsertClipart(SelectedClipart);
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public bool IsDrawingClipart
        {
            get { return (bool)GetValue(IsDrawingClipartProperty); }
            set { SetValue(IsDrawingClipartProperty, value); }
        }

        #endregion

        #region SelectedClipart

        public static readonly DependencyProperty SelectedClipartProperty = DependencyProperty.Register(
            "SelectedClipart",
            typeof(Shape),
            typeof(NoteEditor)
        );

        public Shape SelectedClipart
        {
            get { return (Shape)GetValue(SelectedClipartProperty); }
            set { SetValue(SelectedClipartProperty, value); }
        }

        #endregion

        #region IsDrawingText

        public static readonly DependencyProperty IsDrawingTextProperty = DependencyProperty.Register(
            "IsDrawingText",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsDrawingTextChanged))
        );

        private static void OnIsDrawingTextChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NoteEditor editor = (NoteEditor)target;
            bool newValue = (bool)e.NewValue;

            editor.OnIsDrawingTextChanged(newValue);
        }

        private void OnIsDrawingTextChanged(bool newValue)
        {
            if (newValue)
            {
                InsertTextBox();
                EditingMode = EditingModes.InsertingFreetext;
            }
            else
            {
                UpdateEditingMode();
            }
        }

        public bool IsDrawingText
        {
            get { return (bool)GetValue(IsDrawingTextProperty); }
            set { SetValue(IsDrawingTextProperty, value); }
        }

        #endregion

        #region StrokeBrush

        public static readonly DependencyProperty StrokeBrushProperty = DependencyProperty.Register(
            "StrokeBrush",
            typeof(Brush),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(Brushes.Black, StrokeBrushProperty_Changed)
        );

        private static void StrokeBrushProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).StrokeBrushProperty_Changed(e);
        }

        private void StrokeBrushProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue as Brush;

            base.StrokeBrush = newValue;

            Stroke(newValue);
        }

        public override Brush StrokeBrush
        {
            get { return (Brush)GetValue(StrokeBrushProperty); }
            set { SetValue(StrokeBrushProperty, value); }
        }

        #endregion

        #region StrokeWidth

        public static readonly DependencyProperty StrokeWidthProperty = DependencyProperty.Register(
            "StrokeWidth",
            typeof(double),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(2.0, StrokeWidthProperty_Changed)
        );

        private static void StrokeWidthProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).StrokeWidthProperty_Changed(e);
        }

        private void StrokeWidthProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            var newValue = (double)e.NewValue;

            base.StrokeWidth = newValue;

            Stroke(newValue);
        }

        public override double StrokeWidth
        {
            get { return (double)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        #endregion

        #region StrokeDashArray

        public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
            "StrokeDashArray",
            typeof(DoubleCollection),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, StrokeDashArrayProperty_Changed)
        );

        private static void StrokeDashArrayProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).StrokeDashArrayProperty_Changed(e);
        }

        private void StrokeDashArrayProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue as DoubleCollection;

            base.StrokeDashArray = newValue;

            Stroke(newValue);
        }

        public override DoubleCollection StrokeDashArray
        {
            get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
            set { SetValue(StrokeDashArrayProperty, value); }
        }

        #endregion

        #region StrokeDashCap

        public static readonly DependencyProperty StrokeDashCapProperty = DependencyProperty.Register(
            "StrokeDashCap",
            typeof(PenLineCap),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(PenLineCap.Flat, StrokeDashCapProperty_Changed)
        );

        private static void StrokeDashCapProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).StrokeDashCapProperty_Changed(e);
        }

        private void StrokeDashCapProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            var newValue = (PenLineCap)e.NewValue;

            base.StrokeDashCap = newValue;

            Stroke(newValue);
        }

        public override PenLineCap StrokeDashCap
        {
            get { return (PenLineCap)GetValue(StrokeDashCapProperty); }
            set { SetValue(StrokeDashCapProperty, value); }
        }

        #endregion

        #region FillBrush

        public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register(
            "FillBrush",
            typeof(Brush),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, FillBrushProperty_Changed)
        );

        private static void FillBrushProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).FillBrushProperty_Changed(e);
        }

        void FillBrushProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            base.FillBrush = (Brush)e.NewValue;

            Fill();
        }

        public override Brush FillBrush
        {
            get { return (Brush)GetValue(FillBrushProperty); }
            set { SetValue(FillBrushProperty, value); }
        }

        #endregion

        #region IsGridVisible

        public static readonly DependencyProperty IsGridVisibleProperty = DependencyProperty.Register(
            "IsGridVisible",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsGridVisibleChanged))
        );

        private static void OnIsGridVisibleChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).IsGridVisible = (bool)e.NewValue;
        }

        public override bool IsGridVisible
        {
            get { return (bool)GetValue(IsGridVisibleProperty); }
            set
            {
                if (value != IsGridVisible)
                {
                    SetValue(IsGridVisibleProperty, value);
                }
                base.IsGridVisible = value;
            }
        }
        #endregion

        #region IsGuidelinesEnabled

        public static readonly DependencyProperty IsGuidelinesEnabledProperty = DependencyProperty.Register(
            "IsGuidelinesEnabled",
            typeof(bool),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsGuidelinesEnabledChanged))
        );

        private static void OnIsGuidelinesEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).IsGuidelinesEnabled = (bool)e.NewValue;
        }

        public override bool IsGuidelinesEnabled
        {
            get { return (bool)GetValue(IsGuidelinesEnabledProperty); }
            set
            {
                if (value != IsGuidelinesEnabled)
                {
                    SetValue(IsGuidelinesEnabledProperty, value);
                }
                base.IsGuidelinesEnabled = value;
            }
        }
        #endregion

        #region Zoom

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom",
            typeof(double),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(OnZoomChanged))
        );

        private static void OnZoomChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)sender).OnZoomChanged((double)e.NewValue);
        }

        private void OnZoomChanged(double newValue)
        {
            this.Zoom = newValue;
        }

        public override double Zoom
        {
            get
            {
                return (double)GetValue(ZoomProperty);
            }
            set
            {
                SetValue(ZoomProperty, value);
                base.Zoom = value;
            }
        }

        public static double GetZoom(DependencyObject dep)
        {
            var editor = LayoutHelper.GetSelfOrAncestor<NoteEditor>(dep);
            if (editor != null)
            {
                return editor.Zoom;
            }
            else
            {
                return 1.0;
            }
        }

        #endregion

        #region LookupServices

        public static LookupCollection LookupServices { get; set; }

        #endregion

        #region IsImageSelected

        public static readonly DependencyProperty IsImageSelectedProperty = DependencyProperty.Register(
            "IsImageSelected",
            typeof(bool),
            typeof(NoteEditor)
        );

        public bool IsImageSelected
        {
            get { return (bool)GetValue(IsImageSelectedProperty); }
            set { SetValue(IsImageSelectedProperty, value); }
        }

        #endregion

        #region IsFileSelected

        public static readonly DependencyProperty IsFileSelectedProperty = DependencyProperty.Register(
            "IsFileSelected",
            typeof(bool),
            typeof(NoteEditor)
        );

        public bool IsFileSelected
        {
            get { return (bool)GetValue(IsFileSelectedProperty); }
            set { SetValue(IsFileSelectedProperty, value); }
        }

        #endregion

        #region ImageApplications

        public IEnumerable<FileAssociation> ImageApplications
        {
            get
            {
                var image = Selection.OfType<NImage>().FirstOrDefault();
                if (image != null)
                {
                    return FileAssociation.GetAssociations(image.Type);
                }
                else
                {
                    return new FileAssociation[0];
                }
            }
        }

        #endregion

        #region PreserveAspectRatio

        public static readonly DependencyProperty PreserveAspectRatioProperty = DependencyProperty.Register(
            "PreserveAspectRatio",
            typeof(bool),
            typeof(NoteEditor)
        );

        public bool PreserveAspectRatio
        {
            get { return (bool)GetValue(PreserveAspectRatioProperty); }
            set { SetValue(PreserveAspectRatioProperty, value); }
        }

        #endregion

        #region IsTableSelected

        public static readonly DependencyProperty IsTableSelectedProperty = DependencyProperty.Register(
            "IsTableSelected",
            typeof(bool),
            typeof(NoteEditor)
        );

        public bool IsTableSelected
        {
            get { return (bool)GetValue(IsTableSelectedProperty); }
            set { SetValue(IsTableSelectedProperty, value); }
        }

        #endregion

        #region EditingMode

        public static readonly DependencyProperty EditingModeProperty = DependencyProperty.Register(
            "EditingMode",
            typeof(EditingModes),
            typeof(NoteEditor),
            new FrameworkPropertyMetadata(EditingModes.Normal, EditingModeProperty_Changed)
        );

        private static void EditingModeProperty_Changed(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((NoteEditor)target).EditingModeProperty_Changed(e);
        }

        private void EditingModeProperty_Changed(DependencyPropertyChangedEventArgs e)
        {
            EditingModes newValue = (EditingModes)e.NewValue;

            IsHighlighting = (newValue == EditingModes.Highlighting);
            IsSelectingGeometry = (newValue == EditingModes.Selecting);
            IsDrawingPath = (newValue == EditingModes.Drawing);
            IsErasing = (newValue == EditingModes.Erasing);
            IsFilling = (newValue == EditingModes.Filling);
            IsStroking = (newValue == EditingModes.Stroking);
            IsDrawingLine = (newValue == EditingModes.InsertingLine);
            IsDrawingClipart = (newValue == EditingModes.InsertingClipart);
            IsDrawingText = (newValue == EditingModes.InsertingFreetext);

            if (!IsDrawingPath && !IsDrawingLine && !IsDrawingClipart && !IsDrawingText)
            {
                DrawingBoard.CancelPlacing();
                DrawingBoard.CancelDrawing();
            }

            Cursor = CursorForMode(newValue);
            ForceCursor = newValue != EditingModes.Normal;
        }

        public EditingModes EditingMode
        {
            get { return (EditingModes)GetValue(EditingModeProperty); }
            set { SetValue(EditingModeProperty, value); }
        }

        private void UpdateEditingMode()
        {
            if (IsHighlighting)
            {
                EditingMode = EditingModes.Highlighting;
            }
            else if (IsSelectingGeometry)
            {
                EditingMode = EditingModes.Selecting;
            }
            else if (IsDrawingPath)
            {
                EditingMode = EditingModes.Drawing;
            }
            else if (IsErasing)
            {
                EditingMode = EditingModes.Erasing;
            }
            else if (IsFilling)
            {
                EditingMode = EditingModes.Filling;
            }
            else if (IsStroking)
            {
                EditingMode = EditingModes.Stroking;
            }
            else if (IsDrawingLine)
            {
                EditingMode = EditingModes.InsertingLine;
            }
            else if (IsDrawingClipart)
            {
                EditingMode = EditingModes.InsertingClipart;
            }
            else if (IsDrawingText)
            {
                EditingMode = EditingModes.InsertingFreetext;
            }
            else
            {
                EditingMode = EditingModes.Normal;
            }
        }

        private Cursor CursorForMode(EditingModes mode)
        {
            switch (mode)
            {
                case EditingModes.Highlighting:
                    return GetHighlightCursor(HighlightBrush);
                case EditingModes.Selecting:
                    return Cursors.Cross;
                case EditingModes.Drawing:
                    return GetCursor("Pencil.cur");
                case EditingModes.Erasing:
                    return GetCursor("Eraser.cur");
                case EditingModes.Filling:
                    return GetCursor("Fill.cur");
                case EditingModes.Stroking:
                    return GetCursor("Stroke.cur");
                case EditingModes.InsertingLine:
                    return Cursors.Cross;
                case EditingModes.InsertingClipart:
                    return Cursors.Cross;
                case EditingModes.InsertingFreetext:
                    return Cursors.Cross;
                case EditingModes.Normal:
                    return Cursors.IBeam;
                default:
                    return null;
            }
        }

        #endregion

        #region DebugFlags

        public static readonly DependencyProperty DebugFlagsProperty = DocumentElement.DebugFlagsProperty.AddOwner(
            typeof(NoteEditor)
        );

        public static NDebugFlags GetDebugFlags(DependencyObject obj)
        {
            return (NDebugFlags)obj.GetValue(DebugFlagsProperty);
        }

        public static void SetDebugFlags(DependencyObject obj, NDebugFlags value)
        {
            obj.SetValue(DebugFlagsProperty, value);
        }

        public NDebugFlags DebugFlags
        {
            get { return GetDebugFlags(this); }
            set { SetDebugFlags(this, value); }
        }

        #endregion

        #region Updates

        int _SelectedImagesCount;
        int _SelectedFilesCount;
        int _SelectedTableCount;

        protected override void OnSelectionChanged(UIElement[] removedItems, UIElement[] addedItems)
        {
            base.OnSelectionChanged(removedItems, addedItems);

            OnSelectionFormatChanged();

            if (removedItems != null)
            {
                _SelectedImagesCount -= removedItems.OfType<NImage>().Count();
                _SelectedFilesCount -= removedItems.OfType<NFile>().Count();
                _SelectedTableCount -= removedItems.OfType<NTable>().Count();
            }

            if (addedItems != null)
            {
                _SelectedImagesCount += addedItems.OfType<NImage>().Count();
                _SelectedFilesCount += addedItems.OfType<NFile>().Count();
                _SelectedTableCount += addedItems.OfType<NTable>().Count();
            }

            IsImageSelected = _SelectedImagesCount > 0;
            IsFileSelected = _SelectedFilesCount > 0;
            IsTableSelected = _SelectedTableCount > 0;

            if (IsImageSelected)
            {
                PreserveAspectRatio = Selection.OfType<NImage>().First().PreserveAspectRatio;
            }

        }

        private void Descendant_FormatChanged(object sender, RoutedEventArgs e)
        {
            OnSelectionFormatChanged();

            e.Handled = true;
        }

        private void OnSelectionFormatChanged()
        {
            var fontFamily = (FontFamily)GetProperty(TextProperties.FontFamilyProperty);
            if (fontFamily != null)
            {
                if (_FontFamily != fontFamily)
                {
                    FontFamily = _FontFamily = fontFamily;
                }

                FontClass fontClass = null;
                string fontID = (string)GetProperty(TextProperties.FontClassProperty);
                if (!String.IsNullOrEmpty(fontID))
                {
                    fontClass = FontClass.FromID(fontID);
                }
                else
                {
                    fontClass = FontClass.FromFont(fontFamily);
                }

                if (fontClass != _FontClass)
                {
                    FontClass = _FontClass = fontClass;
                }

                object newValue = GetProperty(TextProperties.FontSizeProperty);
                if (newValue != null)
                {
                    int pointSize = (int)Math.Round((double)newValue * 72.0 / 96.0);
                    if (_FontSize != pointSize)
                    {
                        FontSize = _FontSize = pointSize;
                    }
                }

                newValue = GetProperty(TextProperties.FontWeightProperty);
                if (newValue != null)
                {
                    bool isBold = (FontWeight)newValue == FontWeights.Bold;
                    if (_Bold != isBold)
                    {
                        Bold = _Bold = isBold;
                    }
                }

                newValue = GetProperty(TextProperties.FontStyleProperty);
                if (newValue != null)
                {
                    bool isItalic = (FontStyle)newValue == FontStyles.Italic;
                    if (_Italic != isItalic)
                    {
                        Italic = _Italic = isItalic;
                    }
                }

                var decorations = (TextDecorationCollection)GetProperty(TextProperties.TextDecorationsProperty);

                bool isUnderline = (decorations != null) && decorations.Contains(TextDecorations.Underline[0]);
                if (_Underline != isUnderline)
                {
                    Underline = _Underline = isUnderline;
                }

                bool isStrikethrough = (decorations != null) && decorations.Contains(TextDecorations.Strikethrough[0]);
                if (_Strikethrough != isStrikethrough)
                {
                    Strikethrough = _Strikethrough = isStrikethrough;
                }

                var baseAlign = (BaselineAlignment)GetProperty(TextProperties.BaselineAlignmentProperty);

                bool isSuperscript = (baseAlign == BaselineAlignment.Superscript);
                if (_Superscript != isSuperscript)
                {
                    Superscript = _Superscript = isSuperscript;
                }

                bool isSubscript = (baseAlign == BaselineAlignment.Subscript);
                if (_Subscript != isSubscript)
                {
                    Subscript = _Subscript = isSubscript;
                }

                HasTextProperties = true;
            }
            else
            {
                HasTextProperties = false;
            }

            object alignment = GetProperty(TextParagraph.TextAlignmentPropertyName);
            if (alignment != null)
            {
                if (_Alignment != (TextAlignment)alignment)
                {
                    Alignment = _Alignment = (TextAlignment)alignment;
                }

                var newLineSpacing = this.GetDoubleProperty(TextParagraph.LineSpacingPropertyName);
                if (_LineSpacing != newLineSpacing)
                {
                    LineSpacing = _LineSpacing = newLineSpacing;
                }

                bool isBulleted = this.GetBoolProperty(TextParagraph.IsListItemPropertyName);
                if (_Bulleted != isBulleted)
                {
                    Bulleted = _Bulleted = isBulleted;
                }

                IListStyle newListStyle = null;
                if (isBulleted)
                {
                    newListStyle = (IListStyle)GetProperty(TextParagraph.ListStylePropertyName);
                }

                if (_ListStyle != newListStyle)
                {
                    ListStyle = _ListStyle = newListStyle;
                }

                HasParagraphProperties = true;
            }
            else
            {
                HasParagraphProperties = false;
            }
        }

        #endregion

        #endregion

        #region Drawing

        public PrimaryCanvas DrawingBoard { get; set; }

        private PrimaryCanvas CreateDrawingBoard()
        {
            var result = new PrimaryCanvas();

            DocumentPanel.SetPositioning(result, Positioning.Absolute);
            DocumentPanel.SetLeft(result, 0);
            DocumentPanel.SetRight(result, 0);
            DocumentPanel.SetTop(result, 0);
            DocumentPanel.SetBottom(result, 0);
            result.IsDeletable = false;

            return result;
        }

        public override void SelectTo(UIElement element)
        {
            Selection.Unselect(DrawingBoard);

            base.SelectTo(element);
        }

        protected void Canvas_DrawingSelected(object sender, DrawingEventArgs e)
        {
            NCanvas canvas = (NCanvas)sender;

            if (canvas != DrawingBoard)
            {
                if (UndoStack != null &&
                    (UndoStack.IsUndoing || UndoStack.IsRedoing))
                {
                    return;
                }

                canvas.Drawings.Remove(e.Drawing);
                DrawingBoard.Drawings.Add(e.Drawing);
                var transform = canvas.TransformToVisual(DrawingBoard);
                var position = transform.Transform(new Point(0, 0));
                DrawingBoard.MoveDrawing(e.Drawing, position - new Point(0, 0));
                DrawingBoard.Selection.Select(e.Drawing);

                if (canvas.Drawings.Count == 0)
                {
                    Remove(canvas);
                }
                else
                {
                    if (canvas.Selection.Count == 0)
                    {
                        Selection.Unselect(canvas);
                    }

                    canvas.Positioning = Positioning.Absolute;
                    canvas.SizeToContent();
                    UpdateLayout();
                    canvas.Positioning = Positioning.Overlapped;
                }
            }

            Selection.Select(DrawingBoard);
        }

        protected void Canvas_DrawingUnselected(object sender, DrawingEventArgs e)
        {
            NCanvas sourceCanvas = (NCanvas)sender;
            if (sourceCanvas == DrawingBoard)
            {
                if (!sourceCanvas.Drawings.Contains(e.Drawing))
                {
                    // Drawing is unselected because it's being deleted
                    if (sourceCanvas.Selection.Count == 0)
                    {
                        // Automatically select nearest static element
                        SelectDefault(Positioning.Static, e.Drawing.VisualBounds.TopLeft);
                    }
                    return;
                }

                if (UndoStack != null &&
                    (UndoStack.IsUndoing || UndoStack.IsRedoing))
                {
                    return;
                }

                // Get canvases overlapped by drawing

                NCanvas[] hitCanvases = null;

                if (!e.Drawing.RenderedBounds.IsEmpty)
                {
                    hitCanvases = HitCanvases(e.Drawing.RenderedBounds, except: DrawingBoard);
                }

                // Determine which canvas drawing will be added to

                NCanvas targetCanvas = null;

                if (hitCanvases != null && hitCanvases.Length > 0)
                {
                    // Merge contents of all canvases overlapped by drawing

                    foreach (NCanvas hitCanvas in hitCanvases)
                    {
                        if (targetCanvas == null)
                        {
                            targetCanvas = hitCanvas;
                        }
                        else
                        {
                            targetCanvas.Merge(hitCanvas);
                            Remove(hitCanvas);
                            hitCanvas.SizeToContent();
                        }
                    }
                }
                else
                {
                    targetCanvas = new NCanvas();
                    targetCanvas.Positioning = Positioning.Overlapped;
                    Append(targetCanvas);
                }

                // Remove drawing from source canvas

                sourceCanvas.Drawings.Remove(e.Drawing);

                // Add drawing to target canvas

                if (targetCanvas != null)
                {
                    targetCanvas.Drawings.Add(e.Drawing);

                    // Adjust drawing position
                    var transform = sourceCanvas.TransformToVisual(targetCanvas);
                    var position = transform.Transform(new Point(0, 0));
                    targetCanvas.MoveDrawing(e.Drawing, position - new Point(0, 0));

                    // Adjust canvas size
                    targetCanvas.Positioning = Positioning.Absolute;
                    targetCanvas.SizeToContent();
                    UpdateLayout();
                    targetCanvas.Positioning = Positioning.Overlapped;
                }
            }
        }

        private NCanvas[] HitCanvases(Rect bounds, NCanvas except = null)
        {
            const int MARGIN = 10;

            bounds = new Rect(bounds.X - MARGIN, bounds.Y - MARGIN, bounds.Width + MARGIN * 2.0, bounds.Height + MARGIN * 2.0);

            List<NCanvas> result = new List<NCanvas>();

            VisualTreeHelper.HitTest(
                this,
                null,
                new HitTestResultCallback(hit =>
                {
                    NCanvas hitCanvas = LayoutHelper.GetSelfOrAncestor<NCanvas>(hit.VisualHit);
                    if (hitCanvas != null && hitCanvas.Visibility == Visibility.Visible && hitCanvas != except && !result.Contains(hitCanvas))
                        result.Add(hitCanvas);
                    return HitTestResultBehavior.Continue;
                }),
                new GeometryHitTestParameters(
                    new RectangleGeometry(bounds)
                )
            );

            return result.ToArray();
        }

        #endregion

        #region Children

        public override int InsertIndex
        {
            get
            {
                return Math.Max(base.InsertIndex, InternalChildren.IndexOf(DrawingBoard) + 1);
            }
        }

        protected override void OnChildAdded(UIElement element)
        {
            base.OnChildAdded(element);

            if (element is NHeading)
            {
                OnHeadingAdded((NHeading)element);
            }
            else if (element is NCanvas)
            {
                OnCanvasAdded((NCanvas)element);
            }
            else if (!(element is TextParagraph))
            {
                Panel.SetZIndex(element, 1);
            }
        }

        protected override void OnChildRemoved(UIElement element)
        {
            base.OnChildRemoved(element);

            if (element is NHeading)
            {
                OnHeadingRemoved((NHeading)element);
            }
            else if (element is NCanvas)
            {
                OnCanvasRemoved((NCanvas)element);
            }
        }

        void OnHeadingAdded(NHeading heading)
        {
            if (heading != TitleElement)
            {
                if (TitleElement != null)
                {
                    Remove(TitleElement);
                }
                TitleElement = heading;
            }
        }

        void OnHeadingRemoved(NHeading heading)
        {
            if (heading == TitleElement)
            {
                TitleElement = null;
            }
        }

        void OnCanvasAdded(NCanvas canvas)
        {
            if (canvas == DrawingBoard)
            {
                Panel.SetZIndex(canvas, 100);
            }
            else
            {
                Panel.SetZIndex(canvas, 99);
            }

            if (canvas == DrawingBoard)
            {
                canvas.DrawingModeChanged += DrawingBoard_ModeChanged;
            }

            canvas.DrawingSelected += Canvas_DrawingSelected;
            canvas.DrawingUnselected += Canvas_DrawingUnselected;

        }

        void OnCanvasRemoved(NCanvas canvas)
        {
            canvas.DrawingModeChanged -= DrawingBoard_ModeChanged;
            canvas.DrawingSelected -= Canvas_DrawingSelected;
            canvas.DrawingUnselected -= Canvas_DrawingUnselected;
        }

        #endregion

        #region Editing

        public void Undo()
        {
            if (UndoStack != null)
            {
                UndoStack.Undo();
            }
        }

        public void Redo()
        {
            if (UndoStack != null)
            {
                UndoStack.Redo();
            }
        }

        public override IList<object> Cut()
        {
            return base.Cut();
        }

        public override IList<object> Copy()
        {
            return base.Copy();
        }

        public void Paste(PasteMode mode = PasteMode.Auto)
        {
            var clipboardData = NClipboard.GetData();

            Paste(clipboardData, mode);
        }

        public IList<string> PasteFormats
        {
            get
            {
                return NClipboard.GetFormats();
            }
        }

        public void PasteSpecial(string format, PasteMode mode = PasteMode.Auto)
        {
            var clipboardData = NClipboard.GetData(format);

            Paste(clipboardData, mode);
        }

        public void Paste(IList<object> items, PasteMode mode)
        {
            if (mode == PasteMode.Auto)
            {
                if (items.Count == 1)
                {
                    mode = PasteMode.MatchFormatting;
                }
                else
                {
                    mode = PasteMode.KeepFormatting;
                }
            }

            switch (mode)
            {
                case PasteMode.MatchFormatting:
                    MatchFormatting(items);
                    break;
                case PasteMode.ClearFormatting:
                    Navigable.SelectAll(items);
                    Formattable.ResetProperties(items);
                    break;
                case PasteMode.KeepFormatting:
                default:
                    break;
            }

            Paste(items);
        }

        void MatchFormatting(IList<object> items)
        {
            string[] propertyNames = new[] {
                TextProperties.FontClassProperty,
                TextProperties.FontFamilyProperty,
                TextProperties.FontSizeProperty,
                TextProperties.ForegroundBrushProperty,
                TextProperties.BackgroundBrushProperty,
                TextParagraph.LineSpacingPropertyName
            };

            Navigable.SelectAll(items);

            foreach (string propertyName in propertyNames)
            {
                var value = Formattable.GetProperty(Selection, propertyName);
                if (value != null)
                {
                    Formattable.SetProperty(items, propertyName, value);
                }
            }
        }

        public override bool Delete()
        {
            return base.Delete();
        }

        public override UndoStack UndoStack
        {
            get { return base.UndoStack; }
            set
            {
                var oldValue = base.UndoStack;
                var newValue = value;

                if (newValue != oldValue)
                {
                    base.UndoStack = newValue;
                    OnUndoStackChanged(oldValue, newValue);
                }
            }
        }

        private void OnUndoStackChanged(UndoStack oldStack, UndoStack newStack)
        {
            if (oldStack != null)
            {
                oldStack.Edited -= UndoStack_Edited;
            }

            if (newStack != null)
            {
                newStack.Edited += UndoStack_Edited;
            }
        }

        private void UndoStack_Edited(object sender, EventArgs e)
        {
            OnEdited();
        }

        #endregion

        #region IMoveable

        protected override void MoveSelection(Vector delta)
        {
            base.MoveSelection(delta);

            ResetIdleTimer();
        }

        protected override void EndMoveSelection()
        {
            base.EndMoveSelection();

            OnEdited();
        }

        #endregion

        #region ISearchable

        public override bool FindFirst(string pattern, RegexOptions options)
        {
            var result = Searchable.FindFirst(InternalChildren, pattern, options);
            if (result != null)
            {
                if (!(result is NCanvas))
                {
                    Selection.SelectOnly((UIElement)result);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool FindLast(string pattern, RegexOptions options)
        {
            var result = Searchable.FindLast(InternalChildren, pattern, options);
            if (result != null)
            {
                if (!(result is NCanvas))
                {
                    Selection.SelectOnly((UIElement)result);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool FindNext(string pattern, RegexOptions options)
        {
            var result = Searchable.FindNext(InternalChildren, pattern, options, Selection.LastOrDefault());
            if (result != null)
            {
                if (!(result is NCanvas))
                {
                    Selection.SelectOnly((UIElement)result);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool FindPrevious(string pattern, RegexOptions options)
        {
            var result = Searchable.FindPrevious(InternalChildren, pattern, options, Selection.LastOrDefault());
            if (result != null)
            {
                if (!(result is NCanvas))
                {
                    Selection.SelectOnly((UIElement)result);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IHasResources

        public override IEnumerable<string> ResourceNames
        {
            get
            {
                if (DrawingBoard.Drawings.Count > 0)
                {
                    return base.ResourceNames;
                }
                else
                {
                    return base.ResourceNames.Except(DrawingBoard.ResourceNames);
                }
            }
        }

        protected override void OnSetResource(string fileName, Stream stream)
        {
            if (!IsFlushing)
            {
                using (var undo = new UndoScope(UndoStack, "Load file", isEditing: false))
                {
                    base.OnSetResource(fileName, stream);
                }
            }
        }

        #endregion

        #region Operations

        #region Navigation Operations

        public void SelectTitle()
        {
            if (TitleElement != null)
            {
                Selection.SelectOnly(TitleElement);
                TitleElement.MoveToStart();
                TitleElement.SelectToEnd();
            }
        }

        /// <summary>
        /// Select the nth occurrence of the given phrase.
        /// 
        /// If count > 0, then select the nth occurrence of phrase, starting
        /// the search at the beginning of the note.
        /// 
        /// If count &lt; 0, then select the -nth occurrence of phrase, starting
        /// the search at the end of the note.
        /// 
        /// (n = count)
        /// </summary>
        public void SelectNth(string pattern, RegexOptions options, int count)
        {
            if (_Content == null)
            {
                _SearchPattern = pattern;
                _SearchOptions = options;
                _SelectCount = count;
                return;
            }

            this.FindNth(pattern, options, count);
        }

        private string _SearchPattern;
        private RegexOptions _SearchOptions;
        private int _SelectCount;

        #endregion

        #region Formatting Operations

        public void ClearFormatting()
        {
            ResetProperties();
        }

        public void DecreaseFontSize()
        {
            FontSize -= 1;
        }


        public void IncreaseFontSize()
        {
            FontSize += 1;
        }

        public void IncreaseIndentation()
        {
            using (var undo = new UndoScope(UndoStack, "Indent"))
            {
                UpdateIndentation(1);
            }
        }

        public void DecreaseIndentation()
        {
            using (var undo = new UndoScope(UndoStack, "Outdent"))
            {
                UpdateIndentation(-1);
            }
        }

        void UpdateIndentation(int count)
        {
            // Apply to all selected objects, and all descendants of selected collapsed list items

            var selected = Selection.OfType<IFormattable>().ToArray();

            var descendants = selected.OfType<TextParagraph>()
                .Where(p => p.IsCollapsed)
                .SelectMany(p => p.List.Descendants)
                .Cast<IFormattable>();

            foreach (var element in selected.Union(descendants).ToArray())
            {
                UpdateIndentation(element, count);
            }
        }

        static void UpdateIndentation(IFormattable element, int count)
        {
            // update indentation
            var indentationProperty = element.GetProperty(TextParagraph.LeftMarginPropertyName);
            if (indentationProperty != null)
            {
                var indentation = (double)indentationProperty;
                indentation = Math.Max(indentation + 25.0 * count, 0);
                element.SetProperty(TextParagraph.LeftMarginPropertyName, indentation);
            }

            // update list style
            var listStyle = (Editor.IListStyle)element.GetProperty(TextParagraph.ListStylePropertyName);
            if (listStyle != null)
            {
                for (int i = 0; i < Math.Abs(count) && listStyle != null; i++)
                {
                    listStyle = count > 0 ? listStyle.NextStyle : listStyle.PreviousStyle;
                }
                element.SetProperty(TextParagraph.ListStylePropertyName, listStyle);
            }

            // update list level
            var listLevelProperty = element.GetProperty(TextParagraph.ListLevelPropertyName);
            if (listLevelProperty != null)
            {
                var listLevel = (int)listLevelProperty;
                listLevel = Math.Max(listLevel + count, 0);
                element.SetProperty(TextParagraph.ListLevelPropertyName, listLevel);
            }
        }

        public void AlignLeft()
        {
            SetProperty(TextParagraph.TextAlignmentPropertyName, TextAlignment.Left);
            Alignment = _Alignment = TextAlignment.Left;
        }

        public void AlignCenter()
        {
            SetProperty(TextParagraph.TextAlignmentPropertyName, TextAlignment.Center);
            Alignment = _Alignment = TextAlignment.Center;
        }

        public void AlignRight()
        {
            SetProperty(TextParagraph.TextAlignmentPropertyName, TextAlignment.Right);
            Alignment = _Alignment = TextAlignment.Right;
        }

        public void AlignJustify()
        {
            SetProperty(TextParagraph.TextAlignmentPropertyName, TextAlignment.Justify);
            Alignment = _Alignment = TextAlignment.Justify;
        }

        public void ToggleList()
        {
            Bulleted = !Bulleted;
        }

        public void ApplyListStyle(IListStyle style)
        {
            if (style != null)
            {
                SetProperty(TextParagraph.ListStylePropertyName, style);
                SetProperty(TextParagraph.IsListItemPropertyName, true);
            }
            else
            {
                SetProperty(TextParagraph.IsListItemPropertyName, false);
            }
        }

        #endregion

        #region Drawing Operations

        public void InsertLine(Shape line)
        {
            if (line == null)
            {
                throw new NullReferenceException();
            }

            using (UndoScope undo = new UndoScope(UndoStack, "Insert Line"))
            {
                line = (Shape)line.Clone();

                InsertDrawing(line);

                EditingMode = EditingModes.InsertingLine;

                IsDrawingLine = true;
            }
        }

        public void InsertClipart(Shape clipart, bool oneClick = false)
        {
            if (clipart == null)
            {
                throw new ArgumentNullException();
            }

            using (UndoScope undo = new UndoScope(UndoStack, "Insert Clipart"))
            {
                if (clipart is Marker)
                {
                    clipart = new ShapeGroup((Marker)clipart);
                }
                else
                {
                    clipart = (Shape)clipart.Clone();
                }

                InsertDrawing(clipart, oneClick);

                EditingMode = EditingModes.InsertingClipart;

                IsDrawingClipart = true;
            }
        }

        private void InsertDrawing(Shape drawing, bool oneClick = false)
        {
            // Remove any embedded thumbnails

            if (drawing is ShapeGroup)
            {
                var group = (ShapeGroup)drawing;
                var thumbs = group.Drawings.Where(d => d.IsThumb);
                foreach (var thumb in thumbs.ToArray())
                {
                    group.Drawings.Remove(thumb);
                }
            }

            // Apply currently-selected stroke properties

            if (!(drawing is ShapeGroup))
            {
                drawing.StrokeBrush = StrokeBrush;
                drawing.StrokeWidth = StrokeWidth;
                drawing.StrokeLineCap = StrokeDashCap;

                if (drawing.StrokeDashArray == Shape.DefaultStrokeDashArray)
                {
                    drawing.StrokeDashArray = StrokeDashArray;
                }
            }

            // Apply currently-selected fill properties

            if (!(drawing is ShapeGroup) && !(drawing is NPath) && !(drawing is LineBase))
            {
                drawing.FillBrush = (FillBrush != Brushes.Transparent) ? FillBrush : null;
            }

            if (DrawingBoard.Mode != DrawingMode.Ready)
            {
                DrawingBoard.Delete();
            }

            DrawingBoard.PlaceDrawing(drawing, oneClick);
            Selection.SelectOnly(DrawingBoard);
        }

        public void InsertTextBox()
        {
            using (UndoScope undo = new UndoScope(UndoStack, "Insert TextBox"))
            {
                InsertDrawing(new NTextBox());

                IsDrawingText = true;
            }
        }

        public void FlipHorizontal()
        {
            DrawingBoard.FlipHorizontal();
        }

        public void FlipVertical()
        {
            DrawingBoard.FlipVertical();
        }

        public void BringForward()
        {

        }

        public void BringToFront()
        {

        }

        public void SendBackward()
        {

        }

        public void SendToBack()
        {

        }

        #endregion

        #region Insert Operations

        public void InsertSymbol(string symbol)
        {
            Paste(new[] { symbol });
            MoveRight();
        }


        public void InsertImage(bool isPlacing)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Filter = "Image Files|*.jpg;*.gif;*.png;*.bmp|All Files|*.*";

            if (dialog.ShowDialog() == true)
            {
                InsertImage(dialog.FileName, isPlacing);
            }
        }

        public void InsertImage(string filePath, bool isPlacing)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                InsertImage(isPlacing);
                return;
            }

            byte[] data;

            try
            {
                data = File.ReadAllBytes(filePath);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                return;
            }

            InsertImage(data, isPlacing);
        }

        public NImage InsertImage(byte[] data, bool isPlacing)
        {
            NImage image = new NImage();

            image.Data = data;
            image.PreserveAspectRatio = true;

            InsertImage(image, isPlacing);

            return image;
        }

        public NImage InsertImage(BitmapSource source, bool isPlacing)
        {
            return InsertImage(InsertIndex, source, isPlacing);
        }

        public NImage InsertImage(int index, BitmapSource source, bool isPlacing)
        {
            NImage image = new NImage();

            image.Source = source;
            image.PreserveAspectRatio = true;

            InsertImage(index, image, isPlacing);

            return image;
        }

        public void InsertImage(NImage image, bool isPlacing)
        {
            InsertImage(InsertIndex, image, isPlacing);
        }

        public void InsertImage(int index, NImage image, bool isPlacing)
        {
            if (isPlacing && image.OriginalWidth > 400)
            {
                double deltaX = 400 - image.OriginalWidth;
                double deltaY = deltaX / image.AspectRatio;
                image.Resize(new Vector(deltaX, deltaY));
            }

            Insert(index, image);

            if (isPlacing)
            {
                image.Positioning = Positioning.Overlapped;
                image.IsPlacing = isPlacing;
            }
            else
            {
                image.Positioning = Positioning.Static;
                UpdateLayout();
                image.Positioning = Positioning.Overlapped;
                image.Position = new Point(20, 0);
            }
        }

        public void InsertHyperlink(string type = null)
        {
            string hyperlinkText = SelectedText;
            string hyperlinkURL = (string)GetProperty(TextProperties.HyperlinkURLProperty);

            var dialog = new Dialogs.HyperlinkDialog();

            dialog.HyperlinkText = hyperlinkText;
            dialog.HyperlinkURL = hyperlinkURL;

            switch (type)
            {
                case "URL":
                    dialog.URLRadioButton.IsChecked = true;
                    break;
                case "File":
                    dialog.FileRadioButton.IsChecked = true;
                    break;
                case "Note":
                    dialog.NoteRadioButton.IsChecked = true;
                    break;
            }

            if (dialog.ShowDialog() == true)
            {
                if (dialog.HyperlinkText != hyperlinkText)
                {
                    Replace(dialog.HyperlinkText);
                }

                if (dialog.HyperlinkURL != hyperlinkURL)
                {
                    SetProperty(TextProperties.HyperlinkURLProperty, dialog.HyperlinkURL);
                }
            }
        }

        public void RemoveHyperlink()
        {
            SetProperty(TextProperties.HyperlinkURLProperty, null);
        }

        public void InsertStickyNote()
        {
            var stickyNote = Shape.FromResource("SilverNote.Clipart.Sticky_Notes.02 Sticky note (yellow).svg");
            if (stickyNote != null)
            {
                InsertClipart(stickyNote);
            }
        }

        public void InsertSection()
        {
            using (new UndoScope(UndoStack, "Insert section"))
            {
                if (!IsTextSelected)
                {
                    var heading = new TextParagraph { Text = "Untitled" };
                    Insert(heading);
                    Selection.SelectOnly(heading);
                }

                Selection.OfType<INavigable>().ForEach(item => item.SelectAll());

                SetProperty(TextProperties.FontClassProperty, FontClass.Heading2.ID);
            }
        }

        public void InsertSubsection()
        {
            using (new UndoScope(UndoStack, "Insert sub-section"))
            {
                if (!IsTextSelected)
                {
                    var heading = new TextParagraph { Text = "Untitled" };
                    Insert(heading);
                    Selection.SelectOnly(heading);
                }

                Selection.OfType<INavigable>().ForEach(item => item.SelectAll());

                SetProperty(TextProperties.FontClassProperty, FontClass.Heading3.ID);
            }
        }

        void InsertTable()
        {
            var dialog = new Dialogs.NewTableDialog();

            if (dialog.ShowDialog() == true)
            {
                NTable table = new NTable();

                table.Width = dialog.ColumnCount * 100;
                table.BorderStyle = BorderStyles.Solid;
                table.BorderColor = Colors.Gray;
                table.BorderWidth = 1;
                table.BorderRadius = 6;

                for (int i = 0; i < dialog.RowCount; i++)
                {
                    NTableRow row = new NTableRow(table.Body);

                    for (int j = 0; j < dialog.ColumnCount; j++)
                    {
                        NTableCell cell = row.CreateCell(true);
                        cell.BorderStyle = table.BorderStyle;
                        cell.BorderColor = table.BorderColor;
                        cell.BorderWidth = table.BorderWidth;
                        row.Cells.Add(cell);
                    }

                    table.AppendRow(row);
                }

                Append(table);
                table.IsPlacing = true;
            }
        }

        #endregion

        #region Image Operations

        public void OpenImage()
        {
            foreach (var image in Selection.OfType<NImage>())
            {
                image.OpenImage();
            }
        }

        public void EditImage()
        {
            foreach (var image in Selection.OfType<NImage>())
            {
                image.EditImage();
            }
        }

        public void OpenImageWith(string appName)
        {
            foreach (var image in Selection.OfType<NImage>())
            {
                image.OpenImageWith(appName);
            }
        }

        public void ResetImageSize()
        {
            foreach (var image in Selection.OfType<NImage>())
            {
                image.ResetSize();
            }
        }

        public void TogglePreserveAspectRatio()
        {
            var images = Selection.OfType<NImage>();

            var firstImage = images.FirstOrDefault();
            if (firstImage != null)
            {
                bool newValue = !firstImage.PreserveAspectRatio;

                foreach (var image in images)
                {
                    image.PreserveAspectRatio = newValue;
                }

                PreserveAspectRatio = newValue;
            }
        }

        #endregion

        #region File Operations

        public void OpenFile()
        {
            foreach (var file in Selection.OfType<NFile>())
            {
                file.OpenFile();
            }
        }

        #endregion

        #region Table Operations

        public void SetTableBackgroundBrush(Brush brush)
        {
            foreach (var table in Selection.OfType<NTable>().ToArray())
            {
                table.SelectionBackground = brush;
            }
        }

        public void SetTableBorderBrush(Brush brush)
        {
            foreach (var table in Selection.OfType<NTable>().ToArray())
            {
                if (brush == null)
                {
                    table.SelectionBorderColor = Colors.Transparent;
                }
                else if (brush is SolidColorBrush)
                {
                    table.SelectionBorderColor = ((SolidColorBrush)brush).Color;
                }
            }
        }

        public void SetTableBorderWidth(double width)
        {
            foreach (var table in Selection.OfType<NTable>().ToArray())
            {
                table.SelectionBorderWidth = width;
            }
        }

        public bool CanMergeTableCells
        {
            get
            {
                return Selection.OfType<NTable>().Any(table => table.SelectedCells.Count() > 1);
            }
        }

        public void MergeTableCells()
        {
            foreach (var table in Selection.OfType<NTable>().ToArray())
            {
                table.MergeCells();
            }
        }

        public bool CanSplitTableCells
        {
            get
            {
                return Selection.OfType<NTable>().Any(table => table.SelectedCells.Any(cell => cell.RowSpan > 1 || cell.ColumnSpan > 1));
            }
        }

        public void SplitTableCells()
        {
            foreach (var table in Selection.OfType<NTable>().ToArray())
            {
                table.SplitCells();
            }
        }

        #endregion

        #region Utility Operations

        public void OpenHyperlink(string uri, string target = "_blank")
        {
            var clicked = new HyperlinkClickedEventArgs(TextParagraph.HyperlinkClicked, this, uri, target);

            RaiseEvent(clicked);
        }

        public void Lookup(string serviceName)
        {
            if (LookupServices == null)
            {
                return;
            }

            var service = LookupServices.Find(item => serviceName.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase));
            if (service != null && !String.IsNullOrWhiteSpace(service.Command))
            {
                LookupHelper.Lookup(service.Command, SelectedText);
            }
        }

        #endregion

        #endregion

        #region Commands

        void LoadCommandBindings()
        {
            // SilverNote Commands

            CommandBindings.AddRange(new[] {

                // Editing Commands

                new CommandBinding(NEditingCommands.Undo, UndoCommand_Executed),
                new CommandBinding(NEditingCommands.Redo, RedoCommand_Executed),
                new CommandBinding(NEditingCommands.Cut, CutCommand_Executed),
                new CommandBinding(NEditingCommands.Copy, CopyCommand_Executed),
                new CommandBinding(NEditingCommands.Paste, PasteCommand_Executed),
                new CommandBinding(NEditingCommands.PasteSpecial, PasteSpecialCommand_Executed),
                new CommandBinding(NEditingCommands.DeleteForward, DeleteForwardCommand_Executed),
                new CommandBinding(NEditingCommands.DeleteBack, DeleteBackCommand_Executed),
                new CommandBinding(NEditingCommands.SelectAll, SelectAllCommand_Executed),
                new CommandBinding(NEditingCommands.FindNext, FindNextCommand_Executed),
                new CommandBinding(NEditingCommands.FindPrevious, FindPreviousCommand_Executed),
                new CommandBinding(NEditingCommands.ReplaceOnce, ReplaceOnceCommand_Executed),
                new CommandBinding(NEditingCommands.ReplaceAll, ReplaceAllCommand_Executed),

                // Text Commands

                new CommandBinding(NTextCommands.DeleteForwardByWord, DeleteForwardByWordCommand_Executed, DeleteForwardCommand_CanExecute),
                new CommandBinding(NTextCommands.DeleteForwardByParagraph, DeleteForwardByParagraphCommand_Executed, DeleteForwardCommand_CanExecute),
                new CommandBinding(NTextCommands.DeleteBackByWord, DeleteBackByWordCommand_Executed, DeleteBackCommand_CanExecute),
                new CommandBinding(NTextCommands.DeleteBackByParagraph, DeleteBackByParagraphCommand_Executed, DeleteBackCommand_CanExecute),

                // Navigation Commands

                new CommandBinding(NNavigationCommands.MoveUpByLine, MoveUpByLineCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveDownByLine, MoveDownByLineCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveLeftByCharacter, MoveLeftByCharacterCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveRightByCharacter, MoveRightByCharacterCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveLeftByWord, MoveLeftByWordCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveRightByWord, MoveRightByWordCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToLineEnd, MoveToLineEndCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToLineStart, MoveToLineStartCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToParagraphEnd, MoveToParagraphEndCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToParagraphStart, MoveToParagraphStartCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToDocumentEnd, MoveToDocumentEndCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToDocumentStart, MoveToDocumentStartCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveDownByPage, MoveDownByPageCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveUpByPage, MoveUpByPageCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectDownByLine, SelectDownByLineCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectDownByPage, SelectDownByPageCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectLeftByCharacter, SelectLeftByCharacterCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectRightByCharacter, SelectRightByCharacterCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectLeftByWord, SelectLeftByWordCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectRightByWord, SelectRightByWordCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToLineEnd, SelectToLineEndCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToLineStart, SelectToLineStartCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToParagraphEnd, SelectToParagraphEndCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToParagraphStart, SelectToParagraphStartCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToDocumentEnd, SelectToDocumentEndCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToDocumentStart, SelectToDocumentStartCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectUpByLine, SelectUpByLineCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectUpByPage, SelectUpByPageCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectTitle, SelectTitleCommand_Executed),
                new CommandBinding(NNavigationCommands.TabForward, TabForwardCommand_Executed),
                new CommandBinding(NNavigationCommands.TabBackward, TabBackwardCommand_Executed),

                // View Commands

                new CommandBinding(NViewCommands.ToggleGrid, ToggleGridCommand_Executed),
                new CommandBinding(NViewCommands.ToggleGuidelines, ToggleGuidelinesCommand_Executed),
                new CommandBinding(NViewCommands.ZoomIn, ZoomInCommand_Executed),
                new CommandBinding(NViewCommands.ZoomOut, ZoomOutCommand_Executed),

                // Formatting Commands

                new CommandBinding(NFormattingCommands.AlignLeft, AlignLeftCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.AlignCenter, AlignCenterCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.AlignRight, AlignRightCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.AlignTop, AlignTopCommand_Executed),
                new CommandBinding(NFormattingCommands.AlignMiddle, AlignMiddleCommand_Executed),
                new CommandBinding(NFormattingCommands.AlignBottom, AlignBottomCommand_Executed),
                new CommandBinding(NFormattingCommands.AlignJustify, AlignJustifyCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.DecreaseFontSize, DecreaseFontSizeCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.DecreaseIndentation, DecreaseIndentationCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.EnterLineBreak, EnterLineBreakCommand_Executed, EnterLineBreakCommand_CanExecute),
                new CommandBinding(NFormattingCommands.SetFontFamily, SetFontFamilyCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.SetFontSize, SetFontSizeCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.ClearFormatting, ClearFormattingCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.SetTextColor, SetTextColorCommand_Executed),
                new CommandBinding(NFormattingCommands.Highlight, HighlightCommand_Executed),
                new CommandBinding(NFormattingCommands.SetListStyle, SetListStyleCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.ToggleList, ToggleListCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.IncreaseFontSize, IncreaseFontSizeCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.IncreaseIndentation, IncreaseIndentationCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.ToggleBold, ToggleBoldCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.ToggleItalic, ToggleItalicCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.ToggleUnderline, ToggleUnderlineCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.ToggleStrikethrough, ToggleStrikethroughCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.ToggleSuperscript, ToggleSuperscriptCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.ToggleSubscript, ToggleSubscriptCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.SetLineSpacing, SetLineSpacingCommand_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.SetLineSpacing1, SetLineSpacing1Command_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.SetLineSpacing15, SetLineSpacing15Command_Executed, FormattingCommand_CanExecute),
                new CommandBinding(NFormattingCommands.SetLineSpacing2, SetLineSpacing2Command_Executed, FormattingCommand_CanExecute),

                // Drawing Commands

                new CommandBinding(NDrawingCommands.Select, SelectCommand_Executed),
                new CommandBinding(NDrawingCommands.Erase, EraseCommand_Executed),
                new CommandBinding(NDrawingCommands.InsertPath, InsertPathCommand_Executed),
                new CommandBinding(NDrawingCommands.InsertLine, InsertLineCommand_Executed),
                new CommandBinding(NDrawingCommands.InsertClipart, InsertClipartCommand_Executed),
                new CommandBinding(NDrawingCommands.InsertTextBox, InsertTextBoxCommand_Executed),
                new CommandBinding(NDrawingCommands.Stroke, StrokeCommand_Executed),
                new CommandBinding(NDrawingCommands.Fill, FillCommand_Executed),
                new CommandBinding(NDrawingCommands.SetEffect, SetEffectCommand_Executed),
                new CommandBinding(NDrawingCommands.RotateRight, RotateRightCommand_Executed),
                new CommandBinding(NDrawingCommands.RotateLeft, RotateLeftCommand_Executed),
                new CommandBinding(NDrawingCommands.FlipHorizontal, FlipHorizontalCommand_Executed),
                new CommandBinding(NDrawingCommands.FlipVertical, FlipVerticalCommand_Executed),
                new CommandBinding(NDrawingCommands.BringForward, BringForwardCommand_Executed),
                new CommandBinding(NDrawingCommands.SendBackward, SendBackwardCommand_Executed),
                new CommandBinding(NDrawingCommands.BringToFront, BringToFrontCommand_Executed),
                new CommandBinding(NDrawingCommands.SendToBack, SendToBackCommand_Executed),
                new CommandBinding(NDrawingCommands.Group, GroupCommand_Executed),
                new CommandBinding(NDrawingCommands.Ungroup, UngroupCommand_Executed),
                new CommandBinding(NDrawingCommands.EditSource, EditSourceCommand_Executed),

                // Insertion Commands

                new CommandBinding(NInsertionCommands.Insert, InsertCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertSection, InsertSectionCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertSubsection, InsertSubsectionCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertSymbol, InsertSymbolCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertHyperlink, InsertHyperlinkCommand_Executed),
                new CommandBinding(NInsertionCommands.RemoveHyperlink, RemoveHyperlinkCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertStickyNote, InsertStickyNoteCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertTable, InsertTableCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertImage, InsertImageCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertAudio, InsertAudioCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertVideo, InsertVideoCommand_Executed),
                new CommandBinding(NInsertionCommands.InsertFile, InsertFileCommand_Executed),

                // Image Commands

                new CommandBinding(NImageCommands.Open, OpenImageCommand_Executed),
                new CommandBinding(NImageCommands.Edit, EditImageCommand_Executed),
                new CommandBinding(NImageCommands.OpenWith, OpenImageWithCommand_Executed),
                new CommandBinding(NImageCommands.ResetSize, ResetImageSizeCommand_Executed),
                new CommandBinding(NImageCommands.TogglePreserveAspectRatio, TogglePreserveAspectRatio_Executed),

                // File Commands

                new CommandBinding(NFileCommands.Open, OpenFileCommand_Executed),

                // Table Commands

                new CommandBinding(NTableCommands.SetBorderBrush, SetTableBorderBrushCommand_Executed),
                new CommandBinding(NTableCommands.SetBorderWidth, SetTableBorderWidthCommand_Executed),
                new CommandBinding(NTableCommands.SetBackground, SetTableBackgroundCommand_Executed),
                new CommandBinding(NTableCommands.MergeCells, MergeTableCellsCommand_Executed, MergeTableCellsCommand_CanExecute),
                new CommandBinding(NTableCommands.SplitCells, SplitTableCellsCommand_Executed, SplitTableCellsCommand_CanExecute),

                // Utility Commands

                new CommandBinding(NUtilityCommands.DrillDown, DrillDownCommand_Executed),
                new CommandBinding(NUtilityCommands.DrillThrough, DrillThroughCommand_Executed),
                new CommandBinding(NUtilityCommands.OpenHyperlink, OpenHyperlinkCommand_Executed),
                new CommandBinding(NUtilityCommands.Lookup, LookupCommand_Executed),
                new CommandBinding(NUtilityCommands.WordCount, WordCountCommand_Executed),
            });
        }

        #region Editing Commands

        void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Undo();
        }

        void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Redo();
        }

        void CutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var data = Cut();
            NClipboard.SetData(data);
        }

        void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var data = Copy();
            NClipboard.SetData(data);
        }

        void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is PasteMode)
            {
                var pasteMode = (PasteMode)e.Parameter;
                Paste(pasteMode);
            }
            else
            {
                Paste();
            }
        }

        void PasteSpecialCommand_Executed(object sendr, ExecutedRoutedEventArgs e)
        {
            string format = e.Parameter as string;

            PasteSpecial(format);
        }

        #endregion

        #region Text Commands

        void SelectAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectAll();
        }

        void FindNextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!FindNext())
            {
                FindFirst();
            }
        }

        void FindPreviousCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!FindPrevious())
            {
                FindLast();
            }
        }

        void ReplaceOnceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (FindText != null && ReplaceText != null)
            {
                Replace(FindText, ReplaceText);

                if (!FindNext())
                {
                    FindFirst();
                }
            }
        }

        void ReplaceAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (FindText != null && ReplaceText != null)
            {
                var selected = Selection.LastOrDefault();

                SelectAll();
                Replace(FindText, ReplaceText);

                if (selected != null)
                {
                    Selection.SelectOnly(selected);
                }
            }
        }

        void DeleteForwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Selection.Count != 1 || !(Selection.Last() is ITextElement))
            {
                Delete();
                return;
            }

            var table = Selection.Last() as NTable;
            if (table != null && !table.Selection.Any())
            {
                Delete();
                return;
            }

            DeleteForward();
        }

        void DeleteForwardCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Selection.Count > 0);
        }

        void DeleteForwardByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteForwardByWord();
        }

        void DeleteForwardByParagraphCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteForwardByParagraph();
        }

        void DeleteBackCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Pressing backspace is equivalent to pressing delete when:
            // 1) More than one item is selected, or
            // 2) The last selected item is not a text element, or
            // 3) A text element itself is selected and not its content (applies only to tables)

            if (Selection.Count != 1)
            {
                Delete();
                return;
            }

            if (!(Selection.Last() is ITextElement))
            {
                Delete();
                return;
            }

            var table = Selection.Last() as NTable;
            if (table != null && !table.Selection.Any())
            {
                Delete();
                return;
            }

            DeleteBack();
        }

        void DeleteBackCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void DeleteBackByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteBackByWord();
        }

        void DeleteBackByParagraphCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteBackByParagraph();
        }

        void EnterLineBreakCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EnterLineBreak();
        }

        void EnterLineBreakCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Selection.OfType<ITextElement>().Count() > 0);
        }

        void EnterParagraphBreakCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EnterParagraphBreak();
        }

        void EnterParagraphBreakCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (Selection.OfType<ITextElement>().Count() > 0);
        }

        #endregion

        #region Navigation Commands

        // Navigation Commands

        void MoveUpByLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveUp();
        }

        void MoveDownByLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveDown();
        }

        void MoveLeftByCharacterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveLeft();
        }

        void MoveRightByCharacterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveRight();
        }

        void MoveLeftByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveLeftByWord();
        }

        void MoveRightByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveRightByWord();
        }

        void MoveToLineEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveToLineEnd();
        }

        void MoveToLineStartCommand_Executed(object sStarter, ExecutedRoutedEventArgs e)
        {
            MoveToLineStart();
        }

        void MoveToParagraphEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveToParagraphEnd();
        }

        void MoveToParagraphStartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveToParagraphStart();
        }

        void MoveToDocumentEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveToEnd();
        }

        void MoveToDocumentStartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveToStart();
        }

        void MoveUpByPageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveUpByPage();
        }

        void MoveDownByPageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveDownByPage();
        }

        void SelectLeftByCharacterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectLeft();
        }

        void SelectRightByCharacterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectRight();
        }

        void SelectUpByLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectUp();
        }

        void SelectDownByLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectDown();
        }

        void SelectLeftByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectLeftByWord();
        }

        void SelectRightByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectRightByWord();
        }

        void SelectToLineEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectToLineEnd();
        }

        void SelectToLineStartCommand_Executed(object sStarter, ExecutedRoutedEventArgs e)
        {
            SelectToLineStart();
        }

        void SelectToParagraphEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectToParagraphEnd();
        }

        void SelectToParagraphStartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectToParagraphStart();
        }

        void SelectToDocumentEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectToEnd();
        }

        void SelectToDocumentStartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectToStart();
        }

        void SelectUpByPageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectUpByPage();
        }

        void SelectDownByPageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectDownByPage();
        }

        void SelectTitleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectTitle();
        }

        void TabForwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Selection.OfType<ITextElement>().Count() > 1)
            {
                IncreaseIndentation();
            }
            else
            {
                TabForward();
            }
        }

        void TabBackwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Selection.OfType<ITextElement>().Count() > 1)
            {
                DecreaseIndentation();
            }
            else
            {
                TabBackward();
            }
        }

        #endregion

        #region Formatting Commands

        void FormattingCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Selection.OfType<IFormattable>().Any();
        }

        void SetFontFamilyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter == null)
            {
                return;
            }

            // Sanity check
            bool isFontClass = FontClass.CommonStyles.Any(fontClass => fontClass.Name == e.Parameter.ToString());
            if (isFontClass)
            {
                return;
            }

            var fontFamily = SafeConvert.ToFontFamily(e.Parameter);
            if (fontFamily != null)
            {
                FontFamily = fontFamily;
            }
        }

        void SetFontSizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var fontSize = SafeConvert.ToInt32(e.Parameter);
            if (fontSize != 0)
            {
                SetProperty(TextProperties.FontSizeProperty, fontSize * 96.0 / 72.0);
            }
        }

        void AlignLeftCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AlignLeft();
        }

        void AlignCenterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AlignCenter();
        }

        void AlignRightCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AlignRight();
        }

        void AlignJustifyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AlignJustify();
        }

        void AlignTopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingBoard.SetVerticalAlignment(VerticalAlignment.Top);
        }

        void AlignMiddleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingBoard.SetVerticalAlignment(VerticalAlignment.Center);
        }

        void AlignBottomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingBoard.SetVerticalAlignment(VerticalAlignment.Bottom);
        }

        void DecreaseFontSizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DecreaseFontSize();
        }

        void DecreaseIndentationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DecreaseIndentation();
        }

        void IncreaseFontSizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IncreaseFontSize();
        }

        void IncreaseIndentationCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IncreaseIndentation();
        }

        void ToggleBoldCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Bold = !Bold;
        }

        void SetTextColorCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ApplyTextBrush();
        }

        void HighlightCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsTextSelected)
            {
                ApplyHighlightBrush();
                IsHighlighting = false;
            }
        }

        void ToggleItalicCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Italic = !Italic;
        }

        void ToggleUnderlineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Underline = !Underline;
        }

        void ToggleStrikethroughCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Strikethrough = !Strikethrough;
        }

        void ToggleSuperscriptCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Superscript = !Superscript;
        }

        void ToggleSubscriptCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Subscript = !Subscript;
        }

        void SetListStyleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IListStyle newStyle;

            if (e.Parameter == null)
            {
                newStyle = null;
            }
            else if (e.Parameter is IListStyle)
            {
                newStyle = (IListStyle)e.Parameter;
            }
            else
            {
                newStyle = ListStyles.FromString(e.Parameter.ToString(), ListStyles.Circle);
            }

            ApplyListStyle(newStyle);

            if (newStyle != null)
            {
                DefaultListStyle = newStyle;
            }
        }

        void ToggleListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ToggleList();
        }

        void SetLineSpacingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoubleConverter converter = new DoubleConverter();
            LineSpacing = (double)converter.ConvertFrom(e.Parameter.ToString());
        }

        void SetLineSpacing1Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LineSpacing = 1;
        }

        void SetLineSpacing15Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LineSpacing = 1.5;
        }

        void SetLineSpacing2Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LineSpacing = 2;
        }

        void ClearFormattingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ClearFormatting();
        }

        #endregion

        #region View Commands

        void ToggleGridCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsGridVisible = !IsGridVisible;
        }

        void ToggleGuidelinesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsGuidelinesEnabled = !IsGuidelinesEnabled;
        }

        void ZoomInCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomIn();
        }

        void ZoomOutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut();
        }

        #endregion

        #region Drawing Commands

        void SelectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsSelectingGeometry = true;
        }

        void EraseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsErasing = true;
        }

        void InsertPathCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsDrawingPath = true;
        }

        void InsertLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var drawing = e.Parameter as Shape;

            if (drawing == null)
            {
                drawing = SelectedLine;
            }

            if (drawing != null)
            {
                InsertLine(drawing);
            }
        }

        void InsertClipartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var drawing = e.Parameter as Shape;

            if (drawing == null)
            {
                drawing = SelectedClipart;
            }

            if (drawing != null)
            {
                InsertClipart(drawing);
            }
        }

        void InsertTextBoxCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertTextBox();
        }

        void StrokeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Brush brush = e.Parameter as Brush;

            if (EditingMode == EditingModes.Normal &&
                !Selection.OfType<NCanvas>().Any(canvas => canvas.Selection.Any()))
            {
                // no drawings are selected - enter "stroking" mode
                IsStroking = true;
            }
            else if (brush != null)
            {
                // stroke using the specified brush
                Stroke(brush);
            }
            else
            {
                // stroke using the currently-selected brush
                Stroke();
            }
        }

        void FillCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Brush brush = e.Parameter as Brush;

            if (EditingMode == EditingModes.Normal &&
                !Selection.OfType<NCanvas>().Any(canvas => canvas.Selection.Any()))
            {
                // no drawings are selected - enter "filling" mode
                IsFilling = true;
            }
            else if (brush != null)
            {
                // fill using the specified brush
                Fill(brush);
            }
            else
            {
                // fill using the currently-selected brush
                Fill();
            }
        }

        void SetEffectCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter == null)
            {
                SetProperty(Shape.FilterProperty, null);
            }
            else if (e.Parameter is Effect)
            {
                SetProperty(Shape.FilterProperty, (Effect)e.Parameter);
            }
        }

        void RotateRightCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingBoard.Rotate(90.0);
        }

        void RotateLeftCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingBoard.Rotate(-90.0);
        }

        void FlipHorizontalCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FlipHorizontal();
        }

        void FlipVerticalCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FlipVertical();
        }

        void BringForwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BringForward();
        }

        void SendBackwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SendBackward();
        }

        void BringToFrontCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BringToFront();
        }

        void SendToBackCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SendToBack();
        }

        void GroupCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingBoard.Group();
        }

        void UngroupCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingBoard.Ungroup();
        }

        void EditSourceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingBoard.EditSource();
        }

        #endregion

        #region Insert Commands

        void InsertCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            object[] items;

            if (e.Parameter is object[])
            {
                items = (object[])e.Parameter;
            }
            else
            {
                items = new object[] { e.Parameter };
            }

            if (items != null)
            {
                Paste(items);
            }
        }

        void InsertSymbolCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string symbol = e.Parameter as string;
            InsertSymbol(symbol);
        }

        void InsertHyperlinkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string type = e.Parameter as string;
            InsertHyperlink(type);
        }

        void RemoveHyperlinkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveHyperlink();
        }

        void InsertStickyNoteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertStickyNote();
        }

        void InsertSectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertSection();
        }

        void InsertSubsectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertSubsection();
        }

        void InsertTableCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertTable();
        }

        void InsertImageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertImage(true);
        }

        void InsertAudioCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        void InsertVideoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        void InsertFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "All Files|*.*";
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            NFile file;
            try
            {
                file = new NFile(dialog.FileName);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Append(file);
            file.IsPlacing = true;
        }

        #endregion

        #region Image Commands

        void OpenImageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenImage();
        }

        void EditImageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditImage();
        }

        void OpenImageWithCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string command = (string)e.Parameter;
            OpenImageWith(command);
        }

        void ResetImageSizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ResetImageSize();
        }

        void TogglePreserveAspectRatio_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TogglePreserveAspectRatio();
        }

        #endregion

        #region File Commands

        void OpenFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile();
        }

        #endregion

        #region Table Commands

        void SetTableBorderBrushCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SetTableBorderBrush((Brush)e.Parameter);
        }

        void SetTableBorderWidthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SetTableBorderWidth((double)e.Parameter);
        }

        void SetTableBackgroundCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SetTableBackgroundBrush((Brush)e.Parameter);
        }

        void MergeTableCellsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanMergeTableCells;
        }

        void MergeTableCellsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MergeTableCells();
        }

        void SplitTableCellsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSplitTableCells;
        }

        void SplitTableCellsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SplitTableCells();
        }

        #endregion

        #region Utility Commands

        void DrillDownCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string noteTitle = SelectedText;

            if (String.IsNullOrWhiteSpace(noteTitle))
            {
                MessageBox.Show("To use the \"Drill-down\" feature, you must first select some text.", "SilverNote", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string uri = String.Format("/notes?search=title:{0}", HttpUtility.UrlEncode(noteTitle));

            SetProperty(TextProperties.HyperlinkURLProperty, uri);

            var clicked = new HyperlinkClickedEventArgs(TextParagraph.HyperlinkClicked, this, uri, "_self");
            RaiseEvent(clicked);
        }

        void DrillThroughCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string noteTitle = SelectedText;

            if (String.IsNullOrWhiteSpace(noteTitle))
            {
                MessageBox.Show("To use the \"Drill-through\" feature, you must first select some text.", "SilverNote", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string uri = String.Format("/notes?search=title:{0}", HttpUtility.UrlEncode(noteTitle));

            SetProperty(TextProperties.HyperlinkURLProperty, uri);

            var clicked = new HyperlinkClickedEventArgs(TextParagraph.HyperlinkClicked, this, uri, "_blank");
            RaiseEvent(clicked);
        }

        void OpenHyperlinkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string uri = this.GetStringProperty(TextProperties.HyperlinkURLProperty);
            if (!String.IsNullOrEmpty(uri))
            {
                OpenHyperlink(uri);
            }
        }

        void LookupCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string service = e.Parameter as string;

            if (service != null)
            {
                Lookup(service);
            }
        }

        void WordCountCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string text = this.ToString();

            int wordCount = WordCounting.CountWords(text);
            int charCount1 = WordCounting.CountCharacters(text, includeSpaces: true);
            int charCount2 = WordCounting.CountCharacters(text, includeSpaces: false);

            string message = String.Empty;
            message += String.Format("Words: {0}\n", wordCount);
            message += String.Format("Characters (including spaces): {0}\n", charCount1);
            message += String.Format("Characters (excluding spaces): {0}", charCount2);

            MessageBox.Show(message, "Word Count", MessageBoxButton.OK, MessageBoxImage.None);
        }

        #endregion

        #region Development Commands

        void DebugCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DebugFlags == NDebugFlags.None)
            {
                DebugFlags = NDebugFlags.All;
            }
            else
            {
                DebugFlags = NDebugFlags.None;
            }

            InvalidateVisual();
        }

        #endregion

        #endregion

        #region HTML

        public override string GetHTMLNodeName(NodeContext context)
        {
            return HTMLElements.BODY;
        }

        public override object GetHTMLParentNode(NodeContext context)
        {
            var parentNode = OwnerDocument.DocumentElement;
            if (parentNode == null)
            {
                return null;
            }

            var thisNode = OwnerDocument.GetNode(this);

            if (parentNode.ChildNodes.Contains(thisNode))
            {
                return parentNode;
            }
            else
            {
                return null;
            }
        }

        public override IEnumerable<object> GetHTMLChildNodes(NodeContext context)
        {
            return base.GetHTMLChildNodes(context).Where((node) => !(node is NCanvas) || ((NCanvas)node).Drawings.Count > 0 || ((NCanvas)node).NeedsResource);
        }

        #endregion

        #region SVG

        class SVGWindow : DOM.Windows.Internal.WPF.WindowBase
        {
            NoteEditor _Owner;

            public SVGWindow(NoteEditor owner)
            {
                _Owner = owner;
            }

            public NoteEditor Owner
            {
                get { return _Owner; }
            }

            public override DOM.Views.DocumentView Document
            {
                get { return (DOM.Views.DocumentView)_Owner.SVGDocument; }
            }
        }

        private SVGWindow _SVGContext;

        private SVGWindow SVGContext
        {
            get
            {
                if (_SVGContext == null)
                {
                    _SVGContext = new SVGWindow(this);
                }
                return _SVGContext;
            }
        }

        private SVGDocument _SVGDocument;

        public SVGDocument SVGDocument
        {
            get
            {
                if (_SVGDocument == null)
                {
                    _SVGDocument = DOMFactory.CreateSVGDocument();
                    _SVGDocument.RootElement.Bind(this);
                }
                return _SVGDocument;
            }
        }

        readonly string _SVGNodeName = SVGElements.NAMESPACE + " " + SVGElements.SVG;

        public override string GetSVGNodeName(NodeContext context)
        {
            return _SVGNodeName;
        }

        public override object GetSVGParentNode(NodeContext context)
        {
            return SVGDocument;
        }

        public override IEnumerable<object> GetSVGChildNodes(NodeContext context)
        {
            return InternalChildren.OfType<NCanvas>();
        }

        public override object CreateSVGNode(NodeContext context)
        {
            if (context is SVGScriptElement)
            {
                return context;
            }

            if (context is SVGSVGElement)
            {
                return new NCanvas();
            }

            return null;
        }

        public override void AppendSVGNode(NodeContext context, object newChild)
        {
            if (newChild is SVGScriptElement)
            {
                ExecuteSVGScript((SVGScriptElement)newChild);
            }
            else
            {
                Append((UIElement)newChild);
            }
        }

        public override void InsertSVGNode(NodeContext context, object newChild, object refChild)
        {
            if (newChild is SVGScriptElement)
            {
                ExecuteSVGScript((SVGScriptElement)newChild);
            }
            else
            {
                int index = InternalChildren.IndexOf((UIElement)refChild);
                if (index != -1)
                {
                    Insert(index, (UIElement)newChild);
                }
                else
                {
                    throw new DOMException(DOMException.NOT_FOUND_ERR);
                }
            }
        }

        public override void RemoveSVGNode(NodeContext context, object oldChild)
        {
            if (oldChild is UIElement)
            {
                Remove((UIElement)oldChild);
            }
        }

        private void ExecuteSVGScript(SVGScriptElement script)
        {

        }

        #endregion

        #region Save/Load

        #region Flushing

        const double MAXIMUM_FLUSH_INTERVAL = 60;
        const double MINIMUM_FLUSH_INTERVAL = 5;   // seconds

        private DispatcherTimer _FlushTimer;

        /// <summary>
        /// FlushTimer ensures Flush() is called at least every 60 seconds.
        /// </summary>
        protected DispatcherTimer FlushTimer
        {
            get
            {
                if (_FlushTimer == null)
                {
                    _FlushTimer = CreateFlushTimer();
                }
                return _FlushTimer;
            }
        }

        private DispatcherTimer CreateFlushTimer()
        {
            return new DispatcherTimer(
                TimeSpan.FromSeconds(MAXIMUM_FLUSH_INTERVAL),
                DispatcherPriority.Background,
                FlushTimer_Tick,
                this.Dispatcher
            );
        }

        protected void ResetFlushTimer()
        {
            FlushTimer.Stop();
            FlushTimer.Start();
        }

        protected void FlushTimer_Tick(object sender, EventArgs e)
        {
            Flush();
        }

        private DispatcherTimer _IdleTimer;

        protected DispatcherTimer IdleTimer
        {
            get
            {
                if (_IdleTimer == null)
                {
                    _IdleTimer = CreateIdleTimer();
                }
                return _IdleTimer;
            }
        }

        protected void ResetIdleTimer()
        {
            IdleTimer.Stop();
            IdleTimer.Start();
        }

        private DispatcherTimer CreateIdleTimer()
        {
            return new DispatcherTimer(
                TimeSpan.FromSeconds(MINIMUM_FLUSH_INTERVAL),
                DispatcherPriority.Background,
                IdleTimer_Tick,
                this.Dispatcher
            );
        }

        protected void IdleTimer_Tick(object sender, EventArgs e)
        {
            Flush();
        }

        public bool IsFlushing { get; set; }

        /// <summary>
        /// Update the 'Content' property
        /// </summary>
        public void Flush()
        {
            if (!IsDirty)
            {
                return;
            }

            IsFlushing = true;
            try
            {
                // Convert this document to HTML

                _Content = ToHTML();

                // Mark this document as saved

                IsDirty = false;

                // Raise ResourceChanged before setting content since
                // content change may trigger another view of this note
                // to parse that content and request the files.

                RaiseResourceChanged(null);

                // Update 'Content' and 'Text' properties

                Content = _Content;
                Text = OwnerDocument.Body.InnerText;

                ResetFlushTimer();
            }
            finally
            {
                IsFlushing = false;
            }
        }

        #endregion

        public string ToHTML(bool pretty = false)
        {
            return ToHTML(HTMLFilters.CleanFormatFilter, pretty);
        }

        /// <summary>
        /// Convert this document to an HTML string
        /// </summary>
        /// <param name="filter">A filter to be applied to the document (may be null)</param>
        /// <returns>An HTML string</returns>
        public string ToHTML(HTMLFilterDelegate filter, bool pretty = false)
        {
            var document = ExportDocument();

            // Apply the filter

            if (filter != null)
            {
                filter(document);
            }

            // Serialize

            if (!pretty)
            {
                return document.ToString();
            }

            using (var writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture))
            {
                var output = DOMFactory.CreateLSOutput();
                output.CharacterStream = writer;
                var serializer = DOMFactory.CreateLSSerializer();
                serializer.Config.SetParameter(DOM.LS.LSSerializerParameters.XML_DECLARATION, !pretty);
                serializer.Config.SetParameter(DOM.LS.LSSerializerParameters.FORMAT_PRETTY_PRINT, pretty);
                serializer.Write(document, output);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Load this document from an HTML string
        /// </summary>
        /// <param name="html">The HTML string to be parsed</param>
        public void FromHTML(string html)
        {
            // Load the document from the given HTML

            OwnerDocument.Open();
            OwnerDocument.Write(html);
            OwnerDocument.Close();

            // Apply the given filter

            HTMLFilters.HtmlParseFilter(OwnerDocument);

            // Render the document to this NoteEditor

            RemoveAll();
            OwnerDocument.Body.Bind(this, render: true);

            // Re-add the drawing board

            if (!InternalChildren.Contains(DrawingBoard))
            {
                Insert(0, DrawingBoard);
            }

            // Set initial selection

            if (TitleElement != null && TitleElement.Text == "Untitled")
            {
                // If title = "Untitled", select the title

                Selection.SelectOnly(TitleElement);
                TitleElement.MoveToStart();
                TitleElement.SelectToEnd();
            }
            else
            {
                // Otherwise, put the caret immediately after the title

                SelectDefault(Editor.Positioning.Static, 2);
            }

            // Reset the undo stack

            UndoStack.Clear();
        }

        public HTMLDocument ExportDocument()
        {
            // Sort InternalChildren by the order they should appear in the HTML document

            Normalize();

            // Update each element's style attribute to reflect the rendered style

            OwnerDocument.Body.UpdateStyle(deep: true);

            // Make a copy of the HTML document for saving

            var document = (HTMLDocument)OwnerDocument.CloneNode(true);

            // Normalize

            document.NormalizeDocument();

            // Set document title

            document.Title = this.Title;

            // Set document style

            var head = (HTMLHeadElement)document.GetElementsByTagName(HTMLElements.HEAD)[0];
            var style = head.GetElementsByTagName(HTMLElements.STYLE)[0];
            if (style == null)
            {
                style = document.CreateElement(HTMLElements.STYLE);
                head.AppendChild(style);
            }
            style.TextContent = DEFAULT_STYLESHEET;

            return document;
        }

        public Dictionary<string, byte[]> ExportResources()
        {
            var results = new Dictionary<string, byte[]>();
            var resourceNames = this.ResourceNames;

            foreach (string url in resourceNames)
            {
                // Export canvases as PNGs

                var canvas = InternalChildren.OfType<NCanvas>().FirstOrDefault((c) => c.Filename == url);
                if (canvas != null)
                {
                    string pngFileName = Path.ChangeExtension(url, ".png");
                    canvas = (NCanvas)canvas.Clone();
                    canvas.SizeToContent();
                    if (canvas.Width > 0 && canvas.Height > 0)  // sanity check
                    {
                        canvas.Measure(new Size(canvas.Width, canvas.Height));
                        canvas.Arrange(new Rect(0, 0, canvas.Width, canvas.Height));

                        using (var stream = new MemoryStream())
                        {
                            canvas.Save(stream, ".png");
                            results.Add(pngFileName, stream.ToArray());
                        }
                    }
                    continue;
                }

                string fileName = Uri.UnescapeDataString(url);

                // Export file icons as PNGs

                var file = InternalChildren.OfType<NFile>().FirstOrDefault((f) => f.Url == url);

                if (file != null)
                {
                    string pngFileName = Path.ChangeExtension(fileName, ".icon.png");
                    using (var stream = new MemoryStream())
                    {
                        file.SaveAsImage(stream, ".png");
                        results.Add(pngFileName, stream.ToArray());
                    }
                }

                // Export file data (unmodified)

                using (var stream = new MemoryStream())
                {
                    GetResource(url, stream);
                    results.Add(url, stream.ToArray());
                }
            }

            return results;
        }

        public void EmbedFiles(HTMLDocument document)
        {
            HTMLFilters.CleanFormatFilter(document);

            // IMG

            var images = document.QuerySelectorAll("img");

            foreach (HTMLImageElement img in images)
            {
                DataUri uri = new DataUri
                {
                    Type = "image",
                    Subtype = "png",
                    Encoding = "base64",
                    DataBytes = this.GetResourceData(img.Src)
                };

                img.Src = uri.ToString();
            }

            // SVG

            var objects = document.QuerySelectorAll(@"object[type='image/svg+xml']");

            foreach (HTMLObjectElement obj in objects)
            {
                string str = this.GetResourceString(obj.Data, Encoding.UTF8);
                LSParser parser = DOMFactory.CreateLSParser(DOMImplementationLSMode.MODE_SYNCHRONOUS, "");
                SVGDocument svg = (SVGDocument)parser.ParseString(str);
                SVGElement element = (SVGElement)obj.OwnerDocument.AdoptNode(svg.RootElement);
                element.SetAttribute("style", obj.Style.CssText);
                obj.ParentNode.ReplaceChild(element, obj);
            }
        }

        #endregion

        #region Object

        public override object Clone()
        {
            return new NoteEditor(this);
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            foreach (UIElement child in Children)
            {
                if (child is TextParagraph)
                {
                    buffer.Append(child.ToString());
                    buffer.Append("\n\n");
                }
            }
            return buffer.ToString();
        }

        #endregion

        #region Implementation

        #region Editing Mode

        protected override void OnEscape()
        {
            EditingMode = EditingModes.Normal;
        }

        protected void DrawingBoard_ModeChanged(object sender, DrawingModeChangedEventArgs e)
        {
            if (e.NewMode != DrawingMode.Dragging &&
                e.NewMode != DrawingMode.Drawing &&
                e.NewMode != DrawingMode.Placing)
            {
                HideGuidelines();
                HideGuidepoints();
                HideSnapPoints();
            }

            if (e.NewMode == DrawingMode.Ready)
            {
                IsDrawingPath = false;
                IsDrawingLine = false;
                IsDrawingClipart = false;
                IsDrawingText = false;

                UpdateEditingMode();
            }
        }

        #endregion

        #region Context Menu

        private ContextMenu BuildContextMenu()
        {
            var contextMenu = new ContextMenu();
            MenuItem menuItem, subMenu;

            // Hyperlink

            if (Selection.OfType<IFormattable>().Any(s => s.HasProperty(TextProperties.HyperlinkURLProperty)))
            {
                string hyperlinkURL = this.GetStringProperty(TextProperties.HyperlinkURLProperty);

                if (!String.IsNullOrEmpty(hyperlinkURL))
                {
                    menuItem = new MenuItem();
                    menuItem.Header = "Open hyperlink";
                    menuItem.Command = NUtilityCommands.OpenHyperlink;
                    menuItem.CommandTarget = this;
                    menuItem.InputGestureText = "Ctrl+Click";
                    menuItem.Icon = Images.GetImage("link_go.png", 16, 16);
                    contextMenu.Items.Add(menuItem);

                    menuItem = new MenuItem();
                    menuItem.Header = "Edit hyperlink";
                    menuItem.Command = NInsertionCommands.InsertHyperlink;
                    menuItem.CommandTarget = this;
                    menuItem.Icon = Images.GetImage("link_edit.png", 16, 16);
                    contextMenu.Items.Add(menuItem);

                    menuItem = new MenuItem();
                    menuItem.Header = "Remove Hyperlink";
                    menuItem.Command = NInsertionCommands.RemoveHyperlink;
                    menuItem.CommandTarget = this;
                    menuItem.Icon = Images.GetImage("link_delete.png", 16, 16);
                    contextMenu.Items.Add(menuItem);
                }
                else
                {
                    var linkMenu = new MenuItem();
                    linkMenu.Header = "Link to";
                    linkMenu.Icon = Images.GetImage("link_add.png", 16, 16);
                    contextMenu.Items.Add(linkMenu);

                    menuItem = new MenuItem();
                    menuItem.Header = "URL";
                    menuItem.Command = NInsertionCommands.InsertHyperlink;
                    menuItem.CommandParameter = "URL";
                    menuItem.CommandTarget = this;
                    linkMenu.Items.Add(menuItem);

                    menuItem = new MenuItem();
                    menuItem.Header = "File";
                    menuItem.Command = NInsertionCommands.InsertHyperlink;
                    menuItem.CommandParameter = "File";
                    menuItem.CommandTarget = this;
                    linkMenu.Items.Add(menuItem);

                    menuItem = new MenuItem();
                    menuItem.Header = "Note";
                    menuItem.Command = NInsertionCommands.InsertHyperlink;
                    menuItem.CommandParameter = "Note";
                    menuItem.CommandTarget = this;
                    linkMenu.Items.Add(menuItem);
                }

                contextMenu.Items.Add(new Separator());
            }
            else if (IsTextSelected)
            {
                // Open
                menuItem = new MenuItem();
                menuItem.Header = "Drill through";
                menuItem.InputGestureText = "Ctrl+Click";
                menuItem.Command = NUtilityCommands.DrillThrough;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("link_go.png", 16, 16);
                contextMenu.Items.Add(menuItem);
            }

            if (IsTextSelected)
            {
                // Lookup

                if (LookupServices != null)
                {
                    var lookupMenu = new MenuItem();
                    lookupMenu.Header = "Lookup";
                    lookupMenu.Icon = Images.GetImage("websearch.png", 16, 16);
                    contextMenu.Items.Add(lookupMenu);

                    foreach (var service in LookupServices)
                    {
                        menuItem = new MenuItem();
                        menuItem.Header = service.Name;
                        menuItem.Command = NUtilityCommands.Lookup;
                        menuItem.CommandParameter = service.Name;
                        menuItem.CommandTarget = this;
                        lookupMenu.Items.Add(menuItem);
                    }
                }

                contextMenu.Items.Add(new Separator());
            }


            // Images
            if (IsImageSelected)
            {
                // Open
                menuItem = new MenuItem();
                menuItem.Header = "Open";
                menuItem.FontWeight = FontWeights.Bold;
                menuItem.Command = NImageCommands.Open;
                menuItem.CommandTarget = this;
                contextMenu.Items.Add(menuItem);

                // Edit
                if (Selection.OfType<NImage>().Any(image => image.CanEditImage))
                {
                    menuItem = new MenuItem();
                    menuItem.Header = "Edit";
                    menuItem.Command = NImageCommands.Edit;
                    menuItem.CommandTarget = this;
                    contextMenu.Items.Add(menuItem);
                }

                // Open with
                menuItem = new MenuItem();
                menuItem.Header = "Open with";
                contextMenu.Items.Add(menuItem);

                subMenu = menuItem;

                foreach (var openWith in ImageApplications)
                {
                    menuItem = new MenuItem();
                    menuItem.Header = openWith.FriendlyAppName;
                    menuItem.Command = NImageCommands.OpenWith;
                    menuItem.CommandParameter = openWith.AppCommand;
                    menuItem.CommandTarget = this;
                    menuItem.Icon = openWith.DefaultIconImage;
                    subMenu.Items.Add(menuItem);
                }

                // separator
                if (subMenu.Items.Count > 0)
                {
                    subMenu.Items.Add(new Separator());
                }

                // Choose program...
                menuItem = new MenuItem();
                menuItem.Header = "Choose program...";
                menuItem.Command = NImageCommands.OpenWith;
                menuItem.CommandTarget = this;
                subMenu.Items.Add(menuItem);

                // Save as
                menuItem = new MenuItem();
                menuItem.Header = "Save as...";
                menuItem.Command = NApplicationCommands.SaveImage;
                menuItem.CommandTarget = this;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                menuItem = new MenuItem();
                menuItem.Header = "Reset size";
                menuItem.Command = NImageCommands.ResetSize;
                menuItem.CommandTarget = this;
                contextMenu.Items.Add(menuItem);

                // Preserve Aspect Ratio
                menuItem = new MenuItem();
                menuItem.Header = "Preserve aspect ratio";
                menuItem.IsCheckable = true;
                menuItem.IsChecked = PreserveAspectRatio;
                menuItem.Command = NImageCommands.TogglePreserveAspectRatio;
                menuItem.CommandTarget = this;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());
            }

            // Files
            if (IsFileSelected)
            {
                // Open
                menuItem = new MenuItem();
                menuItem.Header = "Open";
                menuItem.FontWeight = FontWeights.Bold;
                menuItem.Command = NFileCommands.Open;
                menuItem.CommandTarget = this;
                contextMenu.Items.Add(menuItem);

                // Save as
                menuItem = new MenuItem();
                menuItem.Header = "Save as...";
                menuItem.Command = NApplicationCommands.SaveFile;
                menuItem.CommandTarget = this;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());
            }

            // Tables
            NTable table = Selection.OfType<NTable>().FirstOrDefault();
            if (table != null)
            {
                // Insert 

                subMenu = new MenuItem();
                subMenu.Header = "Insert";
                contextMenu.Items.Add(subMenu);

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Row above",
                    Command = NTableCommands.InsertRowAbove,
                    CommandTarget = table
                });

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Row below",
                    Command = NTableCommands.InsertRowBelow,
                    CommandTarget = table
                });

                subMenu.Items.Add(new Separator());

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Column left",
                    Command = NTableCommands.InsertColumnLeft,
                    CommandTarget = table
                });

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Column right",
                    Command = NTableCommands.InsertColumnRight,
                    CommandTarget = table
                });

                // Delete

                subMenu = new MenuItem();
                subMenu.Header = "Delete";
                contextMenu.Items.Add(subMenu);

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Row",
                    Command = NTableCommands.DeleteRow,
                    CommandTarget = table
                });

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Column",
                    Command = NTableCommands.DeleteColumn,
                    CommandTarget = table
                });

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Table",
                    Command = NTableCommands.DeleteTable,
                    CommandTarget = table
                });

                // Select

                subMenu = new MenuItem();
                subMenu.Header = "Select";
                contextMenu.Items.Add(subMenu);

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Row",
                    Command = NTableCommands.SelectRow,
                    CommandTarget = table
                });

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Column",
                    Command = NTableCommands.SelectColumn,
                    CommandTarget = table
                });

                subMenu.Items.Add(new MenuItem
                {
                    Header = "Table",
                    Command = NTableCommands.SelectTable,
                    CommandTarget = table
                });

                // Merge cells

                if (CanMergeTableCells)
                {
                    contextMenu.Items.Add(new MenuItem
                    {
                        Header = "Merge cells",
                        Command = NTableCommands.MergeCells,
                        CommandTarget = table
                    });
                }

                // Split cells

                if (CanSplitTableCells)
                {
                    contextMenu.Items.Add(new MenuItem
                    {
                        Header = "Split cells",
                        Command = NTableCommands.SplitCells,
                        CommandTarget = table
                    });
                }

                contextMenu.Items.Add(new Separator());

                // Background

                var resources = new ResourceDictionary();
                resources.Source = new Uri(@"pack://application:,,,/SilverNote;component/Views/Styles/ContainerMenuItemStyle.xaml");

                menuItem = new MenuItem
                {
                    Header = "Background",
                    Icon = Images.GetImage("bgndcolor.png", 16, 16),
                    ItemContainerStyle = (Style)resources["ContainerMenuItemStyle"]
                };

                var container = new Grid
                {
                    Background = Brushes.White
                };
                container.Children.Add(
                    new Controls.ColorPicker
                    {
                        Width = 120,
                        AllowsNone = true,
                        AllowsGradient = false,
                        Command = NTableCommands.SetBackground,
                        CommandTarget = table
                    });

                menuItem.Items.Add(container);
                contextMenu.Items.Add(menuItem);

                // Borders

                menuItem = new MenuItem
                {
                    Header = "Borders",
                    Icon = Images.GetImage("linecolor.png", 16, 16),
                    ItemContainerStyle = (Style)resources["ContainerMenuItemStyle"]
                };

                container = new Grid
                {
                    Background = Brushes.White
                };
                container.Children.Add(
                    new Controls.StrokeControl
                    {
                        Width = 120,
                        AllowsDash = false,
                        SelectBrushCommand = NTableCommands.SetBorderBrush,
                        SelectWidthCommand = NTableCommands.SetBorderWidth,
                        CommandTarget = table
                    });

                menuItem.Items.Add(container);
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());
            }

            // IEditable
            if (Selection.OfType<IEditable>().Any())
            {
                // Cut
                menuItem = new MenuItem();
                menuItem.Header = "Cu_t";
                menuItem.Command = NEditingCommands.Cut;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("cut.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Copy
                menuItem = new MenuItem();
                menuItem.Header = "_Copy";
                menuItem.Command = NEditingCommands.Copy;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("copy.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Paste
                menuItem = new MenuItem();
                menuItem.Header = "_Paste";
                menuItem.Command = NEditingCommands.Paste;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("paste.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Delete
                menuItem = new MenuItem();
                menuItem.Header = "_Delete";
                menuItem.Command = NEditingCommands.DeleteForward;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("delete.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());
            }

            // NCanvas
            if (DrawingBoard.IsSelected)
            {
                // NTextBox
                if (DrawingBoard.OfType<NTextBox>().Any())
                {
                    // Align
                    var alignMenuItem = new MenuItem();
                    alignMenuItem.Header = "Align";

                    // Left
                    menuItem = new MenuItem();
                    menuItem.Header = "Left";
                    menuItem.Command = NFormattingCommands.AlignLeft;
                    menuItem.CommandTarget = this;
                    alignMenuItem.Items.Add(menuItem);

                    // Center
                    menuItem = new MenuItem();
                    menuItem.Header = "Center";
                    menuItem.Command = NFormattingCommands.AlignCenter;
                    menuItem.CommandTarget = this;
                    alignMenuItem.Items.Add(menuItem);

                    // Left
                    menuItem = new MenuItem();
                    menuItem.Header = "Right";
                    menuItem.Command = NFormattingCommands.AlignRight;
                    menuItem.CommandTarget = this;
                    alignMenuItem.Items.Add(menuItem);

                    alignMenuItem.Items.Add(new Separator());

                    // Top
                    menuItem = new MenuItem();
                    menuItem.Header = "Top";
                    menuItem.Command = NFormattingCommands.AlignTop;
                    menuItem.CommandTarget = this;
                    alignMenuItem.Items.Add(menuItem);

                    // Middle
                    menuItem = new MenuItem();
                    menuItem.Header = "Middle";
                    menuItem.Command = NFormattingCommands.AlignMiddle;
                    menuItem.CommandTarget = this;
                    alignMenuItem.Items.Add(menuItem);

                    // Bottom
                    menuItem = new MenuItem();
                    menuItem.Header = "Bottom";
                    menuItem.Command = NFormattingCommands.AlignBottom;
                    menuItem.CommandTarget = this;
                    alignMenuItem.Items.Add(menuItem);

                    contextMenu.Items.Add(alignMenuItem);
                    contextMenu.Items.Add(new Separator());
                }

                // Rotate right
                menuItem = new MenuItem();
                menuItem.Header = "Rotate Right";
                menuItem.InputGestureText = "R";
                menuItem.Command = NDrawingCommands.RotateRight;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("rotate_right.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Rotate left
                menuItem = new MenuItem();
                menuItem.Header = "Rotate Left";
                menuItem.InputGestureText = "L";
                menuItem.Command = NDrawingCommands.RotateLeft;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("rotate_left.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Flip horizontal
                menuItem = new MenuItem();
                menuItem.Header = "Flip Horizontal";
                menuItem.InputGestureText = "H";
                menuItem.Command = NDrawingCommands.FlipHorizontal;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("fliphorizontal.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Flip vertical
                menuItem = new MenuItem();
                menuItem.Header = "Flip Vertical";
                menuItem.InputGestureText = "V";
                menuItem.Command = NDrawingCommands.FlipVertical;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("flipvertical.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Separator
                contextMenu.Items.Add(new Separator());

                // Group
                menuItem = new MenuItem();
                menuItem.Header = "Group";
                menuItem.InputGestureText = "G";
                menuItem.Command = NDrawingCommands.Group;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("group.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Ungroup
                menuItem = new MenuItem();
                menuItem.Header = "Ungroup";
                menuItem.InputGestureText = "U";
                menuItem.Command = NDrawingCommands.Ungroup;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("ungroup.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Separator
                contextMenu.Items.Add(new Separator());

                // Save Drawing...
                menuItem = new MenuItem();
                menuItem.Header = "Save Drawing...";
                menuItem.InputGestureText = " ";
                menuItem.Command = NApplicationCommands.SaveImage;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("save.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = "Edit Source";
                menuItem.InputGestureText = " ";
                menuItem.Command = NDrawingCommands.EditSource;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("page_code.png", 16, 16);
                contextMenu.Items.Add(menuItem);

                // Separator
                contextMenu.Items.Add(new Separator());

                // Label
                menuItem = new MenuItem();
                menuItem.Header = "Add to Library...";
                menuItem.Command = NDrawingCommands.AddToLibrary;
                menuItem.CommandTarget = this;
                menuItem.Icon = Images.GetImage("plus.png", 16, 16);
                contextMenu.Items.Add(menuItem);
            }

            if (contextMenu.Items.Count > 0)
            {
                if (contextMenu.Items[contextMenu.Items.Count - 1] is Separator)
                {
                    contextMenu.Items.RemoveAt(contextMenu.Items.Count - 1);
                }
            }

            return contextMenu;
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (EditingMode != EditingModes.Normal)
            {
                EditingMode = EditingModes.Normal;
                e.Handled = true;
                return;
            }

            var contextMenu = BuildContextMenu();

            if (contextMenu.Items.Count > 0)
            {
                ContextMenu = contextMenu;
                ContextMenu.IsOpen = true;
                e.Handled = true;
            }
            else
            {
                base.OnMouseRightButtonUp(e);
            }
        }

        #endregion

        #region Keyboard Input

        protected override bool OnMoveDown(INavigable target)
        {
            if (base.OnMoveDown(target))
            {
                return true;
            }

            Append(new TextParagraph());

            return OnMoveDown(target);
        }

        #endregion

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            var scrollViewer = LayoutHelper.GetAncestor<ScrollViewer>(this);
            if (scrollViewer == null)
            {
                return base.MeasureOverride(availableSize);
            }

            double desiredWidth = scrollViewer.ViewportWidth;

            foreach (UIElement child in InternalChildren)
            {
                if (DocumentPanel.GetPositioning(child) == Editor.Positioning.Relative)
                {
                    var childBounds = VisualTreeHelper.GetDescendantBounds(child);

                    desiredWidth = Math.Max(desiredWidth, childBounds.Right);
                }
            }

            double desiredHeight = availableSize.Height;

            var desiredSize = new Size(desiredWidth, desiredHeight);

            return base.MeasureOverride(desiredSize);
        }

        #endregion

        #endregion

    }

    public enum EditingModes
    {
        Normal,
        Highlighting,
        Selecting,
        Drawing,
        Erasing,
        Filling,
        Stroking,
        InsertingLine,
        InsertingClipart,
        InsertingFreetext
    }

    public enum PasteMode
    {
        Auto = 0,
        KeepFormatting,
        ClearFormatting,
        MatchFormatting
    }
}
