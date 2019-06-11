/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Input;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Xml;
using SilverNote.Common;
using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.SVG;
using System.ComponentModel;
using SilverNote.Commands;

namespace SilverNote.Editor
{
    public class TextParagraph : TextField, ICloneable
    {
        #region Fields

        ElementTreeNode _Section;
        ElementTreeNode _List;
        ToolTip _ToolTip;
        DispatcherTimer _HyperlinkTimer;

        #endregion

        #region Constructors

        public TextParagraph()
            : base()
        {
            Initialize();
        }

        public TextParagraph(TextBuffer buffer)
            : base(buffer)
        {
            Initialize();
        }

        public TextParagraph(TextParagraph copy)
            : base(copy)
        {
            Initialize();

            IsHeading = copy.IsHeading;
            HeadingLevel = copy.HeadingLevel;
            IsListItem = copy.IsListItem;
            ListLevel = copy.ListLevel;
            ListStyle = copy.ListStyle;
            ListNumber = copy.ListNumber;
            LeftMargin = copy.LeftMargin;
            IsCollapsed = copy.IsCollapsed;
            IsSourceCode = copy.IsSourceCode;
        }

        private void Initialize()
        {
            _ToolTip = new ToolTip();

            TextFormatChanged += UpdateIsHeading;
            IsHeadingChanged += UpdateHeading;
            IsHeadingChanged += UpdateSectionNumbering;
            HeadingLevelChanged += UpdateHeading;
            HeadingLevelChanged += UpdateSectionNumbering;
            IsCollapsedChanged += UpdateHeading;

            IsListItemChanged += UpdateBullet;
            ListStyleChanged += UpdateBullet;
            ListNumberChanged += UpdateBullet;

            IsListItemChanged += UpdateListExpander;
            IsListItemChanged += UpdateListNumbering;
            LeftMarginChanged += UpdateListNumbering;
            IsCollapsedChanged += UpdateListExpander;
            MouseEnter += UpdateListExpander;
            MouseLeave += UpdateListExpander;

            TextFormatChanged += UpdateIsSourceCode;
            IsSourceCodeChanged += UpdateSourceCode;

            IsHeadingChanged += UpdateIndentation;
            IsListItemChanged += UpdateIndentation;
            LeftMarginChanged += UpdateIndentation;

            MouseMove += UpdateToolTip;
            MouseLeftButtonDown += UpdateToolTip;
            MouseLeave += UpdateToolTip;

            MouseMove += UpdateMouseOverHyperlink;
            MouseLeftButtonDown += UpdateMouseOverHyperlink;
            MouseLeave += UpdateMouseOverHyperlink;

            UpdateHeading(this, EventArgs.Empty);
            UpdateBullet(this, EventArgs.Empty);
            UpdateListExpander(this, EventArgs.Empty);
        }

        #endregion

        #region Routed Events

        #region HyperlinkClicked

        public static readonly RoutedEvent HyperlinkClicked = EventManager.RegisterRoutedEvent(
            "HyperlinkClicked",
            RoutingStrategy.Bubble,
            typeof(HyperlinkClickedEventHandler),
            typeof(TextParagraph)
        );

        public static void AddHyperlinkClickedHandler(DependencyObject dep, HyperlinkClickedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(HyperlinkClicked, handler);
            }
        }

