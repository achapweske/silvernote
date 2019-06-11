/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using DOM;
using System.Threading;

namespace SilverNote.Editor
{
    public static class NClipboard
    {
        #region Operations

        /// <summary>
        /// Set the contents of the system clipboard
        /// </summary>
        /// <param name="items">New clipboard contents</param>
        public static void SetData(IList<object> items)
        {
            var data = NDataObject.CreateDataObject(items);

            SetDataObject(data, true);
        }

        /// <summary>
        /// Get the contents of the system clipboard
        /// </summary>
        /// <returns>Current clipboard contents</returns>
        public static IList<object> GetData()
        {
            var data = GetDataObject();

            return NDataObject.GetData(data);
        }

        /// <summary>
        /// Get the contents of the system clipboard
        /// </summary>
        /// <param name="format">Format of the data to be retrieved.</param>
        /// <returns>Current clipboard contents</returns>
        public static IList<object> GetData(string format)
        {
            var data = GetDataObject();

            return NDataObject.GetData(data, format);
        }

        /// <summary>
        /// Get the list of formats that the clipboard data is stored as
        /// </summary>
        /// <returns>Format names</returns>
        public static IList<string> GetFormats()
        {
            var data = GetDataObject();

            return NDataObject.GetFormats(data);
        }

        /// <summary>
        /// Retrieves the clipboard's IDataObject
        /// </summary>
        /// <returns></returns>
        public static IDataObject GetDataObject()
        {
            // On failure, retry up to 9 times.
            //
            // Required for dealing with the CLIPBRD_E_CANT_OPEN error

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    return Clipboard.GetDataObject();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Warning: " + e.Message);
                    Thread.Sleep(10);
                }
            }

            return null;
        }

        public static void SetDataObject(object data, bool copy)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetDataObject(data, copy);
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Warning: " + e.Message);
                    Thread.Sleep(10);
                }
            }
        }

        #endregion
    }

}
