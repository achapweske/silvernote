/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote.Editor
{
    public class TextDataConverter : IDataConverter
    {
        public string Format
        {
            get { return DataFormats.Text; }
        }

        public void SetData(IDataObject obj, IList<object> items)
        {
            StringBuilder buffer = new StringBuilder();

            foreach (var item in items)
            {
                buffer.Append(item.ToString());
            }

            string text = buffer.ToString();

            // Remove trailing newline

            if (text.EndsWith("\n"))
            {
                text = text.Remove(text.Length - 1);
            }
            if (text.EndsWith("\r"))
            {
                text = text.Remove(text.Length - 1);
            }
            
            obj.SetData(DataFormats.Text, text);
        }

        public IList<object> GetData(IDataObject obj)
        {
            string text = obj.GetData(DataFormats.Text) as string;
            if (text == null)
            {
                return new object[0];
            }

            var results = new List<object>();

            string[] separators = new[] { "\r\n", "\r", "\n" };
            string[] paragraphs = text.Split(separators, StringSplitOptions.None);

            foreach (string paragraph in paragraphs)
            {
                // Translate tab characters to indentation
                int indent = 0;
                while (indent < paragraph.Length && paragraph[indent] == '\t')
                {
                    indent++;
                }

                var newParagraph = new TextParagraph { Text = paragraph.Substring(indent) };
                newParagraph.LeftMargin = indent * 25;
                results.Add(newParagraph);
            }

            return results.ToArray();
        }
    }
}
