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
    internal class CircleListStyle : IListStyle
    {
        public string Description
        {
            get { return "\u25CF Circle"; }
        }

        public bool IsOrdered
        {
            get { return false; }
        }

        public string GetMarker(int index)
        {
            return "\u25CF";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.OpenCircle; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.OpenTriangle; }
        }

        public override string ToString()
        {
            return "disc";
        }
    }
}
