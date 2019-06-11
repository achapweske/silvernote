/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SilverNote.Common;
using System.IO;
using DOM.LS;
using DOM;
using DOM.CSS;
using DOM.SVG;
using System.Globalization;
using System.Reflection;
using System.Windows.Media.Imaging;
using DOM.HTML;
using Jurassic;
using System.Windows.Media.Effects;

namespace SilverNote.Editor
{
    public enum ViewBoxAlignment
    {
        None,
        xMinYMin,
        xMidYMin,
        xMaxYMin,
        xMinYMid,
        xMidYMid,
        xMaxYMid,
        xMinYMax,
        xMidYMax,
        xMaxYMax
    }

    public enum ViewBoxFit
    {
        Meet,
        Slice
    }

    public class DocumentCanvas : DocumentElement, IEnumerable<Shape>, IVisualElement, ICloneable
    {
        #region Fields

        string _Filename;
        Rect _ViewBox;
        ViewBoxAlignment _Align;
        ViewBoxFit _MeetOrSlice;
        VisualCollection _Visuals;
        ShapeCollection _Drawings;
        List<Shape> _InternalDrawings;
        ScriptEngine _JSEngine;

        #endregion

        #region Constructors

        public DocumentCanvas()
        {
            Initialize();
        }

        public DocumentCanvas(DocumentCanvas copy)
            : base(copy)
        {
            Initialize();

            Padding = copy.Padding;
            ViewBox = copy.ViewBox;
            Align = copy.Align;
            MeetOrSlice = copy.MeetOrSlice;

            // Copy drawings

            foreach (Shape drawing in copy)
            {
                Drawings.Add((Shape)drawing.Clone());
            }
        }

        private void Initialize()
        {
            SnapsToDevicePixels = true;
            Padding = new Thickness(10);

            _ViewBox = Rect.Empty;
            _Align = ViewBoxAlignment.None;
            _MeetOrSlice = ViewBoxFit.Meet;
            _Visuals = new VisualCollection(this);
            _Drawings = new ShapeCollection();
            _Drawings.CollectionChanged += Drawings_CollectionChanged;
            _InternalDrawings = new List<Shape>();
        }

        #endregion

        #region Properties

        public string Filename
        {
            get { return _Filename ?? (_Filename = RandomFileName()); }
            set { _Filename = value; }
        }

        public Rect ViewBox
        {
            get
            {
                return _ViewBox;
            }
            set
            {
                if (value != _ViewBox)
                {
                    _ViewBox = value;
                    UpdateRenderTransform();
                }
            }
        }

        public ViewBoxAlignment Align
        {
            get
            {
                return _Align;
            }
            set
            {
                if (value != _Align)
                {
                    _Align = value;
                    UpdateRenderTransform();
                }
            }
        }

        public ViewBoxFit MeetOrSlice
        {
            get
            {
                return _MeetOrSlice;
            }
            set
            {
                if (value != _MeetOrSlice)
                {
                    _MeetOrSlice = value;
                    UpdateRenderTransform();
                }
            }
        }

        public ShapeCollection Drawings
        {
            get { return _Drawings; }
        }

        public Thickness Padding { get; set; }

        #region Drawing

        public static readonly DependencyProperty DrawingProperty = DependencyProperty.Register(
            "Drawing",
            typeof(Shape),
            typeof(DocumentCanvas),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(DrawingProperty_PropertyChanged))
        );

