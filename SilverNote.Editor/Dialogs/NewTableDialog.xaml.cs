/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for NewTableDialog.xaml
    /// </summary>
    public partial class NewTableDialog : Window
    {
        public NewTableDialog()
        {
            RowCount = 5;
            ColumnCount = 2;

            InitializeComponent();
        }

        public int RowCount { get; set; }

        public int ColumnCount { get; set; }
    }
}
