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
using System.Windows.Controls;
using System.Windows.Markup;
using System.IO;

namespace SilverNote.Views
{
    [ContentProperty("DataTemplates")]
    public class SimpleDataTemplateSelector : DataTemplateSelector
    {
        public SimpleDataTemplateSelector()
        {
            DataTemplates = new List<DataTemplate>();

            string xaml = "<DataTemplate><ContentControl Content=\"{Binding}\" /></DataTemplate>";

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xaml)))
            {
                var context = new ParserContext();
                context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
                DefaultTemplate = (DataTemplate)XamlReader.Load(stream, context);
            }
        }

        public List<DataTemplate> DataTemplates { get; set; }

        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            foreach (var dataTemplate in DataTemplates)
            {
                if (((Type)dataTemplate.DataType).IsInstanceOfType(item))
                {
                    return dataTemplate;
                }
            }

            return DefaultTemplate;
        }
    }
}
