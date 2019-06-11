/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using SilverNote.Common;

namespace SilverNote.Common
{
    public static class Images
    {
        const string IMAGES_PATH = "pack://application:,,,/SilverNote;component/Images/";

        public static Image GetImage(string name)
        {
            try
            {
                var baseUri = new Uri(IMAGES_PATH, UriKind.Absolute);
                var imageUri = new Uri(baseUri, name);

                return new Image { Source = new BitmapImage(imageUri) };
            }
            catch (Exception e)
            {
                Debug.WriteLine(name + ": " + e.Message);
                return null;
            }
        }

        public static Image GetImage(string name, double width, double height)
        {
            Image image = GetImage(name);
            
            if (image != null)
            {
                image.Width = width;
                image.Height = height;
            }

            return image;
        }

        public static ImageSource GetFileIcon(string filepath, bool smallIcon = true)
        {
            uint attributes = Win32.FILE_ATTRIBUTE_NORMAL;

            uint flags = Win32.SHGFI_ICON | Win32.SHGFI_USEFILEATTRIBUTES;
            if (smallIcon)
            {
                flags |= Win32.SHGFI_SMALLICON;
            }

            Win32.SHFILEINFO shfi = new Win32.SHFILEINFO();

            if (Win32.SHGetFileInfo(
                filepath,
                attributes,
                ref shfi,
                (uint)Marshal.SizeOf(shfi),
                flags) != IntPtr.Zero)
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    shfi.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            else
            {
                return null;
            }
        }
    }
}
