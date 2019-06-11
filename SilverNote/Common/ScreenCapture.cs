/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SilverNote
{
    /// <summary>
    /// A collection of screen-capturing utilities
    /// </summary>
    public static class ScreenCapture
    {
        /// <summary>
        /// Capture the entire screen
        /// </summary>
        /// <returns></returns>
        public static BitmapSource CaptureScreen()
        {
            Rect screenRect = SystemParameters.WorkArea;

            return CaptureScreen(screenRect);
        }

        /// <summary>
        /// Capture the specified region on the screen
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static BitmapSource CaptureScreen(Rect rect)
        {
            int x = (int)rect.X;
            int y = (int)rect.Y;
            int width = (int)rect.Width;
            int height = (int)rect.Height;

            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(x, y, 0, 0, bitmap.Size);

                    return Imaging.CreateBitmapSourceFromHBitmap(
                        bitmap.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        /// <summary>
        /// Capture the specified window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static BitmapSource CaptureWindow(IntPtr hWnd)
        {
            Win32.RECT rect;
            if (!Win32.GetWindowRect(hWnd, out rect))
            {
                Debug.Fail("GetWindowRect() failed");
                return null;
            }
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    IntPtr hDC = graphics.GetHdc();
                    Win32.PrintWindow(hWnd, hDC, 0);
                    graphics.ReleaseHdc(hDC);

                    return Imaging.CreateBitmapSourceFromHBitmap(
                        bitmap.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        public static BitmapSource CaptureForegroundWindow()
        {
            IntPtr hWnd = Win32.GetForegroundWindow();
            return CaptureWindow(hWnd);
        }

        private static BitmapSource CaptureWindow2(IntPtr hWnd)
        {
            // FYI: This doesn't work right - crappy quality images

            IntPtr hSourceDC = IntPtr.Zero;
            IntPtr hTargetDC = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;

            try
            {
                // Get the window's width and height
                Win32.RECT rect;
                Win32.GetWindowRect(hWnd, out rect);
                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;

                // Get the window's device context
                hSourceDC = Win32.GetDC(hWnd);

                // Create a target device context
                hTargetDC = Win32.CreateCompatibleDC(hSourceDC);
                hBitmap = Win32.CreateCompatibleBitmap(hSourceDC, width, height);
                Win32.SelectObject(hTargetDC, hBitmap);

                // Copy all pixels from source to target
                Win32.BitBlt(hTargetDC, 0, 0, width, height, hSourceDC, 0, 0, Win32.TernaryRasterOperations.SRCCOPY);

                // Return the bitmap as a managed BitmapSource
                return Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, 
                    IntPtr.Zero, 
                    Int32Rect.Empty, 
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            finally
            {
                // Cleanup
                Win32.DeleteObject(hBitmap);
                Win32.ReleaseDC(IntPtr.Zero, hSourceDC);
                Win32.ReleaseDC(IntPtr.Zero, hTargetDC);
            }
        }
    }
}
