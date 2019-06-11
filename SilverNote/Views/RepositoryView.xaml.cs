/*
 * Copyright (c) Adam Chapweske
 * 
 * Use of this software is governed by an MIT license that can be found in the LICENSE file at the root of this project.
 */

using SilverNote.Common;
using SilverNote.Dialogs;
using SilverNote.Editor;
using SilverNote.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SilverNote.Properties;
using System.Windows.Controls.Primitives;
using SilverNote.Commands;
using System.IO;
using System.Diagnostics;
using SilverNote.Behaviors;
using System.Windows.Threading;

namespace SilverNote.Views
{
    /// <summary>
    /// Interaction logic for RepositoryView.xaml
    /// </summary>
    public partial class RepositoryView : Window
    {
        #region Constructors

        public RepositoryView()
        {
            InitializeComponent();
            DataContextChanged += DataContext_Changed;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the RepositoryViewModel for the current repository
        /// </summary>
        public RepositoryViewModel Repository
        {
            get { return DataContext as RepositoryViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        /// Associates NoteViews with NoteViewModels
        /// </summary>
        public NoteViewGenerator NoteViewGenerator
        {
            get { return TryFindResource("NoteViewGenerator") as NoteViewGenerator; }
        }

        /// <summary>
        /// Get a NoteView for the currently selected NoteViewModel
        /// </summary>
        public NoteView NoteView
        {
            get
            {
                var note = NotesTabControl.SelectedItem as NoteViewModel;
                if (note != null)
                {
                    return NoteViewGenerator[note];
                }
                else
                {
                    return null;
                }

            }
        }

        public WidgetManager WidgetManager { get; set; }

        #endregion

        #region Dependency Properties

        #region IsLeftPaneVisible

        public bool IsLeftPaneVisible
        {
            get { return (bool)base.GetValue(IsLeftPaneVisibleProperty); }
            set { base.SetValue(IsLeftPaneVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsLeftPaneVisibleProperty = DependencyProperty.Register(
            "IsLeftPaneVisible",
            typeof(bool),
            typeof(RepositoryView),
            new PropertyMetadata(IsLeftPaneVisible_Changed));

        static void IsLeftPaneVisible_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((RepositoryView)sender).IsLeftPaneVisible_Changed((bool)e.NewValue);
        }

        #endregion

        #region IsRightPaneVisible

        public bool IsRightPaneVisible
        {
            get { return (bool)base.GetValue(IsRightPaneVisibleProperty); }
            set { base.SetValue(IsRightPaneVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsRightPaneVisibleProperty = DependencyProperty.Register(
            "IsRightPaneVisible",
            typeof(bool),
            typeof(RepositoryView),
            new PropertyMetadata(IsRightPaneVisible_Changed));

        static void IsRightPaneVisible_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((RepositoryView)sender).IsRightPaneVisible_Changed((bool)e.NewValue);
        }

        #endregion

        #region IsDebugPaneVisible

        public bool IsDebugPaneVisible
        {
            get { return (bool)base.GetValue(IsDebugPaneVisibleProperty); }
            set { base.SetValue(IsDebugPaneVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsDebugPaneVisibleProperty = DependencyProperty.Register(
            "IsDebugPaneVisible",
            typeof(bool),
            typeof(RepositoryView),
            new PropertyMetadata(IsDebugPaneVisible_Changed));

        static void IsDebugPaneVisible_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((RepositoryView)sender).IsDebugPaneVisible_Changed((bool)e.NewValue);
        }

        #endregion

        #endregion

        #region Operations

        /// <summary>
        /// Save the selected drawing to a file
        /// </summary>
        public void SaveImage()
        {
            if (NoteView == null || NoteView.Editor == null)
            {
                return;
            }

            if (NoteView.Editor.DrawingBoard.Drawings.Count == 0 && !NoteView.Editor.Selection.OfType<NImage>().Any())
            {
                MessageBox.Show("To export a drawing you must first select the drawing to be exported.", "SilverNote", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (NoteView.Editor.DrawingBoard.Drawings.Count > 0)
            {
                // Save selected canvas

                var canvas = (NCanvas)NoteView.Editor.DrawingBoard.Clone();
                canvas.Selection.UnselectAll(); // hide handles
                canvas.SizeToContent();
                canvas.Save();
            }
            else
            {
                // Save selected image

                foreach (var image in NoteView.Editor.Selection.OfType<NImage>())
                {
                    image.SaveImage();
                }
            }
        }

        /// <summary>
        /// Save any selected embedded files to external files
        /// </summary>
        public void SaveFile()
        {
            if (NoteView == null || NoteView.Editor == null)
            {
                return;
            }

            foreach (var file in NoteView.Editor.Selection.OfType<NFile>())
            {
                file.SaveFile();
            }
        }

        /// <summary>
        /// Print the currently selected note
        /// </summary>
        /// <returns></returns>
        public bool Print()
        {
            if (NoteView == null || NoteView.Editor == null)
            {
                return false;
            }

            var paginator = NoteView.Editor.DocumentPaginator;

            try
            {
                PrintDialog dialog = new PrintDialog();
                if (dialog.ShowDialog() == true)
                {
                    var pageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
                    paginator.PageSize = pageSize;
                    dialog.PrintDocument(paginator, NoteView.Editor.Title);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Print failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Display a print preview for the currently selected note
        /// </summary>
        public void PrintPreview()
        {
            if (NoteView == null || NoteView.Editor == null)
            {
                return;
            }

            var paginator = NoteView.Editor.DocumentPaginator;

            PrintDialog print = new PrintDialog();
            var pageSize = new Size(print.PrintableAreaWidth, print.PrintableAreaHeight);
            paginator.PageSize = pageSize;

            // Paginate the note to an xps document and display the result in a PrintPreviewDialog

            using (var stream = new MemoryStream())
            {
                var uri = new Uri(@"memorystream://printpreview.xps");
                var package = System.IO.Packaging.Package.Open(stream, FileMode.Create, FileAccess.ReadWrite);
                System.IO.Packaging.PackageStore.AddPackage(uri, package);
                try
                {
                    var document = new System.Windows.Xps.Packaging.XpsDocument(package) { Uri = uri };
                    var writer = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(document);
                    writer.Write(paginator);

                    var preview = new PrintPreviewDialog
                    {
                        Owner = App.Current.MainWindow,
                        Document = document.GetFixedDocumentSequence()
                    };
                    preview.ShowDialog();
                }
                finally
                {
                    System.IO.Packaging.PackageStore.RemovePackage(uri);
                }
            }
        }

        /// <summary>
        /// Change the password for the current notebook/repository
        /// </summary>
        public void ChangePassword()
        {
            if (Repository.CanChangePassword)
            {
                var dialog = new ChangePasswordDialog();
                dialog.SetBinding(ChangePasswordDialog.HasPasswordProperty, new Binding 
                { 
                    Source = this, 
                    Path = new PropertyPath("Repository.HasPassword") 
                });

                if (dialog.ShowDialog() == true)
                {
                    Repository.ChangePassword(dialog.OldPassword, dialog.NewPassword);
                }
            }
        }

        /// <summary>
        /// Add the selected drawing to the clipart library
        /// </summary>
        public void AddToLibrary()
        {
            if (NoteView == null || NoteView.Editor == null)
            {
                return;
            }

            NCanvas canvas = NoteView.Editor.DrawingBoard;
            if (canvas.Drawings.Count == 0)
            {
                MessageBox.Show("You must first select the drawing you would like to add to your clipart library.", "Add to Library...", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Convert the selected drawing to a single ShapeGroup object
            canvas = (NCanvas)canvas.Clone();
            canvas.Selection.SelectRange(canvas.Drawings);
            canvas.Group();
            canvas.Group(); // (x2 to ensure items that were initially converted to paths are converted to groups)
            canvas.MoveDrawing(canvas.Last(), new Point(0, 0) - canvas.Last().RenderedBounds.TopLeft);
            var drawing = canvas.Last() as ShapeGroup;

            // Prompt user for properties for the new clipart object

            var dialog = new AddToLibraryDialog
            {
                Owner = this,
                Drawing = drawing
            };

            var binding = new System.Windows.Data.Binding
            {
                Source = DataContext,
                Path = new PropertyPath("ClipartGroups")
            };

            dialog.SetBinding(AddToLibraryDialog.ClipartGroupsProperty, binding);

            if (dialog.ShowDialog() == true)
            {
                // Create a new clipart object with the given name

                dialog.SelectedGroup.CreateClipart(dialog.Name, canvas.ToSVG());

                // Additionally save it as a marker if directed

                if (dialog.IsMarker)
                {
                    Marker marker = new Marker(drawing);
                    // TODO: allow the user to choose the reference point
                    //marker.RefX = dialog.RefX;
                    //marker.RefY = dialog.RefY;
                    canvas.Drawings.Remove(drawing);
                    canvas.Drawings.Add(marker);

                    Repository.Markers.CreateClipart(dialog.Name, canvas.ToSVG());
                }
            }
        }

        /// <summary>
        /// Edit the HTML source for the currently selected note
        /// </summary>
        public void EditSource()
        {
            string originalHTML = NoteView.Editor.ToHTML(pretty: true);
            var dialog = new DOMEditor
            {
                Owner = this
            };
            dialog.TextBox.Text = originalHTML;

            // Embed files as inline data as directed by the user
            dialog.InlineDataCheckbox.Checked += (sender, e) =>
            {
                dialog.TextBox.Text = NoteView.Editor.ToHTML(NoteView.Editor.EmbedFiles, pretty: true);
            };
            dialog.InlineDataCheckbox.Unchecked += (sender, e) =>
            {
                dialog.TextBox.Text = NoteView.Editor.ToHTML(pretty: true);
            };

            // Automatically update the note editor when the user makes an edit
            bool changed = false;
            dialog.TextBox.TextChanged += (sender, e) =>
            {
                NoteView.Editor.FromHTML(dialog.TextBox.Text.Trim());
                changed = true;
            };

            if (dialog.ShowDialog() != true && changed)
            {
                // If user clicked Cancel, restore the note to its original state
                NoteView.Editor.FromHTML(originalHTML);
            }
        }

        /// <summary>
        /// Show the request log
        /// </summary>
        public void ViewRequests()
        {
            // Enable request logging (Note: this is always enabled in debug mode)
            Repository.Client.Model.IsLogging = true;

            var window = new ClientViewWindow
            {
                DataContext = Repository.Client
            };

            window.Show();
        }

        /// <summary>
        /// Show/hide the left sidebar
        /// </summary>
        public void ToggleLeftPane()
        {
            IsLeftPaneVisible = !IsLeftPaneVisible;
        }

        /// <summary>
        /// Show/hide the right sidebar
        /// </summary>
        public void ToggleRightPane()
        {
            IsRightPaneVisible = !IsRightPaneVisible;
        }

        /// <summary>
        /// Show/hide the developer tools
        /// </summary>
        public void ToggleDebugPane()
        {
            IsDebugPaneVisible = !IsDebugPaneVisible;
        }

        #endregion

        #region Initialization

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Load window's size/position/state after HWND created, but before it's drawn:
            //
            // http://social.msdn.microsoft.com/Forums/hu-HU/wpf/thread/9df56308-e578-436f-b8d3-575243601443

            // Size

            var size = Settings.Default.MainWindowSize;

            double minWidth = 100;
            double minHeight = 100;
            double maxWidth = SystemParameters.VirtualScreenWidth;
            double maxHeight = SystemParameters.VirtualScreenHeight;

            Width = Math.Max(Math.Min(size.Width, maxWidth), minWidth);
            Height = Math.Max(Math.Min(size.Height, maxHeight), minHeight);

            // Position

            var position = Settings.Default.MainWindowPosition;

            double minX = 0;
            double minY = 0;
            double maxX = SystemParameters.VirtualScreenWidth - 100;
            double maxY = SystemParameters.VirtualScreenHeight - 100;

            Left = Math.Max(Math.Min(position.X, maxX), minX);
            Top = Math.Max(Math.Min(position.Y, maxY), minY);

            // State

            var state = Settings.Default.MainWindowState;

            if (state == WindowState.Minimized)
            {
                state = WindowState.Normal;
            }

            WindowState = state;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Left Pane
            double leftPaneWidth = Math.Max(Settings.Default.LeftPaneWidth, 160);
            LeftPaneWidth = new GridLength(leftPaneWidth);
            IsLeftPaneVisible = Settings.Default.IsLeftPaneVisible;

            // Right Pane
            double rightPaneWidth = Math.Max(Settings.Default.RightPaneWidth, 160);
            RightPaneWidth = new GridLength(rightPaneWidth);
            IsRightPaneVisible = Settings.Default.IsRightPaneVisible;

            // Search Pane
            double searchPaneHeight = Math.Max(Settings.Default.SearchPaneHeight, 100);
            SearchRowHeight = new GridLength(searchPaneHeight);
            AutoHideCheckBox.IsChecked = Settings.Default.SearchPaneAutoHide;
            SearchExpander.IsExpanded = !Settings.Default.SearchPaneAutoHide;

            ShowToolTrayItem();
            RegisterHotKeys();

            WidgetManager = new WidgetManager();
            WidgetManager.WidgetCanvas = WidgetCanvas;
            WidgetManager.AddContainer(LeftPane, LeftPanePanel);
            WidgetManager.AddContainer(RightPane, RightPanePanel);

            // Wait until this window is rendered before checking for an empty repository
            // so that any dialogs are properly displayed on top of this window.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (IsLoaded && Repository != null)
                {
                    Repository_IsEmptyChanged(Repository, null);
                }
            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            HideToolTrayItem();
            UnregisterHotKeys();

            if (Repository != null)
            {
                Repository.WhenPropertyChanged("IsEmpty", Repository_IsEmptyChanged, false);
            }

            if (SearchExpander.IsExpanded)
            {
                Settings.Default.SearchPaneHeight = SearchRow.Height.Value;
            }
            else
            {
                Settings.Default.SearchPaneHeight = SearchRowHeight.Value;
            }

            Settings.Default.IsLeftPaneVisible = IsLeftPaneVisible;
            if (IsLeftPaneVisible)
            {
                Settings.Default.LeftPaneWidth = LeftPaneColumn.Width.Value;
            }
            else
            {
                Settings.Default.LeftPaneWidth = LeftPaneWidth.Value;
            }

            Settings.Default.IsRightPaneVisible = IsRightPaneVisible;
            if (IsRightPaneVisible)
            {
                Settings.Default.RightPaneWidth = RightPaneColumn.Width.Value;
            }
            else
            {
                Settings.Default.RightPaneWidth = RightPaneWidth.Value;
            }

            if (WindowState != WindowState.Minimized)
            {
                Settings.Default.MainWindowState = WindowState;

                if (WindowState != WindowState.Maximized)
                {
                    Settings.Default.MainWindowPosition = new Point(Left, Top);
                    Settings.Default.MainWindowSize = new Size(Width, Height);
                }
            }

            try
            {
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }

        void DataContext_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Call Repository_IsEmptyChanged() when an empty repository is loaded

            if (e.OldValue is RepositoryViewModel oldRepository)
            {
                oldRepository.WhenPropertyChanged("IsEmpty", Repository_IsEmptyChanged, false);
            }

            if (e.NewValue is RepositoryViewModel newRepository)
            {
                newRepository.WhenPropertyChanged("IsEmpty", Repository_IsEmptyChanged, true);

                if (IsLoaded)
                {
                    Repository_IsEmptyChanged(newRepository, null);
                }
            }
        }

        private void Repository_IsEmptyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Automatically prompt the user to create a new notebook when an empty repository is loaded

            if (IsLoaded && Repository.IsEmpty && Repository.Notebooks.Count == 0)
            {
                NApplicationCommands.NewNotebook.Execute(null, this);
            }
        }

        #endregion

        #region Image Menu

        ObservableCollection<FileAssociation> _ImageApplications;

        public ObservableCollection<FileAssociation> ImageApplications
        {
            get
            {
                if (_ImageApplications == null)
                {
                    _ImageApplications = new ObservableCollection<FileAssociation>();
                }
                return _ImageApplications;
            }
        }

        void RefreshImageApplications()
        {
            ImageApplications.Clear();

            foreach (var item in NoteView.Editor.ImageApplications)
            {
                ImageApplications.Add(item);
            }
        }

        private void OpenImageWith_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            RefreshImageApplications();
        }

        #endregion

        #region HotKeys

        public HotKey NewNoteHotKey { get; set; }
        public HotKey CaptureSelectionHotKey { get; set; }
        public HotKey CaptureRegionHotKey { get; set; }
        public HotKey CaptureWindowHotKey { get; set; }
        public HotKey CaptureScreenHotKey { get; set; }

        private void RegisterHotKeys()
        {
            NewNoteHotKey = new HotKey("SilverNote_NewNoteHotKey");
            NewNoteHotKey.Pressed += NewNoteHotKey_Pressed;
            NewNoteHotKey.RegisterHotKey(SilverNote.Properties.Settings.Default.NewNoteHotKey);

            CaptureSelectionHotKey = new HotKey("SilverNote_CaptureSelectionHotKey");
            CaptureSelectionHotKey.Pressed += CaptureSelectionHotKey_Pressed;
            CaptureSelectionHotKey.RegisterHotKey(SilverNote.Properties.Settings.Default.CaptureSelectionHotKey);

            CaptureRegionHotKey = new HotKey("SilverNote_CaptureRegionHotKey");
            CaptureRegionHotKey.Pressed += CaptureRegionHotKey_Pressed;
            CaptureRegionHotKey.RegisterHotKey(SilverNote.Properties.Settings.Default.CaptureRegionHotKey);
            CaptureRegionHotKey.ValueChanged += CaptureRegionHotKey_ValueChanged;

            CaptureWindowHotKey = new HotKey("SilverNote_CaptureWindowHotKey");
            CaptureWindowHotKey.Pressed += CaptureWindowHotKey_Pressed;
            CaptureWindowHotKey.RegisterHotKey(SilverNote.Properties.Settings.Default.CaptureWindowHotKey);
            CaptureWindowHotKey.ValueChanged += CaptureWindowHotKey_ValueChanged;

            CaptureScreenHotKey = new HotKey("SilverNote_CaptureScreenHotKey");
            CaptureScreenHotKey.Pressed += CaptureScreenHotKey_Pressed;
            CaptureScreenHotKey.RegisterHotKey(SilverNote.Properties.Settings.Default.CaptureScreenHotKey);
            CaptureScreenHotKey.ValueChanged += CaptureScreenHotKey_ValueChanged;

            if (ToolTrayItem != null)
            {
                ToolTrayItem.NewNoteHotKey = NewNoteHotKey;
                ToolTrayItem.CaptureSelectionHotKey = CaptureSelectionHotKey;
                ToolTrayItem.CaptureRegionHotKey = CaptureRegionHotKey;
                ToolTrayItem.CaptureWindowHotKey = CaptureWindowHotKey;
                ToolTrayItem.CaptureScreenHotKey = CaptureScreenHotKey;
            }
        }

        private void UnregisterHotKeys()
        {
            if (NewNoteHotKey != null)
            {
                NewNoteHotKey.UnregisterHotKey();
            }

            if (CaptureSelectionHotKey != null)
            {
                CaptureSelectionHotKey.UnregisterHotKey();
            }

            if (CaptureRegionHotKey != null)
            {
                CaptureRegionHotKey.UnregisterHotKey();
            }

            if (CaptureWindowHotKey != null)
            {
                CaptureWindowHotKey.UnregisterHotKey();
            }

            if (CaptureScreenHotKey != null)
            {
                CaptureScreenHotKey.UnregisterHotKey();
            }
        }

        private void NewNoteHotKey_Pressed(object sender, EventArgs e)
        {
            if (Repository.SelectedNotebook != null)
            {
                Repository.SelectedNotebook.CreateNote(true);
            }
        }

        private void CaptureSelectionHotKey_Pressed(object sender, EventArgs e)
        {
            if (Repository.SelectedNotebook != null)
            {
                CaptureSelection(IntPtr.Zero);
            }
        }

        private void CaptureRegionHotKey_Pressed(object sender, EventArgs e)
        {
            if (Repository.SelectedNotebook != null)
            {
                CaptureRegion();
            }
        }

        private void CaptureRegionHotKey_ValueChanged(object sender, EventArgs e)
        {

        }

        private void CaptureWindowHotKey_Pressed(object sender, EventArgs e)
        {
            if (Repository.SelectedNotebook != null)
            {
                CaptureWindow(IntPtr.Zero);
            }
        }

        private void CaptureWindowHotKey_ValueChanged(object sender, EventArgs e)
        {

        }

        private void CaptureScreenHotKey_Pressed(object sender, EventArgs e)
        {
            if (Repository.SelectedNotebook != null)
            {
                CaptureScreen();
            }
        }

        private void CaptureScreenHotKey_ValueChanged(object sender, EventArgs e)
        {

        }

        #endregion

        #region ToolTrayItem

        ToolTrayItem ToolTrayItem { get; set; }

        protected void ShowToolTrayItem()
        {
            HideToolTrayItem();

            ToolTrayItem = new ToolTrayItem();
            ToolTrayItem.NewNoteHotKey = NewNoteHotKey;
            ToolTrayItem.CaptureSelectionHotKey = CaptureSelectionHotKey;
            ToolTrayItem.CaptureRegionHotKey = CaptureRegionHotKey;
            ToolTrayItem.CaptureScreenHotKey = CaptureScreenHotKey;
            ToolTrayItem.MouseMove += ToolTrayItem_MouseMove;
            ToolTrayItem.Popup += ToolTrayItem_Popup;
            ToolTrayItem.OpenApp += ToolTrayItem_Open;
            ToolTrayItem.NewNote += ToolTrayItem_NewNote;
            ToolTrayItem.OpenNote += ToolTrayItem_OpenNote;
            ToolTrayItem.CaptureSelection += ToolTrayItem_CaptureSelection;
            ToolTrayItem.CaptureRegion += ToolTrayItem_CaptureRegion;
            ToolTrayItem.CaptureWindow += ToolTrayItem_CaptureWindow;
            ToolTrayItem.CaptureScreen += ToolTrayItem_CaptureScreen;
            ToolTrayItem.Exit += ToolTrayItem_Exit;
        }

        protected void HideToolTrayItem()
        {
            if (ToolTrayItem != null)
            {
                ToolTrayItem.IsVisible = false;
                ToolTrayItem = null;
            }
        }

        private void ToolTrayItem_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            IntPtr hWnd = Win32.GetForegroundWindow();
            string text = Win32.GetWindowText(hWnd);

            if (!String.IsNullOrEmpty(text))
            {
                _ForegroundWindow = hWnd;
            }
        }

        private void ToolTrayItem_Popup(object sender, EventArgs e)
        {
            ToolTrayItem.ClearNotes();

            foreach (var note in Repository.SelectedNotebook.OpenNotes)
            {
                ToolTrayItem.AddNote(note);
            }
        }

        private void ToolTrayItem_Open(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void ToolTrayItem_NewNote(object sender, EventArgs e)
        {
            Repository.SelectedNotebook.CreateNote(floating: true);
        }

        private void ToolTrayItem_OpenNote(object sender, EventArgs<NoteViewModel> e)
        {
            NoteViewModel note = e.Data;

            note.Notebook.FloatNote(note);
        }

        private IntPtr _ForegroundWindow;

        private void ToolTrayItem_CaptureSelection(object sender, EventArgs e)
        {
            CaptureSelection(_ForegroundWindow);
        }

        private void ToolTrayItem_CaptureRegion(object sender, EventArgs e)
        {
            CaptureRegion();
        }

        private void ToolTrayItem_CaptureWindow(object sender, EventArgs e)
        {
            CaptureWindow(_ForegroundWindow);
        }

        private void ToolTrayItem_CaptureScreen(object sender, EventArgs e)
        {
            CaptureScreen();
        }

        private void ToolTrayItem_Exit(object sender, EventArgs e)
        {
            HideToolTrayItem();
        }

        #endregion

        #region Capturing

        void CaptureSelection(IntPtr hWnd)
        {
            NoteEditor editor = GetCaptureDestination();
            if (editor == null)
            {
                return;
            }

            if (!CaptureSelectionHelper.Copy(hWnd))
            {
                return;
            }

            using (var undo = new UndoScope(editor.UndoStack, "Capture selection"))
            {
                if (editor.IsTextSelected)
                {
                    editor.MoveRight(); // Don't overwrite selected text
                }

                editor.Paste();
            }

            this.Restore();
        }

        void CaptureRegion()
        {
            NoteEditor editor = GetCaptureDestination();
            if (editor == null)
            {
                return;
            }

            var overlay = new ScreenCaptureOverlay();
            if (overlay.ShowDialog() != true)
            {
                return;
            }

            if (overlay.CaptureRect.Width < 5 || overlay.CaptureRect.Height < 5)
            {
                return;
            }

            var bitmap = ScreenCapture.CaptureScreen(overlay.CaptureRect);

            using (var undo = new UndoScope(editor.UndoStack, "Capture region"))
            {
                if (editor.IsTextSelected)
                {
                    editor.MoveRight(); // Don't overwrite selected text
                }

                editor.InsertImage(bitmap, false);
            }

            this.Restore();
        }

        void CaptureWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                hWnd = Win32.GetForegroundWindow();
            }

            var editor = GetCaptureDestination();
            if (editor == null)
            {
                return;
            }

            var bitmap = ScreenCapture.CaptureWindow(hWnd);
            if (bitmap == null)
            {
                return;
            }

            // Now actually capture the image

            var image = new NImage();
            image.Source = bitmap;
            image.PreserveAspectRatio = true;

            // Ensure the image fits within the editor

            editor.UpdateLayout();
            Size editorSize = editor.RenderSize;
            double maxWidth = Math.Max(editorSize.Width * 0.5, 50);

            if (image.OriginalWidth > maxWidth)
            {
                image.Width = maxWidth;
                // maintain aspect ratio
                image.Height = image.Width * image.OriginalHeight / image.OriginalWidth;
            }

            using (var undo = new UndoScope(editor.UndoStack, "Capture region"))
            {
                if (editor.IsTextSelected)
                {
                    editor.MoveRight(); // Don't overwrite selected text
                }

                // Insert the image

                editor.InsertImage(image, false);
            }

            this.Restore();
        }

        void CaptureScreen()
        {
            var editor = GetCaptureDestination();
            if (editor == null)
            {
                return;
            }

            // Now actually capture the image

            NImage image = new NImage();
            image.Source = ScreenCapture.CaptureScreen();
            image.PreserveAspectRatio = true;

            // Ensure the image fits within the editor

            editor.UpdateLayout();
            Size editorSize = editor.RenderSize;
            double maxWidth = Math.Max(editorSize.Width * 0.5, 50);

            if (image.OriginalWidth > maxWidth)
            {
                image.Width = maxWidth;
                // maintain aspect ratio
                image.Height = image.Width * image.OriginalHeight / image.OriginalWidth;
            }

            using (var undo = new UndoScope(editor.UndoStack, "Capture region"))
            {
                // Don't overwrite selected text

                if (editor.IsTextSelected)
                {
                    editor.MoveRight();
                }

                // Insert the image

                editor.InsertImage(image, false);
            }

            this.Restore();
        }

        Editor.NoteEditor GetCaptureDestination()
        {
            // Get the note this will be captured to

            NoteViewModel note = Repository.SelectedNotebook.OpenNotes.FirstOrDefault((n) => n.IsFloating);

            if (note == null)
            {
                note = Repository.SelectedNotebook.SelectedNote;
            }

            if (note == null)
            {
                note = Repository.SelectedNotebook.CreateNote(false);
            }

            // Get the associated editor

            if (note.IsFloating)
            {
                return note.FloatingView.Editor;
            }
            else
            {
                return NoteViewGenerator[note].Editor;
            }
        }

        public void InsertScreenshot()
        {
            ScreenCaptureDialog dialog = new ScreenCaptureDialog();
            dialog.Show();
        }

        #endregion

        #region Commands

        private void NewNoteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Repository.SelectedNotebook != null)
            {
                Repository.SelectedNotebook.CreateNote();
            }
        }

        private void NewNoteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Repository.SelectedNotebook != null;
        }

        private void NewNotebookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var app = (App)App.Current;

            var model = app.NotebookManager.Create();
            var notebook = NotebookViewModel.FromModel(model);

            if (notebook != null)
            {
                notebook.CreateNote();
            }
        }

        private void OpenNotebookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var app = (App)App.Current;
            app.NotebookManager.Open();
        }

        private void DeleteNotebookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var notebook = Repository.SelectedNotebook;
            if (notebook != null)
            {
                Repository.DeleteNotebook(notebook);
            }
        }

        private void DeleteNotebookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Repository != null && Repository.SelectedNotebook != null;
        }

        private void RenameNotebookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var notebook = Repository.SelectedNotebook;
            if (notebook == null)
            {
                return;
            }

            if (notebook.Model.Source == notebook.Repository.Model.Source)
            {
                notebook.Rename();
            }
            else
            {
                var app = (App)App.Current;
                app.NotebookManager.Move(notebook.Model);
            }
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (NoteView != null && NoteView.Editor != null)
            {
                NoteView.Editor.Flush();
            }
        }

        private void SaveAsDOCXCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (NoteView != null && NoteView.Editor != null)
            {
                ExportManager.SaveAsDOCX(NoteView.Editor);
            }
        }

        void SaveAsRTFCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (NoteView != null && NoteView.Editor != null)
            {
                ExportManager.SaveAsRTF(NoteView.Editor);
            }
        }

        void SaveAsTXTCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (NoteView != null && NoteView.Editor != null)
            {
                ExportManager.SaveAsTXT(NoteView.Editor);
            }
        }

