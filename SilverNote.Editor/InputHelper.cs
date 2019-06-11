/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SilverNote.Editor
{
    public static class InputHelper
    {
        public static bool IsNavigationKey(Key key)
        {
            switch (key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                case Key.PageUp:
                case Key.PageDown:
                case Key.Home:
                case Key.End:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsModifierKey(Key key)
        {
            switch (key)
            {
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LWin:
                case Key.RWin:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCommandStroke(KeyEventArgs e)
        {
            return
                e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) ||
                e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt) ||
                e.Key == Key.Escape;
        }

        public static bool IsEditingStroke(KeyEventArgs e)
        {
            return 
                !IsModifierKey(e.Key) && 
                !IsNavigationKey(e.Key) && 
                !IsCommandStroke(e) && 
                e.Key != Key.Tab;
        }
    }
}
