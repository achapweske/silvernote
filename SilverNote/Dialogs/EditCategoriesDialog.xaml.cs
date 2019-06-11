/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for EditCategoriesDialog.xaml
    /// </summary>
    public partial class EditCategoriesDialog : Window
    {
        public EditCategoriesDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
