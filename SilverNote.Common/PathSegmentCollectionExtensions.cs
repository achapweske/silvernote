/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace SilverNote.Common
{
    public static class PathSegmentCollectionExtensions
    {
        public static void AddRange(this PathSegmentCollection segments, IEnumerable<PathSegment> newSegments)
        {
            foreach (var newSegment in newSegments)
            {
                segments.Add(newSegment);
            }
        }

        public static void InsertRange(this PathSegmentCollection segments, int index, IEnumerable<PathSegment> newSegments)
        {
            int i = index;
            foreach (var newSegment in newSegments)
            {
                segments.Insert(i, newSegment);
                i++;
            }
        }
    }
}
