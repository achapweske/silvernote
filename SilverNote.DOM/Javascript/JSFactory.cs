/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Windows;
using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript
{
    public static class JSFactory
    {
        public static ScriptEngine CreateEngine(Window window)
        {
            var engine = new ScriptEngine();
#if DEBUG
            //engine.EnableDebugging = true;
#endif
            Windows.Window.SetWindow(engine, window);
            engine.SetGlobalValue("Math", new Math(engine));
            return engine;
        }

        public static ObjectInstance CreateObject(ScriptEngine engine)
        {
            return new Object(engine);
        }

        public static ObjectInstance CreateObject(ScriptEngine engine, object obj)
        {
            var target = obj as ScriptTarget;
            if (target != null && target.ScriptContext is ObjectInstance)
            {
                return (ObjectInstance)target.ScriptContext;
            }

            ObjectInstance result = null;

            if (obj is DOM.SVG.SVGLineElement)
            {
                result = new Javascript.SVG.SVGLineElement(engine, (DOM.SVG.SVGLineElement)obj);
            }
            else if (obj is DOM.SVG.SVGElement)
            {
                result = new Javascript.SVG.SVGElement(engine, (DOM.SVG.SVGElement)obj);
            }
            else if (obj is DOM.Element)
            {
                result = new Javascript.Core.Element(engine, (DOM.Element)obj);
            }
            else if (obj is DOM.Document)
            {
                result = new Javascript.Core.Document(engine, (DOM.Document)obj);
            }
            else if (obj is DOM.Windows.Window)
            {
                result = new Javascript.Windows.Window(engine, (DOM.Windows.Window)obj);
            }
            else if (obj is DOM.Node)
            {
                result = new Javascript.Core.Node(engine, (DOM.Node)obj);
            }
            else if (obj is DOM.CSS.CSS3StyleDeclaration)
            {
                result = new Javascript.CSS.CSS3StyleDeclaration(engine, (DOM.CSS.CSS3StyleDeclaration)obj);
            }
            else if (obj is DOM.CSS.CSSStyleDeclaration)
            {
                result = new Javascript.CSS.CSSStyleDeclaration(engine, (DOM.CSS.CSSStyleDeclaration)obj);
            }
            else
            {
                result = new Javascript.Object(engine, obj);
            }

            if (target != null)
            {
                target.ScriptContext = result;
            }

            return result;
        }

    }
}
