﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SilverNote.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ClientID {
            get {
                return ((int)(this["ClientID"]));
            }
            set {
                this["ClientID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int ResourceID {
            get {
                return ((int)(this["ResourceID"]));
            }
            set {
                this["ResourceID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::SilverNote.Models.LoginCollection RecentLogins {
            get {
                return ((global::SilverNote.Models.LoginCollection)(this["RecentLogins"]));
            }
            set {
                this["RecentLogins"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Maximized")]
        public global::System.Windows.WindowState MainWindowState {
            get {
                return ((global::System.Windows.WindowState)(this["MainWindowState"]));
            }
            set {
                this["MainWindowState"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1024,768")]
        public global::System.Windows.Size MainWindowSize {
            get {
                return ((global::System.Windows.Size)(this["MainWindowSize"]));
            }
            set {
                this["MainWindowSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0,0")]
        public global::System.Windows.Point MainWindowPosition {
            get {
                return ((global::System.Windows.Point)(this["MainWindowPosition"]));
            }
            set {
                this["MainWindowPosition"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public global::System.Windows.WindowState QuickNoteState {
            get {
                return ((global::System.Windows.WindowState)(this["QuickNoteState"]));
            }
            set {
                this["QuickNoteState"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("550,300")]
        public global::System.Windows.Size QuickNoteSize {
            get {
                return ((global::System.Windows.Size)(this["QuickNoteSize"]));
            }
            set {
                this["QuickNoteSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0,0")]
        public global::System.Windows.Point QuickNotePosition {
            get {
                return ((global::System.Windows.Point)(this["QuickNotePosition"]));
            }
            set {
                this["QuickNotePosition"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShowWelcomeDialog {
            get {
                return ((bool)(this["ShowWelcomeDialog"]));
            }
            set {
                this["ShowWelcomeDialog"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public double SearchPaneHeight {
            get {
                return ((double)(this["SearchPaneHeight"]));
            }
            set {
                this["SearchPaneHeight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Ctrl+Shift+N")]
        public string NewNoteHotKey {
            get {
                return ((string)(this["NewNoteHotKey"]));
            }
            set {
                this["NewNoteHotKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Ctrl+Alt+C")]
        public string CaptureSelectionHotKey {
            get {
                return ((string)(this["CaptureSelectionHotKey"]));
            }
            set {
                this["CaptureSelectionHotKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Ctrl+Alt+S")]
        public string CaptureScreenHotKey {
            get {
                return ((string)(this["CaptureScreenHotKey"]));
            }
            set {
                this["CaptureScreenHotKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SearchPaneAutoHide {
            get {
                return ((bool)(this["SearchPaneAutoHide"]));
            }
            set {
                this["SearchPaneAutoHide"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::SilverNote.LookupCollection LookupCommands {
            get {
                return ((global::SilverNote.LookupCollection)(this["LookupCommands"]));
            }
            set {
                this["LookupCommands"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::SilverNote.Commands.NShortcutCollection Shortcuts {
            get {
                return ((global::SilverNote.Commands.NShortcutCollection)(this["Shortcuts"]));
            }
            set {
                this["Shortcuts"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool HasDropFileAsImage {
            get {
                return ((bool)(this["HasDropFileAsImage"]));
            }
            set {
                this["HasDropFileAsImage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DropFileAsImage {
            get {
                return ((bool)(this["DropFileAsImage"]));
            }
            set {
                this["DropFileAsImage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RememberSettings {
            get {
                return ((bool)(this["RememberSettings"]));
            }
            set {
                this["RememberSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Ctrl+Alt+R")]
        public string CaptureRegionHotKey {
            get {
                return ((string)(this["CaptureRegionHotKey"]));
            }
            set {
                this["CaptureRegionHotKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Ctrl+Alt+W")]
        public string CaptureWindowHotKey {
            get {
                return ((string)(this["CaptureWindowHotKey"]));
            }
            set {
                this["CaptureWindowHotKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsLeftPaneVisible {
            get {
                return ((bool)(this["IsLeftPaneVisible"]));
            }
            set {
                this["IsLeftPaneVisible"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsRightPaneVisible {
            get {
                return ((bool)(this["IsRightPaneVisible"]));
            }
            set {
                this["IsRightPaneVisible"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UpgradeRequired {
            get {
                return ((bool)(this["UpgradeRequired"]));
            }
            set {
                this["UpgradeRequired"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("240")]
        public double LeftPaneWidth {
            get {
                return ((double)(this["LeftPaneWidth"]));
            }
            set {
                this["LeftPaneWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("235")]
        public double RightPaneWidth {
            get {
                return ((double)(this["RightPaneWidth"]));
            }
            set {
                this["RightPaneWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::SilverNote.Models.NotebookLocationCollection RecentNotebooks {
            get {
                return ((global::SilverNote.Models.NotebookLocationCollection)(this["RecentNotebooks"]));
            }
            set {
                this["RecentNotebooks"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::SilverNote.Models.RepositoryLocationCollection RecentRepositories {
            get {
                return ((global::SilverNote.Models.RepositoryLocationCollection)(this["RecentRepositories"]));
            }
            set {
                this["RecentRepositories"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.DateTime InstalledTime {
            get {
                return ((global::System.DateTime)(this["InstalledTime"]));
            }
            set {
                this["InstalledTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool HasInstalledTime {
            get {
                return ((bool)(this["HasInstalledTime"]));
            }
            set {
                this["HasInstalledTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Version OriginalVersion {
            get {
                return ((global::System.Version)(this["OriginalVersion"]));
            }
            set {
                this["OriginalVersion"] = value;
            }
        }
    }
}
