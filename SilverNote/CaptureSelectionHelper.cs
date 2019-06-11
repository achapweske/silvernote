/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Diagnostics;
using SilverNote.Editor;
using System.Threading;
using System.Windows.Input;

namespace SilverNote
{
    public static class CaptureSelectionHelper
    {
        public static bool Copy(IntPtr hWnd)
        {
            var timer = new Stopwatch();
            timer.Start();

            var data = NClipboard.GetDataObject();

            if (hWnd != IntPtr.Zero)
            {
                SetForegroundWindow(hWnd, 1000);
            }

            // Make sure alt isn't pressed, or the the Ctrl+C may be 
            // interpreted as Ctrl+Alt+C, which is the default hotkey
            // assigned to this operation.
            
            while (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                if (timer.ElapsedMilliseconds > 5000)
                {
                    return false;
                }

                Thread.Sleep(20);
            }

            System.Windows.Forms.SendKeys.SendWait("^c");

            timer.Restart();

            do
            {
                if (!Clipboard.IsCurrent(data))
                {
                    break;
                }

                Thread.Sleep(20);

            } while (timer.ElapsedMilliseconds < 1000);

            Thread.Sleep(500);

            return true;
        }

        private static bool SetForegroundWindow(IntPtr hWnd, int timeoutInMilliseconds)
        {
            var timer = new Stopwatch();
            timer.Start();

            do
            {
                Win32.SetForegroundWindow(hWnd);

                if (Win32.GetForegroundWindow() == hWnd)
                {
                    return true;
                }

                Thread.Sleep(20);

            } while (timer.ElapsedMilliseconds < timeoutInMilliseconds);

            return false;
        }

        public static string GetSource(IntPtr hWnd)
        {
            if (Clipboard.ContainsText(TextDataFormat.Html))
            {
                string data = Clipboard.GetText(TextDataFormat.Html);
                var html = new SilverNote.Editor.NHtmlData(data);
                if (!String.IsNullOrEmpty(html.SourceURL))
                {
                    return html.SourceURL;
                }
            }

            if (hWnd == IntPtr.Zero)
            {
                hWnd = Win32.GetForegroundWindow();
            }

            return Win32.GetWindowText(hWnd);
        }

        public static UIElement[] FormatHeader(string source)
        {
            var results = new List<UIElement>();

            var paragraph = new TextParagraph();

            paragraph.SetProperty(TextProperties.FontWeightProperty, FontWeights.Bold);

            Uri sourceURI;
            if (Uri.TryCreate(source, UriKind.Absolute, out sourceURI))
            {
                paragraph.Text = "Captured from " + sourceURI.Host + ":";
                paragraph.SelectionBegin = "Captured from ".Length;
                paragraph.SelectionEnd = paragraph.Length - 1;
                paragraph.SetProperty(TextProperties.FontWeightProperty, FontWeights.Normal);
                paragraph.SetProperty(TextProperties.HyperlinkURLProperty, source);
            }
            else
            {
                paragraph.Text = "Captured from " + source + ":";
                paragraph.SelectionBegin = "Captured from ".Length;
                paragraph.SelectionEnd = paragraph.Length - 1;
                paragraph.SetProperty(TextProperties.FontWeightProperty, FontWeights.Normal);
                paragraph.SetProperty(TextProperties.FontStyleProperty, FontStyles.Italic);
            }

            results.Add(new TextParagraph());
            results.Add(new TextParagraph());
            results.Add(paragraph);
            results.Add(new TextParagraph());
            results.Add(new TextParagraph());

            return results.ToArray();
        }
    }
}
