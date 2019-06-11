/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Style.Internal
{
    public class MutableStyleSheetList : StyleSheetListBase
    {
        #region Constructors

        public MutableStyleSheetList()
        {
            _Items = new List<StyleSheet>();
        }

        public MutableStyleSheetList(IEnumerable<StyleSheetBase> items)
        {
            _Items = new List<StyleSheet>(items);
        }

        #endregion

        #region DOM

        private List<StyleSheet> _Items;

        public override int Length
        {
            get { return _Items.Count; }
        }

        public override StyleSheet this[int index]
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

        #endregion

        #region Extensions

        /// <summary>
        /// Adds the node newSheet to the end of this collection
        /// </summary>
        /// <param name="newSheet">The sheet to add.</param>
        /// <returns>The sheet added.</returns>
        public StyleSheet AppendSheet(StyleSheet newSheet)
        {
            _Items.Remove(newSheet);
            _Items.Add(newSheet);

            RaiseCollectionChanged(null, newSheet);

            return newSheet;
        }

        /// <summary>
        /// Inserts the node newSheet before the existing node refSheet. If refSheet is null, insert newSheet at the end of the list.
        /// </summary>
        /// <param name="newSheet">The sheet to insert.</param>
        /// <param name="refSheet">The reference sheet, i.e., the node before which the new sheet must be inserted.</param>
        /// <returns>The sheet being inserted</returns>
        public StyleSheet InsertBefore(StyleSheet newSheet, StyleSheet refSheet)
        {
            StyleSheet oldSheet = null;

            if (_Items.Remove(newSheet))
            {
                oldSheet = newSheet;
            }

            if (refSheet == null)
            {
                _Items.Add(newSheet);
                RaiseCollectionChanged(oldSheet, newSheet);
                return newSheet;
            }

            int index = _Items.IndexOf(refSheet);
            if (index != -1)
            {
                _Items.Insert(index, newSheet);
            }
            else
            {
                _Items.Add(newSheet);
            }

            RaiseCollectionChanged(oldSheet, newSheet);

            return newSheet;
        }

        /// <summary>
        /// Removes the style sheet indicated by oldSheet from this collection, and returns it.
        /// </summary>
        /// <param name="oldSheet">The sheet being removed.</param>
        /// <returns>The sheet removed.</returns>
        /// <exception cref="DOMException">NOT_FOUND_ERR: Raised if oldSheet is not found in this collection.</exception>
        public StyleSheet RemoveSheet(StyleSheet oldSheet)
        {
            if (!_Items.Remove(oldSheet))
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }

            RaiseCollectionChanged(oldSheet, null);

            return oldSheet;
        }

        #endregion

    }
}
