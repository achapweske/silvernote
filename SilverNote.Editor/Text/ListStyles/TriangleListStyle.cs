/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Editor
{
    internal class TriangleListStyle : IListStyle
    {
        public string Description
        {
            get { return "\u25B6 Triangle"; }
        }

        public bool IsOrdered
        {
            get { return false; }
        }

        public string GetMarker(int index)
        {
            return "\u25B6";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.OpenTriangle; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.OpenDiamond; }
        }

        public override string ToString()
        {
            return "triangle";
        }
    }
}
