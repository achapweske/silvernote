/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace SilverNote.Common
{
    public class DynamicUICommand : RoutedUICommand, INotifyPropertyChanged
    {
        #region Fields

        InputGesture _DefaultGesture;
        readonly bool _IsConfigurable;

        #endregion

        #region Constructors

        public DynamicUICommand()
            : base()
        {
            _IsConfigurable = true;
        }

        public DynamicUICommand(string text, string name, Type ownerType)
            : base(text, name, ownerType)
        {
            _IsConfigurable = true;
        }

        public DynamicUICommand(string text, string name, Type ownerType, InputGesture defaultGesture)
            : base(text, name, ownerType)
        {
            _IsConfigurable = true;
            DefaultGesture = defaultGesture;
        }

        public DynamicUICommand(string text, string name, Type ownerType, bool isConfigurable)
            : base(text, name, ownerType)
        {
            _IsConfigurable = isConfigurable;
        }

        public DynamicUICommand(string text, string name, Type ownerType, InputGesture defaultGesture, bool isConfigurable)
            : base(text, name, ownerType)
        {
            DefaultGesture = defaultGesture;
            _IsConfigurable = isConfigurable;
        }

        #endregion

        #region Properties

        public bool IsConfigurable
        {
            get { return _IsConfigurable; }
        }

        public InputGesture DefaultGesture
        {
            get { return GetDefaultInputGesture(); }
            set { SetDefaultInputGesture(value); }
        }

        public string DefaultGestureText
        {
            get { return FormatInputGesture(DefaultGesture); }
            set { DefaultGesture = ParseInputGesture(value); }
        }

        public InputGesture InputGesture
        {
            get {  return GetCurrentInputGesture(); }
            set { SetCurrentInputGesture(value); }
        }

        public string InputGestureText
        {
            get { return FormatInputGesture(InputGesture); }
            set { InputGesture = ParseInputGesture(value); }
        }

        #endregion

        #region Methods

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        #region Implementation

        InputGesture GetDefaultInputGesture()
        {
            return _DefaultGesture;
        }

        void SetDefaultInputGesture(InputGesture gesture)
        {
            _DefaultGesture = gesture;
            SetCurrentInputGesture(gesture);
        }

        InputGesture GetCurrentInputGesture()
        {
            if (InputGestures.Count > 0)
            {
                return InputGestures[0];
            }
            else
            {
                return null;
            }
        }

        void SetCurrentInputGesture(InputGesture gesture)
        {
            InputGestures.Clear();
            if (gesture != null)
            {
                InputGestures.Add(gesture);
            }

            RaisePropertyChanged("InputGesture");
            RaisePropertyChanged("InputGestureText");
        }

        static string FormatInputGesture(InputGesture gesture)
        {
            var keyGesture = gesture as KeyGesture;
            if (keyGesture != null)
            {
                return FormatKeyGesture(keyGesture);
            }

            var mouseGesture = gesture as MouseGesture;
            if (mouseGesture != null)
            {
                return FormatMouseGesture(mouseGesture);
            }

            return null;
        }

        static InputGesture ParseInputGesture(string str)
        {
            InputGesture gesture;
            if (TryParseInputGesture(str, out gesture))
            {
                return gesture;
            }
            else
            {
                return null;
            }
        }

        static bool TryParseInputGesture(string str, out InputGesture gesture)
        {
            if (String.IsNullOrEmpty(str))
            {
                gesture = null;
                return false;
            }

            return TryParseKeyGesture(str, out gesture) || TryParseMouseGesture(str, out gesture);
        }

        #region KeyGesture

        static KeyGestureConverter _KeyGestureConverter = new KeyGestureConverter();

        static string FormatKeyGesture(KeyGesture gesture)
        {
            if (!String.IsNullOrEmpty(gesture.DisplayString))
            {
                return gesture.DisplayString;
            }

            var buffer = new StringBuilder();

            if (gesture.Modifiers != ModifierKeys.None)
            {
                string modifiers = FormatModifierKeys(gesture.Modifiers);
                buffer.Append(modifiers);
                buffer.Append("+");
            }

            switch (gesture.Key)
            {
                case Key.OemCloseBrackets:
                    buffer.Append("]");
                    break;
                case Key.OemComma:
                    buffer.Append(",");
                    break;
                case Key.OemOpenBrackets:
                    buffer.Append("[");
                    break;
                case Key.OemPeriod:
                    buffer.Append(".");
                    break;
                case Key.OemPipe:
                    buffer.Append("\\");
                    break;
                case Key.OemPlus:
                    buffer.Append("=");
                    break;
                case Key.OemQuotes:
                    buffer.Append("'");
                    break;
                case Key.OemSemicolon:
                    buffer.Append(";");
                    break;
                case Key.PageDown:
                    buffer.Append("PageDown");
                    break;
                default:
                    // Fallback to the built-in formatter
                    return gesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
            }

            return buffer.ToString();
        }

        static bool TryParseKeyGesture(string str, out InputGesture gesture)
        {
            Key key = Key.None;
            ModifierKeys modifiers = ModifierKeys.None;

            string[] tokens = str.Split('+');

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i].Trim().ToLower();

                ModifierKeys modifier = ParseModifierKeys(token);
                if (modifier != ModifierKeys.None)
                {
                    modifiers |= modifier;
                    continue;
                }

                switch (token)
                {
                    case "]":
                    case "}":
                        key = Key.OemCloseBrackets;
                        break;
                    case ",":
                    case "<":
                        key = Key.OemComma;
                        break;
                    case "[":
                    case "{":
                        key = Key.OemOpenBrackets;
                        break;
                    case ".":
                    case ">":
                        key = Key.OemPeriod;
                        break;
                    case "\\":
                    case "|":
                        key = Key.OemPipe;
                        break;
                    case "+":
                    case "=":
                        key = Key.OemPlus;
                        break;
                    case "'":
                    case "\"":
                        key = Key.OemQuotes;
                        break;
                    case ";":
                    case ":":
                        key = Key.OemSemicolon;
                        break;
                    case "PageDown":
                        key = Key.PageDown;
                        break;
                    case "":
                        break;
                    default:
                        break;
                }
            }

            if (key == Key.None)
            {
                // Fallback to the built-in parser
                try
                {
                    gesture = _KeyGestureConverter.ConvertFromInvariantString(str) as KeyGesture;
                    return true;
                }
                catch
                {
                    gesture = null;
                    return false;
                }
            }

            gesture = new KeyGesture(key, modifiers);
            return true;
        }

        #endregion

        #region MouseGesture

        static MouseGestureConverter _MouseGestureConverter = new MouseGestureConverter();

        static string FormatMouseGesture(MouseGesture gesture)
        {
            return _MouseGestureConverter.ConvertToInvariantString(gesture);
        }

        static bool TryParseMouseGesture(string str, out InputGesture gesture)
        {
            try
            {
                gesture = _MouseGestureConverter.ConvertFromInvariantString(str) as MouseGesture;
                return true;
            }
            catch
            {
                gesture = null;
                return false;
            }
        }

        #endregion

        #region ModifierKeys

        static ModifierKeysConverter _ModifierKeysConverter = new ModifierKeysConverter();

        static string FormatModifierKeys(ModifierKeys modifiers)
        {
            return _ModifierKeysConverter.ConvertToString(modifiers);
        }

        static ModifierKeys ParseModifierKeys(string str)
        {
            switch (str.ToLower())
            {
                case "ctrl":
                    return ModifierKeys.Control;
                case "shift":
                    return ModifierKeys.Shift;
                case "alt":
                    return ModifierKeys.Alt;
                case "win":
                    return ModifierKeys.Windows;
                default:
                    return ModifierKeys.None;
            }

            // The following is too slow due to exception throwing
            /*
            try
            {
                return (ModifierKeys)_ModifierKeysConverter.ConvertFromString(str);
            }
            catch
            {
                return ModifierKeys.None;
            }
            */
        }

        #endregion

        #endregion
    }
}
