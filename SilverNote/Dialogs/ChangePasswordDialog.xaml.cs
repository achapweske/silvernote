/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using System;
using System.ComponentModel;
using System.Security;
using System.Windows;
using System.Windows.Media;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for ChangePasswordDialog.xaml
    /// </summary>
    public partial class ChangePasswordDialog : Window
    {
        public ChangePasswordDialog()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HasPasswordProperty = DependencyProperty.Register(
            "HasPassword",
            typeof(bool),
            typeof(ChangePasswordDialog),
            new PropertyMetadata(false)
        );

        public bool HasPassword
        {
            get { return (bool)GetValue(HasPasswordProperty); }
            set { SetValue(HasPasswordProperty, value); }
        }

        public SecureString OldPassword { get; set; }
        public SecureString NewPassword { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasPassword)
            {
                OldPasswordTextBox.Focus();
            }
            else
            {
                NewPasswordTextBox.Focus();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                var newPassword = NewPasswordTextBox.SecurePassword;
                var confirmPassword = ConfirmPasswordTextBox.SecurePassword;

                if (!SecureStringExtensions.Equals(newPassword, confirmPassword))
                {
                    MessageTextBlock.Text = "The passwords you entered do not match.";
                    MessageTextBlock.Visibility = Visibility.Visible;
                    NewPasswordLabel.Foreground = Brushes.Red;
                    ConfirmPasswordLabel.Foreground = Brushes.Red;
                    NewPasswordTextBox.Focus();
                    NewPasswordTextBox.SelectAll();
                    e.Cancel = true;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DialogResult == true)
            {
                OldPassword = OldPasswordTextBox.SecurePassword;
                NewPassword = NewPasswordTextBox.SecurePassword;
                HasPassword = NewPassword != null && NewPassword.Length > 0;
            }
        }

    }
}
