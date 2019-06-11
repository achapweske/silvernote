/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Input;
using System.Diagnostics;

namespace SilverNote.Behaviors
{
    public static class TextBlockBehavior
    {
        #region CollapseWhenEmpty

        public static bool GetCollapseWhenEmpty(DependencyObject dep)
        {
            return (bool)dep.GetValue(CollapseWhenEmptyProperty);
        }

        public static void SetCollapseWhenEmpty(DependencyObject dep, bool value)
        {
            dep.SetValue(CollapseWhenEmptyProperty, value);
        }

        public static readonly DependencyProperty CollapseWhenEmptyProperty = DependencyProperty.RegisterAttached(
            "CollapseWhenEmpty",
            typeof(bool),
            typeof(TextBlockBehavior),
            new UIPropertyMetadata(false, OnCollapseWhenEmptyChanged)
        );

        static void OnCollapseWhenEmptyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            if (!(dep is TextBlock))
            {
                Debug.Write("Error: " + e.Property + " cannot be applied to " + dep.GetType());
                return;
            }

            var textBlock = (TextBlock)dep;
            
            if ((bool)e.NewValue)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
                descriptor.AddValueChanged(textBlock, CollapseWhenEmptyTarget_TextChanged);
                CollapseIfEmpty(textBlock);
            }
            else
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
                descriptor.RemoveValueChanged(textBlock, CollapseWhenEmptyTarget_TextChanged);
                textBlock.Visibility = Visibility.Visible;
            }
        }

        static void CollapseWhenEmptyTarget_TextChanged(object sender, EventArgs e)
        {
            CollapseIfEmpty((TextBlock)sender);
        }

        static void CollapseIfEmpty(TextBlock textBlock)
        {
            if (textBlock.Text.Length > 0)
            {
                textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                textBlock.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Xaml

        public static readonly DependencyProperty Xaml = DependencyProperty.RegisterAttached(
            "Xaml",
            typeof(string),
            typeof(TextBlockBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnXamlChanged)
        );

        private static void OnXamlChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var newText = (string)e.NewValue;

            OnXamlChanged(textBlock, newText);
        }

        private static void OnXamlChanged(TextBlock textBlock, string newText)
        {
            textBlock.Inlines.Clear();

            if (string.IsNullOrEmpty(newText))
            {
                return;
            }

            try
            {
                string xaml = String.Format(
                    "<FlowDocument " +
                        "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                        "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                        "<Paragraph>{0}</Paragraph>" +
                    "</FlowDocument>",
                    newText
                );

                var document = XamlReader.Parse(xaml) as FlowDocument;
                if (document != null)
                {
                    var paragraph = document.Blocks.OfType<Paragraph>().FirstOrDefault();
                    if (paragraph != null)
                    {
                        textBlock.Inlines.AddRange(paragraph.Inlines.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "\n\n" + ex.StackTrace);
                textBlock.Text = newText;
            }
        }

        public static string GetXaml(DependencyObject target)
        {
            return (string)target.GetValue(Xaml);
        }

        public static void SetXaml(DependencyObject target, string value)
        {
            target.SetValue(Xaml, value);
        }

        #endregion


    }
}