        void SaveAsHTMLCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (NoteView != null && NoteView.Editor != null)
            {
                ExportManager.SaveAsHTML(NoteView.Editor);
            }
        }

        void SaveImageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveImage();
        }

        void SaveImageCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (NoteView != null && NoteView.Editor != null)
            {
                e.CanExecute = NoteView.Editor.DrawingBoard.IsSelected || NoteView.Editor.Selection.OfType<NImage>().Any();
            }
            else
            {
                e.CanExecute = false;
            }
        }

        void SaveFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile();
        }

        void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Print();
        }

        void PrintPreviewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintPreview();
        }

        private void RenameNotebookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Repository.SelectedNotebook != null;
        }

        private void ChangePasswordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Repository != null)
            {
                ChangePassword();
            }
        }

        private void ChangePasswordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Repository.CanChangePassword;
        }

        private void SettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string tabName = e.Parameter as string;
            App.Configure(tabName);
        }

        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void InsertScreenshotCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertScreenshot();
        }

        private void InsertScreenshotCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = NoteView != null;
        }

        private void CaptureRegionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CaptureRegion();
        }

        private void CaptureWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CaptureWindow(IntPtr.Zero);
        }

        private void CaptureScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CaptureScreen();
        }

        void AddToLibraryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddToLibrary();
        }

        private void FloatTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Repository != null && Repository.SelectedNotebook != null && Repository.SelectedNotebook.SelectedNote != null)
            {
                Repository.SelectedNotebook.FloatNote(Repository.SelectedNotebook.SelectedNote);
            }
        }

        private void FloatTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Repository != null && Repository.SelectedNotebook != null && Repository.SelectedNotebook.SelectedNote != null;
        }

        private void CloseTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Repository != null && Repository.SelectedNotebook != null && Repository.SelectedNotebook.SelectedNote != null)
            {
                Repository.SelectedNotebook.CloseNote(Repository.SelectedNotebook.SelectedNote);
            }
        }

        private void CloseTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Repository != null && Repository.SelectedNotebook != null && Repository.SelectedNotebook.SelectedNote != null;
        }

        private void CloseOtherTabsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Repository != null && Repository.SelectedNotebook != null && Repository.SelectedNotebook.SelectedNote != null)
            {
                Repository.SelectedNotebook.CloseOtherNotes(Repository.SelectedNotebook.SelectedNote);
            }
        }

        private void CloseOtherTabsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Repository != null && Repository.SelectedNotebook != null && Repository.SelectedNotebook.SelectedNote != null;
        }

        private void SearchNotebookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is CategoryViewModel category)
            {
                Repository.SelectedNotebook.Search.Category = category;
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
                return;
            }

            if (!SearchExpander.IsKeyboardFocusWithin)
            {
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
            }
            else if (NoteView != null)
            {
                NoteView.Editor.Focus();
                Repository.SelectedNotebook.ActivateNote();
            }
        }

        private void SearchNotebookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Repository != null && Repository.SelectedNotebook != null;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void EditSourceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditSource();
        }

        private void ViewRequestsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewRequests();
        }

        private void ToggleLeftPaneCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ToggleLeftPane();
        }

        private void ToggleRightPaneCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ToggleRightPane();
        }

        private void DebugCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ToggleDebugPane();

            if (IsDebugPaneVisible)
            {
                NoteEditor.SetDebugFlags(this, NDebugFlags.All);
            }
            else
            {
                NoteEditor.SetDebugFlags(this, NDebugFlags.None);
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton.HasFlag(MouseButtonState.Pressed) ||
                e.RightButton.HasFlag(MouseButtonState.Pressed))
            {
                return;
            }

            Point position = e.GetPosition(this);
            
            if (HitTestLeftExpander(position) || LeftPaneExpander.IsMouseOver)
            {
                LeftPaneExpander.Opacity = 1.0;
            }
            else
            {
                LeftPaneExpander.Opacity = 0.5;
            }
            
            if (HitTestRightExpander(position) || RightPaneExpander.IsMouseOver)
            {
                RightPaneExpander.Opacity = 1.0;
            }
            else
            {
                RightPaneExpander.Opacity = 0.5;
            }
        }
        
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            LeftPaneExpander.Opacity = 0.5;
            RightPaneExpander.Opacity = 0.5;
        }
        
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            if (HitTestLeftExpander(position) || LeftPaneExpander.IsMouseOver)
            {
                ToggleLeftPane();
            }
            if (HitTestRightExpander(position) || RightPaneExpander.IsMouseOver)
            {
                ToggleRightPane();
            }
        }
		
        private void NotesTabControl_Rearranged(object sender, RearrangeEventArgs e)
        {
            var sourceNote = NotesTabControl.ItemContainerGenerator.ItemFromContainer(e.SourceItem) as NoteViewModel;
            var targetNote = NotesTabControl.ItemContainerGenerator.ItemFromContainer(e.TargetItem) as NoteViewModel;

            if (sourceNote != null && targetNote != null)
            {
                Repository.SelectedNotebook.RearrangeNotes(sourceNote, targetNote);
            }
        }

        private void NotesTabViewSource_Filter(object sender, FilterEventArgs e)
        {
            var note = (NoteViewModel)e.Item;

            e.Accepted = !note.IsFloating;
        }

        private void Editor_FileDropped(object sender, FileDroppedEventArgs e)
        {
            if (e.IsImage)
            {
                if (Settings.Default.HasDropFileAsImage)
                {
                    e.DropAsImage = Settings.Default.DropFileAsImage;
                    return;
                }

                // Preferences not specified - prompt user

                var dialog = new Dialogs.DropFileAsImageDialog();
                dialog.Owner = App.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    e.Cancel = true;
                    return;
                }

                if (Settings.Default.RememberSettings)
                {
                    try
                    {
                        Settings.Default.DropFileAsImage = dialog.InsertAsImage;
                        Settings.Default.HasDropFileAsImage = true;
                        Settings.Default.Save();
                    }
                    catch (Exception ex)
                    {
                        Debug.Assert(false, ex.Message);
                    }
                }

                e.DropAsImage = dialog.InsertAsImage;
            }
        }

        #endregion

        #region Implementation

        #region Left Pane

        GridLength LeftPaneWidth { get; set; }

        void IsLeftPaneVisible_Changed(bool newValue)
        {
            if (newValue)
            {
                // Restore column width
                LeftPaneColumn.Width = LeftPaneWidth;
            }
            else
            {
                // Save column width
                if (LeftPaneColumn.Width.IsAbsolute && LeftPaneColumn.Width.Value > 0)
                {
                    LeftPaneWidth = LeftPaneColumn.Width;
                }

                // Collapse the column
                LeftPaneColumn.Width = GridLength.Auto;
            }
        }

        #endregion

        #region Right Pane

        GridLength RightPaneWidth { get; set; }

        void IsRightPaneVisible_Changed(bool newValue)
        {
            if (newValue)
            {
                // Restore column width
                RightPaneColumn.Width = RightPaneWidth;
            }
            else
            {
                // Save column width
                if (RightPaneColumn.Width.IsAbsolute && RightPaneColumn.Width.Value > 0)
                {
                    RightPaneWidth = RightPaneColumn.Width;
                }

                // Collapse the column
                RightPaneColumn.Width = GridLength.Auto;
            }
        }

        #endregion

        #region Search Pane

        GridLength SearchRowHeight { get; set; }

        private void SearchExpander_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DependencyObject newFocus = e.NewFocus as DependencyObject;

            if (newFocus != null &&
                newFocus != AutoHideCheckBox &&
                !IsVisualAncestorOf(newFocus, CategoriesMenu) &&
                !IsLogicalAncestorOf(newFocus, CategoriesMenu))
            {
                SearchExpander.IsExpanded = true;
            }
        }

        bool IsLogicalAncestorOf(DependencyObject descendant, DependencyObject ancestor = null)
        {
            if (ancestor == null)
            {
                ancestor = this;
            }

            while (descendant != null)
            {
                if (descendant == ancestor)
                    return true;
                descendant = LogicalTreeHelper.GetParent(descendant);
            }
            return false;
        }

        bool IsVisualAncestorOf(DependencyObject descendant, DependencyObject ancestor = null)
        {
            if (ancestor == null)
            {
                ancestor = this;
            }

            while (descendant != null)
            {
                if (descendant == ancestor)
                {
                    return true;
                }

                var frameworkElement = descendant as FrameworkElement;

                if (frameworkElement != null && frameworkElement.TemplatedParent != null)
                {
                    descendant = frameworkElement.TemplatedParent;
                }
                else
                {
                    descendant = VisualTreeHelper.GetParent(descendant);
                }
            }
            return false;
        }

        private void AutoHideCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Update application settings

            Dispatcher.BeginInvoke(new Action(() => 
            {
                try
                {
                    Settings.Default.SearchPaneAutoHide = AutoHideCheckBox.IsChecked == true;
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
            }),  DispatcherPriority.Background);

            // Make sure search pane is collapsed

            SearchExpander.IsExpanded = false;
        }

        private void AutoHideCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Update application settings

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Settings.Default.SearchPaneAutoHide = AutoHideCheckBox.IsChecked == true;
                    SilverNote.Properties.Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
            }), DispatcherPriority.Background);

            // Make sure search pane is expanded

            SearchExpander.IsExpanded = true;
        }

        private void SearchExpander_Expanded(object sender, RoutedEventArgs e)
        {
            SearchRow.Height = SearchRowHeight;
        }

        private void SearchExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (SearchRow.Height.IsAbsolute && SearchRow.Height.Value > 0)
            {
                SearchRowHeight = SearchRow.Height;
            }

            SearchRow.Height = GridLength.Auto;
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SearchTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        #endregion

        #region Debug Pane

        GridLength _DebugPaneHeight = new GridLength(250);

        void IsDebugPaneVisible_Changed(bool newValue)
        {
            if (newValue)
            {
                // Restore row height
                DebugPaneRow.Height = _DebugPaneHeight;
            }
            else
            {
                // Save row height
                if (DebugPaneRow.Height.IsAbsolute && DebugPaneRow.Height.Value > 0)
                {
                    _DebugPaneHeight = DebugPaneRow.Height;
                }

                // Collapse the column
                DebugPaneRow.Height = GridLength.Auto;
            }
        }

        private void DebugPaneSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (!DebugPaneRow.Height.IsAbsolute)
            {
                DebugPaneRow.Height = new GridLength(DebugPaneRow.ActualHeight);
            }

            double newHeight = Math.Max(DebugPaneRow.Height.Value - e.VerticalChange, 20);

            DebugPaneRow.Height = new GridLength(newHeight);

            e.Handled = true;
        }

        private void MaximizeDebugPane_Click(object sender, RoutedEventArgs e)
        {
            double maxHeight = NotesRow.ActualHeight + DebugPaneRow.ActualHeight;

            if (DebugPaneRow.ActualHeight != maxHeight)
            {
                if (DebugPaneRow.Height.IsAbsolute && DebugPaneRow.Height.Value > 0)
                {
                    _DebugPaneHeight = DebugPaneRow.Height;
                }

                DebugPaneRow.Height = new GridLength(maxHeight);
            }
            else
            {
                DebugPaneRow.Height = _DebugPaneHeight;
            }
        }

        #endregion

        private bool HitTestLeftExpander(Point point)
        {
            return point.X < 15.0;
        }

        private bool HitTestRightExpander(Point point)
        {
            double actualWidth = ((FrameworkElement)base.Content).ActualWidth;
            return point.X > actualWidth - 15.0;
        }

        #endregion
    }
}
