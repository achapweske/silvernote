/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Common
{
    public class Array2D<T> where T : class
    {
        public Array2D()
            : this(0)
        {

        }

        public Array2D(int rowCount)
        {
            _Items = new List<List<T>>();
            
            for (int i = 0; i < rowCount; i++)
            {
                _Items.Add(new List<T>());
            }

            _ColumnCount = 0;
        }

        private List<List<T>> _Items;
        private int _ColumnCount;

        /// <summary>
        /// Get the number of rows in this array
        /// </summary>
        public int RowCount
        {
            get { return _Items.Count; }
        }

        /// <summary>
        /// Get the number of columns in this array
        /// </summary>
        public int ColumnCount
        {
            get { return _ColumnCount; }
        }

        /// <summary>
        /// Set the value at the given row and column.
        /// 
        /// The array is automatically expanded as needed to accommodate the new value.
        /// </summary>
        /// <param name="rowIndex">Row index of the value to be set</param>
        /// <param name="columnIndex">Column index of the value to be set</param>
        /// <param name="newValue">The new value being set</param>
        public void SetValue(int rowIndex, int columnIndex, T newValue)
        {
            while (rowIndex >= _Items.Count)
            {
                _Items.Add(null);
            }

            List<T> row = _Items[rowIndex];

            if (row == null)
            {
                row = new List<T>();
                _Items[rowIndex] = row;
            }

            while (columnIndex >= row.Count)
            {
                row.Add(null);
            }

            row[columnIndex] = newValue;

            _ColumnCount = Math.Max(_ColumnCount, columnIndex + 1);
        }

        /// <summary>
        /// Get the value at the given row and column.
        /// </summary>
        /// <param name="rowIndex">Row index of the value to be retrieved</param>
        /// <param name="columnIndex">Column index of the value to be retrieved</param>
        /// <returns>The retrieved value, or null if no value is set for the given row and column</returns>
        public T GetValue(int rowIndex, int columnIndex)
        {
            if (rowIndex >= _Items.Count)
            {
                return null;
            }

            List<T> row = _Items[rowIndex];

            if (row == null || columnIndex >= row.Count)
            {
                return null;
            }

            return row[columnIndex];
        }

        /// <summary>
        /// Convert this array to a native 2-d array
        /// </summary>
        /// <returns></returns>
        public T[,] ToArray()
        {
            var result = new T[RowCount, ColumnCount];

            for (int rowIndex = 0; rowIndex < RowCount; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < ColumnCount; columnIndex++)
                {
                    result[rowIndex, columnIndex] = GetValue(rowIndex, columnIndex);
                }
            }

            return result;
        }
    }
}
