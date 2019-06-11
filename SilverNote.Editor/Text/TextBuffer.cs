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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.TextFormatting;
using System.Xml;
using DOM;
using DOM.HTML;
using DOM.SVG;

namespace SilverNote.Editor
{
    public class TextBuffer : TextSource, IEnumerable<TextRunSource>, ICloneable
    {
        #region Fields

        GenericTextRunProperties _DefaultProperties;
        TextRunSource _First;
        TextRunSource _Last;
        int _Length;
        string _CachedText;
        int _SuppressTextChanged;   // if > 0, TextChanged event is disabled

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new NTextBuffer, using the specified default text properties
        /// </summary>
        public TextBuffer()
            : this(TextProperties.Default)
        {

        }

        public TextBuffer(GenericTextRunProperties defaultProperties)
        {
            DefaultProperties = (GenericTextRunProperties)defaultProperties.Clone();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/set the text in this buffer as a string
        /// </summary>
        public string Text
        {
            get
            {
                if (_CachedText == null)
                {
                    _CachedText = GetText();
                }

                return _CachedText;
            }
            set
            {
                SetText(value);
            }
        }

        /// <summary>
        /// Total number of characters in this buffer
        /// </summary>
        public int Length
        {
            get { return _Length; }
            set { _Length = value; }
        }

        /// <summary>
        /// Default text properties.
        /// 
        /// These are applied to any new text runs added to an empty buffer.
        /// </summary>
        public GenericTextRunProperties DefaultProperties
        {
            get
            {
                return _DefaultProperties;
            }
            set
            {
                if (value != _DefaultProperties)
                {
                    var oldValue = _DefaultProperties;
                    _DefaultProperties = value;
                    OnDefaultPropertiesChanged(value, oldValue);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the text content of this buffer changes
        /// 
        /// Use the Text property to retrieve the new value
        /// </summary>
        public event EventHandler<TextChangedEventArgs> TextChanged;

        /// <summary>
        /// Raise the TextChanged event
        /// </summary>
        /// <param name="offset">Offset of the first character added/removed</param>
        /// <param name="numAdded">Number of characters added</param>
        /// <param name="numRemoved">Number of characters removed</param>
        private void RaiseTextChanged(int offset, int numAdded, int numRemoved)
        {
            if (TextChanged != null && _SuppressTextChanged == 0)
            {
                TextChanged(this, new TextChangedEventArgs(offset, numAdded, numRemoved));
            }
        }

        /// <summary>
        /// Invoked when a text formatting property changes
        /// </summary>
        public event EventHandler<TextFormatChangedEventArgs> FormatChanged;

        /// <summary>
        /// Raise the FormatChanged event
        /// </summary>
        /// <param name="offset">Offset of the first character affected</param>
        /// <param name="length">Number of characters affected</param>
        /// <param name="propertyName">Name of the property that has changed</param>
        private void RaiseFormatChanged(int offset, int length, string propertyName)
        {
            if (FormatChanged != null)
            {
                FormatChanged(this, new TextFormatChangedEventArgs(offset, length, propertyName));
            }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Insert the given text into this buffer
        /// </summary>
        public void Insert(int offset, string text)
        {
            var node = (offset > 0) ? Get(offset - 1) : First;

            if (node == null)
            {
                node = new TextRunSource(DefaultProperties);
                AddFirst(node);
            }

            node.Insert(offset - node.Offset, text);
        }

        /// <summary>
        /// Cut a range of text from this buffer
        /// </summary>
        public TextBuffer Cut(int offset, int length)
        {
            var result = Copy(offset, length);

            Delete(offset, length);

            return result;
        }

        /// <summary>
        /// Copy a range of text form this buffer
        /// </summary>
        public TextBuffer Copy(int offset, int length)
        {
            if (offset < 0 || offset > this.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (length < 0 || offset + length > this.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            var result = new TextBuffer(DefaultProperties);

            if (length == 0)
            {
                return result;
            }

            TextRunSource node = Get(offset);
            offset -= node.Offset;
            while (length > 0)
            {
                int ncopy = Math.Min(length, node.Length - offset);
                var newNode = node.Copy(offset, ncopy);
                result.AddLast(newNode);
                length -= newNode.Length;
                node = node.Next;
                offset = 0;
            }

            return result;
        }

        /// <summary>
        /// Paste a range of text into this buffer.
        /// 
        /// This inserts clones of the TextRuns from the given buffer, so that
        /// the same NTextBuffer can be pasted more than once.
        /// </summary>
        public void Paste(int offset, TextBuffer other)
        {
            TextRunSource insertBefore = Slice(offset);

            for (var node = other.First; node != null; node = node.Next)
            {
                AddBefore(insertBefore, (TextRunSource)node.Clone());
            }
        }

        /// <summary>
        /// Delete a range of text from this buffer
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Delete(int offset, int length)
        {
            if (length == 0)
            {
                return;
            }

            if (offset < 0 || offset >= this.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (offset + length > this.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            var node = Get(offset);

            int ndelete = Math.Min(length, node.Offset + node.Length - offset);
            Delete(node, offset - node.Offset, ndelete);
            int remaining = length - ndelete;

            while (remaining > 0)
            {
                node = node.Next;
                node.Offset = offset;
                ndelete = Math.Min(remaining, node.Length);
                Delete(node, 0, ndelete);
                remaining -= ndelete;
            }
        }

        /// <summary>
        /// Delete a range of text from a child text run
        /// </summary>
        private void Delete(TextRunSource run, int offset, int length)
        {
            if (length < run.Length)
            {
                run.Remove(offset, length);
            }
            else if (length == run.Length)
            {
                Remove(run);
            }
            else
            {
                throw new ArgumentOutOfRangeException("length");
            }
        }

        /// <summary>
        /// Get all properties of the text at the given offset
        /// </summary>
        public GenericTextRunProperties GetProperties(int index)
        {
            if (Length == 0)
            {
                return DefaultProperties;
            }
            else if (index == Length)
            {
                return Last.Properties;
            }
            else
            {
                return Get(index).Properties;
            }
        }
        
        /// <summary>
        /// Get all values for the given property within the given range
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public IList<TextPropertyValue> GetProperty(string propertyName, int index, int length)
        {
            var results = new List<TextPropertyValue>();
            int end = index + length;

            if (Length == 0)
            {
                var result = new TextPropertyValue
                {
                    Value = DefaultProperties.GetProperty(propertyName),
                    Length = 0
                };
                results.Add(result);
            }

            for (TextRunSource run = Get(index); run != null; run = run.Next)
            {
                var result = new TextPropertyValue
                {
                    Value = run.Properties.GetProperty(propertyName),
                    Length = run.Offset + run.Length - index 
                };
                results.Add(result);

                if ((index += result.Length) >= end)
                {
                    break;
                }
            }

            return results;
        }

        /// <summary>
        /// Set all properties for the given range of text.
        /// 
        /// Returns the previously-set properties for that text range.
        /// </summary>
        public IList<TextPropertiesValue> SetProperties(int index, int length, GenericTextRunProperties properties)
        {
            var results = new List<TextPropertiesValue>();

            if (Length == 0)
            {
                results.Add(new TextPropertiesValue
                {
                    Length = Length,
                    Value = DefaultProperties
                });

                DefaultProperties = (GenericTextRunProperties)properties.Clone();
            }
            else
            {
                TextRunSource begin = Slice(index);
                TextRunSource end = Slice(index + length);

                for (TextRunSource node = begin; node != end; node = node.Next)
                {
                    results.Add(new TextPropertiesValue
                    {
                        Length = node.Length,
                        Value = node.Properties
                    });

                    node.Properties = (GenericTextRunProperties)properties.Clone();
                }
            }

            return results;
        }

        /// <summary>
        /// Set a collection of properties for the given range of text
        /// 
        /// Returns the previously-set properties for that text range
        /// </summary>
        public IList<TextPropertiesValue> SetProperties(int index, int length, IList<TextPropertiesValue> values)
        {
            var results = new List<TextPropertiesValue>();

            int offset = index;
            foreach (var value in values)
            {
                var result = SetProperties(offset, value.Length, value.Value);
                results.AddRange(result);
                offset += value.Length;
            }

            return results;
        }

        /// <summary>
        /// Apply a series of values for the given text property.
        /// 
        /// Returns the previously-set property values for that range of text.
        /// </summary>
        public IList<TextPropertyValue> SetProperty(int index, int length, string name, IList<TextPropertyValue> values)
        {
            var results = new List<TextPropertyValue>();

            int offset = index;
            foreach (var value in values)
            {
                var result = SetProperty(offset, value.Length, name, value.Value);
                results.AddRange(result);
                offset += value.Length;
            }

            return results;
        }

        /// <summary>
        /// Apply a new value for the given text property.
        /// 
        /// Returns the previously-set property values for that range of text
        /// </summary>
        public IList<TextPropertyValue> SetProperty(int index, int length, string name, object value)
        {
            var results = new List<TextPropertyValue>();

            if (Length == 0)
            {
                results.Add(new TextPropertyValue
                {
                    Length = 0,
                    Value = DefaultProperties.GetProperty(name)
                });

                DefaultProperties.SetProperty(name, value);
            }
            else
            {
                TextRunSource begin = Slice(index);
                TextRunSource end = Slice(index + length);

                for (TextRunSource run = begin; run != end; run = run.Next)
                {
                    results.Add(new TextPropertyValue
                    {
                        Length = run.Length,
                        Value = run.Properties.GetProperty(name)
                    });

                    run.Properties.SetProperty(name, value);
                }
            }

            return results;
        }

        /// <summary>
        /// Change a property to newValue wherever it's currently set to oldValue.
        /// </summary>
        public int ChangeProperty(string name, object oldValue, object newValue)
        {
            int result = 0;

            if (Length == 0)
            {
                result += DefaultProperties.ChangeProperty(name, oldValue, newValue);
            }
            else
            {
                for (TextRunSource run = First; run != null; run = run.Next)
                {
                    result += run.Properties.ChangeProperty(name, oldValue, newValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Get the index of the first occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int IndexOf(char c)
        {
            return IndexOf(c, Length);
        }

        /// <summary>
        /// Get the index of the first occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int IndexOf(char c, int startIndex)
        {
            return IndexOf(c.ToString(), startIndex, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Get the index of the first occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int IndexOf(string value, StringComparison comparisonType)
        {
            return IndexOf(value, 0, comparisonType);
        }

        /// <summary>
        /// Get the index of the first occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int IndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            return Text.IndexOf(value, startIndex, comparisonType);
        }

        /// <summary>
        /// Get the index of the first occurance of any of the specified characters
        /// </summary>
        /// <param name="anyOf"></param>
        /// <returns>An index, or -1 if not found</returns>
        public int IndexOfAny(char[] anyOf)
        {
            return IndexOfAny(anyOf, 0);
        }

        /// <summary>
        /// Get the index of the first occurance of any of the specified characters
        /// </summary>
        /// <param name="anyOf"></param>
        /// <param name="startIndex">Start searching at this index</param>
        /// <returns>An index or -1 if not found</returns>
        public int IndexOfAny(char[] anyOf, int startIndex)
        {
            if (startIndex < 0 || startIndex > Length)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (startIndex == Length)
            {
                return -1;
            }

            TextRunSource run = Get(startIndex);
            string text = run.Text.ToString();
            startIndex -= run.Offset;

            int offset;
            while ((offset = text.IndexOfAny(anyOf, startIndex)) == -1)
            {
                if ((run = run.Next) == null)
                {
                    break;
                }
                text = run.Text.ToString();
                startIndex = 0;
            }

            if (run != null)
            {
                return run.Offset + offset;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get the index of the last occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int LastIndexOf(char c)
        {
            return LastIndexOf(c, Length);
        }

        /// <summary>
        /// Get the index of the last occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int LastIndexOf(char c, int startIndex)
        {
            return LastIndexOf(c.ToString(), startIndex, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Get the index of the last occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int LastIndexOf(string value)
        {
            return LastIndexOf(value, 0, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Get the index of the last occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int LastIndexOf(string value, StringComparison comparisonType)
        {
            return LastIndexOf(value, 0, comparisonType);
        }

        /// <summary>
        /// Get the index of the last occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int LastIndexOf(string value, int startIndex)
        {
            return Text.LastIndexOf(value, startIndex, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Get the index of the last occurance of the specified string
        /// </summary>
        /// <returns>An index, or -1 if not found</returns>
        public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            return Text.LastIndexOf(value, startIndex, comparisonType);
        }

        /// <summary>
        /// Get the index of the last occurance of any of the specified characters
        /// </summary>
        /// <param name="anyOf"></param>
        /// <returns>An index, or -1 if not found</returns>
        public int LastIndexOfAny(char[] anyOf)
        {
            return LastIndexOfAny(anyOf, Length);
        }

        /// <summary>
        /// Get the index of the last occurance of any of the specified characters
        /// </summary>
        /// <param name="anyOf"></param>
        /// <param name="startIndex">Start searching at this index</param>
        /// <returns>An index, or -1 if not found</returns>
        public int LastIndexOfAny(char[] anyOf, int startIndex)
        {
            if (anyOf == null)
            {
                throw new ArgumentNullException();
            }

            if (Length == 0)
            {
                return -1;
            }

            if (startIndex < 0 || startIndex >= Length)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }

            TextRunSource run = Get(startIndex);
            string text = run.Text.ToString();
            startIndex -= run.Offset;

            int offset;
            while ((offset = text.LastIndexOfAny(anyOf, startIndex)) == -1)
            {
                if ((run = run.Previous) == null)
                    break;
                text = run.Text.ToString();
                startIndex = text.Length - 1;
            }

            if (run != null)
            {
                return run.Offset + offset;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get the substring of text starting at the given index
        /// </summary>
        public string Substring(int startIndex)
        {
            return Substring(startIndex, Length - startIndex);
        }

        /// <summary>
        /// Get the substring of text starting at the given index
        /// </summary>
        public string Substring(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > Length)
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if (length < 0 || startIndex + length > Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (startIndex == Length)
            {
                return String.Empty;
            }

            string result = String.Empty;

            TextRunSource run = Get(startIndex);
            startIndex -= run.Offset;
            while (length > 0)
            {
                int n = Math.Min(length, run.Length - startIndex);
                result += run.Text.ToString().Substring(startIndex, n);
                length -= n;
                startIndex = 0;
                run = run.Next;
            }

            return result;
        }

        /// <summary>
        /// Find the first occurrence of the given pattern
        /// </summary>
        /// <param name="pattern">A regex pattern</param>
        /// <param name="options">Regex options</param>
        /// <returns>An (offset, length) pair, or (-1, 0) if not found</returns>
        public Tuple<int, int> FindFirst(string pattern, RegexOptions options)
        {
            return FindFirst(pattern, options, 0);
        }

        /// <summary>
        /// Find the first occurrence of the given pattern
        /// </summary>
        /// <param name="pattern">A regex pattern</param>
        /// <param name="options">Regex options</param>
        /// <param name="startIndex">Begin searching at this offset</param>
        /// <returns>An (offset, length) pair, or (-1, 0) if not found</returns>
        public Tuple<int, int> FindFirst(string pattern, RegexOptions options, int startIndex)
        {
            return Searchable.FindNext(Text, pattern, options, startIndex);
        }

        /// <summary>
        /// Find the last occurrence of the given pattern
        /// </summary>
        /// <param name="pattern">A regex pattern</param>
        /// <param name="options">Regex options</param>
        /// <returns>An (offset, length) pair, or (-1, 0) if not found</returns>
        public Tuple<int, int> FindLast(string pattern, RegexOptions options)
        {
            return FindLast(pattern, options, Length - 1);
        }

        /// <summary>
        /// Find the last occurrence of the given pattern
        /// </summary>
        /// <param name="pattern">A regex pattern</param>
        /// <param name="options">Regex options</param>
        /// <param name="startIndex">Begin searching at this offset</param>
        /// <returns>An (offset, length) pair, or (-1, 0) if not found</returns>
        public Tuple<int, int> FindLast(string pattern, RegexOptions options, int startIndex)
        {
            return Searchable.FindPrevious(Text, pattern, options, startIndex);
        }

        char[] _WordSeparators = " \t\r\n!@#$%^&*()-=+[]{}\\|;:\'\",.?<>/`~".ToCharArray();

        /// <summary>
        /// Get the index of the first character in a word
        /// 
        /// The buffer is searched backward starting at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetWordStart(int index)
        {
            int result = LastIndexOfAny(_WordSeparators, index);

            while (result == index)
            {
                if (--index < 0)
                {
                    return 0;
                }

                result = LastIndexOfAny(_WordSeparators, index);
            }

            if (result == -1)
            {
                return 0;
            }
            else if (result < Length - 1)
            {
                return result + 1;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Get the index of the last character in a word
        /// 
        /// The buffer is searched forward starting at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetWordEnd(int index)
        {
            int result = IndexOfAny(_WordSeparators, index);

            while (result == index)
            {
                if (++index >= Length)
                {
                    return Length;
                }

                result = IndexOfAny(_WordSeparators, index);
            }

            if (result == -1)
            {
                return Length;
            }
            else
            {
                return result;
            }
        }

        #endregion

        #region Text Runs

        /// <summary>
        /// Get the first child textrun
        /// </summary>
        protected TextRunSource First
        {
            get
            {
                if (_First != null)
                {
                    _First.Offset = 0;
                }
                return _First;
            }
        }

        /// <summary>
        /// Get the last child text run
        /// </summary>
        protected TextRunSource Last
        {
            get
            {
                if (_Last != null)
                {
                    _Last.Offset = Length - _Last.Length;
                }
                return _Last;
            }
        }

        /// <summary>
        /// Add a text run to the beginning of the linked list
        /// </summary>
        /// <param name="newNode"></param>
        protected void AddFirst(TextRunSource newNode)
        {
            newNode.Parent = this;
            newNode.Next = _First;
            newNode.Previous = null;

            if (_First != null)
            {
                _First.Previous = newNode;
            }
            else
            {
                _Last = newNode;
            }
            _First = newNode;

            newNode.Offset = 0;
            OnNodeAdded(newNode);
        }

        /// <summary>
        /// Add a text run to the end of the linked list
        /// </summary>
        /// <param name="newNode"></param>
        protected void AddLast(TextRunSource newNode)
        {
            newNode.Parent = this;
            newNode.Next = null;
            newNode.Previous = _Last;

            if (_Last != null)
            {
                _Last.Next = newNode;
            }
            else
            {
                _First = newNode;
            }
            _Last = newNode;

            newNode.Offset = Length;
            OnNodeAdded(newNode);
        }

        /// <summary>
        /// Add a text run after the given text run
        /// </summary>
        protected void AddAfter(TextRunSource refNode, TextRunSource newNode)
        {
            if (refNode == null)
            {
                AddFirst(newNode);
            }
            else if (refNode == _Last)
            {
                AddLast(newNode);
            }
            else
            {
                newNode.Parent = this;
                newNode.Next = refNode.Next;
                newNode.Previous = refNode;

                refNode.Next.Previous = newNode;
                refNode.Next = newNode;

                newNode.Offset = refNode.Offset + refNode.Length;
                OnNodeAdded(newNode);
            }
        }

        /// <summary>
        /// Add a text run before the given text run
        /// </summary>
        protected void AddBefore(TextRunSource refNode, TextRunSource newNode)
        {
            if (refNode == null)
            {
                AddLast(newNode);
            }
            else if (refNode == _First)
            {
                AddFirst(newNode);
            }
            else
            {
                newNode.Parent = this;
                newNode.Next = refNode;
                newNode.Previous = refNode.Previous;

                refNode.Previous.Next = newNode;
                refNode.Previous = newNode;

                newNode.Offset = refNode.Offset;
                OnNodeAdded(newNode);
            }
        }

        /// <summary>
        /// Remove a child text run
        /// </summary>
        protected void Remove(TextRunSource oldNode)
        {
            // Set node.Next.Previous
            if (oldNode.Next != null)
            {
                oldNode.Next.Previous = oldNode.Previous;
            }
            else
            {
                _Last = oldNode.Previous;
            }

            // Set node.Previous.Next
            if (oldNode.Previous != null)
            {
                oldNode.Previous.Next = oldNode.Next;
            }
            else
            {
                _First = oldNode.Next;
            }

            if (First == null)
            {
                DefaultProperties = (TextProperties)oldNode.Properties.Clone();
            }

            OnNodeRemoved(oldNode);
        }

        /// <summary>
        /// Get the text run at the given character offset
        /// </summary>
        protected TextRunSource Get(int offset)
        {
            // Return the node at the given index

            if (offset < 0 || offset > Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (offset == Length)
            {
                return null;
            }

            TextRunSource node = First;
            while (node.Offset + node.Length <= offset)
            {
                node.Next.Offset = node.Offset + node.Length;
                node = node.Next;
            }

            return node;
        }

        /// <summary>
        /// Ensure a text run ends at the given offset. 
        /// 
        /// If that offset falls within a text run, that run is split in two.
        /// 
        /// Returns the text run that starts at offset, or null if none.
        /// </summary>
        protected TextRunSource Slice(int offset)
        {
            if (offset == Length)
            {
                return null;
            }

            TextRunSource refNode = Get(offset);

            if (refNode.Offset == offset)
            {
                return refNode;
            }

            int sliceOffset = offset - refNode.Offset;
            int sliceLength = refNode.Length - sliceOffset;

            // Split node in two.
            TextRunSource newNode;
            _SuppressTextChanged++;
            try
            {
                newNode = refNode.Cut(sliceOffset, sliceLength);
                AddAfter(refNode, newNode);
            }
            finally
            {
                _SuppressTextChanged--;
            }

            return newNode;
        }

        /// <summary>
        /// Called when a text run has been added
        /// </summary>
        private void OnNodeAdded(TextRunSource newNode)
        {
            // Listen for changes to all text runs we own

            newNode.TextChanged += Run_TextChanged;
            newNode.FormatChanged += Run_FormatChanged;

            if (newNode.Length > 0)
            {
                Run_TextChanged(newNode, new TextChangedEventArgs(0, newNode.Length, 0));
                Run_FormatChanged(newNode, new FormatChangedEventArgs(null));
            }
        }

        /// <summary>
        /// Called when a text run has been removed
        /// </summary>
        private void OnNodeRemoved(TextRunSource oldNode)
        {
            oldNode.TextChanged -= Run_TextChanged;
            oldNode.FormatChanged -= Run_FormatChanged;

            if (oldNode.Length > 0)
            {
                Run_TextChanged(oldNode, new TextChangedEventArgs(0, 0, oldNode.Length));
            }
        }

        /// <summary>
        /// Called when the text contents of a child text run changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Run_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textRun = (TextRunSource)sender;

            Length += e.NumAdded - e.NumRemoved;

            int offset = textRun.Offset + e.Offset;
            // TODO: textRun.Offset is not guaranteed to be valid here

            OnTextChanged(offset, e.NumAdded, e.NumRemoved);
        }

        private void Run_FormatChanged(object sender, FormatChangedEventArgs e)
        {
            var textRun = (TextRunSource)sender;

            OnFormatChanged(textRun.Offset, textRun.Length, e.PropertyName);
        }

        #endregion

        #region TextSource

        public override TextRun GetTextRun(int index)
        {
            if (index < Length)
            {
                TextRunSource node = Get(index);
                return node.GetTextRun(index - node.Offset);
            }
            else if (index == Length)
            {
                return new TextEndOfLine(1);
            }
            else
            {
                return new TextEndOfParagraph(1);
            }
        }

        public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int index)
        {
            TextRunSource node = Get(index - 1);
            return node.GetPrecedingText(index - node.Offset);
        }

        public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int index)
        {
            return index;
        }

        #endregion

        #region INodeSource

        /// <summary>
        /// The INodeSource that owns this NTextBuffer (null if none)
        /// </summary>
        public INodeSource Owner { get; set; }

        /// <summary>
        /// Get our child objects
        /// </summary>
        public IEnumerable<object> GetChildNodes(NodeContext context)
        {
            return this.OfType<INodeSource>();
        }

        /// <summary>
        /// Create a new object represented by the given DOM node
        /// </summary>
        public virtual object CreateNode(Node newNode)
        {
            switch (newNode.NodeType)
            {
                case NodeType.TEXT_NODE:
                    return new TextRunSource(DefaultProperties);
                case NodeType.ELEMENT_NODE:
                    break;
                default:
                    return null;
            }

            string nodeName = newNode.LocalName ?? newNode.NodeName;

            switch (nodeName)
            {
                case SVGElements.TSPAN:
                    var tspan = new TextRunSource(DefaultProperties);
                    // We represent newlines as empty tspans
                    if (String.IsNullOrEmpty(newNode.TextContent))
                        tspan.Text = "\n";
                    return tspan;
                default:
                    break;
            }

            var result = new TextRunSource(DefaultProperties);

            switch (nodeName)
            {
                case HTMLElements.A:
                    break;
                case HTMLElements.B:
                case HTMLElements.STRONG:
                    result.Properties.SetFontWeight(FontWeights.Bold);
                    break;
                case HTMLElements.I:
                case HTMLElements.EM:
                    result.Properties.SetFontStyle(FontStyles.Italic);
                    break;
                case HTMLElements.U:
                    result.Properties.SetTextDecorations(TextDecorations.Underline);
                    break;
                case HTMLElements.SUB:
                    result.Properties.SetBaselineAlignment(BaselineAlignment.Subscript);
                    break;
                case HTMLElements.SUP:
                    result.Properties.SetBaselineAlignment(BaselineAlignment.Subscript);
                    break;
                case HTMLElements.CODE:
                    result.Properties.SetFontFamily(FontClass.Monospace.FontFamily);
                    break;
                case HTMLElements.SPAN:
                    break;
                default:
                    return null;
            }

            return result;
        }

        /// <summary>
        /// Append the given child object
        /// </summary>
        public void AppendNode(NodeContext context, object newChild)
        {
            AddLast((TextRunSource)newChild);
        }

        /// <summary>
        /// Insert the given child object
        /// </summary>
        public void InsertNode(NodeContext context, object newChild, object refChild)
        {
            AddBefore((TextRunSource)refChild, (TextRunSource)newChild);
        }

        /// <summary>
        /// Remove the given child object
        /// </summary>
        public virtual void RemoveNode(NodeContext context, object oldChild)
        {
            Remove((TextRunSource)oldChild);
        }

        #endregion

        #region SVG

        public IEnumerable<object> GetSVGChildNodes(NodeContext context, int charOffset, int charLength, Point position)
        {
            var results = new List<object>();

            int remaining = charLength;

            TextRunSource run = Get(charOffset);
            charOffset -= run.Offset;
            charLength = Math.Min(remaining, run.Length - charOffset);

            results.Add(run.GetSVGChildNode(context, charOffset, charLength, position));

            while ((remaining -= charLength) > 0)
            {
                run = run.Next;
                charOffset = 0;
                charLength = Math.Min(remaining, run.Length);
                
                results.Add(run.GetSVGChildNode(context, charOffset, charLength, new Point(Double.NaN, Double.NaN)));
            }

            return results;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<TextRunSource> GetEnumerator()
        {
            for (var textRun = First; textRun != null; textRun = textRun.Next)
            {
                yield return textRun;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            return Copy(0, Length);
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return Text;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Get the text in this buffer as a string
        /// </summary>
        /// <returns></returns>
        private string GetText()
        {
            var buffer = new StringBuilder();

            for (var node = First; node != null; node = node.Next)
            {
                buffer.Append(node.Text);
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Set the text contents of this buffer
        /// </summary>
        private void SetText(string newValue)
        {
            Delete(0, Length);
            Insert(0, newValue);
        }

        protected virtual void OnDefaultPropertiesChanged(GenericTextRunProperties newValue, GenericTextRunProperties oldValue)
        {
            if (oldValue != null)
            {
                oldValue.FormatChanged -= DefaultProperties_FormatChanged;
            }

            if (newValue != null)
            {
                newValue.FormatChanged += DefaultProperties_FormatChanged;
            }
        }

        void DefaultProperties_FormatChanged(object sender, FormatChangedEventArgs e)
        {
            RaiseFormatChanged(0, 0, e.PropertyName);
        }

        /// <summary>
        /// Callback invoked when the text contents of this buffer changes
        /// </summary>
        /// <param name="offset">Offset of the first character added/removed</param>
        /// <param name="numAdded">Number of characters added</param>
        /// <param name="numRemoved">Number of characters removed</param>
        protected virtual void OnTextChanged(int offset, int numAdded, int numRemoved)
        {
            _CachedText = null;

            RaiseTextChanged(offset, numAdded, numRemoved);
        }

        /// <summary>
        /// Callback invoked when a text formatting property withing this buffer changes
        /// </summary>
        /// <param name="offset">Offset of the first character affected</param>
        /// <param name="length">Number of characters affected</param>
        /// <param name="propertyName">Name of the property that has changed</param>
        protected virtual void OnFormatChanged(int offset, int length, string propertyName)
        {
            RaiseFormatChanged(offset, length, propertyName);
        }

        #endregion
    }


}
