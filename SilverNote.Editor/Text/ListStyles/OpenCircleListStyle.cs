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
    internal class OpenCircleListStyle : IListStyle
    {
        public string Description
        {
            get { return "\u25CB Circle"; }
        }

        public bool IsOrdered
        {
            get { return false; }
        }

        public string GetMarker(int index)
        {
            return "\u25CB";
        }

        public IListStyle NextStyle
        {
            get { return ListStyles.Square; }
        }

        public IListStyle PreviousStyle
        {
            get { return ListStyles.Circle; }
        }

        public override string ToString()
        {
            return "circle";
        }
    }
}
