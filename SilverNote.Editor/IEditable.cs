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
    interface IEditable
    {
        IList<object> Cut();
        IList<object> Copy();
        IList<object> Paste(IList<object> objects);
        bool Delete();
    }

    public static class Editable
    {
        public static IList<object> Cut(object obj)
        {
            var editable = obj as IEditable;
            if (editable != null)
            {
                return editable.Cut();
            }
            else
            {
                return new object[] { obj };
            }
        }

        public static IList<object> Copy(object obj)
        {
            var editable = obj as IEditable;
            if (editable != null)
            {
                return editable.Copy();
            }

            var cloneable = obj as ICloneable;
            if (cloneable != null)
            {
                return new object[] { cloneable.Clone() };
            }
            else
            {
                return new object[] { };
            }
        }

        public static IList<object> Paste(object obj, IList<object> objects)
        {
            var editable = obj as IEditable;
            if (editable != null)
            {
                return editable.Paste(objects);
            }
            else
            {
                return objects;
            }
        }

        public static bool Delete(object obj)
        {
            var editable = obj as IEditable;
            if (editable != null)
            {
                return editable.Delete();
            }
            else
            {
                return false;
            }
        }
    }
}
