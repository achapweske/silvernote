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
    internal class OpenTriangleListStyle : IListStyle
    {
        public string Description
        {
            get { return "\u25B7 Triangle"; }
        }

        public bool IsOrdered
        {
            get { return false; }
        }

        public string GetMarker(int index)
        {
            return "\u25B7";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.Circle; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.Triangle; }
        }

        public override string ToString()
        {
            return "open-triangle";
        }
    }
}
