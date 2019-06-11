/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Windows;
using System.Windows.Documents;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for PrintPreviewDialog.xaml
    /// </summary>
    public partial class PrintPreviewDialog : Window
    {
        public PrintPreviewDialog()
        {
            InitializeComponent();
        }

        public IDocumentPaginatorSource Document
        {
            get { return Viewer.Document; }
            set { Viewer.Document = value; }
        }
    }
}
