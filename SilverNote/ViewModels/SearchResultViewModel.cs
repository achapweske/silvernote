/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using SilverNote.Common;
using SilverNote.Models;
using SilverNote.Editor;

namespace SilverNote.ViewModels
{
    public class SearchResultViewModel : ViewModelBase<SearchResultModel, SearchResultViewModel>
    {
        #region Constructors

        protected override void OnInitialize()
        {
            Model.WhenPropertyChanged("Note", Model_NoteChanged);
            Model_NoteChanged(null, null);
        }

        #endregion

        #region Properties

        public SearchViewModel Search
        {
            get { return SearchViewModel.FromModel(Model.Search); }
        }

        public NoteViewModel Note
        {
            get { return NoteViewModel.FromModel(Model.Note); }
        }

        /// <summary>
        /// Most recent value returned by Model.Text.
        /// </summary>
        private string _ModelText;

        /// <summary>
        /// Most recent snippet returned by this.Text
        /// </summary>
        private Snippet _Snippet;

        public string Text
        {
            get 
            {
                if (Model.Text == null)
                {
                    return null;
                }

                if (!String.ReferenceEquals(Model.Text, _ModelText))
                {
                    _Snippet = null;
                    _ModelText = Model.Text;
                }

                // Determine where to begin our snippet

                int offset = _Selection.Item1;
                int length = _Selection.Item2;

                if (offset == -1)
                {
                    var match = Searchable.FindFirst(_ModelText, Search.SearchPattern, Search.SearchOptions);

                    offset = match.Item1;
                    length = match.Item2;
                }

                // Get the snippet

                if (_Snippet == null ||
                    _Snippet.Offset > offset ||
                    _Snippet.Offset + _Snippet.Text.Length < offset + length)
                {
                    _Snippet = GetSnippet(_ModelText, offset, length);
                }

                string text = _Snippet.Text;
                offset -= _Snippet.Offset;

                // Remove note title

                if (_Snippet.Offset == 0 && Note != null && Note.Title != null && text.StartsWith(Note.Title))
                {
                    text = text.Remove(0, Note.Title.Length);

                    offset -= Note.Title.Length;

                    if (offset < 0)
                    {
                        offset = -1;
                    }
                }

                // Highlighted selected term

                if (_Selection.Item1 != -1)
                {
                    text = FormatText(text, Search.SearchPattern, Search.SearchOptions, offset);
                }
                else
                {
                    text = FormatText(text, Search.SearchPattern, Search.SearchOptions, -1);
                }

                return text;
            }
        }

        public string Title
        {
            get
            {
                if (Note == null || Note.Title == null)
                {
                    return null;
                }

                string title = Note.Title;

                if (_Selection.Item1 != -1 && _Selection.Item1 < title.Length && Model.Text != null && Model.Text.StartsWith(title))
                {
                    title = FormatText(title, Search.SearchPattern, Search.SearchOptions, _Selection.Item1);
                }
                else
                {
                    title = FormatText(title, Search.SearchPattern, Search.SearchOptions, -1);
                }

                return title;
            }
        }

        private bool _IsSelected;

