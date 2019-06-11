/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using SilverNote.Common;
using SilverNote.Dialogs;
using DOM;
using DOM.HTML;
using DOM.CSS;
using DOM.Helpers;
using System.Globalization;

namespace SilverNote.Editor
{
    public class NImage : DocumentElement, ICloneable
    {
        #region Fields

        string _Url;
        string _Type;   // e.g. ".png"
        byte[] _Data;
        BitmapSource _Source;
        bool _PreserveAspectRatio;

        #endregion

        #region Constructors

        public NImage()
        {
            Initialize();
        }

        public NImage(NImage other)
            : base(other)
        {
            Initialize();

            Type = other.Type;

            if (other.Data != null)
            {
                Data = (byte[])other.Data.Clone();
            }

            _PreserveAspectRatio = other.PreserveAspectRatio;
        }

        void Initialize()
        {
            Type = ".png";
            Focusable = true;
            Cursor = Cursors.SizeAll;
            SelectionAdorner = new ImageAdorner(this);
            RequestBringIntoView += NImage_RequestBringIntoView;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Image type (e.g. ".png")
        /// </summary>
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        public byte[] Data
        {
            get 
            {
                if (_Data == null && Source != null)
                {
                    _Data = Encode(Type, Source);
                }
                return _Data; 
            }
            set
            {
                if (value != _Data)
                {
                    byte[] oldData = _Data;
                    _Data = value;
                    OnDataChanged(oldData, value);
                }
            }
        }

        public BitmapSource Source
        {
            get 
            { 
                return _Source; 
            }
            set
            {
                if (value != _Source)
                {
                    BitmapSource oldSource = _Source;
                    _Source = value;
                    OnSourceChanged(oldSource, value);
                }
            }
        }

        public double OriginalWidth
        {
            get { return (Source != null) ? Source.Width : 0; }
        }

        public double OriginalHeight
        {
            get { return (Source != null) ? Source.Height : 0; }
        }

        public double AspectRatio { get; set; }

        public bool PreserveAspectRatio
        {
            get 
            { 
                return _PreserveAspectRatio; 
            }
            set
            {
                if (value != _PreserveAspectRatio)
                {
                    _PreserveAspectRatio = value;

                    if (UndoStack != null)
                    {
                        UndoStack.Edit();
                    }
                }
            }
        }

        public string Url
        {
            get { return _Url ?? (_Url = GenerateUrl(Type)); }
            set { _Url = value; }
        }

        #endregion

        #region Operations

        List<EmbeddedFile> _OpenInstances = new List<EmbeddedFile>();

        public void OpenImage()
        {
            foreach (var instance in _OpenInstances)
            {
                if (instance.IsOpen)
                {
                    instance.Open();
                    return;
                }
            }

            var newInstance = new EmbeddedFile(Type, Data);

            newInstance.Changed += File_Changed;

            if (newInstance.Open())
            {
                _OpenInstances.Add(newInstance);
            }
            else
            {
                newInstance.Changed -= File_Changed;
            }
        }

        public bool CanEditImage
        {
            get
            {
                return FileAssociation.GetAssociations(Type, "edit").Any();
            }
        }

        public void EditImage()
        {
            var editWith = FileAssociation.GetAssociations(Type, "edit");
            if (editWith.Count > 0)
            {
                OpenImageWith(editWith.First().AppCommand);
            }
        }

        public void OpenImageWith(string command)
        {
            var newInstance = new EmbeddedFile(Type, Data);

            newInstance.Changed += File_Changed;

            if (newInstance.OpenWith(command))
            {
                _OpenInstances.Add(newInstance);
            }
            else
            {
                newInstance.Changed -= File_Changed;
            }
        }

        public void ResetSize()
        {
            if (!Double.IsNaN(Width) || !Double.IsNaN(Height))
            {
                Width = Double.NaN;
                Height = Double.NaN;

                if (UndoStack != null)
                {
                    UndoStack.Edit();
                }
            }
        }

        private void File_Changed(object sender, EventArgs e)
        {
            var instance = (EmbeddedFile)sender;

            try
            {
                Data = instance.Data;
                ResetSize();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n\n" + ex.StackTrace);
            }

            if (UndoStack != null)
            {
                UndoStack.Edit();
            }
        }

        public void SaveImage()
        {
            try
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "PNG (.png) - Recommended|*.png|BMP (.bmp)|*.bmp|GIF (.gif)|*.gif|JPEG (.jpg)|*.jpg|TIFF (.tiff)|*.tiff|SVG (.svg)|*.svg";
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                string fileName = dialog.FileName;
                string fileType = Path.GetExtension(fileName).ToLower();

                byte[] data;
                if (fileType == Type && Data != null)
                {
                    data = Data;
                }
                else
                {
                    data = Encode(fileType, Source);
                }

                File.WriteAllBytes(fileName, data);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }
        }

