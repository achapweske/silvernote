/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Objects
{
    public static class Math
    {
        public static double Max(double a, double b)
        {
            return System.Math.Max(a, b);
        }

        public static double Max(double a, double b, double c)
        {
            return System.Math.Max(System.Math.Max(a, b), c);
        }

        public static double Min(double a, double b)
        {
            return System.Math.Min(a, b);
        }

        public static double Min(double a, double b, double c)
        {
            return System.Math.Min(System.Math.Min(a, b), c);
        }
    }
}
