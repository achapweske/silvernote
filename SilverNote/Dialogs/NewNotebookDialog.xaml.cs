/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for NewNotebookDialog.xaml
    /// </summary>
    public partial class NewNotebookDialog : Window
    {
        public NewNotebookDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Notebook name result
        /// </summary>
        public string NotebookName { get; set; }

        /// <summary>
        /// btnOK Click event handler
        /// </summary>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            NotebookName = NameTextBox.Text;

            this.DialogResult = true;
        }
    }
}
