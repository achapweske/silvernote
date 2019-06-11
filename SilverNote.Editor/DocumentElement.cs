/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Xml;
using System.Diagnostics;
using DOM;
using DOM.HTML;
using DOM.CSS;

namespace SilverNote.Editor
{
    public class DocumentElement : FrameworkElement, ISelectable, IEditable, IMovable, IResizable, IHasResources, INodeSource, IStyleable
    {
        #region Constructors

        public DocumentElement()
        {
            Initialize();
        }

        public DocumentElement(DocumentElement copy)
        {
            Initialize();

            Margin = copy.Margin;

            // Positioning
            Positioning = copy.Positioning;
            Left = copy.Left;
            Top = copy.Top;
            Right = copy.Right;
            Bottom = copy.Bottom;
            Width = copy.Width;
            Height = copy.Height;
            HorizontalAlignment = copy.HorizontalAlignment;
            Background = copy.Background;
            // BorderLeft
            BorderLeftStyle = copy.BorderLeftStyle;
            BorderLeftColor = copy.BorderLeftColor;
            BorderLeftWidth = copy.BorderLeftWidth;
            // BorderTop
            BorderTopStyle = copy.BorderTopStyle;
            BorderTopColor = copy.BorderTopColor;
            BorderTopWidth = copy.BorderTopWidth;
            // BorderRight
            BorderRightStyle = copy.BorderRightStyle;
            BorderRightColor = copy.BorderRightColor;
            BorderRightWidth = copy.BorderRightWidth;
            // BorderBottom
            BorderBottomStyle = copy.BorderBottomStyle;
            BorderBottomColor = copy.BorderBottomColor;
            BorderBottomWidth = copy.BorderBottomWidth;
            // BorderRadius
            BorderTopLeftRadius = copy.BorderTopLeftRadius;
            BorderTopRightRadius = copy.BorderTopRightRadius;
            BorderBottomRightRadius = copy.BorderBottomRightRadius;
            BorderBottomLeftRadius = copy.BorderBottomLeftRadius;

            if (ReadLocalValue(EditingPanel.UndoStackProperty) != DependencyProperty.UnsetValue)
            {
                UndoStack = copy.UndoStack;
            }

            IsDeletable = copy.IsDeletable;
        }

        void Initialize()
        {
           
        }

        #endregion

        #region Properties

        #region OwnerDocument

        public static readonly DependencyProperty OwnerDocumentProperty = DocumentPanel.OwnerDocumentProperty.AddOwner(typeof(DocumentElement));

        public HTMLDocument OwnerDocument
        {
            get { return (HTMLDocument)GetValue(OwnerDocumentProperty); }
            set { SetValue(OwnerDocumentProperty, value); }
        }

        #endregion

        #region UndoStack

        public static readonly DependencyProperty UndoStackProperty = EditingPanel.UndoStackProperty.AddOwner(typeof(DocumentElement));

        public virtual UndoStack UndoStack
        {
            get { return (UndoStack)GetValue(UndoStackProperty); }
            set { SetValue(UndoStackProperty, value); }
        }

        #endregion

        #region Positioning

        public static readonly DependencyProperty PositioningProperty = DocumentPanel.PositioningProperty.AddOwner(typeof(DocumentElement));

        public Positioning Positioning
        {
            get { return (Positioning)GetValue(PositioningProperty); }
            set { SetValue(PositioningProperty, value); }
        }

