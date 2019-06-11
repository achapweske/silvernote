/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Jurassic;
using Jurassic.Library;
using System;

namespace DOM.Javascript.Windows
{
    public class Window : ObjectInstance
    {
        public static Window SetWindow(ScriptEngine engine, DOM.Windows.Window window)
        {
            var obj = (Window)JSFactory.CreateObject(engine, window);
            SetWindow(engine, obj);
            return obj;
        }

        public static void SetWindow(ScriptEngine engine, Window window)
        {
            engine.SetGlobalValue("document", window.Document);
            engine.SetGlobalFunction("alert", new Action<string>(window.Alert));

            engine.Global["window"] = engine.Global;
        }

        private readonly DOM.Windows.Window _Window;

        public Window(ScriptEngine engine, DOM.Windows.Window window)
            : base(engine)
        {
            _Window = window;
            PopulateFields();
            PopulateFunctions();
        }

        public Window(ObjectInstance prototype, DOM.Windows.Window window)
            : base(prototype)
        {
            _Window = window;
        }

        [JSProperty(Name = "window")]
        public Window Self
        {
            get { return this;  }
        }

        [JSProperty(Name = "document")]
        public DOM.Javascript.Core.Document Document
        {
            get { return (DOM.Javascript.Core.Document) JSFactory.CreateObject(Engine, _Window.Document);  }
        }

        [JSFunction(Name = "alert")]
        void Alert(string message)
        {
            _Window.Alert(message);
        }
    }
}