        /// <summary>
        /// Get/set whether or not this search result is selected
        /// </summary>
        public bool IsSelected
        {
            get 
            { 
                return _IsSelected; 
            }
            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region Operations

        /// <summary>
        /// (offset, length) of the currently-selected text
        /// </summary>
        private Tuple<int,int> _Selection = new Tuple<int,int>(-1, 0);

        /// <summary>
        /// Index of the currently-selected term.
        /// 
        /// If _SelectionIndex > 0, then _SelectionOffset is the nth instance
        /// of the search term within _Text if one starts at the start of _Text
        /// and searches forward.
        /// 
        /// If _SelectionIndex &lt; 0, then _SelectionOffset is the -nth instance
        /// of the search term within _Text if one starts at the end of _Text and
        /// searches backward.
        /// 
        /// If _SelectionOffset is not valid, then _SelectionIndex = 0.
        /// </summary>
        private int _SelectionIndex;

        public int SelectionIndex
        {
            get { return _SelectionIndex; }
        }

        /// <summary>
        /// Unselect any selected search terms
        /// </summary>
        public void UnselectAll()
        {
            _Selection = Tuple.Create(-1, 0);
            _SelectionIndex = 0;

            RaisePropertyChanged("Title");
            RaisePropertyChanged("Text");
        }

        /// <summary>
        /// Select the first instance of a search term
        /// </summary>
        /// <returns>true on success, or false if this result contains no search terms</returns>
        public bool SelectFirstTerm()
        {
            if (!String.ReferenceEquals(Model.Text, _ModelText))
            {
                _Snippet = null;
                _ModelText = Model.Text;
            }

            // Update SelectionOffset

            if (!String.IsNullOrEmpty(Search.SearchString))
            {
                _Selection = Searchable.FindFirst(_ModelText, Search.SearchPattern, Search.SearchOptions);
            }
            else
            {
                _Selection = Tuple.Create(-1, 0);
            }

            // Update SelectionIndex

            if (_Selection.Item1 != -1)
            {
                _SelectionIndex = 1;
            }
            else
            {
                _SelectionIndex = 0;
            }

            RaisePropertyChanged("Title");
            RaisePropertyChanged("Text");

            return _SelectionIndex != 0;
        }

        /// <summary>
        /// Select the last instance of a search term
        /// </summary>
        /// <returns>true on success, or false if this result contains no search terms</returns>
        public bool SelectLastTerm()
        {
            if (!String.ReferenceEquals(Model.Text, _ModelText))
            {
                _Snippet = null;
                _ModelText = Model.Text;
            }

            // Update SelectionOffset

            if (!String.IsNullOrEmpty(Search.SearchString))
            {
                _Selection = Searchable.FindLast(_ModelText, Search.SearchPattern, Search.SearchOptions);
            }
            else
            {
                _Selection = Tuple.Create(-1, 0);
            }

            // Update SelectionIndex

            if (_Selection.Item1 != -1)
            {
                _SelectionIndex = -1;
            }
            else
            {
                _SelectionIndex = 0;
            }

            RaisePropertyChanged("Title");
            RaisePropertyChanged("Text");

            return _SelectionIndex != 0;
        }

        /// <summary>
        /// Select the next instance of a search term
        /// </summary>
        /// <returns>true on success, or false if not found</returns>
        public bool SelectNextTerm()
        {
            if (_Selection.Item1 == -1)
            {
                return SelectFirstTerm();
            }

            if (!String.ReferenceEquals(Model.Text, _ModelText))
            {
                _Snippet = null;
                _ModelText = Model.Text;
            }

            if (String.IsNullOrEmpty(Search.SearchString))
            {
                return false;
            }

            // Update SelectionOffset

            int startIndex = _Selection.Item1 + _Selection.Item2;

            _Selection = Searchable.FindNext(_ModelText, Search.SearchPattern, Search.SearchOptions, startIndex);

            // Update SelectionIndex

            if (_Selection.Item1 != -1)
            {
                _SelectionIndex += 1;
            }
            else
            {
                _SelectionIndex = 0;
            }

            RaisePropertyChanged("Title");
            RaisePropertyChanged("Text");

            return _SelectionIndex != 0;
        }

        /// <summary>
        /// Select the previous instance of a search term
        /// </summary>
        /// <returns>true on success, or false if not found</returns>
        public bool SelectPreviousterm()
        {
            if (_Selection.Item1 == -1)
            {
                return SelectLastTerm();
            }

            if (!String.ReferenceEquals(Model.Text, _ModelText))
            {
                _Snippet = null;
                _ModelText = Model.Text;
            }

            if (String.IsNullOrEmpty(Search.SearchString))
            {
                return false;
            }

            // Update SelectionOffset

            int startIndex = _Selection.Item1 - _Selection.Item2;

            _Selection = Searchable.FindPrevious(_ModelText, Search.SearchPattern, Search.SearchOptions, startIndex);

            // Update SelectionIndex

            if (_Selection.Item1 != -1)
            {
                _SelectionIndex -= 1;
            }
            else
            {
                _SelectionIndex = 0;
            }

            RaisePropertyChanged("Title");
            RaisePropertyChanged("Text");

            return _SelectionIndex != 0;
        }

        /// <summary>
        /// Open the note associated with this search result.
        /// 
        /// The note will automatically be set as the active note.
        /// </summary>
        public void Open()
        {
            if (Note != null)
            {
                Note.Notebook.SelectNote(Note, this);
            }
        }

        #endregion

        #region Commands

        private ICommand _OpenCommand = null;

        public ICommand OpenCommand
        {
            get
            {
                if (_OpenCommand == null)
                {
                    _OpenCommand = new DelegateCommand(o => Open());
                }
                return _OpenCommand;
            }
        }


        #endregion

        #region Implementation

        private NoteModel _Note;

        private void Model_NoteChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Model.Note != _Note)
            {
                if (_Note != null)
                {
                    _Note.WhenPropertyChanged("Title", Note_TitleChanged, false);
                }

                _Note = Model.Note;

                if (_Note != null)
                {
                    _Note.WhenPropertyChanged("Title", Note_TitleChanged, true);
                }

                RaisePropertyChanged("Title");
            }
        }

