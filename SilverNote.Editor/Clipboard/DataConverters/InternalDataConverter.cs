/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;

namespace SilverNote.Editor
{
    public class InternalDataConverter : IDataConverter
    {
        public string Format
        {
            get { return "SilverNote"; }
        }

        public void SetData(IDataObject obj, IList<object> items)
        {
            DocumentPanel panel = new DocumentPanel();
            foreach (var item in items.OfType<UIElement>())
            {
                panel.Append(item);
            }

            try
            {
                panel.OwnerDocument = DOMFactory.CreateHTMLDocument();
                panel.OwnerDocument.SetUserStyleSheet(CSSParser.ParseStylesheet(NoteEditor.DEFAULT_STYLESHEET));
                panel.SetNodeName(HTMLElements.BODY);
                panel.OwnerDocument.Body.Bind(panel);
                panel.OwnerDocument.Body.UpdateStyle(deep: true);
                var clone = (HTMLDocument)panel.OwnerDocument.CloneNode(true);
                HTMLFilters.HtmlFormatFilter(clone);

                var files = new FileCollection();
                foreach (string fileName in panel.ResourceNames)
                {
                    files.Add(fileName, panel.GetResourceData(fileName));
                }

                string html = clone.ToString();
                obj.SetData(Format, html);
                obj.SetData(ResourceFormat, files);
            }
            finally
            {
                panel.RemoveAll();
            }
        }

        public IList<object> GetData(IDataObject obj)
        {
            string html = (string)obj.GetData(Format);
            var files = obj.GetData(ResourceFormat) as FileCollection;

            DocumentPanel panel = new DocumentPanel();
            panel.OwnerDocument = DOMFactory.CreateHTMLDocument();
            panel.OwnerDocument.SetUserStyleSheet(CSSParser.ParseStylesheet(NoteEditor.DEFAULT_STYLESHEET));
            panel.SetNodeName(HTMLElements.BODY);
            panel.OwnerDocument.Body.Bind(panel);
            panel.OwnerDocument.Open();
            panel.OwnerDocument.Write(html);
            panel.OwnerDocument.Close();
            HTMLFilters.HtmlParseFilter(panel.OwnerDocument);
            panel.RemoveAll();
            panel.OwnerDocument.Body.Bind(panel, render: true);

            if (files != null)
            {
                foreach (var file in files)
                {
                    panel.SetResourceData(file.Key, file.Value);
                }
            }

            var result = panel.Children.Cast<object>().ToArray();

            panel.RemoveAll();

            return result;
        }

        const string ResourceFormat = "SilverNote Files";

        [Serializable]
        class FileCollection : Dictionary<string, byte[]>
        {
            public FileCollection()
            { }

            public FileCollection(SerializationInfo info, StreamingContext context)
                : base(info, context)
            { }
        }

    }
}
