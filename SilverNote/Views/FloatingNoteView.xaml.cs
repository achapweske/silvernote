/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Editor;
using System.Windows;

namespace SilverNote.Views
{
    /// <summary>
    /// Interaction logic for FloatingNoteView.xaml
    /// </summary>
    public partial class FloatingNoteView : Window
    {
        public FloatingNoteView()
        {
            InitializeComponent();
        }

        public NoteEditor Editor
        {
            get { return this.NoteView.Editor; }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NoteView.Editor.Flush();
        }
    }
}
