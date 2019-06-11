/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Windows
{
    /// <summary>
    /// http://www.w3.org/TR/Window/
    /// </summary>
    public interface Window : Views.AbstractView
    {
        Window Window { get; }
        Location Location { get; }
        int SetTimeout(TimerListener listener, int milliseconds);
        void ClearTimeout(int timerID);
        int SetInterval(TimerListener listener, int milliseconds);
        void ClearInterval(int timerID);
        void SetProperty(string name, object value);
        object GetProperty(string name);
        bool HasProperty(string name);
        void Alert(string message);
    }
}
