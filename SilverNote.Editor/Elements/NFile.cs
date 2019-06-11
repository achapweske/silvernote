/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DOM;
using DOM.HTML;
using DOM.CSS;
using SilverNote.Common;

namespace SilverNote.Editor
{
    public class NFile : DocumentElement, ICloneable
    {
        #region Constructor

        public NFile()
        {
            Initialize();
        }

        public NFile(string filePath)
        {
            Initialize();

            FileName = System.IO.Path.GetFileName(filePath);
            Data = File.ReadAllBytes(filePath);
        }

        public NFile(NFile copy)
            : base(copy)
        {
            Initialize();

            FileName = copy.FileName;

            if (copy.Data != null)
            {
                Data = (byte[])copy.Data.Clone();
            }
        }

        void Initialize()
        {
            Cursor = Cursors.Arrow;
            Focusable = true;
            Positioning = Positioning.Overlapped;

            var panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;

            _Icon = new Image();
            _Icon.Width = SystemParameters.IconWidth;
            _Icon.Height = SystemParameters.IconHeight;
            _Icon.HorizontalAlignment = HorizontalAlignment.Center;
            _Icon.Source = DefaultIcon;
            panel.Children.Add(_Icon);

            _FileName = new TextBlock();
            _FileName.Margin = new Thickness(0, 4, 0, 0);
            _FileName.Padding = new Thickness(1, 1, 2, 2);
            _FileName.HorizontalAlignment = HorizontalAlignment.Center;
            _FileName.MaxWidth = SystemParameters.IconGridWidth;
            _FileName.TextAlignment = TextAlignment.Center;
            _FileName.Background = Brushes.Transparent;
            _FileName.FontFamily = SystemFonts.IconFontFamily;
            _FileName.FontSize = SystemFonts.IconFontSize;
            _FileName.FontStyle = SystemFonts.IconFontStyle;
            _FileName.TextDecorations = SystemFonts.IconFontTextDecorations;
            _FileName.FontWeight = SystemFonts.IconFontWeight;
            _FileName.TextWrapping = SystemParameters.IconTitleWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
            _FileName.TextTrimming = TextTrimming.CharacterEllipsis;
            
            panel.Children.Add(_FileName);
            panel.Margin = new Thickness(5);
            var grid = new Grid();
            grid.Children.Add(panel);

            Content = grid;
            Children = new VisualCollection(this);
            Children.Add(Content);

            // Context menu

            var contextMenu = new ContextMenu();

            // Open
            var menuItem = new MenuItem();
            TextBlock.SetFontWeight(menuItem, FontWeights.Bold);
            menuItem.Header = "Open";
            menuItem.Click += Open_Click;
            contextMenu.Items.Add(menuItem);

            // Save As...
            menuItem = new MenuItem();
            menuItem.Header = "Save As...";
            menuItem.Click += SaveAs_Click;
            contextMenu.Items.Add(menuItem);

            ContextMenu = contextMenu;
        }

        #endregion

        #region Properties

        private string _Url;

        public string Url
        {
            get { return _Url ?? (_Url = Uri.EscapeUriString(FileName)); }
            set { _Url = value; }
        }

        private TextBlock _FileName;

        public string FileName
        {
            get 
            {
                return _FileName.Text; 
            }
            set 
            { 
                _FileName.Text = value;

                UpdateIcon();
            }
        }

        private byte[] _Data = null;

        public byte[] Data
        {
            get 
            {
                try
                {
                    if (_TempFilePath != null && File.Exists(_TempFilePath))
                    {
                        return File.ReadAllBytes(_TempFilePath);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                }

                return _Data; 
            }
            set
            {
                try
                {
                    if (_TempFilePath != null)
                    {
                        File.WriteAllBytes(_TempFilePath, value);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                }

                _Data = value;
                Url = null;

                UpdateIcon();
            }
        }

        private Image _Icon;

        public BitmapSource Icon
        {
            get 
            {
                return _Icon.Source as BitmapSource; 
            }
            set
            {
                _Icon.Source = value;
                InvalidateVisual();
            }
        }

        private void UpdateIcon()
        {
            if (FileName != null && Data != null)
            {
                Icon = GetIcon(FileName, Data);
            }
            else
            {
                Icon = DefaultIcon;
            }
        }

        static BitmapSource GetIcon(string fileName, byte[] data)
        {
            string tmpPath = Path.GetTempPath();
            tmpPath = Path.Combine(tmpPath, Path.GetRandomFileName());
            tmpPath = Path.ChangeExtension(tmpPath, Path.GetExtension(fileName));

            File.WriteAllBytes(tmpPath, data);

            using (var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(tmpPath))
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    sysicon.Handle,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()
                );
            }
        }

        static BitmapSource _DefaultIcon;

        static BitmapSource DefaultIcon
        {
            get
            {
                if (_DefaultIcon == null)
                {
                    _DefaultIcon = LoadDefaultIcon();
                }

                return _DefaultIcon;
            }
        }

        static BitmapSource LoadDefaultIcon()
        {
            Uri uri = new Uri("pack://application:,,,/SilverNote;component/Images/unassoc.ico");
            return new BitmapImage(uri);
        }

        #endregion

        #region Operations

        public void SaveFile(string filePath = null)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = this.FileName;
            dialog.Filter = "All Files (*.*)|*.*";
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            filePath = dialog.FileName;

            File.WriteAllBytes(filePath, Data);
        }

        private string _TempFilePath;

