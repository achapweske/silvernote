/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Editor;
using SilverNote.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote.Views
{
    public class ActivationMediator
    {
        #region Fields

        NoteEditor _Editor;
        NoteViewModel _Context;

        #endregion 

        #region Constructors

        public ActivationMediator(NoteEditor editor)
        {
            _Editor = editor;

            editor.Loaded += Editor_Loaded;
        }

        #endregion

        #region Properties

        /*
         * Get the associated editor, as set in the constructor
         */
        public NoteEditor Editor
        {
            get { return _Editor; }
        }

        /*
         * Get/set the associated note
         */
        public NoteViewModel Context
        {
            get
            {
                return _Context;
            }
            set
            {
                if (value != _Context)
                {
                    var oldValue = _Context;
                    _Context = value;
                    OnContextChanged(oldValue, value);
                }
            }
        }

        #endregion

        #region Methods

        public void Activate()
        {
            var activationContext = Context.ActivationContext as SearchResultViewModel;

            if (activationContext != null)
            {
                // FindReplace.FindText.Text = activationContext.Search.SearchString;

                if (activationContext.SelectionIndex != 0)
                {
                    Editor.SelectNth(activationContext.Search.SearchPattern, activationContext.Search.SearchOptions, activationContext.SelectionIndex);
                }
                else
                {
                    Editor.SelectNth(activationContext.Search.SearchPattern, activationContext.Search.SearchOptions, 1);
                }
            }

            Context.ActivationContext = null;
        }

        #endregion

        #region Implementation

        protected void OnContextChanged(NoteViewModel oldValue, NoteViewModel newValue)
        {
            if (oldValue != null)
            {
                oldValue.WhenPropertyChanged("ActivationContext", Context_ActivationContextChanged, false);
            }

            if (newValue != null)
            {
                newValue.WhenPropertyChanged("ActivationContext", Context_ActivationContextChanged, true);
            }
        }

        void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            Activate();
        }

        private void Context_ActivationContextChanged(object sender, PropertyChangedEventArgs e)
        {
            Activate();
        }

        #endregion
    }
}
