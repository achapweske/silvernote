/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Security;
using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        public PasswordDialog()
        {
            InitializeComponent();
        }

        public string Message { get; set; }

        public SecureString Password { get; private set; }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DialogResult == true)
            {
                Password = PasswordTextBox.SecurePassword;
            }
        }
    }
}