        public Positioning DefaultPositioning
        {
            get { return (Positioning)PositioningProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Position

        public Point Position
        {
            get { return DocumentPanel.GetPosition(this); }
            set { DocumentPanel.SetPosition(this, value); }
        }

        #region Left

        public double Left
        {
            get { return DocumentPanel.GetLeft(this); }
            set { DocumentPanel.SetLeft(this, value); }
        }

        public double DefaultLeft
        {
            get { return (double)DocumentPanel.LeftProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Top

        public double Top
        {
            get { return DocumentPanel.GetTop(this); }
            set { DocumentPanel.SetTop(this, value); }
        }

        public double DefaultTop
        {
            get { return (double)DocumentPanel.TopProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Right

        public double Right
        {
            get { return DocumentPanel.GetRight(this); }
            set { DocumentPanel.SetRight(this, value); }
        }

        public double DefaultRight
        {
            get { return (double)DocumentPanel.RightProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Bottom

        public double Bottom
        {
            get { return DocumentPanel.GetBottom(this); }
            set { DocumentPanel.SetBottom(this, value); }
        }

        public double DefaultBottom
        {
            get { return (double)DocumentPanel.BottomProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #region Float

        private static readonly DependencyProperty FloatProperty = DocumentPanel.FloatProperty.AddOwner(typeof(DocumentElement));

        public DocumentPanel.FloatDirection Float
        {
            get { return (DocumentPanel.FloatDirection)GetValue(FloatProperty); }
            set { SetValue(FloatProperty, value); }
        }

        public DocumentPanel.FloatDirection DefaultFloat
        {
            get { return (DocumentPanel.FloatDirection)FloatProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Clear

        private static readonly DependencyProperty ClearProperty = DocumentPanel.ClearProperty.AddOwner(typeof(DocumentElement));

        public DocumentPanel.ClearDirection Clear
        {
            get { return (DocumentPanel.ClearDirection)GetValue(ClearProperty); }
            set { SetValue(ClearProperty, value); }
        }

        public DocumentPanel.ClearDirection DefaultClear
        {
            get { return (DocumentPanel.ClearDirection)ClearProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region IsDeletable

        public bool IsDeletable
        {
            get { return EditingPanel.GetIsDeletable(this); }
            set { EditingPanel.SetIsDeletable(this, value); }
        }

        #endregion

        #region Background

        public static readonly DependencyProperty BackgroundProperty = Control.BackgroundProperty.AddOwner ( 
            typeof(DocumentElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush DefaultBackground
        {
            get { return (Brush)BackgroundProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Border

        #region BorderStyle

        public DocumentPanel.BorderStyles BorderStyle
        {
            get
            {
                DocumentPanel.BorderStyles style = BorderLeftStyle;

                if (BorderTopStyle == style &&
                    BorderRightStyle == style &&
                    BorderBottomStyle == style)
                {
                    return style;
                }
                else
                {
                    return DefaultBorderLeftStyle;
                }
            }
            set
            {
                BorderLeftStyle = value;
                BorderTopStyle = value;
                BorderRightStyle = value;
                BorderBottomStyle = value;
            }
        }

        #region BorderLeftStyle

        public static readonly DependencyProperty BorderLeftStyleProperty =
            DocumentPanel.BorderLeftStyleProperty.AddOwner(typeof(DocumentElement));

        public DocumentPanel.BorderStyles BorderLeftStyle
        {
            get { return (DocumentPanel.BorderStyles)GetValue(BorderLeftStyleProperty); }
            set { SetValue(BorderLeftStyleProperty, value); }
        }

        public DocumentPanel.BorderStyles DefaultBorderLeftStyle
        {
            get { return (DocumentPanel.BorderStyles)BorderLeftStyleProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderTopStyle

        public static readonly DependencyProperty BorderTopStyleProperty =
            DocumentPanel.BorderTopStyleProperty.AddOwner(typeof(DocumentElement));

        public DocumentPanel.BorderStyles BorderTopStyle
        {
            get { return (DocumentPanel.BorderStyles)GetValue(BorderTopStyleProperty); }
            set { SetValue(BorderTopStyleProperty, value); }
        }

        public DocumentPanel.BorderStyles DefaultBorderTopStyle
        {
            get { return (DocumentPanel.BorderStyles)BorderTopStyleProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderRightStyle

        public static readonly DependencyProperty BorderRightStyleProperty =
            DocumentPanel.BorderRightStyleProperty.AddOwner(typeof(DocumentElement));

        public DocumentPanel.BorderStyles BorderRightStyle
        {
            get { return (DocumentPanel.BorderStyles)GetValue(BorderRightStyleProperty); }
            set { SetValue(BorderRightStyleProperty, value); }
        }

        public DocumentPanel.BorderStyles DefaultBorderRightStyle
        {
            get { return (DocumentPanel.BorderStyles)BorderRightStyleProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomStyle

        public static readonly DependencyProperty BorderBottomStyleProperty =
            DocumentPanel.BorderBottomStyleProperty.AddOwner(typeof(DocumentElement));

        public DocumentPanel.BorderStyles BorderBottomStyle
        {
            get { return (DocumentPanel.BorderStyles)GetValue(BorderBottomStyleProperty); }
            set { SetValue(BorderBottomStyleProperty, value); }
        }

        public DocumentPanel.BorderStyles DefaultBorderBottomStyle
        {
            get { return (DocumentPanel.BorderStyles)BorderBottomStyleProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #region BorderColor

        public Color BorderColor
        {
            get
            {
                Color color = BorderLeftColor;

                if (Object.Equals(BorderTopColor, color) &&
                    Object.Equals(BorderRightColor, color) &&
                    Object.Equals(BorderBottomColor, color))
                {
                    return color;
                }
                else
                {
                    return DefaultBorderLeftColor;
                }
            }
            set
            {
                BorderLeftColor = value;
                BorderTopColor = value;
                BorderRightColor = value;
                BorderBottomColor = value;
            }
        }

        #region BorderLeftColor

        public static readonly DependencyProperty BorderLeftColorProperty =
            DocumentPanel.BorderLeftColorProperty.AddOwner(typeof(DocumentElement));

        public Color BorderLeftColor
        {
            get { return (Color)GetValue(BorderLeftColorProperty); }
            set { SetValue(BorderLeftColorProperty, value); }
        }

        public Color DefaultBorderLeftColor
        {
            get { return (Color)BorderLeftColorProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderTopColor

        public static readonly DependencyProperty BorderTopColorProperty =
            DocumentPanel.BorderTopColorProperty.AddOwner(typeof(DocumentElement));

        public Color BorderTopColor
        {
            get { return (Color)GetValue(BorderTopColorProperty); }
            set { SetValue(BorderTopColorProperty, value); }
        }

        public Color DefaultBorderTopColor
        {
            get { return (Color)BorderTopColorProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderRightColor

        public static readonly DependencyProperty BorderRightColorProperty =
            DocumentPanel.BorderRightColorProperty.AddOwner(typeof(DocumentElement));

        public Color BorderRightColor
        {
            get { return (Color)GetValue(BorderRightColorProperty); }
            set { SetValue(BorderRightColorProperty, value); }
        }

        public Color DefaultBorderRightColor
        {
            get { return (Color)BorderRightColorProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomColor

        public static readonly DependencyProperty BorderBottomColorProperty =
            DocumentPanel.BorderBottomColorProperty.AddOwner(typeof(DocumentElement));

        public Color BorderBottomColor
        {
            get { return (Color)GetValue(BorderBottomColorProperty); }
            set { SetValue(BorderBottomColorProperty, value); }
        }

        public Color DefaultBorderBottomColor
        {
            get { return (Color)BorderBottomColorProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #region BorderWidth

        public double BorderWidth
        {
            get
            {
                double width = BorderLeftWidth;

                if (Object.Equals(BorderTopWidth, width) &&
                    Object.Equals(BorderRightWidth, width) &&
                    Object.Equals(BorderBottomWidth, width))
                {
                    return width;
                }
                else
                {
                    return DefaultBorderLeftWidth;
                }
            }
            set
            {
                BorderLeftWidth = value;
                BorderTopWidth = value;
                BorderRightWidth = value;
                BorderBottomWidth = value;
            }
        }

        #region BorderLeftWidth

        public static readonly DependencyProperty BorderLeftWidthProperty =
            DocumentPanel.BorderLeftWidthProperty.AddOwner(typeof(DocumentElement));

        public double BorderLeftWidth
        {
            get { return (double)GetValue(BorderLeftWidthProperty); }
            set { SetValue(BorderLeftWidthProperty, value); }
        }

        public double DefaultBorderLeftWidth
        {
            get { return (double)BorderLeftWidthProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderTopWidth

        public static readonly DependencyProperty BorderTopWidthProperty =
            DocumentPanel.BorderTopWidthProperty.AddOwner(typeof(DocumentElement));

        public double BorderTopWidth
        {
            get { return (double)GetValue(BorderTopWidthProperty); }
            set { SetValue(BorderTopWidthProperty, value); }
        }

        public double DefaultBorderTopWidth
        {
            get { return (double)BorderTopWidthProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderRightWidth

        public static readonly DependencyProperty BorderRightWidthProperty =
            DocumentPanel.BorderRightWidthProperty.AddOwner(typeof(DocumentElement));

        public double BorderRightWidth
        {
            get { return (double)GetValue(BorderRightWidthProperty); }
            set { SetValue(BorderRightWidthProperty, value); }
        }

        public double DefaultBorderRightWidth
        {
            get { return (double)BorderRightWidthProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomWidth

        public static readonly DependencyProperty BorderBottomWidthProperty =
            DocumentPanel.BorderBottomWidthProperty.AddOwner(typeof(DocumentElement));

        public double BorderBottomWidth
        {
            get { return (double)GetValue(BorderBottomWidthProperty); }
            set { SetValue(BorderBottomWidthProperty, value); }
        }

        public double DefaultBorderBottomWidth
        {
            get { return (double)BorderBottomWidthProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #region BorderRadius

        public double BorderRadius
        {
            set
            {
                BorderTopLeftRadius = value;
                BorderTopRightRadius = value;
                BorderBottomRightRadius = value;
                BorderBottomLeftRadius = value;
            }
        }

        #region BorderTopLeftRadius

        public static readonly DependencyProperty BorderTopLeftRadiusProperty =
            DocumentPanel.BorderTopLeftRadiusProperty.AddOwner(typeof(DocumentElement));

        public double BorderTopLeftRadius
        {
            get { return (double)GetValue(BorderTopLeftRadiusProperty); }
            set { SetValue(BorderTopLeftRadiusProperty, value); }
        }

        public double DefaultBorderTopLeftRadius
        {
            get { return (double)BorderTopLeftRadiusProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderTopRightRadius

        public static readonly DependencyProperty BorderTopRightRadiusProperty =
            DocumentPanel.BorderTopRightRadiusProperty.AddOwner(typeof(DocumentElement));

        public double BorderTopRightRadius
        {
            get { return (double)GetValue(BorderTopRightRadiusProperty); }
            set { SetValue(BorderTopRightRadiusProperty, value); }
        }

        public double DefaultBorderTopRightRadius
        {
            get { return (double)BorderTopRightRadiusProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomRightRadius

        public static readonly DependencyProperty BorderBottomRightRadiusProperty =
            DocumentPanel.BorderBottomRightRadiusProperty.AddOwner(typeof(DocumentElement));

        public double BorderBottomRightRadius
        {
            get { return (double)GetValue(BorderBottomRightRadiusProperty); }
            set { SetValue(BorderBottomRightRadiusProperty, value); }
        }

        public double DefaultBorderBottomRightRadius
        {
            get { return (double)BorderBottomRightRadiusProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomLeftRadius

        public static readonly DependencyProperty BorderBottomLeftRadiusProperty =
            DocumentPanel.BorderBottomLeftRadiusProperty.AddOwner(typeof(DocumentElement));

        public double BorderBottomLeftRadius
        {
            get { return (double)GetValue(BorderBottomLeftRadiusProperty); }
            set { SetValue(BorderBottomLeftRadiusProperty, value); }
        }

        public double DefaultBorderBottomLeftRadius
        {
            get { return (double)BorderBottomLeftRadiusProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #endregion

        #region Margin

        public Thickness DefaultMargin
        {
            get { return (Thickness)MarginProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region DebugFlags

        public static readonly DependencyProperty DebugFlagsProperty = DependencyProperty.RegisterAttached(
            "DebugFlags",
            typeof(NDebugFlags),
            typeof(DocumentElement),
            new FrameworkPropertyMetadata(NDebugFlags.None, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static void SetDebugFlags(DependencyObject target, NDebugFlags value)
        {
            target.SetValue(DebugFlagsProperty, value);
        }

        public static NDebugFlags GetDebugFlags(DependencyObject target)
        {
            return (NDebugFlags)target.GetValue(DebugFlagsProperty);
        }

        public NDebugFlags DebugFlags
        {
            get { return GetDebugFlags(this); }
            set { SetDebugFlags(this, value); }
        }

        #endregion

        #endregion

        #region ISelectable

        private bool _IsSelected;

        /// <summary>
        /// Determine if this element is selected
        /// </summary>
        public virtual bool IsSelected
        {
            get { return _IsSelected; }
        }

        /// <summary>
        /// Select this element
        /// 
        /// This base implementation merely shows a selection adorner
        /// </summary>
        public virtual void Select()
        {
            _IsSelected = true;

            AddSelectionAdorner();
        }

        /// <summary>
        /// Unselect this element
        /// 
        /// This base implementation merely hides the selection adorner
        /// </summary>
        public virtual void Unselect()
        {
            _IsSelected = false;

            RemoveSelectionAdorner();
        }

        #region Selection Adorner

        public Adorner SelectionAdorner { get; set; }

        private Adorner CurrentSelectionAdorner { get; set; }

        private void AddSelectionAdorner()
        {
            RemoveSelectionAdorner();

            if (SelectionAdorner != null)
            {
                AddAdorner(SelectionAdorner);
                CurrentSelectionAdorner = SelectionAdorner;
            }
        }

        private void RemoveSelectionAdorner()
        {
            if (CurrentSelectionAdorner != null)
            {
                RemoveAdorner(CurrentSelectionAdorner);
                CurrentSelectionAdorner = null;
            }
        }

        public void AddAdorner(Adorner adorner)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null && !ContainsAdorner(adorner))
            {
                adornerLayer.Add(adorner);
            }
        }

        public void RemoveAdorner(Adorner adorner)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(adorner);
            }
        }

        public bool ContainsAdorner(Adorner adorner)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer == null)
            {
                return false;
            }

            var adorners = adornerLayer.GetAdorners(this);
            if (adorners == null)
            {
                return false;
            }

            return adorners.Contains(adorner);
        }

        #endregion

        #endregion

        #region IEditable

        /// <summary>
        /// Cut the currently-selected portion of this element
        /// </summary>
        /// <returns>An array of the objects that were cut</returns>
        public virtual IList<object> Cut()
        {
            return new object[] { this };
        }

        /// <summary>
        /// Copy the currently-selected portion of this element
        /// </summary>
        /// <returns>An array of the objects that were copied</returns>
        public virtual IList<object> Copy()
        {
            var cloneable = this as ICloneable;
            if (cloneable != null)
            {
                return new object[] { cloneable.Clone() };
            }
            else
            {
                return new object[0];
            }
        }

        /// <summary>
        /// Paste the given objects into this element at the current selection point
        /// </summary>
        /// <param name="objects">Objects to be pasted</param>
        /// <returns>An array of objects that could not be pasted (this may include newly-created objects if the element had to be split)</returns>
        public virtual IList<object> Paste(IList<object> objects)
        {
            return objects;
        }

        /// <summary>
        /// Delete the currently-selected portion of this element
        /// </summary>
        /// <returns></returns>
        public virtual bool Delete()
        {
            return false;
        }

        #endregion

        #region IFormattable

        public virtual bool HasProperty(string name)
        {
            return false;
        }

        public virtual object GetProperty(string name)
        {
            return null;
        }

        public virtual void SetProperty(string name, object value)
        {

        }

        public virtual void ResetProperties()
        {

        }

        public virtual int ChangeProperty(string name, object oldValue, object newValue)
        {
            return 0;
        }

        #endregion

        #region IMovable

        /// <summary>
        /// Called when this element is about to be moved
        /// </summary>
        public virtual void MoveStarted()
        {
            // Temporarily freeze our parent's width.
            // This provides better UX when selecting an image wider than the current viewport.
            var panel = Parent as Panel;
            if (panel != null && panel.IsArrangeValid)
            {
                panel.Width = panel.ActualWidth;
                panel.Height = panel.ActualHeight;
            }

            Positioning = Positioning.Absolute;
            Margin = new Thickness(0);
        }

        /// <summary>
        /// Called when this element is being moved
        /// </summary>
        /// <param name="delta"></param>
        public virtual void MoveDelta(Vector delta)
        {
            Position += delta;
        }

        /// <summary>
        /// Called when this element is done being moved
        /// </summary>
        public virtual void MoveCompleted()
        {
            var panel = Parent as Panel;
            if (panel != null)
            {
                panel.Width = Double.NaN;
                panel.Height = Double.NaN;
            }

            Positioning = Positioning.Overlapped;
        }

        /// <summary>
        /// Request our container to call MoveStarted()
        /// </summary>
        public void RequestBeginMove()
        {
            var e = new RoutedEventArgs(Movable.RequestingBeginMove);

            this.RaiseEvent(e);
        }

        /// <summary>
        /// Request our container to call MoveDelta()
        /// </summary>
        public void RequestMoveDelta(Vector delta)
        {
            var e = new MoveDeltaEventArgs(Movable.RequestingMoveDelta, delta);

            this.RaiseEvent(e);
        }

        /// <summary>
        /// Request our container to call MoveEnd()
        /// </summary>
        public void RequestEndMove()
        {
            var e = new RoutedEventArgs(Movable.RequestingEndMove);

            this.RaiseEvent(e);
        }

        #endregion

        #region IResizable

        /// <summary>
        /// Resize this element by the specified amount
        /// </summary>
        public virtual void Resize(Vector delta)
        {
            double width = ComputedWidth;
            double height = ComputedHeight;

            Width = Math.Max(width + delta.X, 0);
            Height = Math.Max(height + delta.Y, 0);
        }

        /// <summary>
        /// Determine the width at which this element will be rendered
        /// </summary>
        protected virtual double ComputedWidth
        {
            get
            {
                if (!Double.IsNaN(Width))
                {
                    return Width;
                }
                else if (IsArrangeValid)
                {
                    return RenderSize.Width;
                }
                else if (IsMeasureValid)
                {
                    return DesiredSize.Width;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Determine the height at which this element will be rendered
        /// </summary>
        protected virtual double ComputedHeight
        {
            get
            {
                if (!Double.IsNaN(Height))
                {
                    return Height;
                }
                else if (IsArrangeValid)
                {
                    return RenderSize.Height;
                }
                else if (IsMeasureValid)
                {
                    return DesiredSize.Height;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion

        #region IHasResources

        #region ResourceChangedEvent

        public static RoutedEvent ResourceChangedEvent = ResourceContainer.ResourceChangedEvent.AddOwner(typeof(DocumentElement));

        protected void RaiseResourceChanged(string url)
        {
            RaiseEvent(new ResourceEventArgs(ResourceChangedEvent, this, url));
        }

        public event ResourceEventHandler ResourceChanged
        {
            add { base.AddHandler(ResourceChangedEvent, value); }
            remove { base.RemoveHandler(ResourceChangedEvent, value); }
        }

        #endregion

        #region ResourceRequestedEvent

        public static RoutedEvent ResourceRequestedEvent = ResourceContainer.ResourceRequestedEvent.AddOwner(typeof(DocumentElement));

        protected void RaiseResourceRequested(string url)
        {
            RaiseEvent(new ResourceEventArgs(ResourceRequestedEvent, this, url));
        }

        public event ResourceEventHandler ResourceRequested
        {
            add { base.AddHandler(ResourceRequestedEvent, value); }
            remove { base.RemoveHandler(ResourceRequestedEvent, value); }
        }

        #endregion

        private string[] _ResourceNames = new string[0];

        /// <summary>
        /// Get the names of all embedded resources
        /// </summary>
        public virtual IEnumerable<string> ResourceNames
        {
            get { return _ResourceNames; }
        }

        /// <summary>
        /// Write an embedded resource to the specified stream
        /// </summary>
        public virtual void GetResource(string url, Stream stream)
        {
            OnGetResource(url, stream);
        }

        /// <summary>
        /// Callback to write an embedded resource to the specified stream
        /// </summary>
        protected virtual void OnGetResource(string url, Stream stream)
        {

        }

        /// <summary>
        /// Read an embedded resource from the specified stream
        /// </summary>
        public virtual void SetResource(string url, Stream stream)
        {
            OnSetResource(url, stream);
        }

        /// <summary>
        /// Callback to read an embeded resource from the specified stream
        /// </summary>
        protected virtual void OnSetResource(string url, Stream stream)
        {

        }

        #endregion

        #region INodeSource

        public virtual NodeType GetNodeType(NodeContext context)
        {
            if (context is HTMLDocument || context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLNodeType(context);
            }
            else
            {
                return GetSVGNodeType(context);
            }
        }

        public virtual string GetNodeName(NodeContext context)
        {
            if (context is HTMLDocument || context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLNodeName(context);
            }
            else
            {
                return GetSVGNodeName(context);
            }
        }

        public virtual IList<string> GetNodeAttributes(ElementContext context)
        {
            if (context is HTMLElement)
            {
                return GetHTMLAttributes(context);
            }
            else
            {
                return GetSVGAttributes(context);
            }
        }

        public virtual string GetNodeAttribute(ElementContext context, string name)
        {
            if (context is HTMLElement)
            {
                return GetHTMLAttribute(context, name);
            }
            else
            {
                return GetSVGAttribute(context, name);
            }
        }

        public virtual void SetNodeAttribute(ElementContext context, string name, string value)
        {
            if (context is HTMLElement)
            {
                SetHTMLAttribute(context, name, value);
            }
            else
            {
                SetSVGAttribute(context, name, value);
            }
        }

        public virtual void ResetNodeAttribute(ElementContext context, string name)
        {
            if (context is HTMLElement)
            {
                ResetHTMLAttribute(context, name);
            }
            else
            {
                ResetSVGAttribute(context, name);
            }
        }

        public virtual object GetParentNode(NodeContext context)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLParentNode(context);
            }
            else
            {
                return GetSVGParentNode(context);
            }
        }

        public virtual IEnumerable<object> GetChildNodes(NodeContext context)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLChildNodes(context);
            }
            else
            {
                return GetSVGChildNodes(context);
            }
        }

        public virtual object CreateNode(NodeContext context)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return CreateHTMLNode(context);
            }
            else
            {
                return CreateSVGNode(context);
            }
        }

        public virtual void AppendNode(NodeContext context, object newChild)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                AppendHTMLNode(context, newChild);
            }
            else
            {
                AppendSVGNode(context, newChild);
            }
        }

        public virtual void InsertNode(NodeContext context, object newChild, object refChild)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                InsertHTMLNode(context, newChild, refChild);
            }
            else
            {
                InsertSVGNode(context, newChild, refChild);
            }
        }

        public virtual void RemoveNode(NodeContext context, object oldChild)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                RemoveHTMLNode(context, oldChild);
            }
            else
            {
                RemoveSVGNode(context, oldChild);
            }
        }

        public event NodeEventHandler NodeEvent;

        protected void RaiseNodeEvent(IEventSource e)
        {
            if (NodeEvent != null)
            {
                NodeEvent(e);
            }
        }

        #endregion

        #region IStyleable

        public virtual IList<string> GetSupportedStyles(ElementContext context)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLStyles(context);
            }
            else
            {
                return GetSVGStyles(context);
            }
        }

        public virtual CSSValue GetStyleProperty(ElementContext context, string name)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLStyle(context, name);
            }
            else
            {
                return GetSVGStyle(context, name);
            }
        }

        public virtual void SetStyleProperty(ElementContext context, string name, CSSValue value)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                SetHTMLStyle(context, name, value);
            }
            else
            {
                SetSVGStyle(context, name, value);
            }
        }

        #endregion

        #region HTML

        public virtual NodeType GetHTMLNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        public virtual string GetHTMLNodeName(NodeContext context)
        {
            throw new NotImplementedException();
        }

        public virtual IList<string> GetHTMLAttributes(ElementContext context)
        {
            return context.GetInternalAttributes();
        }

        public virtual string GetHTMLAttribute(ElementContext context, string name)
        {
            return context.GetInternalAttribute(name);
        }

        public virtual void SetHTMLAttribute(ElementContext context, string name, string value)
        {
            context.SetInternalAttribute(name, value);
        }

        public virtual void ResetHTMLAttribute(ElementContext context, string name)
        {
            context.RemoveInternalAttribute(name);
        }

        public virtual object GetHTMLParentNode(NodeContext context)
        {
            return Parent;
        }

        public virtual IEnumerable<object> GetHTMLChildNodes(NodeContext context)
        {
            return null;
        }

        public virtual object CreateHTMLNode(NodeContext newNode)
        {
            return null;
        }

        public virtual void AppendHTMLNode(NodeContext context, object newChild)
        {

        }

        public virtual void InsertHTMLNode(NodeContext context, object newChild, object refChild)
        {

        }

        public virtual void RemoveHTMLNode(NodeContext context, object oldChild)
        {

        }

        private static readonly string[] _HTMLStyles = new[]
            {
                CSSProperties.BackgroundColor,
                CSSProperties.BorderTopColor,
                CSSProperties.BorderRightColor,
                CSSProperties.BorderBottomColor,
                CSSProperties.BorderLeftColor,
                CSSProperties.BorderTopStyle,
                CSSProperties.BorderRightStyle,
                CSSProperties.BorderBottomStyle,
                CSSProperties.BorderLeftStyle,
                CSSProperties.BorderTopWidth,
                CSSProperties.BorderRightWidth,
                CSSProperties.BorderBottomWidth,
                CSSProperties.BorderLeftWidth,
                CSSProperties.BorderTopLeftRadius,
                CSSProperties.BorderTopRightRadius,
                CSSProperties.BorderBottomRightRadius,
                CSSProperties.BorderBottomLeftRadius,
                CSSProperties.MarginTop,
                CSSProperties.MarginRight,
                CSSProperties.MarginBottom,
                CSSProperties.MarginLeft,
                CSSProperties.Position,
                CSSProperties.Left,
                CSSProperties.Top,
                CSSProperties.Right,
                CSSProperties.Bottom,
                CSSProperties.Width,
                CSSProperties.Height,
                CSSProperties.Float,
                CSSProperties.Clear
            };

        public virtual IList<string> GetHTMLStyles(ElementContext context)
        {
            return _HTMLStyles;
        }

        public virtual CSSValue GetHTMLStyle(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.BackgroundColor:

                    return CSSConverter.ToCSSValue(Background, CSSValues.Transparent);

                case CSSProperties.BorderTopColor:

                    return CSSConverter.ToCSSValue(BorderTopColor);

                case CSSProperties.BorderRightColor:

                    return CSSConverter.ToCSSValue(BorderRightColor);

                case CSSProperties.BorderBottomColor:

                    return CSSConverter.ToCSSValue(BorderBottomColor);

                case CSSProperties.BorderLeftColor:

                    return CSSConverter.ToCSSValue(BorderLeftColor);

                case CSSProperties.BorderTopStyle:

                    return DocumentPanel.ToBorderStyleCSS(BorderTopStyle);

                case CSSProperties.BorderRightStyle:

                    return DocumentPanel.ToBorderStyleCSS(BorderRightStyle);

                case CSSProperties.BorderBottomStyle:

                    return DocumentPanel.ToBorderStyleCSS(BorderBottomStyle);

                case CSSProperties.BorderLeftStyle:

                    return DocumentPanel.ToBorderStyleCSS(BorderLeftStyle);

                case CSSProperties.BorderTopWidth:

                    if (BorderTopStyle != DocumentPanel.BorderStyles.None)
                        return CSSValues.Pixels(BorderTopWidth);
                    else
                        return CSSValues.Zero;

                case CSSProperties.BorderRightWidth:

                    if (BorderRightStyle != DocumentPanel.BorderStyles.None)
                        return CSSValues.Pixels(BorderRightWidth);
                    else
                        return CSSValues.Zero;

                case CSSProperties.BorderBottomWidth:

                    if (BorderBottomStyle != DocumentPanel.BorderStyles.None)
                        return CSSValues.Pixels(BorderBottomWidth);
                    else
                        return CSSValues.Zero;

                case CSSProperties.BorderLeftWidth:

                    if (BorderLeftStyle != DocumentPanel.BorderStyles.None)
                        return CSSValues.Pixels(BorderLeftWidth);
                    else
                        return CSSValues.Zero;

                case CSSProperties.BorderTopLeftRadius:

                    var borderTopLeftRadius = CSSValues.Pixels(BorderTopLeftRadius);
                    return CSSValues.List(new[] { borderTopLeftRadius });

                case CSSProperties.BorderTopRightRadius:

                    var borderTopRightRadius = CSSValues.Pixels(BorderTopRightRadius);
                    return CSSValues.List(new[] { borderTopRightRadius });

                case CSSProperties.BorderBottomRightRadius:

                    var borderBottomRightRadius = CSSValues.Pixels(BorderBottomRightRadius);
                    return CSSValues.List(new[] { borderBottomRightRadius });

                case CSSProperties.BorderBottomLeftRadius:

                    var borderBottomLeftRadius = CSSValues.Pixels(BorderBottomLeftRadius);
                    return CSSValues.List(new[] { borderBottomLeftRadius });

                case CSSProperties.MarginTop:

                    return CSSValues.Pixels(Margin.Top);

                case CSSProperties.MarginRight:

                    if (DocumentPanel.GetRelativeAlignment(this) == HorizontalAlignment.Center)
                        return CSSValues.Auto;
                    return CSSValues.Pixels(Margin.Right);

                case CSSProperties.MarginBottom:

                    return CSSValues.Pixels(Margin.Bottom);

                case CSSProperties.MarginLeft:

                    if (DocumentPanel.GetRelativeAlignment(this) == HorizontalAlignment.Center)
                        return CSSValues.Auto;
                    return CSSValues.Pixels(Margin.Left);

                case CSSProperties.Position:

                    var positioning = this.Positioning;
                    if (positioning == Editor.Positioning.Absolute)
                        return DocumentPanel.ToCSSPositioning(Editor.Positioning.Overlapped);
                    else
                        return DocumentPanel.ToCSSPositioning(positioning);

                case CSSProperties.Left:

                    double left = DocumentPanel.GetRelativePosition(this).X;
                    if (Double.IsNaN(left))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(left);

                case CSSProperties.Top:

                    double top = DocumentPanel.GetRelativePosition(this).Y;
                    if (Double.IsNaN(top))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(top);

                case CSSProperties.Right:

                    if (DocumentPanel.GetRelativeAlignment(this) == HorizontalAlignment.Right)
                        return CSSValues.Pixels(0);
                    else if (Double.IsNaN(Right) || Right == 0)
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(Right);

                case CSSProperties.Bottom:

                    if (Double.IsNaN(Bottom))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(Bottom);

                case CSSProperties.Width:

                    if (Double.IsNaN(Width))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(Width);

                case CSSProperties.Height:

                    if (Double.IsNaN(Height))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(Height);

                case CSSProperties.Float:

                    return DocumentPanel.ToFloatCSS(Float);

                case CSSProperties.Clear:

                    return DocumentPanel.ToClearCSS(Clear);

                default:
                    return null;
            }
        }

        private bool _IsMarginLeftAuto;
        private bool _IsMarginRightAuto;

        public virtual void SetHTMLStyle(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.BackgroundColor:

                    var backgroundColor = (CSSPrimitiveValue)value;
                    if (backgroundColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        Background = CSSConverter.ToBrush(backgroundColor);
                    else if (backgroundColor == CSSValues.Transparent)
                        Background = DefaultBackground;
                    break;

                case CSSProperties.BorderTopColor:

                    var borderTopColor = (CSSPrimitiveValue)value;
                    if (borderTopColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        BorderTopColor = CSSConverter.ToColor(borderTopColor);
                    else if (borderTopColor == CSSValues.Transparent)
                        BorderTopColor = DefaultBorderTopColor;
                    break;

                case CSSProperties.BorderRightColor:

                    var borderRightColor = (CSSPrimitiveValue)value;
                    if (borderRightColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        BorderRightColor = CSSConverter.ToColor(borderRightColor);
                    else if (borderRightColor == CSSValues.Transparent)
                        BorderRightColor = DefaultBorderRightColor;
                    break;

                case CSSProperties.BorderBottomColor:

                    var borderBottomColor = (CSSPrimitiveValue)value;
                    if (borderBottomColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        BorderBottomColor = CSSConverter.ToColor(borderBottomColor);
                    else if (borderBottomColor == CSSValues.Transparent)
                        BorderBottomColor = DefaultBorderBottomColor;
                    break;

                case CSSProperties.BorderLeftColor:

                    var borderLeftColor = (CSSPrimitiveValue)value;
                    if (borderLeftColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        BorderLeftColor = CSSConverter.ToColor(borderLeftColor);
                    else if (borderLeftColor == CSSValues.Transparent)
                        BorderLeftColor = DefaultBorderLeftColor;
                    break;

                case CSSProperties.BorderTopStyle:

                    var borderTopStyle = (CSSPrimitiveValue)value;
                    BorderTopStyle = DocumentPanel.FromBorderStyleCSS(borderTopStyle);
                    break;

                case CSSProperties.BorderRightStyle:

                    var borderRightStyle = (CSSPrimitiveValue)value;
                    BorderRightStyle = DocumentPanel.FromBorderStyleCSS(borderRightStyle);
                    break;

                case CSSProperties.BorderBottomStyle:

                    var borderBottomStyle = (CSSPrimitiveValue)value;
                    BorderBottomStyle = DocumentPanel.FromBorderStyleCSS(borderBottomStyle);
                    break;

                case CSSProperties.BorderLeftStyle:

                    var borderLeftStyle = (CSSPrimitiveValue)value;
                    BorderLeftStyle = DocumentPanel.FromBorderStyleCSS(borderLeftStyle);
                    break;

                case CSSProperties.BorderTopWidth:

                    var borderTopWidth = (CSSPrimitiveValue)value;
                    if (borderTopWidth.IsLength())
                        BorderTopWidth = borderTopWidth.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.BorderRightWidth:

                    var borderRightWidth = (CSSPrimitiveValue)value;
                    if (borderRightWidth.IsLength())
                        BorderRightWidth = borderRightWidth.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.BorderBottomWidth:

                    var borderBottomWidth = (CSSPrimitiveValue)value;
                    if (borderBottomWidth.IsLength())
                        BorderBottomWidth = borderBottomWidth.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.BorderLeftWidth:

                    var borderLeftWidth = (CSSPrimitiveValue)value;
                    if (borderLeftWidth.IsLength())
                        BorderLeftWidth = borderLeftWidth.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.BorderTopLeftRadius:

                    var borderTopLeftRadius = (CSSValueList)value;
                    if (borderTopLeftRadius.Length > 0)
                    {
                        var borderTopLeftRadiusX = (CSSPrimitiveValue)borderTopLeftRadius[0];
                        if (borderTopLeftRadiusX.IsLength())
                        {
                            BorderTopLeftRadius = borderTopLeftRadiusX.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;

                case CSSProperties.BorderTopRightRadius:

                    var borderTopRightRadius = (CSSValueList)value;
                    if (borderTopRightRadius.Length > 0)
                    {
                        var borderTopRightRadiusX = (CSSPrimitiveValue)borderTopRightRadius[0];
                        if (borderTopRightRadiusX.IsLength())
                        {
                            BorderTopRightRadius = borderTopRightRadiusX.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;

                case CSSProperties.BorderBottomRightRadius:

                    var borderBottomRightRadius = (CSSValueList)value;
                    if (borderBottomRightRadius.Length > 0)
                    {
                        var borderBottomRightRadiusX = (CSSPrimitiveValue)borderBottomRightRadius[0];
                        if (borderBottomRightRadiusX.IsLength())
                        {
                            BorderBottomRightRadius = borderBottomRightRadiusX.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;

                case CSSProperties.BorderBottomLeftRadius:

                    var borderBottomLeftRadius = (CSSValueList)value;
                    if (borderBottomLeftRadius.Length > 0)
                    {
                        var borderBottomLeftRadiusX = (CSSPrimitiveValue)borderBottomLeftRadius[0];
                        if (borderBottomLeftRadiusX.IsLength())
                        {
                            BorderBottomLeftRadius = borderBottomLeftRadiusX.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;

                case CSSProperties.MarginTop:

                    var marginTop = (CSSPrimitiveValue)value;
                    if (marginTop.IsLength())
                        Margin = new Thickness(Margin.Left, marginTop.GetFloatValue(CSSPrimitiveType.CSS_PX), Margin.Right, Margin.Bottom);
                    break;

                case CSSProperties.MarginRight:

                    var marginRight = (CSSPrimitiveValue)value;
                    if (marginRight.IsLength())
                        Margin = new Thickness(Margin.Left, Margin.Top, marginRight.GetFloatValue(CSSPrimitiveType.CSS_PX), Margin.Bottom);
                    _IsMarginRightAuto = (marginRight == CSSValues.Auto);
                    if (_IsMarginLeftAuto && _IsMarginRightAuto)
                        HorizontalAlignment = HorizontalAlignment.Center;
                    break;

                case CSSProperties.MarginBottom:

                    var marginBottom = (CSSPrimitiveValue)value;
                    if (marginBottom.IsLength())
                        Margin = new Thickness(Margin.Left, Margin.Top, Margin.Right, marginBottom.GetFloatValue(CSSPrimitiveType.CSS_PX));
                    break;

                case CSSProperties.MarginLeft:

                    var marginLeft = (CSSPrimitiveValue)value;
                    if (marginLeft.IsLength())
                        Margin = new Thickness(marginLeft.GetFloatValue(CSSPrimitiveType.CSS_PX), Margin.Top, Margin.Right, Margin.Bottom);
                    _IsMarginLeftAuto = (marginLeft == CSSValues.Auto);
                    if (_IsMarginLeftAuto && _IsMarginRightAuto)
                        HorizontalAlignment = HorizontalAlignment.Center;
                    break;

                case CSSProperties.Position:

                    var position = (CSSPrimitiveValue)value;
                    Positioning = DocumentPanel.FromCSSPositioning(position);
                    break;

                case CSSProperties.Left:

                    var left = (CSSPrimitiveValue)value;
                    if (left == CSSValues.Auto)
                        Left = DefaultLeft;
                    else if (left.IsLength())
                        Left = left.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Top:

                    var top = (CSSPrimitiveValue)value;
                    if (top == CSSValues.Auto)
                        Top = DefaultTop;
                    else if (top.IsLength())
                        Top = top.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Right:

                    var right = (CSSPrimitiveValue)value;
                    if (right == CSSValues.Auto)
                        Right = DefaultRight;
                    else if (right.IsLength())
                        Right = right.GetFloatValue(CSSPrimitiveType.CSS_PX);

                    if (!Right.Equals(DefaultRight))
                        HorizontalAlignment = HorizontalAlignment.Right;
                    break;

                case CSSProperties.Bottom:

                    var bottom = (CSSPrimitiveValue)value;
                    if (bottom == CSSValues.Auto)
                        Bottom = DefaultBottom;
                    else if (bottom.IsLength())
                        Bottom = bottom.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Width:

                    var width = (CSSPrimitiveValue)value;
                    if (width == CSSValues.Auto)
                        Width = Double.NaN;
                    else if (width.IsLength())
                        Width = width.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Height:

                    var height = (CSSPrimitiveValue)value;
                    if (height == CSSValues.Auto)
                        Height = Double.NaN;
                    else if (height.IsLength())
                        Height = height.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Float:

                    var floatValue = (CSSPrimitiveValue)value;
                    Float = DocumentPanel.FromFloatCSS(floatValue);
                    break;

                case CSSProperties.Clear:

                    var clearValue = (CSSPrimitiveValue)value;
                    Clear = DocumentPanel.FromClearCSS(clearValue);
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region SVG

        public virtual NodeType GetSVGNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        public virtual string GetSVGNodeName(NodeContext context)
        {
            throw new NotImplementedException();
        }

        public virtual IList<string> GetSVGAttributes(ElementContext context)
        {
            return context.GetInternalAttributes();
        }

        public virtual string GetSVGAttribute(ElementContext context, string name)
        {
            return context.GetInternalAttribute(name);
        }

        public virtual void SetSVGAttribute(ElementContext context, string name, string value)
        {
            context.SetInternalAttribute(name, value);
        }

        public virtual void ResetSVGAttribute(ElementContext context, string name)
        {
            context.RemoveInternalAttribute(name);
        }

        public virtual object GetSVGParentNode(NodeContext context)
        {
            return Parent;
        }

        public virtual IEnumerable<object> GetSVGChildNodes(NodeContext context)
        {
            return null;
        }

        public virtual object CreateSVGNode(NodeContext newNode)
        {
            return null;
        }

        public virtual void AppendSVGNode(NodeContext context, object newChild)
        {

        }

        public virtual void InsertSVGNode(NodeContext context, object newChild, object refChild)
        {

        }

        public virtual void RemoveSVGNode(NodeContext context, object oldChild)
        {

        }

        public virtual IList<string> GetSVGStyles(ElementContext context)
        {
            return null;
        }

        public virtual CSSValue GetSVGStyle(ElementContext context, string name)
        {
            return null;
        }

        public virtual void SetSVGStyle(ElementContext context, string name, CSSValue value)
        {

        }

        #endregion

        #region SaveAsImage

        /// <summary>
        /// Save this element as an image to the specified file
        /// 
        /// The image encoding is determined by the file's extension
        /// </summary>
        public void SaveAsImage(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    stream.SetLength(0);
                    SaveAsImage(stream, Path.GetExtension(filePath));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Save this element as an image to the specified stream
        /// </summary>
        /// <param name="stream">Save to this stream</param>
        /// <param name="type">Image format (.bmp, .png, etc.)</param>
        public virtual void SaveAsImage(Stream stream, string type)
        {
            SaveAsImage(this, stream, type);
        }

        /// <summary>
        /// Save a UIElement as an image to the specified stream
        /// </summary>
        public static void SaveAsImage(UIElement visual, Stream stream, string type)
        {
            var encoder = CreateBitmapEncoder(type);

            if (encoder != null)
            {
                var bitmap = ToBitmap(visual);
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }
            else
            {
                Debug.WriteLine("File type \"" + type + "\" not supported");
            }
        }

        /// <summary>
        /// Create a bitmap encoder for the specified image type
        /// </summary>
        private static BitmapEncoder CreateBitmapEncoder(string type)
        {
            type = type.ToLower();
            type = type.TrimStart('.');

            switch (type)
            {
                case "bmp":
                    return new BmpBitmapEncoder();
                case "gif":
                    return new GifBitmapEncoder();
                case "jpg":
                case "jpeg":
                    return new JpegBitmapEncoder();
                case "png":
                    return new PngBitmapEncoder();
                case "tiff":
                    return new TiffBitmapEncoder();
                default:
                    // Not supported
                    return null;
            }
        }

        /// <summary>
        /// Convert this element to a bitmap
        /// </summary>
        /// <returns></returns>
        public virtual BitmapSource ToBitmap()
        {
            return ToBitmap(this);
        }

        /// <summary>
        /// Convert the specified UIElement to a bitmap
        /// </summary>
        public static BitmapSource ToBitmap(UIElement target)
        {
            var size = target.RenderSize;

            if (size.Width == 0 || size.Height == 0)
            {
                return null;
            }

            var bitmap = new RenderTargetBitmap (
                pixelWidth: (int)size.Width,
                pixelHeight: (int)size.Height,
                dpiX: 96,
                dpiY: 96,
                pixelFormat: PixelFormats.Default
            );

            // Draw the UIElement onto a DrawingVisual

            var drawing = new DrawingVisual();

            using (var dc = drawing.RenderOpen())
            {
                var backgroundRect = new Rect(0, 0, size.Width, size.Height);
                dc.DrawRectangle(Brushes.Transparent, null, backgroundRect);

                var visualBrush = new VisualBrush(target);
                var visualSize = VisualTreeHelper.GetDescendantBounds(target).Size;
                var position = new Point(
                    (size.Width - visualSize.Width) / 2,
                    (size.Height - visualSize.Height) / 2
                );

                dc.DrawRectangle(visualBrush, null, new Rect(position, visualSize));
            }

            // Render the DrawingVisual to the bitmap

            bitmap.Render(drawing);

            return bitmap;
        }

        #endregion

        #region Implementation

        protected override void OnRender(DrawingContext dc)
        {
            var rect = new Rect(RenderSize);

            // Background

            var geometry = DocumentPanel.GetBorderGeometry(
                rect,
                BorderLeftStyle, BorderLeftColor, BorderLeftWidth, BorderTopLeftRadius,
                BorderTopStyle, BorderTopColor, BorderTopWidth, BorderTopRightRadius,
                BorderRightStyle, BorderRightColor, BorderRightWidth, BorderBottomRightRadius,
                BorderBottomStyle, BorderBottomColor, BorderBottomWidth, BorderBottomLeftRadius
            );

            dc.DrawGeometry(Background, null, geometry);

            // Border
            DocumentPanel.DrawBorders(
                dc, rect,
                BorderLeftStyle, BorderLeftColor, BorderLeftWidth, BorderTopLeftRadius,
                BorderTopStyle, BorderTopColor, BorderTopWidth, BorderTopRightRadius,
                BorderRightStyle, BorderRightColor, BorderRightWidth, BorderBottomRightRadius,
                BorderBottomStyle, BorderBottomColor, BorderBottomWidth, BorderBottomLeftRadius
            );

            // Debug visuals
            if (DebugFlags.HasFlag(NDebugFlags.ShowPosition))
            {
                var guidelines = new GuidelineSet(new double[] { 0.5 }, new double[] { 0.5 });
                dc.PushGuidelineSet(guidelines);
                var outlinePen = new Pen(Brushes.Gray, 1);
                outlinePen.Freeze();
                dc.DrawRectangle(null, outlinePen, new Rect(RenderSize));
            }

            if (DebugFlags.HasFlag(NDebugFlags.ShowAttributes))
            {
                ToolTip = "...";
            }
            else
            {
                ToolTip = null;
            }
        }

        /// <summary>
        /// Draw an arc to the given drawing context
        /// </summary>
        private static void DrawArc(DrawingContext dc, Brush brush, Pen pen, Point startPoint, Point endPoint, double radius)
        {
            var segment = new ArcSegment(endPoint, new Size(radius, radius), 0, false, SweepDirection.Clockwise, true);
            segment.Freeze();

            var figure = new PathFigure();
            figure.StartPoint = startPoint;
            figure.Segments.Add(segment);
            figure.Freeze();

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.Freeze();

            dc.DrawGeometry(brush, pen, geometry);
        }

        protected override void OnToolTipOpening(System.Windows.Controls.ToolTipEventArgs e)
        {
            if (DebugFlags.HasFlag(NDebugFlags.ShowAttributes))
            {
                var node = OwnerDocument.GetNode(this);

                if (node is HTMLElement)
                {
                    ((HTMLElement)node).UpdateStyle(true);
                }

                ToolTip = node.ToString();

                ToolTipService.SetInitialShowDelay(this, 500);
                ToolTipService.SetShowDuration(this, int.MaxValue);
            }

            base.OnToolTipOpening(e);
        }

        private static string PrettifyCSS(string css)
        {
            css = css.Replace(";", "; ");
            for (int i = 100; i < css.Length; i += 100)
            {
                int j = css.IndexOf(';', i);
                if (j != -1)
                {
                    css = css.Insert(j + 1, "\n");
                }
            }
            return css;
        }

        #endregion
    }

    public enum NDebugFlags
    {
        None = 0x00,
        ShowPosition = 0x01,
        ShowAttributes = 0x02,
        All = 0xFF
    }
}
