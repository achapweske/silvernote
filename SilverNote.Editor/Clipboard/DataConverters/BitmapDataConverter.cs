/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SilverNote.Editor
{
    public class BitmapDataConverter : IDataConverter
    {
        public string Format
        {
            get { return DataFormats.Bitmap; }
        }

        public void SetData(IDataObject obj, IList<object> items)
        {
            foreach (var item in items)
            {
                var bitmap = ToBitmap(item);
                if (bitmap != null)
                {
                    obj.SetData(DataFormats.Bitmap, bitmap);
                    break;
                }
            }
        }

        public IList<object> GetData(IDataObject obj)
        {
            var bitmap = obj.GetData(DataFormats.Bitmap) as BitmapSource;
            if (bitmap == null)
            {
                return new object[0];
            }

            var image = new NImage 
            { 
                Source = bitmap, 
                Positioning = Editor.Positioning.Overlapped 
            };
            return new[] { image };
        }

        static BitmapSource ToBitmap(object obj)
        {
            var image = obj as NImage;
            if (image != null)
            {
                return image.Source;
            }

            var canvas = obj as NCanvas;
            if (canvas != null)
            {
                return canvas.ToBitmap();
            }

            return null;
        }
    }
}
