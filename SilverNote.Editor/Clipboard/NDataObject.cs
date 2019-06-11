/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using DOM;
using DOM.HTML;
using DOM.CSS;
using DOM.Views;

namespace SilverNote.Editor
{
    public static class NDataObject
    {
        #region Fields

        static List<IDataConverter> _DataConvereters;

        #endregion

        #region Properties

        public static IList<IDataConverter> DataConverters
        {
            get { return _DataConvereters ?? (_DataConvereters = CreateDataConverters()); }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Create an IDataObject containing the given set of items
        /// </summary>
        /// <param name="items">An array of NElement- or NPanel-derived objects</param>
        /// <returns>The newly-created IDataObject</returns>
        public static IDataObject CreateDataObject(IList<object> items)
        {
            var data = new DataObject();
            SetData(data, items);
            return data;
        }

        /// <summary>
        /// Get the list of supported data formats compatible with the given IDataObject
        /// </summary>
        /// <param name="data">The IDataObject whose formats are to be retrieved</param>
        /// <returns>A set of format names</returns>
        public static IList<string> GetFormats(IDataObject data)
        {
            return data.GetFormats().Where(IsSupportedFormat).ToArray();
        }

        /// <summary>
        /// Set the contents of the given IDataObject
        /// </summary>
        /// <param name="data">The IDataObject whose contents is to be set</param>
        /// <param name="items">An array of NElement- or NPanel-derived objects</param>
        public static void SetData(IDataObject data, IList<object> items)
        {
            // Replace any item that currently belongs to a panel with a clone of that item
            // so that it can be safely added to our own panel.
            items = GetSafeItems(items);

            foreach (var converter in DataConverters)
            {
                try
                {
                    converter.SetData(data, items);
                }
                catch (Exception e)
                {
                    Debug.Assert(false, e.Message);
                }
            }
        }

        /// <summary>
        /// Get the contents of the given IDataObject
        /// </summary>
        /// <param name="data">The IDatObject whose content is to be retrieved</param>
        /// <returns>A set of NElement- or NPanel-derived objects</returns>
        public static IList<object> GetData(IDataObject data)
        {
            return GetData(data, null);
        }

        /// <summary>
        /// Get the contents of the given IDataObject
        /// </summary>
        /// <param name="data">The IDatObject whose content is to be retrieved</param>
        /// <param name="format">Format of the contents to be retrieved</param>
        /// <returns>A set of NElement- or NPanel-derived objects</returns>
        public static IList<object> GetData(IDataObject data, string format)
        {
            foreach (var converter in DataConverters)
            {
                try
                {
                    if (String.IsNullOrEmpty(format) || format.Equals(converter.Format, StringComparison.OrdinalIgnoreCase))
                    {
                        if (data.GetDataPresent(converter.Format))
                        {
                            var results = converter.GetData(data);
                            if (results != null && results.Count > 0)
                            {
                                return results;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Assert(false, e.Message);
                }
            }

            return new object[0];
        }

        #endregion

        #region Implementation

        private static List<IDataConverter> CreateDataConverters()
        {
            _DataConvereters = new List<IDataConverter>();
            _DataConvereters.Add(new InternalDataConverter());
            _DataConvereters.Add(new FileDataConverter());
            _DataConvereters.Add(new BitmapDataConverter());
            _DataConvereters.Add(new HtmlDataConverter());
            _DataConvereters.Add(new TextDataConverter());
            return _DataConvereters;
        }

        static bool IsSupportedFormat(string format)
        {
            return DataConverters.Any((converter) => converter.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Transform the given collection of objects such that all UIElements
        /// that already belong to a visual container are replaced by a clone.
        /// 
        /// This ensures that all UIElements in the collection can be safely
        /// inserted into another panel for subsequent processing.
        /// </summary>
        static object[] GetSafeItems(IEnumerable<object> items)
        {
            var results = new List<object>();

            foreach (object item in items)
            {
                var element = item as DependencyObject;
                if (element == null || VisualTreeHelper.GetParent(element) == null)
                {
                    results.Add(item);
                    continue;
                }

                var cloneable = item as ICloneable;
                if (cloneable != null)
                {
                    results.Add(cloneable.Clone());
                }
            }

            return results.ToArray();
        }

        #endregion
    }
}