        #endregion

        #region IMovable

        Point MoveStartPosition { get; set; }

        bool IsMoving { get; set; }

        void DoMoveStarted(Point startPosition)
        {
            MoveStartPosition = startPosition;
            RequestBeginMove();
            IsMoving = true;
            CaptureMouse();
        }

        void DoMoveDelta(Point currentPosition)
        {
            Vector delta = currentPosition - MoveStartPosition;
            RequestMoveDelta(delta);
        }

        void DoMoveCompleted()
        {
            MoveStartPosition = default(Point);
            RequestEndMove();
            IsMoving = false;
            ReleaseMouseCapture();
        }

        #endregion

        #region IResizable

        public override void Resize(Vector delta)
        {
            if (PreserveAspectRatio && !Double.IsNaN(AspectRatio) && AspectRatio != 0)
            {
                delta = new Vector(delta.X, delta.X / AspectRatio);
            }

            base.Resize(delta);
        }

        protected override double ComputedWidth
        {
            get
            {
                if (!double.IsNaN(Width))
                {
                    return Width;
                }
                else
                {
                    return OriginalWidth;
                }
            }
        }

        protected override double ComputedHeight
        {
            get
            {
                if (!double.IsNaN(Height))
                {
                    return Height;
                }
                else
                {
                    return OriginalHeight;
                }
            }
        }

        #endregion

        #region IEditable

        public override IList<object> Copy()
        {
            return new object[] { Clone() };
        }

        #endregion

        #region HTML

        public override string GetHTMLNodeName(NodeContext context)
        {
            return HTMLElements.IMG;
        }

        private string[] _Attributes = new[] 
        { 
            HTMLAttributes.CLASS,
            HTMLAttributes.WIDTH,
            HTMLAttributes.HEIGHT,
            HTMLAttributes.SRC
        };

        public override IList<string> GetHTMLAttributes(ElementContext context)
        {
            return _Attributes;
        }

