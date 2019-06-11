using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;
using SilverNote.ViewModels;

namespace SilverNote
{
    /// <summary>
    /// Represents this application's tooltray item
    /// </summary>
    public class ToolTrayItem
    {
        const string NOTIFY_ICON_IMAGE_URL = "pack://application:,,,/SilverNote;component/SilverNote.ico";

        #region Fields

        private HotKey _NewNoteHotKey;
        private HotKey _CaptureSelectionHotKey;
        private HotKey _CaptureRegionHotKey;
        private HotKey _CaptureWindowHotKey;
        private HotKey _CaptureScreenHotKey;

        #endregion

        #region Constructors

        public ToolTrayItem()
        {
            NotifyIcon = new NotifyIcon();

            // Initialize the icon
            var resource = System.Windows.Application.GetResourceStream(new Uri(NOTIFY_ICON_IMAGE_URL));
            NotifyIcon.Icon = new System.Drawing.Icon(resource.Stream);

            // Initialize the context menu
            NotifyIcon.ContextMenu = new ContextMenu();
            NotifyIcon.ContextMenu.Popup += NotifyIcon_Popup;

            MenuItem_OpenApp = new MenuItem("Open SilverNote", NotifyIcon_OpenApp)
            {
                DefaultItem = true
            };
            MenuItem_NewNote = new MenuItem("New Note", NotifyIcon_NewNote);
            MenuItem_OpenNote = new MenuItem("Open Note");
            MenuItem_CaptureSelection = new MenuItem("Capture Selection", NotifyIcon_CaptureSelection);
            MenuItem_CaptureRegion = new MenuItem("Capture Region", NotifyIcon_CaptureRegion);
            MenuItem_CaptureWindow = new MenuItem("Capture Window", NotifyIcon_CaptureWindow);
            MenuItem_CaptureScreen = new MenuItem("Capture Screen", NotifyIcon_CaptureScreen);
            MenuItem_Exit = new MenuItem("Exit", NotifyIcon_Exit);

            NotifyIcon.ContextMenu.MenuItems.Add(MenuItem_OpenApp);
            NotifyIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
            NotifyIcon.ContextMenu.MenuItems.Add(MenuItem_NewNote);
            NotifyIcon.ContextMenu.MenuItems.Add(MenuItem_OpenNote);
            NotifyIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
            NotifyIcon.ContextMenu.MenuItems.Add(MenuItem_CaptureSelection);
            NotifyIcon.ContextMenu.MenuItems.Add(MenuItem_CaptureRegion);
            NotifyIcon.ContextMenu.MenuItems.Add(MenuItem_CaptureWindow);
            NotifyIcon.ContextMenu.MenuItems.Add(MenuItem_CaptureScreen);
            NotifyIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
            NotifyIcon.ContextMenu.MenuItems.Add(MenuItem_Exit);

            NotifyIcon.MouseMove += NotifyIcon_MouseMove;
            NotifyIcon.MouseClick += NotifyIcon_Click;
            NotifyIcon.Visible = true;
        }

        #endregion

        #region Properties

        public bool IsVisible
        {
            get { return NotifyIcon.Visible; }
            set { NotifyIcon.Visible = value; }
        }

        /// <summary>
        /// A hotkey for creating a new note
        /// </summary>
        public HotKey NewNoteHotKey
        {
            get
            {
                return _NewNoteHotKey;
            }
            set
            {
                var oldValue = _NewNoteHotKey;
                var newValue = value;

                if (oldValue != newValue)
                {
                    _NewNoteHotKey = newValue;
                    HotKey_Changed(oldValue, newValue);
                }
            }
        }

        /// <summary>
        /// A hotkey for capturing the current selection
        /// </summary>
        public HotKey CaptureSelectionHotKey
        {
            get
            {
                return _CaptureSelectionHotKey;
            }
            set
            {
                var oldValue = _CaptureSelectionHotKey;
                var newValue = value;

                if (oldValue != newValue)
                {
                    _CaptureSelectionHotKey = newValue;
                    HotKey_Changed(oldValue, newValue);
                }
            }
        }

        /// <summary>
        /// A hotkey for capturing a region of the screen
        /// </summary>
        public HotKey CaptureRegionHotKey
        {
            get
            {
                return _CaptureRegionHotKey;
            }
            set
            {
                var oldValue = _CaptureRegionHotKey;
                var newValue = value;

                if (oldValue != newValue)
                {
                    _CaptureRegionHotKey = newValue;
                    HotKey_Changed(oldValue, newValue);
                }
            }
        }