        public void OpenFile()
        {
            try
            {
                if (_TempFilePath != null)
                {
                    var viewer = (from process in Process.GetProcesses()
                                  where !String.IsNullOrEmpty(process.StartInfo.FileName)
                                  select process).FirstOrDefault();
                    if (viewer != null)
                    {
                        SwitchToThisWindow(viewer.MainWindowHandle, true);
                        return;
                    }
                }

                if (_TempFilePath == null)
                {
                    string dirPath = Path.GetTempPath();
                    dirPath = Path.Combine(dirPath, Path.GetRandomFileName());
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    _TempFilePath = Path.Combine(dirPath, FileName);
                }

                File.WriteAllBytes(_TempFilePath, Data);

                var newProcess = Process.Start(_TempFilePath);
                if (newProcess != null)
                {
                    newProcess.Exited += Process_Exited;
                    newProcess.EnableRaisingEvents = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            // This method is called from a non-UI thread

            Dispatcher.BeginInvoke(new Action(() =>
                {
                    ((Process)sender).Dispose();

                    try
                    {
                        _Data = File.ReadAllBytes(_TempFilePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + "\n\n" + ex.StackTrace);
                    }

                    _TempFilePath = null;
                }), null);
        }

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        #endregion

        #region IEditable

        public override IList<object> Copy()
        {
            return new object[] { Clone() };
        }

        #endregion

        #region HTML

        public const string MIME_TYPE = "application/silvernote.file";

        public override string GetHTMLNodeName(NodeContext context)
        {
            return HTMLElements.OBJECT;
        }

        private string[] _Attributes = new[] 
        {
            HTMLAttributes.TYPE,
            HTMLAttributes.DATA,
            HTMLAttributes.WIDTH,
            HTMLAttributes.HEIGHT
        };

        public override IList<string> GetHTMLAttributes(ElementContext context)
        {
            return _Attributes;
        }

        public override string GetHTMLAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case HTMLAttributes.TYPE:
                    return MIME_TYPE;
                case HTMLAttributes.DATA:
                    return Url;
                case HTMLAttributes.WIDTH:
                    return RenderSize.Width.ToString();
                case HTMLAttributes.HEIGHT:
                    return RenderSize.Height.ToString();
                default:
                    return base.GetHTMLAttribute(context, name);
            }
        }

        public override void SetHTMLAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case HTMLAttributes.TYPE:
                    break;
                case HTMLAttributes.DATA:
                    FileName = UriHelper.Segments(value).LastOrDefault() ?? value;
                    FileName = Uri.UnescapeDataString(FileName);
                    break;
                case HTMLAttributes.WIDTH:
                    break;
                case HTMLAttributes.HEIGHT:
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
                case HTMLAttributes.TYPE:
                    break;
                case HTMLAttributes.DATA:
                    Url = null;
                    break;
                case HTMLAttributes.WIDTH:
                    break;
                case HTMLAttributes.HEIGHT:
                    break;
                default:
                    base.ResetHTMLAttribute(context, name);
                    break;
            }
        }

        #endregion

        #region IStyleable

        public override CSSValue GetStyleProperty(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.Width:
                case CSSProperties.Height:
                    return CSSValues.Auto;
                default:
                    return base.GetStyleProperty(context, name);
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

        #region ISelectable

        public override void Select()
        {
            base.Select();

            _FileName.Foreground = SystemColors.HighlightTextBrush;
            _FileName.Background = SystemColors.HighlightBrush;
        }

        public override void Unselect()
        {
            base.Unselect();

            _FileName.Foreground = Brushes.Black;
            _FileName.Background = Brushes.Transparent;
        }

        #endregion

        #region Implementation

        public static readonly new DependencyProperty BackgroundProperty = DocumentElement.BackgroundProperty.AddOwner(
            typeof(NFile),
            new FrameworkPropertyMetadata(Brushes.Transparent)
        );

        private bool _IsPlacing = false;

        public bool IsPlacing
        {
            get { return _IsPlacing; }
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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsPlacing)
            {
                IsPlacing = false;
            }

            Focus();

            if (e.ClickCount == 1)
            {
                DoMoveStarted(e.GetPosition(this));
            }
            else
            {
                OpenFile();
            }

            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            DoMoveCompleted();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsPlacing)
            {
                Point point = e.GetPosition(DocumentPanel.FindPanel(this));
                DocumentPanel.SetLeft(this, point.X - RenderSize.Width / 2);
                DocumentPanel.SetTop(this, point.Y - RenderSize.Height / 2);

                e.Handled = true;
            }

            if (IsMoving)
            {
                DoMoveDelta(e.GetPosition(this));
            }
        }

        void Open_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        void SaveAs_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private UIElement Content { get; set; }

        VisualCollection Children;

        protected override int VisualChildrenCount
        {
            get { return Children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Children[index];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Content.Measure(availableSize);

            return Content.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Content.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

            return finalSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Data == null && Url != null)
            {
                RaiseResourceRequested(Url);
            }

            base.OnRender(dc);
        }

        private Point MoveStartPosition { get; set; }

        private bool IsMoving { get; set; }

        private void DoMoveStarted(Point startPosition)
        {
            MoveStartPosition = startPosition;
            RequestBeginMove();
            IsMoving = true;
            CaptureMouse();
        }

        private void DoMoveDelta(Point currentPosition)
        {
            Vector delta = currentPosition - MoveStartPosition;
            RequestMoveDelta(delta);
        }

        private void DoMoveCompleted()
        {
            MoveStartPosition = default(Point);
            RequestEndMove();
            IsMoving = false;
            ReleaseMouseCapture();
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new NFile(this);
        }

        #endregion
    }

}
