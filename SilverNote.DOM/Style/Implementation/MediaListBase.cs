/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.Style.Internal
{
    /// <summary>
    /// The MediaList interface provides the abstraction of an ordered collection of media.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-Style/stylesheets.html#StyleSheets-MediaList
    /// </summary>
    public class MediaListBase : MediaList, IEnumerable<string>
    {
        #region Fields

        List<string> _Items = new List<string>();

        #endregion

        #region Constructors

        public MediaListBase()
        {

        }

        public MediaListBase(bool isReadOnly)
            : this()
        {
            IsReadOnly = isReadOnly;
        }

        public MediaListBase(IEnumerable<string> items)
            : this()
        {
            foreach (var item in items)
            {
                AppendMedium(item);
            }
        }

        public MediaListBase(IEnumerable<string> items, bool isReadOnly)
            : this(isReadOnly)
        {
            foreach (var item in items)
            {
                AppendMedium(item);
            }
        }

        #endregion

        #region IMediaList

        protected bool IsReadOnly { get; set; }

        /// <summary>
        /// The number of media in the list.
        /// </summary>
        public virtual int Length 
        {
            get { return _Items.Count; }
        }

        /// <summary>
        /// Returns the indexth in the list. If index is greater than or equal to the number of media in the list, this returns null.
        /// </summary>
        /// <param name="index">Index into the collection.</param>
        /// <returns>The medium at the indexth position in the MediaList, or null if that is not a valid index.</returns>
        public virtual string this[int index]
        {
            get
            {
                if (index >= 0 && index < _Items.Count)
                {
                    return _Items[index];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds the medium newMedium to the end of the list. If the newMedium is already used, it is first removed.
        /// </summary>
        /// <param name="newMedium">The new medium to add.</param>
        /// <exception cref="DOMException">
        /// INVALID_CHARACTER_ERR: If the medium contains characters that are invalid in the underlying style language.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this list is readonly.
        /// </exception>
        public virtual void AppendMedium(string newMedium)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            _Items.Remove(newMedium);
            _Items.Add(newMedium);

            RaiseCollectionChanged();
        }

        /// <summary>
        /// Deletes the medium indicated by oldMedium from the list.
        /// </summary>
        /// <param name="oldMedium">The medium to delete in the media list.</param>
        /// <exception cref="DOMException">
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this list is readonly.
        /// NOT_FOUND_ERR: Raised if oldMedium is not in the list.
        /// </exception>
        public virtual void DeleteMedium(string oldMedium)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            if (!_Items.Remove(oldMedium))
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }

            RaiseCollectionChanged();
        }

        /// <summary>
        /// The parsable textual representation of the media list. This is a comma-separated list of media.
        /// </summary>
        /// <exception cref="DOMException">
        /// SYNTAX_ERR: Raised if the specified string value has a syntax error and is unparsable.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this media list is readonly.
        /// </exception>
        public virtual string MediaText
        {
            get 
            {
                using (var writer = new StringWriter())
                {
                    Format(writer);
                    return writer.ToString();
                }
            }
            set 
            {
                MediaListBase mediaList = Parse(value);

                SuppressCollectionChanged = true;

                try
                {
                    for (int i = Length - 1; i >= 0; i--)
                    {
                        DeleteMedium(this[i]);
                    }

                    foreach (var medium in mediaList)
                    {
                        AppendMedium(medium);
                    }
                }
                finally
                {
                    SuppressCollectionChanged = false;
                }

                RaiseCollectionChanged();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when one or more media have been added or removed from this list
        /// </summary>
        public event EventHandler CollectionChanged;

        /// <summary>
        /// Suppress the CollectionChanged event
        /// </summary>
        protected bool SuppressCollectionChanged { get; set; }

        /// <summary>
        /// Raise the CollectionChanged event
        /// </summary>
        protected void RaiseCollectionChanged()
        {
            if (!SuppressCollectionChanged && CollectionChanged != null)
            {
                CollectionChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Parsing

        public static MediaListBase Parse(string str)
        {
            MediaListBase result;

            if (TryParse(str, out result))
            {
                return result;
            }
            else
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        public static bool TryParse(string str, out MediaListBase result)
        {
            result = new MediaListBase();

            string[] items = str.Split(',');

            foreach (var item in items)
            {
                if (!String.IsNullOrWhiteSpace(item))
                {
                    result.AppendMedium(item.Trim());
                }
            }

            return true;
        }

        #endregion

        #region Formatting

        protected void Format(TextWriter writer)
        {
            int length = this.Length;

            if (length > 0)
            {
                writer.Write(this[0]);

                for (int i = 1; i < length; i++)
                {
                    writer.Write(',');
                    writer.Write(this[i]);
                }
            }
        }

        #endregion

        #region IEnumerable

        public IEnumerator<string> GetEnumerator()
        {
            int length = this.Length;

            for (int i = 0; i < length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Object

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                Format(writer);
                return writer.ToString();
            }
        }

        #endregion
    }
}
