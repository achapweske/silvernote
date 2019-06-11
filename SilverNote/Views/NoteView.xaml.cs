/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using SilverNote.Editor;
using SilverNote.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SilverNote.Views
{
    /// <summary>
    /// Interaction logic for NoteView.xaml
    /// </summary>
    public partial class NoteView : UserControl
    {
        #region Fields

        ActivationMediator _ActivationMediator;
        ResourceMediator _ResourceMediator;
        HyperlinkMediator _HyperlinkMediator;

        #endregion

        #region Constructors

        public NoteView()
        {
            InitializeComponent();
        }

        #endregion
        
        #region Properties

        public NoteEditor Editor
        {
            get { return _Editor; }
        }

        #endregion

        #region Initialization

        protected override void OnInitialized(EventArgs e)
        {
            _ActivationMediator = new ActivationMediator(Editor);
            _ResourceMediator = new ResourceMediator(Editor);
            _HyperlinkMediator = new HyperlinkMediator(Editor);

            // Set _*Mediator.Context = this.DataContext
            DataContextChanged += (object sender, DependencyPropertyChangedEventArgs dce) =>
            {
                var newValue = dce.NewValue as NoteViewModel;

                _ActivationMediator.Context = newValue;
                _ResourceMediator.Context = newValue;
                _HyperlinkMediator.Context = newValue;
            };

            Messenger.Instance.Register("Flush", new Action<object>(Messenger_Flush));

            base.OnInitialized(e);  // Be sure to call the base implementation!
        }

        /*
         * Handle the global "Flush" message to save the current note
         */
        void Messenger_Flush(object model)
        {
            var note = DataContext as NoteViewModel;
            if (note == null || !note.Model.Source.IsOpen)
            {
                return;
            }

            if (model == null || model == note.Model || model == note.Notebook.Model || model == note.Repository.Model)
            {
                Editor.Flush();
            }
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Editor.Focus();
        }

        #endregion

        #region Implementation

        /*
         * Toggle the Find Text popup
         */
        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FindReplace.IsReplaceEnabled = false;

            FindReplace.Visibility = Visibility.Visible;

            if (FindReplace.FindText.Length > 0)
            {
                Editor.Find(FindReplace.FindText);
            }
        }

        /*
         * Toggle the Replace Text popup
         */
        private void ReplaceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FindReplace.IsReplaceEnabled = true;

            FindReplace.Visibility = Visibility.Visible;

            if (FindReplace.FindText.Length > 0)
            {
                Editor.Find(FindReplace.FindText);
            }
        }

        /*
         * Called when Find/Replace popup search text changes
         */
        private void FindReplace_FindTextChanged(object sender, RoutedEventArgs e)
        {
            if (FindReplace.Visibility == Visibility.Visible)
            {
                Editor.Find(FindReplace.FindText);
            }
        }

        /*
         * Called when Find/Replace popup closes
         */
        private void FindReplace_Closed(object sender, RoutedEventArgs e)
        {
            FindReplace.Visibility = Visibility.Collapsed;

            Editor.Find("");

            Editor.Focus();
        }

        /*
         * Called when scroller properties change
         */
        private void Scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Redraw editor when viewport width changes

            if (e.ViewportWidthChange != 0.0)
            {
                Editor.InvalidateMeasure();
            }
        }

        #endregion

    }
}
