/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Jurassic;
using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DOM.Javascript
{
    public class Object : ObjectInstance
    {
        public Object(ScriptEngine engine)
            : base(engine)
        {
            
        }

        public Object(ScriptEngine engine, object obj)
            : base(engine)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(string) ||
                    property.PropertyType == typeof(double) ||
                    property.PropertyType == typeof(bool) ||
                    property.PropertyType == typeof(int) ||
                    property.PropertyType == typeof(uint) ||
                    property.PropertyType == typeof(ConcatenatedString) ||
                    property.PropertyType.IsAssignableFrom(typeof(ObjectInstance)))
                {
                    string name = "" + Char.ToLower(property.Name[0], CultureInfo.InvariantCulture);
                    if (property.Name.Length > 1)
                    {
                        name += property.Name.Substring(1);
                    }
                    this[name] = property.GetValue(obj, null);
                }
            }
        }
    }
}
