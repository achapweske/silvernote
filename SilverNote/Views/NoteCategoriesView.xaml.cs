/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Dialogs;
using SilverNote.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SilverNote.Views
{
    /// <summary>
    /// Interaction logic for NoteCategoriesView.xaml
    /// </summary>
    public partial class NoteCategoriesView : UserControl
    {
        public NoteCategoriesView()
        {
            InitializeComponent();
        }

        public NoteViewModel NoteViewModel
        {
            get { return DataContext as NoteViewModel; }
        }

        private void EditCategories_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditCategoriesDialog();
            dialog.Owner = Window.GetWindow(this);
            dialog.DataContext = NoteViewModel.Notebook;
            dialog.ShowDialog();
        }
    }
}
