/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Diagnostics;

namespace SilverNote
{
    public class HotKey
    {
        public HotKey(string name)
        {
            ID = Win32.GlobalAddAtom(name);
        }

        public ushort ID { get; private set; }

        public uint Modifiers { get; private set; }

        public uint Key { get; private set; }

        public string Value
        {
            get { return FormatHotKey(Modifiers, Key); }
        }

        #region Registration

        private static Dictionary<ushort, HotKey> RegisteredKeys = new Dictionary<ushort, HotKey>();

        public bool RegisterHotKey(string value)
        {
            uint modifiers;
            uint key;

            if (ParseHotKey(value, out modifiers, out key))
            {
                return RegisterHotKey(modifiers, key);
            }
            else
            {
                return false;
            }
        }

        public bool RegisterHotKey(uint modifiers, uint key)
        {
            if (!IsWindowHookSet)
            {
                SetWindowHook(Application.Current.MainWindow);
            }

            UnregisterHotKey();

            if (!Win32.RegisterHotKey(WindowInteropHelper.Handle, ID, modifiers, key))
            {
                var e = new Win32Exception(Marshal.GetLastWin32Error());
                string message = String.Format("Unable to register hotkey {0} (modifiers = {1}): {2}", key, modifiers, e.Message);
                Debug.WriteLine(message);
                return false;
            }

            Modifiers = modifiers;
            Key = key;
            RegisteredKeys[ID] = this;

            RaiseValueChanged();
            return true;
        }

        public void UnregisterHotKey()
        {
            if (RegisteredKeys.ContainsKey(ID))
            {
                Win32.UnregisterHotKey(HwndSource.Handle, ID);
                RegisteredKeys.Remove(ID);
            }
        }

        #endregion

        #region Window Hook

        static WindowInteropHelper WindowInteropHelper { get; set; }

        static HwndSource HwndSource { get; set; }

        public static bool IsWindowHookSet
        {
            get { return WindowInteropHelper != null; }
        }

        public static void SetWindowHook(Window window)
        {
            ClearWindowHook();
            WindowInteropHelper = new WindowInteropHelper(window);
            HwndSource = HwndSource.FromHwnd(WindowInteropHelper.Handle);
            HwndSource.AddHook(WindowProc);
        }

        public static void ClearWindowHook()
        {
            if (HwndSource != null)
            {
                HwndSource.RemoveHook(WindowProc);
                HwndSource.Dispose();
                HwndSource = null;
                WindowInteropHelper = null;
            }
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.WM_HOTKEY:

                    HotKey hotKey;
                    if (RegisteredKeys.TryGetValue((ushort)wParam, out hotKey))
                    {
                        hotKey.RaisePressed();
                        handled = true;
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion

        #region Events

        public event EventHandler ValueChanged;

        private void RaiseValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler Pressed;

        private void RaisePressed()
        {
            if (Pressed != null)
            {
                Pressed(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Parsing/Formatting

        public static string FormatHotKey(uint modifiers, uint key)
        {
            return FormatModifiers(modifiers) + FormatKey(key);
        }

        public static string FormatModifiers(uint modifiers)
        {
            var buffer = new StringBuilder();

            if ((modifiers & Win32.MOD_CONTROL) != 0)
            {
                buffer.Append("Ctrl+");
            }

            if ((modifiers & Win32.MOD_ALT) != 0)
            {
                buffer.Append("Alt+");
            }

            if ((modifiers & Win32.MOD_SHIFT) != 0)
            {
                buffer.Append("Shift+");
            }

            if ((modifiers & Win32.MOD_WIN) != 0)
            {
                buffer.Append("Win+");
            }

            return buffer.ToString();
        }

        public static string FormatKey(uint key)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            Win32.ToUnicode(key, 0, keyboardState, buf, 256, 0);
            return buf.ToString().ToUpper();
        }

        public static bool ParseHotKey(string str, out uint modifiers, out uint key)
        {
            string[] tokens = str.Split("+ ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            modifiers = 0;
            key = 0;

            foreach (var token in tokens)
            {
                uint modifier = ParseModifier(token);
                if (modifier != 0)
                {
                    modifiers |= modifier;
                    continue;
                }

                uint value = ParseKey(token);
                if (value != 0)
                {
                    key = value;
                }
            }

            return true;
        }

        public static uint ParseModifier(string str)
        {
            switch (str.ToLower())
            {
                case "ctrl":
                case "control":
                case "ctl":
                    return Win32.MOD_CONTROL;
                case "alt":
                    return Win32.MOD_ALT;
                case "shift":
                    return Win32.MOD_SHIFT;
                case "win":
                case "window":
                case "windows":
                    return Win32.MOD_WIN;
                default:
                    return 0;
            }
        }

        public static uint ParseKey(string str)
        {
            if (str.Length != 1)
            {
                return 0;
            }
            
            return (uint)Win32.VkKeyScan(str.ToLower()[0]);
        }

        #endregion

    }
}