        public override string GetHTMLAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.CLASS:
                    return PreserveAspectRatio ? "preserveAspectRatio" : null;
                case HTMLAttributes.WIDTH:
                    return ComputedWidth.ToString(CultureInfo.InvariantCulture);
                case HTMLAttributes.HEIGHT:
                    return ComputedHeight.ToString(CultureInfo.InvariantCulture);
                case HTMLAttributes.SRC:
                    return Url;
                default:
                    return base.GetHTMLAttribute(context, name);
            }
        }

        public override void SetHTMLAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case HTMLAttributes.CLASS:
                    PreserveAspectRatio = DOMHelper.HasClass(value, "preserveAspectRatio");
                    break;
                case HTMLAttributes.WIDTH:
                    Width = SafeConvert.ToDouble(value, Double.NaN);
                    break;
                case HTMLAttributes.HEIGHT:
                    Height = SafeConvert.ToDouble(value, Double.NaN);
                    break;
                case HTMLAttributes.SRC:
                    Url = value;
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
                case HTMLAttributes.CLASS:
                    PreserveAspectRatio = false;
                    break;
                case HTMLAttributes.WIDTH:
                    Width = OriginalWidth;
                    break;
                case HTMLAttributes.HEIGHT:
                    Height = OriginalHeight;
                    break;
                case HTMLAttributes.SRC:
                    Url = null;
                    break;
                default:
                    base.ResetHTMLAttribute(context, name);
                    break;
            }
        }

        public override void SetHTMLStyle(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.Width:
                    if (!Double.IsNaN(Width))
                    {
                        var width = (CSSPrimitiveValue)value;
                        if (width.IsLength())
                        {
                            Width = width.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;
                case CSSProperties.Height:
                    if (!Double.IsNaN(Height))
                    {
                        var height = (CSSPrimitiveValue)value;
                        if (height.IsLength())
                        {
                            Height = height.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;
                default:
                    base.SetHTMLStyle(context, name, value);
                    break;
            }
        }

        public override CSSValue GetHTMLStyle(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.Width:
                case CSSProperties.Height:
                    return CSSValues.Auto;
                default:
                    return base.GetHTMLStyle(context, name);
            }
        }

        #endregion

        #region IHasResources

        public override IEnumerable<string> ResourceNames
        {
            get { yield return Url; }
        }

        protected override void OnGetResource(string url, Stream stream)
        {
            if (url == Url && Data != null)
            {
                stream.Write(Data, 0, Data.Length);
            }
        }

        protected override void OnSetResource(string url, Stream stream)
        {
            if (url == Url)
            {
                int offset = 0;
                var result = new byte[stream.Length];
                while (offset < result.Length)
                {
                    int count = stream.Read(result, offset, result.Length - offset);
                    if (count == 0)
                        break;
                    offset += count;
                }
                Data = result;
            }
        }

        #endregion

        #region Implementation

        private string GenerateUrl(string extension)
        {
            var result = System.IO.Path.GetRandomFileName();
            return System.IO.Path.ChangeExtension(result, extension);
        }

        protected void OnDataChanged(byte[] oldValue, byte[] newValue)
        {
            if (newValue != null)
            {
                Source = Decode(newValue, out _Type);
            }
            else
            {
                Source = null;
            }

            Url = null;
        }

        protected void OnSourceChanged(BitmapSource oldValue, BitmapSource newValue)
        {
            if (newValue != null)
            {
                AspectRatio = newValue.Width / newValue.Height;
            }
            else
            {
                AspectRatio = Double.NaN;
            }

            _Data = null;

            InvalidateMeasure();
            InvalidateVisual();
        }

        void NImage_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;   // Suppress scrolling
        }

        #region Encode/Decode

        public static byte[] Encode(string type, BitmapSource source)
        {
            try
            {
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
                    Debug.WriteLine("Error: File type \"" + type + "\" not supported");
                    return null;
                }

                var frame = BitmapFrame.Create(source);
                encoder.Frames.Add(frame);

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    return stream.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                return null;
            }
        }

        public static BitmapFrame Decode(byte[] data, out string type)
        {
            try
            {
                using (var stream = new MemoryStream(data))
                {
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    type = GetFileExtension(decoder.CodecInfo);
                    return decoder.Frames.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                type = String.Empty;
                return null;
            }
        }

        static string[] _SupportedDecoders = new string[] {
            ".bmp",
            ".png",
            ".gif",
            ".ico",
            ".jpg",
            ".jpeg",
            ".png",
            ".tiff",
            ".tif"
        };

        public static IEnumerable<string> SupportedDecoders
        {
            get
            {
                return _SupportedDecoders;
            }
        }

        static string GetFileExtension(BitmapCodecInfo codecInfo)
        {
            string[] extensions = codecInfo.FileExtensions.Split(',');

            if (extensions.Length > 0)
            {
                return extensions[0];
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Mouse Input

        bool _IsPlacing = false;

        public bool IsPlacing
        {
            get 
            { 
                return _IsPlacing; 
            }
            set
            {
                if (value != _IsPlacing)
                {
                    _IsPlacing = value;
                    OnIsPlacingChanged(value);
                }
            }
        }

        protected void OnIsPlacingChanged(bool newValue)
        {
            if (newValue)
            {
                Opacity = 0.75;
                DocumentPanel.SetPositioning(this, Positioning.Absolute);
                CaptureMouse();
            }
            else
            {
                ReleaseMouseCapture();
                DocumentPanel.SetPositioning(this, Positioning.Overlapped);
                Opacity = 1.0;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (!IsSelected)
            {
                AddAdorner(SelectionAdorner);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!IsSelected)
            {
                RemoveAdorner(SelectionAdorner);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsPlacing)
            {
                IsPlacing = false;
            }

            Focus();

            if (e.ClickCount == 1)
            {
                OnMouseClick(e);
            }
            else
            {
                OnMouseDoubleClick(e);
            }
        }

        protected void OnMouseClick(MouseButtonEventArgs e)
        {
            DoMoveStarted(e.GetPosition(this));

            e.Handled = true;
        }

        protected void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            OpenImage();

            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMoving)
            {
                DoMoveCompleted();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsPlacing)
            {
                Point point = e.GetPosition(DocumentPanel.FindPanel(this));
                point.X -= RenderSize.Width / 2;
                point.Y -= RenderSize.Height / 2;
                DocumentPanel.SetPosition(this, point);

                e.Handled = true;
            }

            if (IsMoving)
            {
                DoMoveDelta(e.GetPosition(this));
            }
        }

        #endregion

        #region Rendering

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(ComputedWidth, ComputedHeight);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Source != null)
            {
                dc.DrawImage(Source, new Rect(RenderSize));
            }
            else if (!String.IsNullOrEmpty(Url))
            {
                // Draw placeholder outline
                var pen = new Pen(Brushes.Gray, 1);
                pen.Freeze();
                dc.DrawRectangle(Brushes.Transparent, pen, new Rect(RenderSize));

                // Fetch the image
                RaiseResourceRequested(Url);
            }
        }

        #endregion

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new NImage(this);
        }

        #endregion
    }

    public class ImageAdorner : ResizingAdorner<NImage>
    {
        public ImageAdorner(NImage adornedElement)
            : base(adornedElement)
        {

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            MoverThumb = null;
        }
    }
}
