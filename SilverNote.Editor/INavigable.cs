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
using System.Windows;

namespace SilverNote.Editor
{
    public interface INavigable
    {
        Point NavigationOffset { get; set; }

        bool MoveLeft();
        bool MoveRight();
        bool MoveUp();
        bool MoveDown();
        void MoveToStart();
        void MoveToEnd();
        void MoveToTop();
        void MoveToBottom();
        void MoveToLeft();
        void MoveToRight();

        bool SelectLeft();
        bool SelectRight();
        bool SelectUp();
        bool SelectDown();
        void SelectToStart();
        void SelectToEnd();
        void SelectToTop();
        void SelectToBottom();
        void SelectToLeft();
        void SelectToRight();

        bool TabForward();
        bool TabBackward();
    }

    public static class Navigable
    {
        public static void MoveToStart(object item)
        {
            var navigable = item as INavigable;
            if (navigable != null)
            {
                navigable.MoveToStart();
            }
        }

        public static void SelectToEnd(object item)
        {
            var navigable = item as INavigable;
            if (navigable != null)
            {
                navigable.SelectToEnd();
            }
        }

        public static void SelectAll(this INavigable item)
        {
            item.MoveToStart();
            item.SelectToEnd();
        }

        public static void SelectAll(IEnumerable items)
        {
            SelectAll(items.OfType<INavigable>());
        }

        public static void SelectAll(IEnumerable<INavigable> items)
        {
            foreach (var item in items)
            {
                item.MoveToStart();
                item.SelectToEnd();
            }
        }

        public static void SelectRange(IList items, object firstItem, object lastItem, Func<object, bool> filter = null)
        {
            int firstIndex = items.IndexOf(firstItem);
            if (firstIndex == -1)
            {
                throw new ArgumentException("firstItem");
            }

            int lastIndex = items.IndexOf(lastItem);
            if (lastIndex == -1)
            {
                throw new ArgumentException("lastItem");
            }

            SelectRange(items, firstIndex, lastIndex, filter);
        }

        public static void SelectRange(IList items, int startIndex, int endIndex, Func<object, bool> filter = null)
        {
            int tick = (endIndex > startIndex) ? 1 : -1;

            for (int i = startIndex; i != endIndex + tick; i += tick)
            {
                var item = items[i] as INavigable;
                if (item == null || filter != null && !filter(item))
                {
                    continue;
                }

                if (i == startIndex)
                {
                    if (endIndex > startIndex)
                        item.SelectToEnd();
                    else if (endIndex < startIndex)
                        item.SelectToStart();
                }
                else if (i == endIndex)
                {
                    if (endIndex > startIndex)
                        item.MoveToStart();
                    else if (endIndex < startIndex)
                        item.MoveToEnd();
                }
                else if (endIndex > startIndex)
                {
                    item.MoveToStart();
                    item.SelectToEnd();
                }
                else if (endIndex < startIndex)
                {
                    item.MoveToEnd();
                    item.SelectToStart();
                }
            }
        }
    }
}
