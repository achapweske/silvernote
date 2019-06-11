/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.SVG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace SilverNote.Editor
{
    public class TextElement : DocumentElement, ITextParagraphPropertiesSource
    {
        #region Fields

        TextBuffer _Buffer;
        TextRenderer _Renderer;
        VisualCollection _Visuals;
        Dictionary<Visual, int> _VisualLayers;

        #endregion

        #region Constructors

        public TextElement()
        {
            Initialize();
        }

        public TextElement(TextBuffer buffer)
        {
            Initialize(buffer);
        }

        public TextElement(TextElement copy)
            : base(copy)
        {
            Initialize((TextBuffer)copy.Buffer.Clone());

            if (copy.ReadLocalValue(AlwaysCollapsibleProperty) != DependencyProperty.UnsetValue)
            {
                AlwaysCollapsible = copy.AlwaysCollapsible;
            }

            if (copy.ReadLocalValue(DefaultIncrementalTabProperty) != DependencyProperty.UnsetValue)
            {
                DefaultIncrementalTab = copy.DefaultIncrementalTab;
            }

            if (copy.ReadLocalValue(FlowDirectionProperty) != DependencyProperty.UnsetValue)
            {
                FlowDirection = copy.FlowDirection;
            }

            if (copy.ReadLocalValue(IndentProperty) != DependencyProperty.UnsetValue)
            {
                Indent = copy.Indent;
            }

            if (copy.ReadLocalValue(LineHeightProperty) != DependencyProperty.UnsetValue)
            {
                LineHeight = copy.LineHeight;
            }

            if (copy.ReadLocalValue(ParagraphIndentProperty) != DependencyProperty.UnsetValue)
            {
                ParagraphIndent = copy.ParagraphIndent;
            }

            if (copy.ReadLocalValue(TabsProperty) != DependencyProperty.UnsetValue)
            {
                Tabs = copy.Tabs;
            }

            if (copy.ReadLocalValue(TabSizeProperty) != DependencyProperty.UnsetValue)
            {
                TabSize = copy.TabSize;
            }

            if (copy.ReadLocalValue(TextAlignmentProperty) != DependencyProperty.UnsetValue)
            {
                TextAlignment = copy.TextAlignment;
            }

            TextDecorations = copy.TextDecorations;

            if (copy.ReadLocalValue(TextMarkerPropertiesProperty) != DependencyProperty.UnsetValue)
            {
                TextMarkerProperties = copy.TextMarkerProperties;
            }

            if (copy.ReadLocalValue(TextWrappingProperty) != DependencyProperty.UnsetValue)
            {
                TextWrapping = copy.TextWrapping;
            }

            Padding = copy.Padding;
            VerticalContentAlignment = copy.VerticalContentAlignment;
        }

        private void Initialize(TextBuffer buffer = null)
        {
            _Buffer = buffer ?? new TextBuffer(TextProperties.Default);
            _Buffer.Owner = this;
            _Buffer.TextChanged += Buffer_TextChanged;
            _Buffer.FormatChanged += Buffer_FormatChanged;
            _Renderer = new TextRenderer(_Buffer, new TextParagraphPropertiesProxy(this));
            _Renderer.HeightChanged += Formatter_HeightChanged;
            _Renderer.TextRendered += Formatter_TextRendered;
            _Visuals = new VisualCollection(this);
            _VisualLayers = new Dictionary<Visual, int>();

            AddVisual(Renderer, 0);
        }

        #endregion

        #region Properties

        #region AlwaysCollapsible

        public static readonly DependencyProperty AlwaysCollapsibleProperty = DependencyProperty.RegisterAttached(
            "AlwaysCollapsible",
            typeof(bool),
            typeof(TextElement),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, AlwaysCollapsibleProperty_Changed)
        );

        public static void SetAlwaysCollapsible(DependencyObject dep, bool alwaysCollapsible)
        {
            dep.SetValue(AlwaysCollapsibleProperty, alwaysCollapsible);
        }

        public static bool GetAlwaysCollapsible(DependencyObject dep)
        {
            return (bool)dep.GetValue(AlwaysCollapsibleProperty);
        }

        public bool AlwaysCollapsible
        {
            get { return GetAlwaysCollapsible(this); }
            set { SetAlwaysCollapsible(this, value); }
        }

        static void AlwaysCollapsibleProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnAlwaysCollapsibleChanged((bool)e.OldValue, (bool)e.NewValue);
            }
        }

        protected virtual void OnAlwaysCollapsibleChanged(bool oldValue, bool newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => AlwaysCollapsible = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region DefaultIncrementalTab

        public static readonly DependencyProperty DefaultIncrementalTabProperty = DependencyProperty.RegisterAttached(
            "DefaultIncrementalTab",
            typeof(double),
            typeof(TextElement),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, DefaultIncrementalTabProperty_Changed)
        );

        public static void SetDefaultIncrementalTab(DependencyObject dep, double defaultIncrementalTab)
        {
            dep.SetValue(DefaultIncrementalTabProperty, defaultIncrementalTab);
        }

        public static double GetDefaultIncrementalTab(DependencyObject dep)
        {
            return (double)dep.GetValue(DefaultIncrementalTabProperty);
        }

        public double DefaultIncrementalTab
        {
            get { return GetDefaultIncrementalTab(this); }
            set { SetDefaultIncrementalTab(this, value); }
        }

        static void DefaultIncrementalTabProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnDefaultIncrementalTabChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        protected virtual void OnDefaultIncrementalTabChanged(double oldValue, double newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => DefaultIncrementalTab = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region DefaultTextRunProperties

        public virtual TextRunProperties DefaultTextRunProperties
        {
            get 
            { 
                return Buffer.GetProperties(Buffer.Length);
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        #endregion

        #region FlowDirection

        new public static readonly DependencyProperty FlowDirectionProperty = FrameworkElement.FlowDirectionProperty.AddOwner(
            typeof(TextElement),
            new FrameworkPropertyMetadata(FlowDirection.LeftToRight, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.Inherits, FlowDirectionProperty_Changed)
        );

        static void FlowDirectionProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnFlowDirectionChanged((FlowDirection)e.OldValue, (FlowDirection)e.NewValue);
            }
        }

        protected virtual void OnFlowDirectionChanged(FlowDirection oldValue, FlowDirection newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => FlowDirection = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region Indent

        public static readonly DependencyProperty IndentProperty = DependencyProperty.RegisterAttached(
            "Indent",
            typeof(double),
            typeof(TextElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, IndentProperty_Changed)
        );

        public static void SetIndent(DependencyObject dep, double indent)
        {
            dep.SetValue(IndentProperty, indent);
        }

        public static double GetIndent(DependencyObject dep)
        {
            return (double)dep.GetValue(IndentProperty);
        }

        public double Indent
        {
            get { return GetIndent(this); }
            set { SetIndent(this, value); }
        }

        static void IndentProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnIndentChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        protected virtual void OnIndentChanged(double oldValue, double newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => Indent = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region Length

        /// <summary>
        /// Get the number of characters in this paragraph
        /// </summary>
        public int Length
        {
            get { return Buffer.Length; }
        }

        #endregion

        #region LineHeight

        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.RegisterAttached(
            "LineHeight",
            typeof(double),
            typeof(TextElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, LineHeightProperty_Changed)
        );

        public static void SetLineHeight(DependencyObject dep, double lineHeight)
        {
            dep.SetValue(LineHeightProperty, lineHeight);
        }

        public static double GetLineHeight(DependencyObject dep)
        {
            return (double)dep.GetValue(LineHeightProperty);
        }

        [TypeConverterAttribute(typeof(LengthConverter))]
        public double LineHeight
        {
            get { return GetLineHeight(this); }
            set { SetLineHeight(this, value); }
        }

        static void LineHeightProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnLineHeightChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        protected virtual void OnLineHeightChanged(double oldValue, double newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => LineHeight = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region LineSpacing

        public static readonly DependencyProperty LineSpacingProperty = DependencyProperty.RegisterAttached(
            "LineSpacing",
            typeof(double),
            typeof(TextElement),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, LineSpacingProperty_Changed)
        );

        public static void SetLineSpacing(DependencyObject dep, double lineSpacing)
        {
            dep.SetValue(LineSpacingProperty, lineSpacing);
        }

        public static double GetLineSpacing(DependencyObject dep)
        {
            return (double)dep.GetValue(LineSpacingProperty);
        }

        public double LineSpacing
        {
            get { return GetLineSpacing(this); }
            set { SetLineSpacing(this, value); }
        }

        static void LineSpacingProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.Renderer.LineSpacing = (double)e.NewValue;
                textBlock.OnLineSpacingChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        protected virtual void OnLineSpacingChanged(double oldValue, double newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => LineSpacing = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region ParagraphIndent

        public static readonly DependencyProperty ParagraphIndentProperty = DependencyProperty.RegisterAttached(
            "ParagraphIndent",
            typeof(double),
            typeof(TextElement),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, ParagraphIndentProperty_Changed)
        );

        public static void SetParagraphIndent(DependencyObject dep, double paragraphIndent)
        {
            dep.SetValue(ParagraphIndentProperty, paragraphIndent);
        }

        public static double GetParagraphIndent(DependencyObject dep)
        {
            return (double)dep.GetValue(ParagraphIndentProperty);
        }

        public double ParagraphIndent
        {
            get { return GetParagraphIndent(this); }
            set { SetParagraphIndent(this, value); }
        }

        static void ParagraphIndentProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnParagraphIndentChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        protected virtual void OnParagraphIndentChanged(double oldValue, double newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => ParagraphIndent = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region ColumnWidth

        double _ColumnWidth;
        TextRunProperties _ColumnWidthProperties;

        public virtual double ColumnWidth
        {
            get
            {
                var properties = DefaultTextRunProperties;
                if (properties != _ColumnWidthProperties)
                {
                    var text = new FormattedText("x", properties.CultureInfo, FlowDirection, properties.Typeface, properties.FontRenderingEmSize, properties.ForegroundBrush);
                    _ColumnWidth = Math.Floor(text.Width);
                    _ColumnWidthProperties = properties;   
                }
                return _ColumnWidth;
            }
        }
        
        #endregion

        #region Tabs

        public static readonly DependencyProperty TabsProperty = DependencyProperty.RegisterAttached(
            "Tabs",
            typeof(IList<TextTabProperties>),
            typeof(TextElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, TabsProperty_Changed)
        );

        public static void SetTabs(DependencyObject dep, IList<TextTabProperties> tabs)
        {
            dep.SetValue(TabsProperty, tabs);
        }

        public static IList<TextTabProperties> GetTabs(DependencyObject dep)
        {
            return (IList<TextTabProperties>)dep.GetValue(TabsProperty);
        }

        public IList<TextTabProperties> Tabs
        {
            get { return GetTabs(this); }
            set { SetTabs(this, value); }
        }

        static void TabsProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnTabsChanged((IList<TextTabProperties>)e.OldValue, (IList<TextTabProperties>)e.NewValue);
            }
        }

        protected virtual void OnTabsChanged(IList<TextTabProperties> oldValue, IList<TextTabProperties> newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => Tabs = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region TabSize

        public static readonly DependencyProperty TabSizeProperty = DependencyProperty.RegisterAttached(
            "TabSize",
            typeof(int),
            typeof(TextElement),
            new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, TabSizeProperty_Changed)
        );

        public static void SetTabSize(DependencyObject dep, int defaultIncrementalTab)
        {
            dep.SetValue(TabSizeProperty, defaultIncrementalTab);
        }

        public static int GetTabSize(DependencyObject dep)
        {
            return (int)dep.GetValue(TabSizeProperty);
        }

        public int TabSize
        {
            get { return GetTabSize(this); }
            set { SetTabSize(this, value); }
        }

        static void TabSizeProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnTabSizeChanged((int)e.OldValue, (int)e.NewValue);
            }
        }

        protected virtual void OnTabSizeChanged(int oldValue, int newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => TabSize = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region Text

        /// <summary>
        /// Get/set the underlying text
        /// </summary>
        public string Text
        {
            get { return Buffer.Text; }
            set { Buffer.Text = value; }
        }

        #endregion

        #region TextAlignment

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.RegisterAttached(
             "TextAlignment",
             typeof(TextAlignment),
             typeof(TextElement),
             new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, TextAlignmentProperty_Changed)
        );

        public static void SetTextAlignment(DependencyObject dep, TextAlignment textAlignment)
        {
            dep.SetValue(TextAlignmentProperty, textAlignment);
        }

        public static TextAlignment GetTextAlignment(DependencyObject dep)
        {
            return (TextAlignment)dep.GetValue(TextAlignmentProperty);
        }

        public TextAlignment TextAlignment
        {
            get { return GetTextAlignment(this); }
            set { SetTextAlignment(this, value); }
        }

        static void TextAlignmentProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnTextAlignmentChanged((TextAlignment)e.OldValue, (TextAlignment)e.NewValue);
            }
        }

        protected virtual void OnTextAlignmentChanged(TextAlignment oldValue, TextAlignment newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => TextAlignment = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region TextDecorations

        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
            "TextDecorations",
            typeof(TextDecorationCollection),
            typeof(TextElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, TextDecorationsProperty_Changed)
        );

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        static void TextDecorationsProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnTextDecorationsChanged((TextDecorationCollection)e.OldValue, (TextDecorationCollection)e.NewValue);
            }
        }

        protected virtual void OnTextDecorationsChanged(TextDecorationCollection oldValue, TextDecorationCollection newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => TextDecorations = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region TextMarkerProperties

        public static readonly DependencyProperty TextMarkerPropertiesProperty = DependencyProperty.RegisterAttached(
            "TextMarkerProperties",
            typeof(TextMarkerProperties),
            typeof(TextElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, TextMarkerPropertiesProperty_Changed)
        );

        public static void SetTextMarkerProperties(DependencyObject dep, TextMarkerProperties textMarkerProperties)
        {
            dep.SetValue(TextMarkerPropertiesProperty, textMarkerProperties);
        }

        public static TextMarkerProperties GetTextMarkerProperties(DependencyObject dep)
        {
            return (TextMarkerProperties)dep.GetValue(TextMarkerPropertiesProperty);
        }

        public TextMarkerProperties TextMarkerProperties
        {
            get { return GetTextMarkerProperties(this); }
            set { SetTextMarkerProperties(this, value); }
        }

        static void TextMarkerPropertiesProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnTextMarkerPropertiesChanged((TextMarkerProperties)e.OldValue, (TextMarkerProperties)e.NewValue);
            }
        }

        protected virtual void OnTextMarkerPropertiesChanged(TextMarkerProperties oldValue, TextMarkerProperties newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => TextMarkerProperties = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region TextWrapping

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.RegisterAttached(
            "TextWrapping",
            typeof(TextWrapping),
            typeof(TextElement),
            new FrameworkPropertyMetadata(TextWrapping.Wrap, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, TextWrappingProperty_Changed)
        );

        public static void SetTextWrapping(DependencyObject dep, TextWrapping textWrapping)
        {
            dep.SetValue(TextWrappingProperty, textWrapping);
        }

        public static TextWrapping GetTextWrapping(DependencyObject dep)
        {
            return (TextWrapping)dep.GetValue(TextWrappingProperty);
        }

        public TextWrapping TextWrapping
        {
            get { return GetTextWrapping(this); }
            set { SetTextWrapping(this, value); }
        }

        static void TextWrappingProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextElement;
            if (textBlock != null)
            {
                textBlock.OnTextWrappingChanged((TextWrapping)e.OldValue, (TextWrapping)e.NewValue);
            }
        }

        protected virtual void OnTextWrappingChanged(TextWrapping oldValue, TextWrapping newValue)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => TextWrapping = oldValue);
            }

            Renderer.Invalidate();
        }

        #endregion

        #region Padding

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            "Padding",
            typeof(Thickness),
            typeof(TextElement),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        #endregion

        #region VerticalContentAlignment

        public static readonly DependencyProperty VerticalContentAlignmentProperty = DependencyProperty.Register(
            "VerticalContentAlignment",
            typeof(VerticalAlignment),
            typeof(TextElement),
            new FrameworkPropertyMetadata(VerticalAlignment.Top, FrameworkPropertyMetadataOptions.AffectsArrange)
        );

        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Insert a string into this paragraph.
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void Insert(int offset, string value)
        {
            int length = value.Length;
            if (length == 0)
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Insert Text"))
            {
                // Update undo stack
                undo.Push(() => Delete(offset, length));

                // Update text buffer
                Buffer.Insert(offset, value);

                // Redraw text
                Renderer.Draw(offset, length, 0);
            }
        }

        /// <summary>
        /// Delete characters from this paragraph.
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void Delete(int offset, int length)
        {
            if (length == 0)
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Delete Text"))
            {
                // Delete text
                var deleted = Buffer.Cut(offset, length);

                // Update undo stack
                undo.Push(() => Paste(offset, deleted));

                // Redraw text
                Renderer.Draw(offset, 0, length);
            }
        }

        /// <summary>
        /// Cut text from this paragraph. 
        /// 
        /// This operation is undo-able.
        /// </summary>
        public TextBuffer Cut(int offset, int length)
        {
            using (var undo = new UndoScope(UndoStack, "Cut Text"))
            {
                // Cut text
                var deleted = Buffer.Cut(offset, length);

                // Update undo stack
                undo.Push(() => Paste(offset, deleted));

                // Redraw text
                Renderer.Draw(offset, 0, length);

                return (TextBuffer)deleted.Clone();
            }
        }

        /// <summary>
        /// Get a copy of the text within the specified range.
        /// </summary>
        public TextBuffer Copy(int offset, int length)
        {
            return Buffer.Copy(offset, length);
        }

        /// <summary>
        /// Paste text into this paragraph.
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void Paste(int offset, TextBuffer value)
        {
            if (value.Length == 0)
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Paste Text"))
            {
                // Update undo stack
                int length = value.Length;
                undo.Push(() => Delete(offset, length));

                // Paste new data
                Buffer.Paste(offset, value);

                // Redraw text
                Renderer.Draw(offset, length, 0);
            }
        }

        /// <summary>
        /// Get the text properties at the given character offset.
        /// 
        /// More precisely, this returns the properties that would be
        /// applied to a character inserted at the given offset:
        /// 
        ///  *If Length = 0, this returns the default text properties.
        ///  *If Length > 0 and offset = Length, this returns the text
        ///   properties of the last character in the text block.
        ///  *Otherwise, this returns the text properties of the character
        ///   at the given offset
        /// </summary>
        public GenericTextRunProperties GetTextProperties(int offset)
        {
            return Buffer.GetProperties(offset);
        }

        /// <summary>
        /// Set all properties for a given span of text (font family, size, color, etc.)
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void SetTextProperties(int offset, int length, GenericTextRunProperties properties)
        {
            using (var undo = new UndoScope(UndoStack, "Set text properties"))
            {
                // Apply properties
                var oldProperties = Buffer.SetProperties(offset, length, properties);

                // Update undo stack
                undo.Push(() => SetTextProperties(offset, length, oldProperties));

                // Redraw text
                Renderer.Draw(offset, length, length);
            }
        }

        /// <summary>
        /// Set selected text's properties (font family, size, color, etc.)
        /// 
        /// This operation is undo-able.
        /// </summary>
        /// <param name="newValues">An array where each item contains the new property 
        /// values and the number of character to which they should be applied</param>
        public void SetTextProperties(int offset, int length, IList<TextPropertiesValue> newValues)
        {
            using (var undo = new UndoScope(UndoStack, "Set text properties"))
            {
                // Apply properties
                var oldValues = Buffer.SetProperties(offset, length, newValues);

                // Update undo stack
                undo.Push(() => SetTextProperties(offset, length, oldValues));

                // Redraw text
                Renderer.Draw(offset, length, length);
            }
        }

        /// <summary>
        /// Reset all text properties to their default values for the specified range
        /// </summary>
        public void ResetTextProperties(int offset, int length)
        {
            SetTextProperties(offset, length, TextProperties.Default);
        }

        /// <summary>
        /// Get a property of the text at the given character offset
        /// </summary>
        public object GetTextProperty(int charOffset, string name)
        {
            return Buffer.GetProperties(charOffset).GetProperty(name);
        }

        /// <summary>
        /// Set selected text's properties (font family, size, color, etc.)
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void SetTextProperty(int offset, int length, string name, object newValue)
        {
            if (!((IFormattable)DefaultTextRunProperties).HasProperty(name))
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Set text property: " + name))
            {
                // Update undo stack
                var oldValues = Buffer.GetProperty(name, offset, length);
                undo.Push(() => SetTextProperty(offset, length, name, oldValues));

                // Apply property
                Buffer.SetProperty(offset, length, name, newValue);

                // Redraw text
                Renderer.Draw(offset, length, length);
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
        public void SetTextProperty(int offset, int length, string name, IList<TextPropertyValue> newValues)
        {
            if (newValues == null)
            {
                SetTextProperty(offset, length, name, (object)null);
                return;
            }

            if (!((IFormattable)DefaultTextRunProperties).HasProperty(name))
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Set text property: " + name))
            {
                // Update undo stack
                var oldValues = Buffer.GetProperty(name, offset, length);
                undo.Push(() => SetTextProperty(offset, length, name, oldValues));

                // Apply property
                Buffer.SetProperty(offset, length, name, newValues);

                // Redraw text
                Renderer.Draw(offset, length, length);
            }
        }

        /// <summary>
        /// Change a property to newValue wherever it's currently set to oldValue
        /// 
        /// This operation is undo-able.
        /// </summary>
        public int ChangeTextProperty(string name, object oldValue, object newValue)
        {
            if (!((IFormattable)DefaultTextRunProperties).HasProperty(name))
            {
                return 0;
            }

            var oldValues = Buffer.GetProperty(name, 0, Buffer.Length);
            if (!oldValues.Any(property => Object.Equals(property.Value, oldValue)))
            {
                return 0;
            }

            using (var undo = new UndoScope(UndoStack, "Change text property: " + name))
            {
                // Update undo stack
                undo.Push(() => SetTextProperty(0, Length, name, oldValues));

                // Apply property
                int result = Buffer.ChangeProperty(name, oldValue, newValue);

                // Redraw text
                Renderer.Draw();

                return result;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the text content of this paragraph changes
        /// </summary>
        public event EventHandler<TextChangedEventArgs> TextChanged;

        /// <summary>
        /// Invoke the TextChanged event
        /// </summary>
        /// <param name="charOffset">
        ///   Index of the first character that was modified
        /// </param>
        /// <param name="numAdded">
        ///   Number of characters beginning at charOffset that were added
        /// </param>
        /// <param name="numRemoved">
        ///   Number of characters beginning at charOffset that were removed
        /// </param>
        private void RaiseTextChanged(int charOffset, int numAdded, int numRemoved)
        {
            if (TextChanged != null)
            {
                var e = new TextChangedEventArgs(charOffset, numAdded, numRemoved);
                TextChanged(this, e);
            }
        }

        /// <summary>
        /// Invoked when text formatting within this paragraph changes
        /// </summary>
        public event EventHandler<TextFormatChangedEventArgs> TextFormatChanged;

        /// <summary>
        /// Invoke the TextFormatChanged event
        /// </summary>
        /// <param name="offset">
        ///   Offset of the first affected character
        /// </param>
        /// <param name="length">
        ///   Number of affected characters
        /// </param>
        /// <param name="propertyName">
        ///   Name of the property that changed or null if they all changed
        /// </param>
        private void RaiseTextFormatChanged(int offset, int length, string propertyName)
        {
            if (TextFormatChanged != null)
            {
                var e = new TextFormatChangedEventArgs(offset, length, propertyName);
                TextFormatChanged(this, e);
            }
        }

        /// <summary>
        /// Invoked when the text has been re-drawn
        /// </summary>
        public event EventHandler TextRendered;

        /// <summary>
        /// Invoke the TextRendered event
        /// </summary>
        private void RaiseTextRendered()
        {
            if (TextRendered != null)
            {
                TextRendered(this, EventArgs.Empty);
            }
        }

        #endregion

        #region HTML

        public override string GetHTMLNodeName(NodeContext context)
        {
            if (Length == 0)
            {
                return HTMLElements.BR;
            }
            else
            {
                return HTMLElements.P;
            }
        }

        public override void SetHTMLAttribute(ElementContext context, string name, string value)
        {
            // Hack to allow inlining embedded SVG within an HTML document
            if (context.NodeName == SVGElements.TEXT)
            {
                SetSVGAttribute(context, name, value);
            }
            else
            {
                base.SetHTMLAttribute(context, name, value);
            }
        }

        public override IEnumerable<object> GetHTMLChildNodes(NodeContext context)
        {
            return Buffer.GetChildNodes(context);
        }

        public override object CreateHTMLNode(NodeContext context)
        {
            return Buffer.CreateNode(context);
        }

        public override void AppendHTMLNode(NodeContext context, object newChild)
        {
            Buffer.AppendNode(context, newChild);
        }

        public override void InsertHTMLNode(NodeContext context, object newChild, object refChild)
        {
            Buffer.InsertNode(context, newChild, refChild);
        }

        public override void RemoveHTMLNode(NodeContext context, object oldChild)
        {
            Buffer.RemoveNode(context, oldChild);
        }

        private static readonly string[] _HTMLStyles = new[] {
            CSSProperties.TextAlign, 
            CSSProperties.LineHeight,
            CSSProperties.PaddingLeft,
            CSSProperties.PaddingRight,
            CSSProperties.PaddingTop,
            CSSProperties.PaddingBottom
        };

        public override IList<string> GetHTMLStyles(ElementContext context)
        {
            return _HTMLStyles
                .Union(base.GetHTMLStyles(context))
                .ToList();
        }

        public override CSSValue GetHTMLStyle(ElementContext context, string propertyName)
        {
            switch (propertyName)
            {
                case CSSProperties.TextAlign:
                    return CSSConverter.ToCSSValue(TextAlignment);
                case CSSProperties.LineHeight:
                    if (LineHeight != 0)
                        return CSSValues.Pixels(LineHeight);
                    else if (LineSpacing > 1.0)
                        return CSSValues.Number(LineSpacing);
                    return CSSValues.Normal;
                case CSSProperties.PaddingTop:
                    return CSSValues.Pixels(Padding.Top);
                case CSSProperties.PaddingRight:
                    return CSSValues.Pixels(Padding.Right);
                case CSSProperties.PaddingBottom:
                    return CSSValues.Pixels(Padding.Bottom);
                case CSSProperties.PaddingLeft:
                    return CSSValues.Pixels(Padding.Left);
                default:
                    return base.GetHTMLStyle(context, propertyName);
            }
        }

        public override void SetHTMLStyle(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.TextAlign:
                    SetTextAlignment(value);
                    break;
                case CSSProperties.LineHeight:
                    SetLineHeight(value);
                    break;
                case CSSProperties.PaddingTop:
                    SetPaddingTop(value);
                    break;
                case CSSProperties.PaddingRight:
                    SetPaddingRight(value);
                    break;
                case CSSProperties.PaddingBottom:
                    SetPaddingBottom(value);
                    break;
                case CSSProperties.PaddingLeft:
                    SetPaddingLeft(value);
                    break;
                default:
                    base.SetHTMLStyle(context, name, value);
                    break;
            }
        }

        protected virtual void SetTextAlignment(CSSValue cssValue)
        {
            TextAlignment = CSSConverter.ToTextAlignment(cssValue);
        }

        protected virtual void SetLineHeight(CSSValue cssValue)
        {
            if (cssValue == CSSValues.Normal)
            {
                LineSpacing = 1.0;
                return;
            }

            var primitiveValue = cssValue as CSSPrimitiveValue;
            if (primitiveValue != null)
            {
                if (primitiveValue.PrimitiveType == CSSPrimitiveType.CSS_NUMBER)
                {
                    LineSpacing = primitiveValue.GetFloatValue(CSSPrimitiveType.CSS_NUMBER);
                }
                else if (primitiveValue.IsLength())
                {
                    LineHeight = primitiveValue.GetFloatValue(CSSPrimitiveType.CSS_PX);
                }
            }
        }

        protected virtual void SetPaddingTop(CSSValue cssValue)
        {
            double paddingTop = CSSConverter.ToLength(cssValue, CSSPrimitiveType.CSS_PX, Padding.Top);
            Padding = new Thickness(Padding.Left, paddingTop, Padding.Right, Padding.Bottom);
        }

        protected virtual void SetPaddingRight(CSSValue cssValue)
        {
            double paddingRight = CSSConverter.ToLength(cssValue, CSSPrimitiveType.CSS_PX, Padding.Right);
            Padding = new Thickness(Padding.Left, Padding.Top, paddingRight, Padding.Bottom);
        }

        protected virtual void SetPaddingBottom(CSSValue cssValue)
        {
            double paddingBottom = CSSConverter.ToLength(cssValue, CSSPrimitiveType.CSS_PX, Padding.Bottom);
            Padding = new Thickness(Padding.Left, Padding.Top, Padding.Right, paddingBottom);
        }

        protected virtual void SetPaddingLeft(CSSValue cssValue)
        {
            double paddingLeft = CSSConverter.ToLength(cssValue, CSSPrimitiveType.CSS_PX, Padding.Left);
            Padding = new Thickness(paddingLeft, Padding.Top, Padding.Right, Padding.Bottom);
        }

        #endregion

        #region SVG

        string _SVGNodeName = SVGElements.NAMESPACE + " " + SVGElements.TEXT;

        public override string GetSVGNodeName(NodeContext context)
        {
            return _SVGNodeName;
        }

        string[] _SVGAttributes = new[] {
            SVGAttributes.X,
            SVGAttributes.Y,
            SVGAttributes.FILL
        };

        public override IList<string> GetSVGAttributes(ElementContext context)
        {
            if (Buffer.Length == 0)
            {
                return _SVGAttributes.Concat(Buffer.DefaultProperties.GetNodeAttributes(context)).ToList();
            }
            else
            {
                return _SVGAttributes;
            }
        }

        public override string GetSVGAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.X:
                    return SafeConvert.ToString(Left);
                case SVGAttributes.Y:
                    return SafeConvert.ToString(Top);
                case SVGAttributes.FILL:
                    if (Buffer.Length > 0)
                        return "black";
                    else
                        return null;
                default:
                    if (Buffer.Length == 0)
                        return Buffer.DefaultProperties.GetNodeAttribute(context, name);
                    else
                        return null;
            }
        }

        public override void SetSVGAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.X:
                    Left = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.Y:
                    Top = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.FILL:
                    break;
                default:
                    Buffer.DefaultProperties.SetNodeAttribute(context, name, value);
                    break;
            }
        }

        public override void ResetSVGAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.X:
                    Left = 0;
                    break;
                case SVGAttributes.Y:
                    Top = 0;
                    break;
                case SVGAttributes.FILL:
                    break;
                default:
                    Buffer.DefaultProperties.ResetNodeAttribute(context, name);
                    break;
            }
        }

        public override IEnumerable<object> GetSVGChildNodes(NodeContext context)
        {
            if (Buffer.Length == 0)
            {
                return null;
            }

            UpdateLayout();

            // Ensure rendering is up-to-date (since SVG looks at rendered properties).

            //Debug.Assert(IsMeasureValid && IsArrangeValid);

            Point offset = new Point(Left, Top) + Renderer.Offset;

            return Renderer.GetSVGChildNodes(context, offset);
        }

        public override object CreateSVGNode(NodeContext context)
        {
            return Buffer.CreateNode(context);
        }

        public override void AppendSVGNode(NodeContext context, object newChild)
        {
            Buffer.AppendNode(context, newChild);
        }

        public override void InsertSVGNode(NodeContext context, object newChild, object refChild)
        {
            Buffer.InsertNode(context, newChild, refChild);
        }

        public override void RemoveSVGNode(NodeContext context, object oldChild)
        {
            Buffer.RemoveNode(context, oldChild);
        }

        string[] _SVGStyles = new[] {
            CSSProperties.Width,
            CSSProperties.Height,
            CSSProperties.VerticalAlign
        };

        public override IList<string> GetSVGStyles(ElementContext context)
        {
            return _SVGStyles.Concat(GetHTMLStyles(context)).ToList();
        }

        public override CSSValue GetSVGStyle(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.Width:
                    return CSSValues.Pixels(Width);
                case CSSProperties.Height:
                    return CSSValues.Pixels(Height);
                case CSSProperties.VerticalAlign:
                    if (VerticalContentAlignment != VerticalAlignment.Top)
                        return CSSConverter.ToCSSValue(VerticalContentAlignment);
                    else
                        return null;
                default:
                    return GetHTMLStyle(context, name);
            }
        }

        public override void SetSVGStyle(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.Width:
                    var width = (CSSPrimitiveValue)value;
                    if (width.IsLength())
                        Width = Math.Max(width.GetFloatValue(CSSPrimitiveType.CSS_PX), 0);
                    else if (width.PrimitiveType == CSSPrimitiveType.CSS_NUMBER)
                        Width = Math.Max(width.GetFloatValue(CSSPrimitiveType.CSS_NUMBER), 0);
                    break;
                case CSSProperties.Height:
                    var height = (CSSPrimitiveValue)value;
                    if (height.IsLength())
                        Height = Math.Max(height.GetFloatValue(CSSPrimitiveType.CSS_PX), 0);
                    else if (height.PrimitiveType == CSSPrimitiveType.CSS_NUMBER)
                        Height = Math.Max(height.GetFloatValue(CSSPrimitiveType.CSS_NUMBER), 0);
                    break;
                case CSSProperties.VerticalAlign:
                    VerticalContentAlignment = CSSConverter.ToVerticalAlignment(value, VerticalAlignment.Top);
                    break;
                default:
                    SetHTMLStyle(context, name, value);
                    break;
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        /// The underlying text buffer
        /// </summary>
        protected TextBuffer Buffer
        {
            get { return _Buffer; }
        }

        /// <summary>
        /// The formatter is what actually draws the text glyphs
        /// </summary>
        protected TextRenderer Renderer
        {
            get { return _Renderer; }
        }

        /// <summary>
        /// Called when the text content of this paragraph changes
        /// </summary>
        /// <param name="offset">Index of the first character that was modified</param>
        /// <param name="numAdded">Number of characters beginning at charOffset that were added</param>
        /// <param name="numRemoved">Number of characters beginning at charOffset that were removed</param>
        protected virtual void OnTextChanged(int offset, int numAdded, int numRemoved)
        {
            RaiseTextChanged(offset, numAdded, numRemoved);
        }

        /// <summary>
        /// Called when one or more text properties change.
        /// </summary>
        /// <param name="offset">Offset of first affected character</param>
        /// <param name="length">Number of affected characters</param>
        /// <param name="propertyName">Name of the changed property, or null if all properties have changed</param>
        protected virtual void OnTextPropertyChanged(int offset, int length, string propertyName)
        {
            RaiseTextFormatChanged(offset, length, propertyName);
        }

        /// <summary>
        /// Called when the text has been re-drawn
        /// </summary>
        protected virtual void OnTextRendered()
        {
            RaiseTextRendered();
        }

        /// <summary>
        /// Called when the contents of the text buffer changes
        /// </summary>
        void Buffer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Renderer.Invalidate(e.Offset, e.NumAdded, e.NumRemoved);

            OnTextChanged(e.Offset, e.NumAdded, e.NumRemoved);
        }

        /// <summary>
        /// Called when a text property changes for any text in this paragraph
        /// </summary>
        void Buffer_FormatChanged(object sender, TextFormatChangedEventArgs e)
        {
            Renderer.Invalidate(e.Offset, e.Length, e.Length);

            OnTextPropertyChanged(e.Offset, e.Length, e.PropertyName);
        }

        /// <summary>
        /// Called when the formatter's Height property changes
        /// </summary>
        void Formatter_HeightChanged(object sender, EventArgs e)
        {
            // If height has changed, trigger a new layout pass
            //
            // Only do this if we're NOT being called within a layout pass

            if (IsMeasureValid && IsArrangeValid)
            {
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Called when any portion of the text has been (re)rendered
        /// </summary>
        void Formatter_TextRendered(object sender, EventArgs e)
        {
            OnTextRendered();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = availableSize;

            if (!Double.IsNaN(Width))
            {
                desiredSize.Width = Width;
            }

            // Determine our desired width after padding has been added

            double desiredContentWidth = Math.Max(desiredSize.Width - Padding.Left - Padding.Right, 0);

            // Measure the text

            if (Renderer.Width != desiredContentWidth || Renderer.LineCount == 0)
            {
                if (Double.IsInfinity(availableSize.Width))
                {
                    Renderer.Width = Renderer.MinMaxParagraphWidth.MaxWidth;
                }
                else
                {
                    Renderer.Width = desiredContentWidth;
                }
            }

            // Re-render text as needed so that Formatter.Height is valid

            if (!Renderer.IsRenderValid)
            {
                Renderer.Draw();
            }

            // Return the *actual* width/height of the paragraph

            if (Double.IsInfinity(desiredSize.Width))
            {
                desiredSize.Width = Renderer.Width + Padding.Left + Padding.Right;
            }

            desiredSize.Height = Renderer.Height + Padding.Top + Padding.Bottom;

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // Due to the formatter's whitespace handling, it can draw whitespace
            // (and place the caret) out-of-bounds, so we must explicitly clip it

            Renderer.Clip = new RectangleGeometry(new Rect(finalSize));

            // Arrange the text

            if (finalSize.Width != DesiredSize.Width || Renderer.LineCount == 0)
            {
                Renderer.Width = Math.Max(finalSize.Width - Padding.Left - Padding.Right, 0);
            }

            // Re-render text as needed so that Formatter.Height is valid

            if (!Renderer.IsRenderValid)
            {
                Renderer.Draw();
            }

            // Vertically-align the paragraph as specified

            double yOffset = 0;
            switch (VerticalContentAlignment)
            {
                case VerticalAlignment.Top:
                    yOffset = Padding.Top;
                    break;
                case VerticalAlignment.Center:
                    yOffset = finalSize.Height / 2 - Renderer.Height / 2;
                    break;
                case VerticalAlignment.Bottom:
                    yOffset = finalSize.Height - Renderer.Height - Padding.Bottom;
                    break;
                case VerticalAlignment.Stretch:
                default:
                    yOffset = Padding.Top;
                    break;
            }
            Renderer.Offset = new Vector(Padding.Left, yOffset);

            return finalSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (!Renderer.IsRenderValid)
            {
                Renderer.Draw();
            }
        }

        #region Visual Children

        protected void AddVisual(Visual visual, int layer = 0)
        {
            RemoveVisual(visual);

            int i;
            for (i = 0; i < _Visuals.Count; i++)
            {
                int currentLayer;
                if (!_VisualLayers.TryGetValue(_Visuals[i], out currentLayer))
                {
                    continue;
                }

                if (currentLayer > layer)
                {
                    break;
                }
            }

            _Visuals.Insert(i, visual);
            _VisualLayers[visual] = layer;

        }

        protected void RemoveVisual(Visual visual)
        {
            _VisualLayers.Remove(visual);
            _Visuals.Remove(visual);
        }

        protected bool HasVisual(Visual visual)
        {
            return _Visuals.Contains(visual);
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return base.VisualChildrenCount + _Visuals.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < base.VisualChildrenCount)
            {
                return base.GetVisualChild(index);
            }

            index -= base.VisualChildrenCount;

            return _Visuals[index];
        }

        #endregion

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new TextElement(this);
        }

        #endregion

    }
}