        private void Note_TitleChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("Title");
        }

        private class Snippet
        {
            public int Offset { get; set; }
            public string Text { get; set; }
        }

        /// <summary>
        /// Get a snippet that includes text in the range (offset, offset + length)
        /// </summary>
        private Snippet GetSnippet(string text, int maxOffset, int minLength)
        {
            const int MIN_PADDING = 12;
            const int MAX_PADDING = 32;

            // Start several characters ahead of maxOffset

            int offset = Math.Max(maxOffset - MIN_PADDING, 0);
            int padding = maxOffset - offset;

            // Try to start on a word boundary

            char[] whitespace = " \t\r\n".ToCharArray();
            int maxExtraPadding = Math.Min(MAX_PADDING - padding, offset);

            int wordAlignedOffset = text.LastIndexOfAny(whitespace, offset, maxExtraPadding);

            if (wordAlignedOffset != -1)
            {
                offset = wordAlignedOffset + 1;
            }

            if (offset > 0)
            {
                text = text.Substring(offset);

                // Insert leading ellipsis

                text = "..." + text;
                offset -= "...".Length;
            }

            // Limit text length

            if (text.Length > 500)
            {
                text = text.Substring(0, 500);

                // Insert trailing ellipsis

                text = text + "...";
            }

            return new Snippet { Text = text, Offset = offset };
        }

        /// <summary>
        /// Format text as XAML
        /// 
        /// This method applies a light highlighting to all instances of 
        /// search terms, and applies a darker highlighting to the currently-
        /// selected search term (if any)
        /// </summary>
        /// <param name="text">Text to be formatted</param>
        /// <param name="pattern">Regex pattern to highlight</param>
        /// <param name="options">Search options</param>
        /// <param name="selectionOffset"></param>
        /// <returns>XAML-formatted text</returns>
        private string FormatText(string text, string pattern, RegexOptions options, int selectionOffset)
        {
            var matches = Searchable.FindAll(text, pattern, options).ToArray();
            if (matches.Length == 0)
            {
                return XamlEscape(text);
            }

            var buffer = new StringBuilder();

            int offset = 0;

            foreach (var match in matches)
            {
                string substr = text.Substring(offset, match.Item1 - offset);
                substr = XamlEscape(substr);
                buffer.Append(substr);

                substr = text.Substring(match.Item1, match.Item2);
                substr = XamlEscape(substr);

                string background = (match.Item1 == selectionOffset) ? "DarkBlue" : "Yellow";
                string foreground = (match.Item1 == selectionOffset) ? "White" : "Black";

                string openTag = String.Format("<Span Background=\"{0}\" Foreground=\"{1}\">", background, foreground);
                string closeTag = "</Span>";

                buffer.Append(openTag + substr + closeTag);

                offset = match.Item1 + match.Item2;
            }

            string str = text.Substring(matches.Last().Item1 + matches.Last().Item2);
            str = XamlEscape(str);
            buffer.Append(str);

            return buffer.ToString();
        }

        /// <summary>
        /// XAML-escape the given string
        /// </summary>
        private static string XamlEscape(string str)
        {
            var buffer = new StringBuilder(str);
            buffer.Replace("&", "&amp;");
            buffer.Replace("<", "&lt;");
            buffer.Replace(">", "&gt;");
            return buffer.ToString();
        }

        #endregion
    }
}
