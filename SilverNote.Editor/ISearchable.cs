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

namespace SilverNote.Editor
{
    public interface ISearchable
    {
        int Find(string findText);
        bool FindFirst();
        bool FindLast();
        bool FindNext();
        bool FindPrevious();
        bool FindFirst(string pattern, RegexOptions options);
        bool FindLast(string pattern, RegexOptions options);
        bool FindNext(string pattern, RegexOptions options);
        bool FindPrevious(string pattern, RegexOptions options);
    }

    public static class Searchable
    {
        /// <summary>
        /// Highlight all instances of the given string within 'items'
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="str">String to search for</param>
        /// <returns>Number of instances of the given string</returns>
        public static int Find(IEnumerable items, string str)
        {
            int count = 0;

            foreach (var item in items)
            {
                var searchable = item as ISearchable;
                if (searchable != null)
                {
                    count += searchable.Find(str);
                }
            }

            return count;
        }

        /// <summary>
        /// Select the first instance of the string highlighted by Find()
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <returns>The item containing the selected string</returns>
        public static ISearchable FindFirst(IEnumerable items)
        {
            foreach (var item in items)
            {
                var searchable = item as ISearchable;
                if (searchable != null)
                {
                    if (searchable.FindFirst())
                    {
                        return searchable;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Select the last instance of the string highlighted by Find()
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <returns>The item containing the selected string</returns>
        public static ISearchable FindLast(IEnumerable items)
        {
            foreach (var item in items.Cast<object>().Reverse())
            {
                var searchable = item as ISearchable;
                if (searchable != null)
                {
                    if (searchable.FindLast())
                    {
                        return searchable;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Select the next instance of the string highlighted by Find()
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="startItem">Begin searching within this item</param>
        /// <returns>The item containing the selected string</returns>
        public static ISearchable FindNext(IList items, object startItem)
        {
            if (startItem == null)
            {
                return FindFirst(items);
            }

            int startIndex = items.IndexOf(startItem);
            if (startIndex != -1)
            {
                return FindNext(items, startIndex);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Select the next instance of the string highlighted by Find()
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="startItem">Begin searching within the item at this index</param>
        /// <returns>The item containing the selected string</returns>
        public static ISearchable FindNext(IList items, int startIndex)
        {
            var item = items[startIndex] as ISearchable;
            if (item != null)
            {
                if (item.FindNext())
                {
                    return item;
                }
            }

            for (int i = startIndex + 1; i < items.Count; i++)
            {
                item = items[i] as ISearchable;
                if (item != null)
                {
                    if (item.FindFirst())
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Select the previous instance of the string highlighted by Find()
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="startItem">Begin searching within this item</param>
        /// <returns>The item containing the selected string</returns>
        public static ISearchable FindPrevious(IList items, object startItem)
        {
            if (startItem == null)
            {
                return FindLast(items);
            }

            int startIndex = items.IndexOf(startItem);
            if (startIndex != -1)
            {
                return FindPrevious(items, startIndex);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Select the previous instance of the string highlighted by Find()
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="startItem">Begin searching within the item at this index</param>
        /// <returns>The item containing the selected string</returns>
        public static ISearchable FindPrevious(IList items, int startIndex)
        {
            var item = items[startIndex] as ISearchable;
            if (item != null && item.FindPrevious())
            {
                return item;
            }

            for (int i = startIndex - 1; i >= 0; i--)
            {
                item = items[i] as ISearchable;
                if (item != null)
                {
                    if (item.FindLast())
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Select the first match for the given regex pattern
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="pattern">Regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <returns>The item containing the selected string, or null if not found</returns>
        public static ISearchable FindFirst(IEnumerable items, string pattern, RegexOptions options)
        {
            foreach (var searchable in items.OfType<ISearchable>())
            {
                if (searchable.FindFirst(pattern, options))
                {
                    return searchable;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the first match for the given regex pattern
        /// </summary>
        /// <param name="str">The string to be searched</param>
        /// <param name="pattern">The regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <returns>An (offset, length) pair, or (-1, 0) if not found</returns>
        public static Tuple<int, int> FindFirst(string str, string pattern, RegexOptions options)
        {
            return FindNext(str, pattern, options, 0);
        }

        /// <summary>
        /// Select the last match for the given regex pattern
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="pattern">Regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <returns>The item containing the selected string, or null if not found</returns>
        public static ISearchable FindLast(IEnumerable items, string pattern, RegexOptions options)
        {
            foreach (var searchable in items.OfType<ISearchable>().Reverse())
            {
                if (searchable.FindLast(pattern, options))
                {
                    return searchable;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the first match for the given regex pattern
        /// </summary>
        /// <param name="str">The string to be searched</param>
        /// <param name="pattern">The regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <returns>An (offset, length) pair, or (-1, 0) if not found</returns>
        public static Tuple<int, int> FindLast(string str, string pattern, RegexOptions options)
        {
            return FindPrevious(str, pattern, options, str.Length - 1);
        }

        /// <summary>
        /// Select the next match for the given regex pattern
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="pattern">Regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <param name="startItem">Begin searching within this item</param>
        /// <returns>The item containing the selected string, or null if not found</returns>
        public static ISearchable FindNext(IList items, string pattern, RegexOptions options, object startItem)
        {
            if (startItem == null)
            {
                return FindFirst(items, pattern, options);
            }

            int startIndex = items.IndexOf(startItem);
            if (startIndex != -1)
            {
                return FindNext(items, pattern, options, startIndex);
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Select the next match for the given regex pattern
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="startItem">Begin searching within the item at this index</param>
        /// <param name="pattern">Regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <returns>The item containing the selected string, or null if not found</returns>
        public static ISearchable FindNext(IList items, string pattern, RegexOptions options, int startIndex)
        {
            var item = items[startIndex] as ISearchable;
            if (item != null)
            {
                if (item.FindNext(pattern, options))
                {
                    return item;
                }
            }

            for (int i = startIndex + 1; i < items.Count; i++)
            {
                item = items[i] as ISearchable;
                if (item != null)
                {
                    if (item.FindFirst(pattern, options))
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find the next match for the given regex pattern
        /// </summary>
        /// <param name="str">The string to be searched</param>
        /// <param name="pattern">The regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <param name="startIndex">Begin searching at this offset</param>
        /// <returns>An (offset, length) pair, or (-1, 0) if not found</returns>
        public static Tuple<int, int> FindNext(string str, string pattern, RegexOptions options, int startIndex)
        {
            if (!String.IsNullOrEmpty(pattern))
            {
                var matches = Regex.Matches(str, pattern, options);

                if (matches != null)
                {
                    foreach (Match match in matches)
                    {
                        if (match.Index >= startIndex)
                        {
                            return Tuple.Create(match.Index, match.Length);
                        }
                    }
                }
            }

            return Tuple.Create(-1, 0);
        }

        /// <summary>
        /// Select the previous match for the given regex pattern
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="pattern">Regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <param name="startItem">Begin searching within this item</param>
        /// <returns>The item containing the selected string, or null if not found</returns>
        public static ISearchable FindPrevious(IList items, string pattern, RegexOptions options, object startItem)
        {
            if (startItem == null)
            {
                return FindLast(items, pattern, options);
            }

            int startIndex = items.IndexOf(startItem);
            if (startIndex != -1)
            {
                return FindPrevious(items, pattern, options, startIndex);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Select the previous match for the given regex pattern
        /// </summary>
        /// <param name="items">Items to be searched</param>
        /// <param name="startItem">Begin searching within the item at this index</param>
        /// <param name="pattern">Regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <returns>The item containing the selected string, or null if not found</returns>
        public static ISearchable FindPrevious(IList items, string pattern, RegexOptions options, int startIndex)
        {
            var item = items[startIndex] as ISearchable;
            if (item != null && item.FindPrevious(pattern, options))
            {
                return item;
            }

            for (int i = startIndex - 1; i >= 0; i--)
            {
                item = items[i] as ISearchable;
                if (item != null)
                {
                    if (item.FindLast(pattern, options))
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find the previous match for the given regex pattern
        /// </summary>
        /// <param name="str">The string to be searched</param>
        /// <param name="pattern">The regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <param name="startIndex">Begin searching at this offset</param>
        /// <returns>An (offset, length) pair, or (-1, 0) if not found</returns>
        public static Tuple<int, int> FindPrevious(string str, string pattern, RegexOptions options, int startIndex)
        {
            if (!String.IsNullOrEmpty(pattern))
            {
                var matches = Regex.Matches(str, pattern, options);

                if (matches != null)
                {
                    foreach (Match match in matches.OfType<Match>().Reverse())
                    {
                        if (match.Index <= startIndex)
                        {
                            return Tuple.Create(match.Index, match.Length);
                        }
                    }
                }
            }

            return Tuple.Create(-1, 0);
        }

        /// <summary>
        /// Find all matches for the given regex pattern
        /// </summary>
        /// <param name="str">The string to be searched</param>
        /// <param name="pattern">The regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <returns>An array of zero or more (offset, length) pairs</returns>
        public static IEnumerable<Tuple<int, int>> FindAll(string str, string pattern, RegexOptions options)
        {
            if (!String.IsNullOrEmpty(pattern))
            {
                var result = FindFirst(str, pattern, options);

                while (result.Item1 != -1)
                {
                    yield return result;

                    result = FindNext(str, pattern, options, result.Item1 + result.Item2);
                }
            }
        }

        /// <summary>
        /// Select the Nth match for the given regex pattern.
        /// 
        /// If n > 0, this will select the nth match relative to the beginning.
        /// If n &lt; 0, this will select the -nth match relative to the end.
        /// If n = 0, no match will be selected and this will return false.
        /// </summary>
        /// <param name="searchable">Extension method parameter</param>
        /// <param name="pattern">Regex pattern to search for</param>
        /// <param name="options">Search options</param>
        /// <param name="n">See summary</param>
        /// <returns>true on success, or false if not found</returns>
        public static bool FindNth(this ISearchable searchable, string pattern, RegexOptions options, int n)
        {
            if (n > 0)
            {
                if (!searchable.FindFirst(pattern, options))
                {
                    return false;
                }

                for (int i = 1; i < n; i++)
                {
                    if (!searchable.FindNext(pattern, options))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (n < 0)
            {
                if (!searchable.FindLast(pattern, options))
                {
                    return false;
                }

                for (int i = -1; i > n; i--)
                {
                    if (!searchable.FindPrevious(pattern, options))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

    }


}