        /// <summary>
        /// A hotkey for capturing a window
        /// </summary>
        public HotKey CaptureWindowHotKey
        {
            get
            {
                return _CaptureWindowHotKey;
            }
            set
            {
                var oldValue = _CaptureWindowHotKey;
                var newValue = value;

                if (oldValue != newValue)
                {
                    _CaptureWindowHotKey = newValue;
                    HotKey_Changed(oldValue, newValue);
                }
            }
        }

        /// <summary>
        /// A hotkey for capturing the entire screen
        /// </summary>
        public HotKey CaptureScreenHotKey
        {
            get
            {
                return _CaptureScreenHotKey;
            }
            set
            {
                var oldValue = _CaptureScreenHotKey;
                var newValue = value;

                if (oldValue != newValue)
                {
                    _CaptureScreenHotKey = newValue;
                    HotKey_Changed(oldValue, newValue);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clear all items from the "Open Note" submenu
        /// </summary>
        public void ClearNotes()
        {
            MenuItem_OpenNote.MenuItems.Clear();
        }

        /// <summary>
        /// Add a note to the "Open Note" submenu
        /// </summary>
        /// <param name="note"></param>
        public void AddNote(NoteViewModel note)
        {
            var menuItem = new MenuItem(note.Title, NotifyIcon_OpenNote);
            menuItem.Tag = note;
            MenuItem_OpenNote.MenuItems.Add(menuItem);
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the mouse moves over the tool tray icon
        /// </summary>
        public event MouseEventHandler MouseMove;

        /// <summary>
        /// Invoked when the tool tray icon's popup menu is displayed
        /// </summary>
        public event EventHandler Popup;

        /// <summary>
        /// Invoked when the user requests to open SilverNote
        /// </summary>
        public event EventHandler OpenApp;

        /// <summary>
        /// Invoked when the user requests to create a new node
        /// </summary>
        public event EventHandler NewNote;

        /// <summary>
        /// Invoked when the user requests to open the given note
        /// </summary>
        public event EventHandler<EventArgs<NoteViewModel>> OpenNote;

        /// <summary>
        /// Invoked when the user requests to capture the current selection
        /// </summary>
        public event EventHandler CaptureSelection;

        /// <summary>
        /// Invoked when the user requests to capture a region of the screen
        /// </summary>
        public event EventHandler CaptureRegion;

        /// <summary>
        ///  Invoked when the user requests to capture a window
        /// </summary>
        public event EventHandler CaptureWindow;

        /// <summary>
        /// Invoked when the user request to capture the entire screen
        /// </summary>
        public event EventHandler CaptureScreen;

        /// <summary>
        /// Invoked when the user requests to remove the tooltray item
        /// </summary>
        public event EventHandler Exit;

        #endregion

        #region Implementation

        private NotifyIcon NotifyIcon { get; set; }

        MenuItem MenuItem_OpenApp { get; set; }
        MenuItem MenuItem_NewNote { get; set; }
        MenuItem MenuItem_OpenNote { get; set; }
        MenuItem MenuItem_CaptureSelection { get; set; }
        MenuItem MenuItem_CaptureRegion { get; set; }
        MenuItem MenuItem_CaptureWindow { get; set; }
        MenuItem MenuItem_CaptureScreen { get; set; }
        MenuItem MenuItem_Exit { get; set; }

        private void RaiseMouseMove(MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void RaisePopup()
        {
            Popup?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseOpenApp()
        {
            OpenApp?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseNewNote()
        {
            NewNote?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseOpenNote(NoteViewModel note)
        {
            OpenNote?.Invoke(this, new EventArgs<NoteViewModel>(note));
        }

        private void RaiseCaptureSelection()
        {
            CaptureSelection?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseCaptureRegion()
        {
            CaptureRegion?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseCaptureWindow()
        {
            CaptureWindow?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseCaptureScreen()
        {
            CaptureScreen?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseExit()
        {
            Exit?.Invoke(this, EventArgs.Empty);
        }

        private void NotifyIcon_MouseMove(object sender, MouseEventArgs e)
        {
            RaiseMouseMove(e);
        }

        private void NotifyIcon_Click(object sender, MouseEventArgs e)
        {
            // Show the context menu when the user left-clicks the icon

            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    // Invoke the non-public method NotifyIcon.ShowContextMenu()

                    var type = typeof(NotifyIcon);
                    var method = "ShowContextMenu";
                    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                    var mi = type.GetMethod(method, flags);
                    mi.Invoke(NotifyIcon, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + "\n\n" + ex.StackTrace);
                }
            }
        }

        private void NotifyIcon_Popup(object sender, EventArgs e)
        {
            RaisePopup();
        }

        private void NotifyIcon_OpenApp(object sender, EventArgs e)
        {
            RaiseOpenApp();
        }

        private void NotifyIcon_NewNote(object sender, EventArgs e)
        {
            RaiseNewNote();
        }

        private void NotifyIcon_OpenNote(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem && 
                menuItem.Tag is NoteViewModel note)
            {
                RaiseOpenNote(note);
            }
        }

        private void NotifyIcon_CaptureSelection(object sender, EventArgs e)
        {
            RaiseCaptureSelection();
        }

        private void NotifyIcon_CaptureRegion(object sender, EventArgs e)
        {
            RaiseCaptureRegion();
        }

        private void NotifyIcon_CaptureWindow(object sender, EventArgs e)
        {
            RaiseCaptureWindow();
        }

        private void NotifyIcon_CaptureScreen(object sender, EventArgs e)
        {
            RaiseCaptureScreen();
        }

        private void NotifyIcon_Exit(object sender, EventArgs e)
        {
            RaiseExit();
        }

        private void HotKey_Changed(HotKey oldValue, HotKey newValue)
        {
            if (oldValue != null)
            {
                oldValue.ValueChanged -= HotKey_ValueChanged;
            }

            if (newValue != null)
            {
                newValue.ValueChanged += HotKey_ValueChanged;
                HotKey_ValueChanged(newValue, EventArgs.Empty);
            }
        }

        private void HotKey_ValueChanged(object sender, EventArgs e)
        {
            // Update the shortcut text for the menu item corresponding to the hotkey

            HotKey hotKey = (HotKey)sender;

            Shortcut shortcut = (Shortcut)(KeysFromModifiers(hotKey.Modifiers) | (Keys)hotKey.Key);

            try
            {
                if (hotKey == NewNoteHotKey)
                {
                    int index = NotifyIcon.ContextMenu.MenuItems.IndexOf(MenuItem_NewNote);
                    NotifyIcon.ContextMenu.MenuItems.RemoveAt(index);
                    MenuItem_NewNote = new MenuItem("New Note", NotifyIcon_NewNote, shortcut);
                    NotifyIcon.ContextMenu.MenuItems.Add(index, MenuItem_NewNote);
                }

                if (hotKey == CaptureSelectionHotKey)
                {
                    int index = NotifyIcon.ContextMenu.MenuItems.IndexOf(MenuItem_CaptureSelection);
                    NotifyIcon.ContextMenu.MenuItems.RemoveAt(index);
                    MenuItem_CaptureSelection = new MenuItem("Capture Selection", NotifyIcon_CaptureSelection, shortcut);
                    NotifyIcon.ContextMenu.MenuItems.Add(index, MenuItem_CaptureSelection);
                }

                if (hotKey == CaptureRegionHotKey)
                {
                    int index = NotifyIcon.ContextMenu.MenuItems.IndexOf(MenuItem_CaptureRegion);
                    NotifyIcon.ContextMenu.MenuItems.RemoveAt(index);
                    MenuItem_CaptureRegion = new MenuItem("Capture Region", NotifyIcon_CaptureRegion, shortcut);
                    NotifyIcon.ContextMenu.MenuItems.Add(index, MenuItem_CaptureRegion);
                }

                if (hotKey == CaptureWindowHotKey)
                {
                    int index = NotifyIcon.ContextMenu.MenuItems.IndexOf(MenuItem_CaptureWindow);
                    NotifyIcon.ContextMenu.MenuItems.RemoveAt(index);
                    MenuItem_CaptureWindow = new MenuItem("Capture Window", NotifyIcon_CaptureWindow, shortcut);
                    NotifyIcon.ContextMenu.MenuItems.Add(index, MenuItem_CaptureWindow);
                }

                if (hotKey == CaptureScreenHotKey)
                {
                    int index = NotifyIcon.ContextMenu.MenuItems.IndexOf(MenuItem_CaptureScreen);
                    NotifyIcon.ContextMenu.MenuItems.RemoveAt(index);
                    MenuItem_CaptureScreen = new MenuItem("Capture Screen", NotifyIcon_CaptureScreen, shortcut);
                    NotifyIcon.ContextMenu.MenuItems.Add(index, MenuItem_CaptureScreen);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n\n" + ex.StackTrace);
            }

        }

        private static Keys KeysFromModifiers(uint modifiers)
        {
            Keys result = Keys.None;

            if ((modifiers & Win32.MOD_CONTROL) != 0)
            {
                result |= Keys.Control;
            }

            if ((modifiers & Win32.MOD_ALT) != 0)
            {
                result |= Keys.Alt;
            }
            
            if ((modifiers & Win32.MOD_SHIFT) != 0)
            {
                result |= Keys.Shift;
            }

            if ((modifiers & Win32.MOD_WIN) != 0)
            {
                result |= Keys.LWin;
            }

            return result;
        }

        #endregion

    }
}
