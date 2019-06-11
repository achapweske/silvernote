/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript
{
    public class Math : ObjectInstance
    {
        public Math(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        [JSFunction(Name = "max")]
        public static double Max(double a, double b)
        {
            return DOM.Objects.Math.Max(a, b);
        }

        [JSFunction(Name = "max")]
        public static double Max(double a, double b, double c)
        {
            return DOM.Objects.Math.Max(a, b, c);
        }

        [JSFunction(Name = "min")]
        public static double Min(double a, double b)
        {
            return DOM.Objects.Math.Min(a, b);
        }

        [JSFunction(Name = "min")]
        public static double Min(double a, double b, double c)
        {
            return DOM.Objects.Math.Min(a, b, c);
        }
    }
}
