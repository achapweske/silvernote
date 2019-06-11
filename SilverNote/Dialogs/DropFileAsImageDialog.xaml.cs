/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Properties;
using System;
using System.Diagnostics;
using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for DropFileAsImageDialog.xaml
    /// </summary>
    public partial class DropFileAsImageDialog : Window
    {
        public DropFileAsImageDialog()
        {
            InsertAsFile = true;
            InsertAsImage = false;

            InitializeComponent();
        }

        public bool InsertAsFile { get; set; }
        public bool InsertAsImage { get; set; }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }

            base.OnClosed(e);
        }
    }
}
