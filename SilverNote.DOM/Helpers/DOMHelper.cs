/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Helpers
{
    public static class DOMHelper
    {
        public static bool HasClass(string str, string className)
        {
            return (" " + str + " ").Contains(" " + className + " ");
        }

        public static string AppendClass(string str, string className)
        {
            str = RemoveClass(str, className);

            if (str.Length > 0)
            {
                return str + " " + className;
            }
            else
            {
                return className;
            }
        }

        public static string PrependClass(string str, string className)
        {
            str = RemoveClass(str, className);

            if (str.Length > 0)
            {
                return className + " " + str;
            }
            else
            {
                return className;
            }
        }

        public static string RemoveClass(string str, string className)
        {
            return (" " + str + " ").Replace(" " + className + " ", "").Trim();
        }
    }
}