        static void DrawingProperty_PropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((DocumentCanvas)target).DrawingProperty_PropertyChanged((Shape)e.NewValue);
        }

        void DrawingProperty_PropertyChanged(Shape newValue)
        {
            Drawings.Clear();

            if (newValue != null)
            {
                Drawings.Add((Shape)newValue.Clone());
            }
        }

        public Shape Drawing
        {
            get { return (Shape)GetValue(DrawingProperty); }
            set { SetValue(DrawingProperty, value); }
        }

        #endregion

        #endregion

        #region Methods

        public void SizeToContent()
        {
            Rect autoBounds = VisualBounds;
            if (!autoBounds.IsEmpty)
            {
                autoBounds.X -= Padding.Left;
                autoBounds.Y -= Padding.Top;
                autoBounds.Width += Padding.Left + Padding.Right;
                autoBounds.Height += Padding.Top + Padding.Bottom;

                autoBounds.X = Math.Round(autoBounds.X);
                autoBounds.Y = Math.Round(autoBounds.Y);
                autoBounds.Width = Math.Round(autoBounds.Width);
                autoBounds.Height = Math.Round(autoBounds.Height);

                ActualBounds = autoBounds;
            }
        }

        #endregion

        #region IEnumerable

        public IEnumerator<Shape> GetEnumerator()
        {
            return Drawings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Drawings.GetEnumerator();
        }

        #endregion

        #region IVisualElement

        public Rect VisualBounds
        {
            get { return VisualElement.GetVisualBounds(Drawings, this); }
        }

        #endregion

        #region IHasResources

        bool _HasResource;

        public bool NeedsResource
        {
            get { return !(this is PrimaryCanvas) && Drawings.Count == 0 && !String.IsNullOrEmpty(Filename) && !_HasResource; }
            set { _HasResource = !value; }
        }

        public override IEnumerable<string> ResourceNames
        {
            get { yield return Filename; }
        }

        protected override void OnGetResource(string name, Stream stream)
        {
            if (!NeedsResource)
            {
                Save(stream);
            }
        }

        protected override void OnSetResource(string name, Stream stream)
        {
            Load(stream);
            NeedsResource = false;
        }

        #endregion

        #region Load/Save

        public void Save(Stream stream, bool pretty = false)
        {
            LSOutput output = DOMFactory.CreateLSOutput();
            output.ByteStream = stream;
            Save(output, pretty);
        }

        public void Save(TextWriter writer, bool pretty = false)
        {
            LSOutput output = DOMFactory.CreateLSOutput();
            output.CharacterStream = writer;
            Save(output, pretty);
        }

        public void Save(LSOutput output, bool pretty = false)
        {
            LSSerializer serializer = DOMFactory.CreateLSSerializer();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, !pretty);
            serializer.Config.SetParameter(LSSerializerParameters.FORMAT_PRETTY_PRINT, pretty);
            Save(serializer, output);
        }

        public void Save(LSSerializer serializer, LSOutput output)
        {
            _Defs = null;
            SVGDocument.RootElement.UpdateStyle(true);
            SVGDocument document = (SVGDocument)SVGDocument.CloneNode(true);

            // Add version comments

            if (!(bool)serializer.Config.GetParameter(LSSerializerParameters.FORMAT_PRETTY_PRINT))
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string createdWith = String.Format("Created with SilverNote v{0} (http://www.silver-note.com/)", version);
                if (document.FirstChild.NodeType == NodeType.COMMENT_NODE)
                    document.RemoveChild(document.FirstChild);
                Comment comment = document.CreateComment(createdWith);
                document.InsertBefore(comment, document.RootElement);
            }

            // Remove old defs elements
            NodeList oldDefs = document.GetElementsByTagNameNS(SVGElements.NAMESPACE, SVGElements.DEFS);
            foreach (Node oldDef in oldDefs)
            {
                oldDef.ParentNode.RemoveChild(oldDef);
            }

            // Add new defs element
            var defs = SVGDocument.GetElementsByTagNameNS(SVGElements.NAMESPACE, SVGElements.DEFS)[0];
            if (defs != null)
            {
                defs = document.AdoptNode(defs.CloneNode(true));
                document.RootElement.InsertBefore(defs, document.RootElement.FirstChild);
            }

            serializer.Write(document, output);
        }

        public bool Save(string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "PNG (.png) - Recommended|*.png|BMP (.bmp)|*.bmp|GIF (.gif)|*.gif|JPEG (.jpg)|*.jpg|TIFF (.tiff)|*.tiff|SVG (.svg)|*.svg";
                if (dialog.ShowDialog() != true)
                {
                    return false;
                }

                filePath = dialog.FileName;
            }

            string fileType = Path.GetExtension(filePath).ToLower();

            using (var file = File.Open(filePath, FileMode.OpenOrCreate))
            {
                Save(file, fileType);
                file.SetLength(file.Position);
            }

            return true;
        }

        public bool Save(Stream stream, string type)
        {
            try
            {
                if (type == ".svg")
                {
                    this.GetResource("", stream);
                    return true;
                }

                var bitmap = new RenderTargetBitmap(
                    (int)this.Width,
                    (int)this.Height,
                    96d, 96d,
                    PixelFormats.Default
                );

                // Render a background

                var background = new DrawingVisual();
                var dc = background.RenderOpen();
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, this.Width, this.Height));
                dc.Close();
                bitmap.Render(background);

                // Render the content

                bitmap.Render(this);

                BitmapEncoder encoder;
                switch (type)
                {
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".gif":
                        encoder = new GifBitmapEncoder();
                        break;
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    default:
                        encoder = null;
                        break;
                }

                if (encoder == null)
                {
                    Debug.WriteLine("File type \"" + type + "\" not supported");
                    return false;
                }

                var frame = BitmapFrame.Create(bitmap);
                encoder.Frames.Add(frame);
                encoder.Save(stream);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                return false;
            }
        }

        public bool Load(string str)
        {
            using (var reader = new StringReader(str))
            {
                return Load(reader);
            }
        }

        public bool Load(Stream stream)
        {
            LSInput input = DOMFactory.CreateLSInput();
            input.ByteStream = stream;
            return Load(input);
        }

        public bool Load(TextReader reader)
        {
            LSInput input = DOMFactory.CreateLSInput();
            input.CharacterStream = reader;
            return Load(input);
        }

        public bool Load(LSInput input)
        {
            LSParser parser = DOMFactory.CreateLSParser(DOMImplementationLSMode.MODE_SYNCHRONOUS, "");

            SVGDocument newDocument;
            try
            {
                newDocument = (SVGDocument)parser.Parse(input);
            }
            catch
            {
                return false;
            }

            _Defs = null;
            JSEngine = null;
            Drawings.Clear();
            SVGDocument = newDocument;

            return true;
        }

        public static string ToSVG(Shape drawing)
        {
            var canvas = new NCanvas();
            canvas.Drawings.Add((Shape)drawing.Clone());
            return canvas.ToSVG();
        }

        public string ToSVG(bool pretty = false)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                Save(writer, pretty);
                return writer.ToString();
            }
        }

        public static NCanvas FromSVG(string svg)
        {
            var canvas = new NCanvas();

            if (canvas.Load(svg))
            {
                return canvas;
            }
            else
            {
                return null;
            }
        }

        public static NCanvas FromResource(string path)
        {
            var result = new NCanvas();

            using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(path))
            {
                if (result.Load(stream))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public void EditSource()
        {
            var editor = new Dialogs.DOMEditor();
            editor.Owner = LayoutHelper.GetAncestor<Window>(this);
            editor.TextBox.Text = ToSVG(pretty: true);
            string originalText = editor.TextBox.Text;
            editor.TextBox.TextChanged += (sender, e) =>
            {
                this.Load(editor.TextBox.Text.Trim());
            };
            if (editor.ShowDialog() != true)
            {
                this.Load(originalText);
            }
        }

        #endregion

        #region HTML

        public const string MIME_TYPE = "image/svg+xml";

        public override string GetHTMLNodeName(NodeContext context)
        {
            return HTMLElements.OBJECT;
        }

        private string[] _HTMLAttributes = new[] { 
            HTMLAttributes.DATA,
            HTMLAttributes.TYPE,
            HTMLAttributes.WIDTH,
            HTMLAttributes.HEIGHT
        };

        public override IList<string> GetHTMLAttributes(ElementContext context)
        {
            return _HTMLAttributes;
        }

        public override string GetHTMLAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.DATA:
                    return Filename;
                case HTMLAttributes.TYPE:
                    return MIME_TYPE;
                case HTMLAttributes.WIDTH:
                    if (Double.IsNaN(Width))
                        return null;
                    return Width.ToString();
                case HTMLAttributes.HEIGHT:
                    if (Double.IsNaN(Height))
                        return null;
                    return Height.ToString();
                default:
                    return base.GetHTMLAttribute(context, name);
            }
        }

        public override void SetHTMLAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case HTMLAttributes.DATA:
                    Filename = value;
                    break;
                case HTMLAttributes.TYPE:
                    break;
                case HTMLAttributes.WIDTH:
                    Width = SafeConvert.ToDouble(value, Double.NaN);
                    break;
                case HTMLAttributes.HEIGHT:
                    Height = SafeConvert.ToDouble(value, Double.NaN);
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
                case HTMLAttributes.DATA:
                    Filename = null;
                    break;
                case HTMLAttributes.TYPE:
                    break;
                case HTMLAttributes.WIDTH:
                    Width = Double.NaN;
                    break;
                case HTMLAttributes.HEIGHT:
                    Height = Double.NaN;
                    break;
                default:
                    base.ResetHTMLAttribute(context, name);
                    break;
            }
        }

        public override object CreateHTMLNode(NodeContext newNode)
        {
            return CreateSVGNode(newNode);
        }

        public override IEnumerable<object> GetHTMLChildNodes(NodeContext context)
        {
            var buffer = new StringBuilder();

            foreach (var drawing in Drawings)
            {
                buffer.Append(drawing.Text);
            }

            yield return context.OwnerDocument.CreateTextNode(buffer.ToString());
        }

        public override void AppendHTMLNode(NodeContext context, object newChild)
        {
            AppendSVGNode(context, newChild);
        }

        #endregion

        #region SVG

        DOM.Windows.Window _Window;
        SVGDocument _Document;
        SVGDefsElement _Defs;

        public DOM.Windows.Window SVGWindow
        {
            get
            {
                if (_Window == null)
                {
                    _Window = new DOM.Windows.Internal.WPF.WindowBase((DOM.Views.DocumentView)SVGDocument);
                }
                return _Window;
            }
        }

        public SVGDocument SVGDocument
        {
            get
            {
                if (_Document == null)
                {
                    _Document = DOMFactory.CreateSVGDocument();
                    _Document.RootElement.Bind(this);
                }
                return _Document;
            }
            set
            {
                if (value != _Document)
                {
                    _Window = null;
                    _JSEngine = null;
                    _Document = value;
                    _Document.RootElement.Bind(this, render: true);
                }
            }
        }

        string _SVGNodeName = SVGElements.NAMESPACE + ' ' + SVGElements.SVG;

        public override string GetSVGNodeName(NodeContext context)
        {
            return _SVGNodeName;
        }

        public override object GetSVGParentNode(NodeContext context)
        {
            if (context.OwnerDocument == SVGDocument)
            {
                return SVGDocument;
            }
            else
            {
                return context.OwnerDocument.DocumentElement;
            }
        }

        public override IEnumerable<object> GetSVGChildNodes(NodeContext context)
        {
            if (_Defs != null)
            {
                yield return _Defs;
            }

            foreach (var drawing in Drawings)
            {
                yield return drawing;
            }
        }

        public override object CreateSVGNode(NodeContext context)
        {
            Element newElement = context as Element;
            if (newElement == null)
            {
                return null;
            }

            string nodeName = newElement.LocalName ?? newElement.NodeName;

            if (nodeName == SVGElements.SCRIPT || nodeName == SVGElements.DEFS)
            {
                return newElement;
            }

            Shape result = null;

            if (newElement.HasAttribute(SVGAttributes.CLASS))
            {
                string className = newElement.GetAttribute(SVGAttributes.CLASS);
                result = Shape.Create(className);
            }

            if (result == null)
            {
                result = Shape.Create(nodeName);
            }

            if (result != null)
            {
                result.Canvas = (NCanvas)this;
            }

            return result;
        }

        public override void AppendSVGNode(NodeContext context, object newChild)
        {
            if (newChild is SVGScriptElement)
            {
                EvaluateScript((SVGScriptElement)newChild);
            }
            else if (newChild is SVGDefsElement)
            {
                _Defs = (SVGDefsElement)newChild;
            }
            else
            {
                Drawings.Add((Shape)newChild);
            }
        }

        public override void InsertSVGNode(NodeContext context, object newChild, object refChild)
        {
            if (newChild is SVGScriptElement)
            {
                EvaluateScript((SVGScriptElement)newChild);
            }
            else if (newChild is SVGDefsElement)
            {
                _Defs = (SVGDefsElement)newChild;
            }
            else
            {
                int index = Drawings.IndexOf((Shape)refChild);
                if (index != -1)
                {
                    Drawings.Insert(index, (Shape)newChild);
                }
                else
                {
                    throw new DOMException(DOMException.NOT_FOUND_ERR);
                }
            }
        }

        public override void RemoveSVGNode(NodeContext context, object oldChild)
        {
            if (oldChild is SVGDefsElement)
            {
                _Defs = null;
            }
            else
            {
                Drawings.Remove((Shape)oldChild);
            }
        }

        string[] _SVGAttributes = new string[] {
                SVGAttributes.VERSION,
                SVGAttributes.WIDTH,
                SVGAttributes.HEIGHT
            };

        public override IList<string> GetSVGAttributes(ElementContext context)
        {
            return _SVGAttributes;
        }

        public override string GetSVGAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.VERSION:
                    return "1.1";
                case SVGAttributes.WIDTH:
                    if (!Double.IsNaN(Width))
                        return SafeConvert.ToString(Width);
                    else
                        return null;
                case SVGAttributes.HEIGHT:
                    if (!Double.IsNaN(Height))
                        return SafeConvert.ToString(Height);
                    else
                        return null;
                default:
                    return base.GetSVGAttribute(context, name);
            }
        }

        public override void SetSVGAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.VERSION:
                    break;
                case SVGAttributes.WIDTH:
                    Width = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.HEIGHT:
                    Height = SafeConvert.ToDouble(value);
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
                case SVGAttributes.VERSION:
                    break;
                case SVGAttributes.WIDTH:
                    Width = Double.NaN;
                    break;
                case SVGAttributes.HEIGHT:
                    Height = Double.NaN;
                    break;
                default:
                    base.ResetSVGAttribute(context, name);
                    break;
            }
        }


        public object GetDefinition(string id)
        {
            if (id.StartsWith("url"))
            {
                id = id.Substring("url".Length);
                id = id.TrimStart('(').TrimEnd(')');
            }
            id = id.TrimStart('#');

            SVGElement element = (SVGElement)_Document.GetElementById(id);
            if (element == null)
            {
                return null;
            }

            element = (SVGElement)element.CloneNode(true);
            element.RemoveAttribute(SVGAttributes.ID);

            switch (element.LocalName)
            {
                case SVGElements.LINEAR_GRADIENT:
                    return SVGConverter.ToLinearGradientBrush((SVGLinearGradientElement)element);
                case SVGElements.FILTER:
                    return SVGConverter.ToEffect((SVGFilterElement)element);
                default:
                    string className = element.GetAttribute(SVGAttributes.CLASS);
                    Shape result = Shape.Create(className);
                    if (result == null)
                    {
                        result = Shape.Create(element.LocalName);
                    }
                    if (result != null)
                    {
                        element.Bind(result, render: true);
                    }
                    return result;
            }
        }

        public string GetDefinitionId(object resource)
        {
            SVGElement element;

            if (resource is LinearGradientBrush)
            {
                element = SVGConverter.ToLinearGradientElement((LinearGradientBrush)resource, _Document);
            }
            else if (resource is Effect)
            {
                element = SVGConverter.ToFilterElement((Effect)resource, _Document);
            }
            else if (resource is Shape)
            {
                element = (SVGElement)_Document.GetNode((Shape)resource).CloneNode(true);
            }
            else
            {
                return "";
            }

            string id = SVGHelper.GetDefinitionId(_Document, element);
            if (String.IsNullOrEmpty(id))
            {
                return SVGHelper.AddDefinition(_Document, element);
            }
            return id;
        }

        public string GetDefinitionURL(object resource)
        {
            string id = GetDefinitionId(resource);

            if (!String.IsNullOrEmpty(id))
            {
                return "url(#" + id + ")";
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Javascript

        public ScriptEngine JSEngine
        {
            get
            {
                if (_JSEngine == null)
                {
                    _JSEngine = DOM.Javascript.JSFactory.CreateEngine(SVGWindow);
                }
                return _JSEngine;
            }
            set
            {
                _JSEngine = value;
            }
        }

        public void EvaluateScript(SVGScriptElement script)
        {
            if (String.IsNullOrWhiteSpace(script.Type) ||
                script.Type.ToLower() == "application/ecmascript" ||
                script.Type.ToLower() == "text/javascript")
            {
                EvaluateJavascript(script.TextContent);
            }
        }

        public object EvaluateJavascript(string code)
        {
            try
            {
                return JSEngine.Evaluate(code);
            }
            catch (Exception e)
            {
                Debug.Fail("Error: " + e.Message + "\n\n" + e.StackTrace);
                return null;
            }
        }

        #endregion

        #region Implementation

        #region Drawings

        private void Drawings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItems = e.OldItems;
            var newItems = e.NewItems;

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                oldItems = _InternalDrawings.Except(Drawings).ToArray();
                newItems = Drawings.Except(_InternalDrawings).ToArray();
            }

            if (oldItems != null)
            {
                OnDrawingsRemoved(oldItems);
            }

            if (newItems != null)
            {
                OnDrawingsAdded(newItems);
            }
        }

        private void OnDrawingsRemoved(IEnumerable removedDrawings)
        {
            foreach (Shape drawing in removedDrawings)
            {
                int index = _InternalDrawings.IndexOf(drawing);
                _InternalDrawings.RemoveAt(index);
                OnDrawingRemoved(index, drawing);
            }
        }

        private void OnDrawingsAdded(IEnumerable addedDrawings)
        {
            foreach (Shape drawing in addedDrawings)
            {
                int index = Drawings.IndexOf(drawing);
                _InternalDrawings.Insert(index, drawing);
                OnDrawingAdded(index, drawing);
            }
        }

        protected virtual void OnDrawingAdded(int index, Shape drawing)
        {
            Debug.Assert(index >= 0 && index <= _Visuals.Count);
            index = Math.Max(0, Math.Min(_Visuals.Count, index));

            drawing.Canvas = (NCanvas)this;

            _Visuals.Insert(index, drawing);

            if (UndoStack != null)
            {
                UndoStack.Push(() => Drawings.Remove(drawing));
            }
        }

        protected virtual void OnDrawingRemoved(int index, Shape drawing)
        {
            _Visuals.Remove(drawing);

            drawing.Canvas = null;

            if (UndoStack != null)
            {
                UndoStack.Push(() => Drawings.Insert(index, drawing));
            }
        }

        #endregion

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size(0, 0);

            if (!Double.IsNaN(Width))
            {
                desiredSize.Width = Width;
            }
            else if (availableSize.Width != Double.PositiveInfinity)
            {
                desiredSize.Width = availableSize.Width;
            }
            else
            {
                foreach (Shape drawing in Drawings)
                {
                    desiredSize.Width = Math.Max(desiredSize.Width, drawing.RenderedBounds.Right);
                }
            }

            if (!Double.IsNaN(Height))
            {
                desiredSize.Height = Height;
            }
            else if (availableSize.Height != Double.PositiveInfinity)
            {
                desiredSize.Height = availableSize.Height;
            }
            else
            {
                foreach (Shape drawing in Drawings)
                {
                    desiredSize.Height = Math.Max(desiredSize.Height, drawing.RenderedBounds.Bottom);
                }
            }

            return desiredSize;
        }

        #endregion

        #region Rendering

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            Drawings.ForEach(drawing => drawing.Redraw());

            if (NeedsResource)
            {
                RaiseResourceRequested(Filename);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            UpdateRenderTransform();
        }

        #endregion

        #region Visual Children

        protected override int VisualChildrenCount
        {
            get { return _Visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _Visuals[index];
        }

        #endregion

        static string RandomFileName()
        {
            string fileName = System.IO.Path.GetRandomFileName();
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            fileName += ".svg";
            return fileName;
        }

        Rect ActualBounds
        {
            get
            {
                return new Rect(Left, Top, Width, Height);
            }
            set
            {
                using (UndoScope undo = new UndoScope(UndoStack))
                {
                    Vector delta = value.Location - new Point(0, 0);
                    Position += delta;
                    Width = value.Width;
                    Height = value.Height;

                    foreach (Shape drawing in Drawings)
                    {
                        MoveDrawing(drawing, -delta);
                    }
                }
            }
        }

        private void MoveDrawing(Shape drawing, Vector delta)
        {
            Point savedPosition = drawing.RenderedBounds.Location;

            drawing.Translate(delta);

            if (UndoStack != null)
            {
                UndoStack.Push(delegate()
                {
                    Vector undoDelta = savedPosition - drawing.RenderedBounds.Location;
                    MoveDrawing(drawing, undoDelta);
                });
            }
        }

        Transform ViewBoxTransform
        {
            get
            {
                if (_ViewBox.IsEmpty ||
                    Double.IsNaN(Width) || Double.IsNaN(Height) ||
                    Width == 0 || Height == 0)
                {
                    return null;
                }

                double translateX = 0;
                double scaleCenterX = 0;

                if (Align == ViewBoxAlignment.None)
                {
                    translateX = -ViewBox.X;
                }
                else if (Align == ViewBoxAlignment.xMinYMax ||
                         Align == ViewBoxAlignment.xMinYMid ||
                         Align == ViewBoxAlignment.xMinYMin)
                {
                    translateX = -ViewBox.X;
                    scaleCenterX = 0;
                }
                else if (Align == ViewBoxAlignment.xMidYMax ||
                         Align == ViewBoxAlignment.xMidYMid ||
                         Align == ViewBoxAlignment.xMidYMin)
                {
                    translateX = Width / 2 - (ViewBox.Left + ViewBox.Right) / 2;
                    scaleCenterX = Width / 2;
                }
                else if (Align == ViewBoxAlignment.xMaxYMax ||
                         Align == ViewBoxAlignment.xMaxYMid ||
                         Align == ViewBoxAlignment.xMaxYMin)
                {
                    translateX = Width - ViewBox.Right;
                    scaleCenterX = Width;
                }

                double translateY = 0;
                double scaleCenterY = 0;

                if (Align == ViewBoxAlignment.None)
                {
                    translateY = -ViewBox.Y;
                }
                else if (Align == ViewBoxAlignment.xMinYMin ||
                         Align == ViewBoxAlignment.xMidYMin ||
                         Align == ViewBoxAlignment.xMaxYMin)
                {
                    translateY = -ViewBox.Y;
                    scaleCenterY = 0;
                }
                else if (Align == ViewBoxAlignment.xMinYMid ||
                         Align == ViewBoxAlignment.xMidYMid ||
                         Align == ViewBoxAlignment.xMaxYMid)
                {
                    translateY = Height / 2 - (ViewBox.Top + ViewBox.Bottom) / 2;
                    scaleCenterY = Height / 2;
                }
                else if (Align == ViewBoxAlignment.xMinYMax ||
                         Align == ViewBoxAlignment.xMidYMax ||
                         Align == ViewBoxAlignment.xMaxYMax)
                {
                    translateY = Height - ViewBox.Bottom;
                    scaleCenterY = Height;
                }

                double scaleX = Width / ViewBox.Width;
                double scaleY = Height / ViewBox.Height;

                if (Align != ViewBoxAlignment.None)
                {
                    if ((MeetOrSlice == ViewBoxFit.Meet) && scaleX <= scaleY ||
                        (MeetOrSlice == ViewBoxFit.Slice) && scaleY <= scaleX)
                    {
                        scaleY = scaleX;
                        translateX = -ViewBox.X;
                        scaleCenterX = 0;
                    }
                    else
                    {
                        scaleX = scaleY;
                        translateY = -ViewBox.Y;
                        scaleCenterY = 0;
                    }
                }

                Matrix transform = new Matrix();
                transform.Translate(translateX, translateY);
                transform.ScaleAt(scaleX, scaleY, scaleCenterX, scaleCenterY);
                return new MatrixTransform(transform);
            }
        }

        void UpdateRenderTransform()
        {
            if (!ViewBox.IsEmpty)
            {
                RenderTransform = ViewBoxTransform;
            }
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new DocumentCanvas(this);
        }

        #endregion

    }
}
