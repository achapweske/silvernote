/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Collections.Generic;
using SilverNote.Editor;
using System.Text;

namespace SilverNote.ViewModels.CategoryTree
{
    [ValueConversion(typeof(IEnumerable<ITreeNode>), typeof(List<Uri>))]
    public class ClipboardConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nodes = (IEnumerable<ITreeNode>)value;
            if (nodes == null)
            {
                return null;
            }

            var data = new DataObject();
            
            // Add Uri list to data object
            List<Uri> uriList = Convert(nodes);
            data.SetData(uriList);

            // Add hyperlinks to data object
            string html = ToHtml(nodes);
            if (!String.IsNullOrEmpty(html))
            {
                data.SetData(DataFormats.Html, html);
            }

            return data;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        private static string ToHtml(IEnumerable<ITreeNode> nodes)
        {
            var buffer = new StringBuilder();

            var searchResults = nodes.OfType<SearchResultNode>();
            if (searchResults.Any())
            {
                buffer.Append("<html><body>");
                foreach (var node in nodes.OfType<SearchResultNode>())
                {
                    if (buffer.Length > 0) buffer.Append("<br/>");
                    string uri = node.Uri.ToString();
                    int i = uri.LastIndexOf("/note");
                    if (i != -1) uri = uri.Substring(i);
                    buffer.AppendFormat("<a href=\"{0}\">{1}</a>", uri, node.SearchResult.Title);
                }
                buffer.Append("</body></html>");
            }

            if (buffer.Length > 0)
            {
                var data = new NHtmlData { Html = buffer.ToString() };
                return data.ToString();
            }
            else
            {
                return "";
            }
        }

        public static List<Uri> Convert(IEnumerable<ITreeNode> nodes)
        {
            return Normalize(nodes).Select(node => node.Uri).ToList();
        }

        public static IEnumerable<ITreeNode> Normalize(IEnumerable<ITreeNode> nodes)
        {
            var categories = nodes.OfType<CategoryNode>().ToArray();

            return nodes.Where(node => !HasAncestor(node, categories));
        }

        /// <summary>
        /// Determine if the given collection includes any ancestors of the given node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        private static bool HasAncestor(ITreeNode node, IEnumerable<CategoryNode> categories)
        {
            node = node.Parent;

            while (node != null)
            {
                if (categories.Contains(node))
                {
                    return true;
                }

                node = node.Parent;
            }

            return false;
        }

    }
}
