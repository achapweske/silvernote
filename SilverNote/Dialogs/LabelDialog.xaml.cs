/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for LabelDialog.xaml
    /// </summary>
    public partial class LabelDialog : Window
    {
        public LabelDialog()
        {
            InitializeComponent();
        }

        public string Label { get; set; }
    }
}