        public static void RemoveHyperlinkClickedHandler(DependencyObject dep, HyperlinkClickedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(HyperlinkClicked, handler);
            }
        }

        private void RaiseHyperlinkClicked(string uri, string target)
        {
            var e = new HyperlinkClickedEventArgs(TextParagraph.HyperlinkClicked, this, uri, target);

            RaiseEvent(e);
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler IsHeadingChanged;

        protected void RaiseIsHeadingChanged()
        {
            if (IsHeadingChanged != null)
            {
                IsHeadingChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler HeadingLevelChanged;

        protected void RaiseHeadingLevelChanged()
        {
            if (HeadingLevelChanged != null)
            {
                HeadingLevelChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler IsListItemChanged;

        protected void RaiseIsListItemChanged()
        {
            if (IsListItemChanged != null)
            {
                IsListItemChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ListLevelChanged;

        protected void RaiseListLevelChanged()
        {
            if (ListLevelChanged != null)
            {
                ListLevelChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ListStyleChanged;

        protected void RaiseListStyleChanged()
        {
            if (ListStyleChanged != null)
            {
                ListStyleChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ListNumberChanged;

        protected void RaiseListNumberChanged()
        {
            if (ListNumberChanged != null)
            {
                ListNumberChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler LeftMarginChanged;

        protected void RaiseLeftMarginChanged()
        {
            if (LeftMarginChanged != null)
            {
                LeftMarginChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler IsCollapsedChanged;

        protected void RaiseIsCollapsedChanged()
        {
            if (IsCollapsedChanged != null)
            {
                IsCollapsedChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler IsSourceCodeChanged;

        protected void RaiseIsSourceCodeChanged()
        {
            if (IsSourceCodeChanged != null)
            {
                IsSourceCodeChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Dependency Properties

        #region IsHeading

        public static readonly DependencyProperty IsHeadingProperty = DependencyProperty.RegisterAttached(
            "IsHeading",
            typeof(bool),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(
                false, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnIsHeadingChanged(e);
                })
        );

        public bool IsHeading
        {
            get { return (bool)GetValue(IsHeadingProperty); }
            set { SetValue(IsHeadingProperty, value); }
        }

        protected virtual void OnIsHeadingChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                bool oldValue = (bool)e.OldValue;
                UndoStack.Push(() => IsHeading = oldValue);
            }

            RaiseIsHeadingChanged();
        }

        #endregion

        #region HeadingLevel

        public static readonly DependencyProperty HeadingLevelProperty = DependencyProperty.RegisterAttached(
            "HeadingLevel",
            typeof(int),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(
                0, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnHeadingLevelChanged(e);
                })
        );

        public int HeadingLevel
        {
            get { return (int)GetValue(HeadingLevelProperty); }
            set { SetValue(HeadingLevelProperty, value); }
        }

        protected virtual void OnHeadingLevelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                int oldValue = (int)e.OldValue;
                UndoStack.Push(() => HeadingLevel = oldValue);
            }

            RaiseHeadingLevelChanged();
        }

        #endregion

        #region IsListItem

        public static readonly DependencyProperty IsListItemProperty = DependencyProperty.RegisterAttached(
            "IsListItem",
            typeof(bool),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(
                false, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnIsListItemChanged(e);
                })
        );

        public bool IsListItem
        {
            get { return (bool)GetValue(IsListItemProperty); }
            set { SetValue(IsListItemProperty, value); }
        }

        protected virtual void OnIsListItemChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                bool oldValue = (bool)e.OldValue;
                UndoStack.Push(() => IsListItem = oldValue);
            }

            RaiseIsListItemChanged();
        }

        #endregion

        #region ListLevel

        public static readonly DependencyProperty ListLevelProperty = DependencyProperty.RegisterAttached(
            "ListLevel",
            typeof(int),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(
                0, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnListLevelChanged(e);
                })
        );

        public int ListLevel
        {
            get { return (int)GetValue(ListLevelProperty); }
            set { SetValue(ListLevelProperty, value); }
        }

        protected virtual void OnListLevelChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                int oldValue = (int)e.OldValue;
                UndoStack.Push(() => ListLevel = oldValue);
            }

            RaiseListLevelChanged();
        }

        #endregion

        #region ListStyle

        public static readonly DependencyProperty ListStyleProperty = DependencyProperty.RegisterAttached(
            "ListStyle",
            typeof(IListStyle),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(
                ListStyles.Circle, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnListStyleChanged(e);
                })
        );

        public IListStyle ListStyle
        {
            get { return (IListStyle)GetValue(ListStyleProperty); }
            set { SetValue(ListStyleProperty, value); }
        }

        protected virtual void OnListStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                IListStyle oldValue = (IListStyle)e.OldValue;
                UndoStack.Push(() => ListStyle = oldValue);
            }

            RaiseListStyleChanged();
        }

        #endregion

        #region ListNumber

        public static readonly DependencyProperty ListNumberProperty = DependencyProperty.RegisterAttached(
            "ListNumber",
            typeof(int),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(
                0, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnListNumberChanged(e);
                })
        );

        public int ListNumber
        {
            get { return (int)GetValue(ListNumberProperty); }
            set { SetValue(ListNumberProperty, value); }
        }

        protected virtual void OnListNumberChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                int oldValue = (int)e.OldValue;
                UndoStack.Push(() => ListNumber = oldValue);
            }

            RaiseListNumberChanged();
        }

        #endregion

        #region LeftMargin

        public static readonly DependencyProperty LeftMarginProperty = DependencyProperty.RegisterAttached(
            "LeftMargin",
            typeof(double),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(
                0.0, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnLeftMarginChanged(e);
                })
        );

        public double LeftMargin
        {
            get { return (double)GetValue(LeftMarginProperty); }
            set { SetValue(LeftMarginProperty, value); }
        }

        protected virtual void OnLeftMarginChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                double oldValue = (double)e.OldValue;
                UndoStack.Push(() => LeftMargin = oldValue);
            }

            RaiseLeftMarginChanged();
        }

        #endregion

        #region IsCollapsed

        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.RegisterAttached(
            "IsCollapsed",
            typeof(bool),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(false, 
                FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnIsCollapsedChanged(e);
                })
        );

        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        protected virtual void OnIsCollapsedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                bool oldValue = (bool)e.OldValue;
                UndoStack.Push(() => IsCollapsed = oldValue);
            }

            RaiseIsCollapsedChanged();
        }

        #endregion

        #region IsSourceCode

        public static readonly DependencyProperty IsSourceCodeProperty = DependencyProperty.RegisterAttached(
            "IsSourceCode",
            typeof(bool),
            typeof(TextParagraph),
            new FrameworkPropertyMetadata(
                false, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                (DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
                {
                    ((TextParagraph)sender).OnIsSourceCodeChanged(e);
                })
        );

        public bool IsSourceCode
        {
            get { return (bool)GetValue(IsSourceCodeProperty); }
            set { SetValue(IsSourceCodeProperty, value); }
        }

        protected virtual void OnIsSourceCodeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UndoStack != null)
            {
                bool oldValue = (bool)e.OldValue;
                UndoStack.Push(() => IsSourceCode = oldValue);
            }

            RaiseIsSourceCodeChanged();
        }

        #endregion

        #endregion

        #region Properties

        public ElementTreeNode List
        {
            get
            {
                if (_List == null)
                {
                    _List = new ElementTreeNode(this, element => GetList(element));
                    _List.Children.CollectionChanged += UpdateListExpander;
                }
                return _List;
            }
        }

        private ElementTreeNode GetList(UIElement element)
        {
            var paragraph = element as TextParagraph;
            if (paragraph != null)
            {
                return paragraph.List;
            }
            else
            {
                return null;
            }
        }

        public ElementTreeNode Section
        {
            get
            {
                if (_Section == null)
                {
                    _Section = new ElementTreeNode(this, element => GetSection(element));
                }
                return _Section;
            }
        }

        private ElementTreeNode GetSection(UIElement element)
        {
            var paragraph = element as TextParagraph;
            if (paragraph != null)
            {
                return paragraph.Section;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Methods

        public static void UpdateSectionNumbering(UIElementCollection elements)
        {
            var ancestors = new List<TextParagraph>();
            UIElement previousSibling = null;

            foreach (UIElement element in elements)
            {
                if (DocumentPanel.GetPositioning(element) == Positioning.Absolute)
                {
                    continue;
                }

                var paragraph = element as TextParagraph;
                if (paragraph != null)
                {
                    paragraph.Section.Children.Clear();
                }

                if (previousSibling is TextParagraph && ((TextParagraph)previousSibling).IsHeading)
                {
                    if (paragraph == null || !paragraph.IsHeading || paragraph.HeadingLevel > ((TextParagraph)previousSibling).HeadingLevel)
                    {
                        ancestors.Add(((TextParagraph)previousSibling));
                        previousSibling = null;
                    }
                }

                if (paragraph != null && paragraph.IsHeading)
                {
                    while (ancestors.Count > 0 && paragraph.HeadingLevel <= ancestors.Last().HeadingLevel)
                    {
                        previousSibling = ancestors.Last();
                        ancestors.RemoveAt(ancestors.Count - 1);
                    }
                }

                // Update tree

                var parent = ancestors.LastOrDefault();
                if (paragraph != null)
                {
                    paragraph.Section.Parent = parent;
                }
                if (parent != null)
                {
                    parent.Section.Children.Add(element);
                }

                // Update visibility

                if (ancestors.Any(ancestor => ancestor.IsCollapsed))
                {
                    element.Visibility = Visibility.Collapsed;
                }
                else
                {
                    element.Visibility = Visibility.Visible;
                }

                previousSibling = element;
            }
        }

        public static void UpdateListNumbering(UIElementCollection elements)
        {
            var ancestors = new List<TextParagraph>();
            TextParagraph previousSibling = null;

            foreach (TextParagraph paragraph in elements.OfType<TextParagraph>())
            {
                if (paragraph.Visibility != Visibility.Visible)
                {
                    ancestors.Clear();
                    previousSibling = null;
                    continue;
                }

                paragraph.List.Children.Clear();

                if (!paragraph.IsListItem)
                {
                    ancestors.Clear();
                    previousSibling = null;
                    continue;
                }

                if (previousSibling != null && paragraph.LeftMargin > previousSibling.LeftMargin)
                {
                    ancestors.Add(previousSibling);
                    previousSibling = null;
                }

                while (ancestors.Count > 0 && paragraph.LeftMargin <= ancestors.Last().LeftMargin)
                {
                    previousSibling = ancestors.Last();
                    ancestors.RemoveAt(ancestors.Count - 1);
                }

                if (previousSibling != null && paragraph.LeftMargin < previousSibling.LeftMargin)
                {
                    previousSibling = null;
                }

                // Update tree

                var parent = ancestors.LastOrDefault();
                paragraph.List.Parent = parent;
                if (parent != null)
                {
                    parent.List.Children.Add(paragraph);
                }

                // Update numbering

                if (previousSibling != null)
                {
                    paragraph.ListNumber = previousSibling.ListNumber + 1;
                }
                else
                {
                    paragraph.ListNumber = 0;
                }

                // Update visibility

                if (ancestors.Any(ancestor => ancestor.IsCollapsed))
                {
                    paragraph.Visibility = Visibility.Collapsed;
                }
                else
                {
                    paragraph.Visibility = Visibility.Visible;
                }

                previousSibling = paragraph;
            }
        }

        public void RevealSectionItem()
        {
            if (RevealSectionItem(this))
            {
                UpdateSectionNumbering(this, EventArgs.Empty);
            }
        }

        static bool RevealSectionItem(TextParagraph paragraph)
        {
            bool result = false;

            var parent = (TextParagraph)paragraph.Section.Parent;
            if (parent != null)
            {
                if (parent.IsCollapsed)
                {
                    parent.IsCollapsed = false;
                    result = true;
                }

                result |= RevealSectionItem(parent);
            }

            return result;
        }

        public void RevealListItem()
        {
            if (RevealListItem(this))
            {
                UpdateListNumbering(this, EventArgs.Empty);
            }
        }

        static bool RevealListItem(TextParagraph listItem)
        {
            bool result = false;

            var parent = (TextParagraph)listItem.List.Parent;
            if (parent != null)
            {
                if (parent.IsCollapsed)
                {
                    parent.IsCollapsed = false;
                    result = true;
                }

                result |= RevealListItem(parent);
            }

            return result;
        }

        #endregion

        #region ITextElement

        public override bool DeleteBack()
        {
            if (base.DeleteBack())
            {
                return true;
            }

            // Backspace pressed at beginning of paragraph:
            // 1) If IsHeading=true, reset IsHeading to false
            // 2) If IsListItem=true, reset IsListItem to false
            // 3) If LeftMargin > 0, de-indent

            using (var undo = new UndoScope(UndoStack, "Delete back"))
            {
                if (IsHeading)
                {
                    SetTextProperty(0, Length, TextProperties.FontClassProperty, FontClass.Normal.ID);
                    return true;
                }

                if (IsListItem)
                {
                    IsListItem = false;
                    return true;
                }

                if (LeftMargin > 0)
                {
                    LeftMargin = Math.Max(LeftMargin - 30, 0);
                    return true;
                }
            }

            return false;
        }

        public override bool EnterLineBreak()
        {
            int offset = SelectionOffset;

            if (!base.EnterLineBreak())
            {
                return false;
            }

            // Don't inherit hyperlink URL on the new line

            SetTextProperty(offset, 1, TextProperties.HyperlinkURLProperty, null);

            return true;
        }

        public override ITextElement Split()
        {
            var result = (TextParagraph)base.Split();

            // Don't inherit hyperlink URL as a default text property

            if (result.Length == 0)
            {
                result.SetProperty(TextProperties.HyperlinkURLProperty, null);
            }

            // Automatically reset heading formatting

            if (result.IsHeading && !IsCollapsed)
            {
                if (Length == 0)
                {
                    this.SetTextProperty(0, Length, TextProperties.FontClassProperty, FontClass.Normal.ID);
                }
                else
                {
                    result.SetTextProperty(0, result.Length, TextProperties.FontClassProperty, FontClass.Normal.ID);
                }
            }

            if (Length != 0)
            {
                result.IsCollapsed = false;
            }

            // Automatically reset bulleting following empty lines

            if (Length == 0 && result.Length == 0 && result.IsListItem)
            {
                result.IsListItem = false;
                result.ListLevel = 0;
                result.LeftMargin = 0;
            }

            return result;
        }

        public override bool Merge(ITextElement other)
        {
            if (!base.Merge(other))
            {
                return false;
            }

            // Automatically apply heading formatting

            if (IsHeading)
            {
                var fontClass = Buffer.GetProperties(0).GetProperty(TextProperties.FontClassProperty);
                SetTextProperty(0, Length, TextProperties.FontClassProperty, fontClass);
            }

            return true;
        }

        #endregion

        #region IFormattable

        public const string IsHeadingPropertyName = "IsHeading";
        public const string HeadingLevelPropertyName = "HeadingLevel";
        public const string IsListItemPropertyName = "IsListItem";
        public const string ListLevelPropertyName = "ListLevel";
        public const string ListStylePropertyName = "ListStyle";
        public const string ListNumberPropertyName = "ListNumber";
        public const string LeftMarginPropertyName = "LeftMargin";
        public const string IsCollapsedPropertyName = "IsCollapse";

        public override bool HasProperty(string name)
        {
            switch (name)
            {
                case IsHeadingPropertyName:
                case HeadingLevelPropertyName:
                case IsListItemPropertyName:
                case ListLevelPropertyName:
                case ListStylePropertyName:
                case ListNumberPropertyName:
                case LeftMarginPropertyName:
                case IsCollapsedPropertyName:
                    return true;
                default:
                    return base.HasProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case IsHeadingPropertyName:
                    IsHeading = SafeConvert.ToBool(value, false).Value;
                    break;
                case HeadingLevelPropertyName:
                    HeadingLevel = SafeConvert.ToInt32(value);
                    break;
                case IsListItemPropertyName:
                    IsListItem = SafeConvert.ToBool(value, false).Value;
                    break;
                case ListLevelPropertyName:
                    ListLevel = SafeConvert.ToInt32(value);
                    break;
                case ListStylePropertyName:
                    ListStyle = value as IListStyle;
                    break;
                case ListNumberPropertyName:
                    ListNumber = SafeConvert.ToInt32(value);
                    break;
                case LeftMarginPropertyName:
                    LeftMargin = SafeConvert.ToDouble(value);
                    break;
                case IsCollapsedPropertyName:
                    IsCollapsed = SafeConvert.ToBool(value, false).Value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case IsHeadingPropertyName:
                    return IsHeading;
                case HeadingLevelPropertyName:
                    return HeadingLevel;
                case IsListItemPropertyName:
                    return IsListItem;
                case ListLevelPropertyName:
                    return ListLevel;
                case ListStylePropertyName:
                    return ListStyle;
                case ListNumberPropertyName:
                    return ListNumber;
                case LeftMarginPropertyName:
                    return LeftMargin;
                case IsCollapsedPropertyName:
                    return IsCollapsed;
                default:
                    return base.GetProperty(name);
            }
        }

        public override int ChangeProperty(string name, object oldValue, object newValue)
        {
            return base.ChangeProperty(name, oldValue, newValue);
        }

        public override void ResetProperties()
        {
            base.ResetProperties();
        }

        #endregion

        #region IEditable

        public override IList<object> Paste(IList<object> items)
        {
            if (IsSourceCode)
            {
                var buffer = new StringBuilder();
                foreach (var item in items)
                {
                    buffer.Append(item.ToString());
                }
                
                var result = base.Paste(new object[] { buffer.ToString() });

                Selection = SelectionEnd;

                return result;
            }
            else
            {
                return base.Paste(items);
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new TextParagraph(this);
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return Buffer.Text + "\n";
        }

        #endregion

        #region HTML

        public override string GetHTMLNodeName(NodeContext context)
        {
            if (IsHeading && HeadingLevel == 2)
            {
                return HTMLElements.H2;
            }
            else if (IsHeading && HeadingLevel == 3)
            {
                return HTMLElements.H3;
            }
            else if (IsListItem)
            {
                return HTMLElements.LI;
            }
            else if (IsSourceCode)
            {
                return HTMLElements.PRE;
            }
            else
            {
                return base.GetHTMLNodeName(context);
            }
        }

        static readonly string[] _NodeAttributes = { Attributes.CLASS };

        public override IList<string> GetHTMLAttributes(ElementContext context)
        {
            return _NodeAttributes.Concat(base.GetHTMLAttributes(context)).ToList();
        }

        public override string GetHTMLAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case Attributes.CLASS:
                    string classNames = context.GetInternalAttribute(name) ?? "";
                    if ((IsHeading || IsListItem) && IsCollapsed)
                        classNames = DOM.Helpers.DOMHelper.AppendClass(classNames, "collapsed");
                    return !String.IsNullOrWhiteSpace(classNames) ? classNames : null;
                default:
                    return base.GetHTMLAttribute(context, name);
            }
        }

        public override void SetHTMLAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case Attributes.CLASS:
                    IsCollapsed = DOM.Helpers.DOMHelper.HasClass(value, "collapsed");
                    value = DOM.Helpers.DOMHelper.RemoveClass(value, "collapsed");
                    context.SetInternalAttribute(name, value);
                    break;
                default:
                    base.SetHTMLAttribute(context, name, value);
                    break;
            }
        }

        public override void ResetHTMLAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case Attributes.CLASS:
                    IsCollapsed = false;
                    context.RemoveInternalAttribute(name);
                    break;
                default:
                    base.ResetHTMLAttribute(context, name);
                    break;
            }
        }

        private static readonly string[] _HTMLStyles = new string[] {
            CSSProperties.Display,
            CSSProperties.MarginLeft, 
            CSSProperties.ListStyleType, 
            "list-level"
        };

        public override IList<string> GetHTMLStyles(ElementContext context)
        {
            return _HTMLStyles.Concat(base.GetHTMLStyles(context)).ToList();
        }

        public override CSSValue GetHTMLStyle(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.Display:
                    return IsListItem ? CSSValues.ListItem : (GetNodeName(context) == HTMLElements.BR ? CSSValues.Inline : CSSValues.Block);
                case CSSProperties.MarginLeft:
                    return CSSValues.Pixels(LeftMargin);
                case CSSProperties.ListStyleType:
                    return (ListStyle != null) ? CSSValues.Ident(ListStyle.ToString()) : CSSValues.None;
                case "list-level":
                    return ListLevel != 0 ? CSSValues.Number(ListLevel) : null;
                default:
                    return base.GetHTMLStyle(context, name);
            }
        }

        public override void SetHTMLStyle(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.Display:
                    IsListItem = (value == CSSValues.ListItem);
                    break;
                case CSSProperties.MarginLeft:
                    LeftMargin = CSSConverter.ToLength(value, CSSPrimitiveType.CSS_PX, LeftMargin);
                    break;
                case CSSProperties.ListStyleType:
                    ListStyle = ListStyles.FromString(value.CssText, ListStyles.Circle);
                    break;
                case "list-level":
                    if (value == null)
                        ListLevel = 0;
                    else
                        ListLevel = (int)CSSConverter.ToFloat(value, CSSPrimitiveType.CSS_NUMBER, ListLevel);
                    break;
                default:
                    base.SetHTMLStyle(context, name, value);
                    break;

            }

        }

        #endregion

        #region Implementation

        #region Pending Properties

        protected override void SetPendingProperty(string name, object newValue)
        {
            // If applying a hyperlink URL and no text is selected, automatically
            // select the hyperlinked text under the caret or the entire word
            // if not hyperlinked.

            if (name == TextProperties.HyperlinkURLProperty)
            {
                int startIndex;
                int endIndex;

                var url = GetTextProperty(CaretIndex, TextProperties.HyperlinkURLProperty) as string;
                if (!String.IsNullOrEmpty(url))
                {
                    startIndex = GetHyperlinkStart(CaretIndex);
                    endIndex = GetHyperlinkEnd(CaretIndex);;
                }
                else
                {
                    int offset = Math.Max(Math.Min(CaretIndex, Buffer.Length - 1), 0);
                    startIndex = Buffer.GetWordStart(offset);
                    endIndex = Buffer.GetWordEnd(offset);
                }
                SetTextProperty(startIndex, endIndex - startIndex, name, newValue);
            }
            else
            {
                base.SetPendingProperty(name, newValue);
            }
        }

        int GetHyperlinkStart(int charOffset)
        {
            int startIndex;

            for (startIndex = charOffset; startIndex > 0; startIndex--)
            {
                string url = GetTextProperty(startIndex - 1, TextProperties.HyperlinkURLProperty) as string;

                if (String.IsNullOrEmpty(url))
                {
                    break;
                }
            }

            return startIndex;
        }

        int GetHyperlinkEnd(int charOffset)
        {
            // Find the end of the hyperlink

            int endIndex;

            for (endIndex = charOffset; endIndex < Length; endIndex++)
            {
                string url = GetTextProperty(endIndex, TextProperties.HyperlinkURLProperty) as string;

                if (String.IsNullOrEmpty(url))
                {
                    break;
                }
            }

            return endIndex;
        }

        #endregion

        #region Heading

        SectionToggleButton _SectionExpander;
        SectionHandle _SectionHandle;

        void UpdateIsHeading(object sender, TextFormatChangedEventArgs e)
        {
            if (e.PropertyName != TextProperties.FontClassProperty && e.PropertyName != null)
            {
                return;
            }

            var values = Buffer.GetProperty(TextProperties.FontClassProperty, 0, Buffer.Length);

            if (IsHeading)
            {
                IsHeading = values.Any(value => ParseHeadingType((string)value.Value) == HeadingLevel);
                HeadingLevel = IsHeading ? HeadingLevel : -1;
            }
            else
            {
                IsHeading = values.All(value => ParseHeadingType((string)value.Value) != -1);
                HeadingLevel = ParseHeadingType((string)values.First().Value);
                if (IsHeading) IsCollapsed = false;
            }
        }

        static int ParseHeadingType(string heading)
        {
            if (heading != null && heading.Length == 2 && heading[0] == 'h' && Char.IsNumber(heading[1]))
            {
                return int.Parse(heading.Substring(1, 1));
            }
            else
            {
                return -1;
            }
        }

        private void UpdateHeading(object sender, EventArgs e)
        {
            if (!IsHeading && _SectionExpander == null && _SectionHandle == null)
            {
                return;
            }

            if (_SectionExpander == null)
            {
                _SectionExpander = new SectionToggleButton();
                _SectionExpander.Checked += SectionExpander_Checked;
                _SectionExpander.Unchecked += SectionExpander_Unchecked;
                _SectionExpander.Dragging += SectionHandle_Dragging;
                AddVisual(_SectionExpander);
            }

            if (_SectionHandle == null)
            {
                _SectionHandle = new SectionHandle();
                _SectionHandle.Dragging += SectionHandle_Dragging;
                AddVisual(_SectionHandle);
            }

            _SectionExpander.HeadingLevel = HeadingLevel;
            _SectionExpander.IsChecked = !IsCollapsed;

            if (IsHeading)
            {
                Margin = new Thickness(0, 12, 0, 12);
                Padding = new Thickness(4);
                _SectionExpander.Visibility = Visibility.Visible;
                _SectionHandle.Visibility = Visibility.Visible;

                if (HeadingLevel == 2)
                {
                    Background = HeadingBackgroundBrush;
                    BorderBottomWidth = 2;
                    BorderBottomColor = Colors.Black;
                    BorderBottomStyle = DocumentPanel.BorderStyles.None;
                }
                else if (HeadingLevel == 3)
                {
                    Background = null;
                    BorderBottomWidth = 1;
                    BorderBottomColor = Colors.LightGray;
                    BorderBottomStyle = DocumentPanel.BorderStyles.Solid;
                }
                else
                {
                    Background = null;
                    BorderBottomWidth = 2;
                    BorderBottomColor = Colors.Black;
                    BorderBottomStyle = DocumentPanel.BorderStyles.None;
                }
            }
            else
            {
                Margin = new Thickness(0);
                Padding = new Thickness(0);
                Background = null;
                BorderBottomWidth = 2;
                BorderBottomColor = Colors.Black;
                BorderBottomStyle = DocumentPanel.BorderStyles.None;
                _SectionExpander.Visibility = Visibility.Hidden;
                _SectionHandle.Visibility = Visibility.Hidden;
            }

            if (IsMeasureValid)
            {
                MeasureSectionExpander(DesiredSize);
                MeasureSectionHandle(DesiredSize);
            }

            if (IsArrangeValid)
            {
                ArrangeSectionExpander(RenderSize);
                ArrangeSectionHandle(RenderSize);
            }
        }

        void SectionExpander_Checked(object sender, RoutedEventArgs e)
        {
            IsCollapsed = false;
        }

        void SectionExpander_Unchecked(object sender, RoutedEventArgs e)
        {
            IsCollapsed = true;
        }

        void SectionHandle_Dragging(object sender, EventArgs e)
        {
            DoDragSection();
        }


        static Brush _HeadingBackgroundBrush;

        protected static Brush HeadingBackgroundBrush
        {
            get
            {
                if (_HeadingBackgroundBrush == null)
                {
                    Color color = Color.FromRgb(0xEC, 0xF5, 0xFF);
                    _HeadingBackgroundBrush = new SolidColorBrush(color);
                    _HeadingBackgroundBrush.Freeze();
                }
                return _HeadingBackgroundBrush;
            }
        }

        private void MeasureSectionExpander(Size availableSize)
        {
            if (_SectionExpander != null)
            {
                Size toggleButtonSize = new Size();
                toggleButtonSize.Width = ParagraphIndent - Indent;
                toggleButtonSize.Height = Renderer.GetCaretBounds(0).Height;
                _SectionExpander.Measure(toggleButtonSize);
            }
        }

        private void ArrangeSectionExpander(Size finalSize)
        {
            if (_SectionExpander != null)
            {
                var expanderRect = Renderer.GetCaretBounds(0);
                if (!expanderRect.IsEmpty)  // sanity check
                {
                    expanderRect.X -= Indent + _SectionExpander.DesiredSize.Width;
                    expanderRect.Y += Padding.Top + (expanderRect.Height - _SectionExpander.DesiredSize.Height) / 2;
                    expanderRect.Size = _SectionExpander.DesiredSize;
                    _SectionExpander.Arrange(expanderRect);
                }
            }
        }

        private void MeasureSectionHandle(Size availableSize)
        {
            if (_SectionHandle != null)
            {
                _SectionHandle.Measure(availableSize);
            }
        }

        private void ArrangeSectionHandle(Size finalSize)
        {
            if (_SectionHandle != null)
            {
                Rect handleRect = new Rect();
                handleRect.X = finalSize.Width - _SectionHandle.DesiredSize.Width - Padding.Right - 10;
                handleRect.Y = Padding.Top + (finalSize.Height - Padding.Top - Padding.Bottom - _SectionHandle.DesiredSize.Height) / 2;
                handleRect.Size = _SectionHandle.DesiredSize;
                _SectionHandle.Arrange(handleRect);
            }
        }

        public void DoDragSection()
        {
            var section = Section.SelfAndDescendants.ToArray();

            section.ForEach(p => p.Opacity = 0.5);
            try
            {
                IDataObject data = NDataObject.CreateDataObject(section);

                DragDropEffects result = DragDrop.DoDragDrop(this, data, DragDropEffects.Move | DragDropEffects.Scroll);
                if (result.HasFlag(DragDropEffects.Move))
                {
                    EditingPanel.DeleteCommand.Execute(section, this);
                }
            }
            finally
            {
                section.ForEach(p => p.Opacity = 1.0);
            }
        }

        #endregion

        #region Bullet

        private Bullet _Bullet;

        private void UpdateBullet(object sender, EventArgs e)
        {
            if (_Bullet == null && !IsListItem)
            {
                return;
            }

            if (_Bullet == null)
            {
                _Bullet = new Bullet(this);
                AddVisual(_Bullet);
            }

            if (IsListItem)
            {
                _Bullet.Visibility = Visibility.Visible;
            }
            else
            {
                _Bullet.Visibility = Visibility.Hidden;
            }

            if (ListStyle != null)
            {
                _Bullet.Text = ListStyle.GetMarker(ListNumber);
            }
            else
            {
                _Bullet.Text = String.Empty;
            }

            if (IsMeasureValid)
            {
                MeasureBullet(DesiredSize);
            }

            if (IsArrangeValid)
            {
                ArrangeBullet(RenderSize);
            }
        }

        private void UpdateIndentation(object sender, EventArgs e)
        {
            if (IsListItem || IsHeading)
            {
                Indent = 5;
                ParagraphIndent = LeftMargin + 20;
            }
            else
            {
                Indent = 0;
                ParagraphIndent = LeftMargin;
            }
        }

        private void MeasureBullet(Size availableSize)
        {
            if (_Bullet != null)
            {
                Rect bulletRect = Renderer.GetCaretBounds(0);
                if (!bulletRect.IsEmpty)    // sanity check
                {
                    bulletRect.Y = 0;
                    bulletRect.Width = ParagraphIndent - Indent;
                    _Bullet.Measure(bulletRect.Size);
                }
            }
        }

        private void ArrangeBullet(Size finalSize)
        {
            if (_Bullet != null)
            {
                Rect bulletRect = Renderer.GetCaretBounds(0);
                if (!bulletRect.IsEmpty)    // sanity check
                {
                    bulletRect.X -= Indent + _Bullet.DesiredSize.Width;
                    bulletRect.Y = (bulletRect.Height - _Bullet.DesiredSize.Height) / 2;
                    bulletRect.Size = _Bullet.DesiredSize;
                    _Bullet.Arrange(bulletRect);
                }
            }
        }

        #endregion

        #region List Expander

        ListToggleButton _ListExpander;

        private void UpdateListExpander(object sender, EventArgs e)
        {
            if (List.Children.Count == 0 && _ListExpander == null)
            {
                return;
            }

            if (_ListExpander == null)
            {
                _ListExpander = new ListToggleButton();
                _ListExpander.IsChecked = !IsCollapsed;
                _ListExpander.Click += ListExpander_Click;
                AddVisual(_ListExpander, 1);
            }

            if (List.Children.Count > 0 && (IsMouseOver || IsCollapsed))
            {
                _ListExpander.Visibility = Visibility.Visible;
            }
            else
            {
                _ListExpander.Visibility = Visibility.Hidden;
            }

            if (IsMeasureValid)
            {
                MeasureListExpander(DesiredSize);
            }

            if (IsArrangeValid)
            {
                ArrangeListExpander(RenderSize);
            }
        }

        private void ListExpander_Click(object sender, RoutedEventArgs e)
        {
            IsCollapsed = (_ListExpander.IsChecked == false);
        }

        private void MeasureListExpander(Size availableSize)
        {
            if (_ListExpander != null)
            {
                _ListExpander.Measure(availableSize);
            }
        }

        private void ArrangeListExpander(Size finalSize)
        {
            if (_ListExpander != null)
            {
                Rect rect = Renderer.GetCaretBounds(0);
                if (!rect.IsEmpty)
                {
                    rect.X -= 27;
                    rect.Y = (rect.Height - _ListExpander.DesiredSize.Height) / 2;
                    rect.Size = _ListExpander.DesiredSize;
                    _ListExpander.Arrange(rect);
                }
            }
        }

        private void UpdateListNumbering(object sender, EventArgs e)
        {
            DocumentPanel panel = DocumentPanel.FindPanel(this);
            if (panel != null)
            {
                UpdateListNumbering(panel.Children);
            }
        }

        private void UpdateSectionNumbering(object sender, EventArgs e)
        {
            DocumentPanel panel = DocumentPanel.FindPanel(this);
            if (panel != null)
            {
                UpdateSectionNumbering(panel.Children);
            }
        }

        #endregion

        #region Source Code

        void UpdateIsSourceCode(object sender, TextFormatChangedEventArgs e)
        {
            if (e.PropertyName != TextProperties.FontClassProperty && e.PropertyName != null)
            {
                return;
            }

            var values = Buffer.GetProperty(TextProperties.FontClassProperty, 0, Buffer.Length);

            if (IsSourceCode)
            {
                IsSourceCode = values.Any(value => (string)value.Value == FontClass.SourceCode.ID);
            }
            else
            {
                IsSourceCode = values.All(value => (string)value.Value == FontClass.SourceCode.ID);
            }
        }

        void UpdateSourceCode(object sender, EventArgs e)
        {
            TextChanged -= SourceCode_TextChanged;

            if (IsSourceCode)
            {
                Margin = new Thickness(20, 5, 20, 5);
                Padding = new Thickness(10);
                BorderWidth = 2.0;
                BorderStyle = DocumentPanel.BorderStyles.Solid;
                BorderColor = Colors.LightGray;
                AcceptsReturn = true;
                AutoIndent = true;

                TextChanged += SourceCode_TextChanged;
            }
            else
            {
                Margin = new Thickness(0);
                Padding = new Thickness(0);
                BorderWidth = DefaultBorderLeftWidth;
                BorderStyle = DefaultBorderLeftStyle;
                BorderColor = DefaultBorderLeftColor;
                AcceptsReturn = false;
                AutoIndent = false;
            }

            Renderer.Invalidate();
        }

        void SourceCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            int delta = e.NumAdded - e.NumRemoved;
            if (delta > 0)
            {
                string addedText = Buffer.Substring(e.Offset, delta);
                for (int i = addedText.Length - 1; i >= 0; i--)
                {
                }
            }
        }

        #endregion

        #region Hyperlink

        private bool _IsMouseOverHyperlink;

        public bool IsMouseOverHyperlink
        {
            get
            {
                return _IsMouseOverHyperlink;
            }
            set
            {
                _IsMouseOverHyperlink = value;
                OnMouseOverHyperlinkChanged();
            }
        }

        private bool _IsMouseOverSelection;

        public bool IsMouseOverSelection
        {
            get
            {
                return _IsMouseOverSelection;
            }
            set
            {
                _IsMouseOverSelection = value;
                OnMouseOverHyperlinkChanged();
            }
        }

        private void UpdateMouseOverHyperlink(object sender, MouseEventArgs e)
        {
            if (!IsMouseOver)
            {
                IsMouseOverHyperlink = false;
                IsMouseOverSelection = false;
                return;
            }

            // Get the character under the cursor

            Point point = TransformToDescendant(Renderer).Transform(e.GetPosition(this));
            int index = Renderer.CharFromPoint(point);
            if (index < 0 || index >= Text.Length)
            {
                IsMouseOverHyperlink = false;
                IsMouseOverSelection = false;
                return;
            }

            // Get the hyperlink (if any) at the given character offset

            string url = (string)GetTextProperty(index, TextProperties.HyperlinkURLProperty);
            IsMouseOverHyperlink = !String.IsNullOrEmpty(url);
            IsMouseOverSelection = index >= SelectionOffset && index < SelectionOffset + SelectionLength;
        }

        private void OnMouseOverHyperlinkChanged()
        {
            if (IsMouseOverHyperlink || IsMouseOverSelection)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    Cursor = Cursors.Hand;
                }

                if (_HyperlinkTimer == null)
                {
                    _HyperlinkTimer = new DispatcherTimer();
                    _HyperlinkTimer.Interval = new TimeSpan(500);
                    _HyperlinkTimer.Tick += HyperlinkTimer_Tick;
                }

                _HyperlinkTimer.Start();
            }
            else
            {
                Cursor = Cursors.IBeam;

                if (_HyperlinkTimer != null)
                {
                    _HyperlinkTimer.Stop();
                }
            }
        }

        protected void HyperlinkTimer_Tick(object sender, EventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.IBeam;
            }
        }

        #endregion

        #region ToolTip

        private void UpdateToolTip(object sender, MouseEventArgs e)
        {
            if (!IsMouseOver)
            {
                _ToolTip.IsOpen = false;
                return;
            }

            // Get the character under the cursor

            Point point = TransformToDescendant(Renderer).Transform(e.GetPosition(this));

            int charOffset = Renderer.CharFromPoint(point);
            if (charOffset < 0 || charOffset >= Text.Length)
            {
                _ToolTip.IsOpen = false;
                return;
            }

            // Get the tooltip (if any) for the text under the cursor

            object toolTip = GetTextProperty(charOffset, TextProperties.ToolTipProperty);
            if (toolTip == null)
            {
                // Automatically create one for hyperlinked text
                string hyperlinkURL = GetTextProperty(charOffset, TextProperties.HyperlinkURLProperty) as string;
                if (!String.IsNullOrEmpty(hyperlinkURL))
                {
                    toolTip = CreateHyperlinkToolTip(hyperlinkURL);
                }
            }

            // Update tooltip

            if (toolTip != null &&
                !e.LeftButton.HasFlag(MouseButtonState.Pressed) &&
                !e.RightButton.HasFlag(MouseButtonState.Pressed))
            {
                ToolTip = _ToolTip;
                _ToolTip.Content = toolTip;
                _ToolTip.IsOpen = true;
            }
            else
            {
                _ToolTip.IsOpen = false;
            }
        }

        private object CreateHyperlinkToolTip(string hyperlinkURL)
        {
            Uri uri;
            if (Uri.TryCreate(hyperlinkURL, UriKind.RelativeOrAbsolute, out uri))
            {
                if (!uri.IsAbsoluteUri)
                {
                    string title = UriToNoteTitle(hyperlinkURL);
                    if (!String.IsNullOrWhiteSpace(title))
                    {
                        hyperlinkURL = "Title: " + title;
                    }
                    else
                    {
                        hyperlinkURL = "Link to Note";
                    }
                }
                else if (uri.IsAbsoluteUri && uri.IsFile)
                {
                    hyperlinkURL = uri.LocalPath;
                    hyperlinkURL = hyperlinkURL.Replace("\\\\relative\\", "");
                }
                else
                {
                    hyperlinkURL = hyperlinkURL.Replace("http://", "");
                    hyperlinkURL = hyperlinkURL.Replace("https://", "");
                    hyperlinkURL = hyperlinkURL.TrimEnd('/');
                }
            }

            var tipURL = new TextBlock();
            tipURL.Text = hyperlinkURL;
            tipURL.Foreground = Brushes.Blue;
            tipURL.TextDecorations = System.Windows.TextDecorations.Underline;

            var tipMessage = new TextBlock();
            tipMessage.Margin = new Thickness(0, 10, 0, 0);
            tipMessage.Text = "Ctrl+Click to open link";

            var tipPanel = new StackPanel();
            tipPanel.Orientation = Orientation.Vertical;
            tipPanel.Children.Add(tipURL);
            tipPanel.Children.Add(tipMessage);

            return tipPanel;
        }

        public static string UriToNoteTitle(string uri)
        {
            string[] segments = UriHelper.PathSegments(uri);
            if (segments.FirstOrDefault() != "notes")
            {
                return String.Empty;
            }

            string pattern = @"title:([^&#]*)";
            var match = Regex.Match(uri, pattern);
            if (match != null)
            {
                string title = match.Groups[1].Value;
                title = title.Trim('\"');
                return HttpUtility.UrlDecode(title);
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion

        #region Mouse Input

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Process a Ctrl-click

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                Point point = e.GetPosition(this);

                // Get cursor position relative to the text area

                point = TransformToDescendant(Renderer).Transform(point);

                int charOffset = Renderer.CharFromPoint(point);

                if (charOffset >= 0 && charOffset < Text.Length)
                {
                    // Is the mouse over a hyperlink?

                    string hyperlinkURL = (string)GetTextProperty(charOffset, TextProperties.HyperlinkURLProperty);

                    if (!String.IsNullOrEmpty(hyperlinkURL))
                    {
                        // Open the hyperlink

                        string target = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? "_self" : "_blank";
                        RaiseHyperlinkClicked(hyperlinkURL, target);
                        e.Handled = true;
                        return;
                    }

                    // Is the mouse over selected text?

                    bool isSelected = charOffset >= SelectionOffset && charOffset < SelectionOffset + SelectionLength;

                    if (isSelected)
                    {
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                            NUtilityCommands.DrillDown.Execute(null, this);
                        else
                            NUtilityCommands.DrillThrough.Execute(null, this);
                        e.Handled = true;
                        return;
                    }
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        #endregion

        #region Keyboard Input

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    OnCtrlKeyDown(e);
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.Key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    OnCtrlKeyUp(e);
                    break;
            }
        }

        protected void OnCtrlKeyDown(KeyEventArgs e)
        {
            Point position = Mouse.GetPosition(this);

            string hyperlinkURL = (string)GetTextProperty(position, TextProperties.HyperlinkURLProperty);

            if (!String.IsNullOrEmpty(hyperlinkURL))
            {
                Cursor = Cursors.Hand;
                ForceCursor = true;
                ForceCursor = false;
            }
        }

        protected void OnCtrlKeyUp(KeyEventArgs e)
        {
            Cursor = Cursors.IBeam;
        }

        /// <summary>
        /// Get a property of the text at the given point
        /// </summary>
        private object GetTextProperty(Point point, string name)
        {
            point = TransformToDescendant(Renderer).Transform(point);

            int charOffset = Renderer.CharFromPoint(point);

            if (charOffset >= 0 && charOffset < Text.Length)
            {
                return GetTextProperty(charOffset, name);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = base.MeasureOverride(availableSize);

            if (IsHeading)
            {
                MeasureSectionExpander(desiredSize);
                MeasureSectionHandle(desiredSize);
            }

            if (IsListItem)
            {
                MeasureBullet(desiredSize);
                MeasureListExpander(desiredSize);
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            finalSize = base.ArrangeOverride(finalSize);

            if (IsHeading)
            {
                ArrangeSectionExpander(finalSize);
                ArrangeSectionHandle(finalSize);
            }

            if (IsListItem)
            {
                ArrangeBullet(finalSize);
                ArrangeListExpander(finalSize);
            }

            return finalSize;
        }

        #endregion

        #endregion

    }

    public delegate void HyperlinkClickedEventHandler(object sender, HyperlinkClickedEventArgs e);

    public class HyperlinkClickedEventArgs : RoutedEventArgs
    {
        public HyperlinkClickedEventArgs(string uri)
        {
            Uri = uri;
        }

        public HyperlinkClickedEventArgs(RoutedEvent routedEvent, string uri)
            : base(routedEvent)
        {
            Uri = uri;
        }

        public HyperlinkClickedEventArgs(RoutedEvent routedEvent, object source, string uri)
            : base(routedEvent, source)
        {
            Uri = uri;
        }

        public HyperlinkClickedEventArgs(RoutedEvent routedEvent, object source, string uri, string target)
            : base(routedEvent, source)
        {
            Uri = uri;
            Target = target;
        }

        public string Uri { get; set; }
        public string Target { get; set; }
    }
}
