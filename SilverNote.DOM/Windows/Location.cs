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
    public interface Location
    {
        string HRef { get; set; }
        string Hash { get; set; }
        string Host { get; set; }
        string HostName { get; set; }
        string PathName { get; set; }
        string Port { get; set; }
        string Protocol { get; set; }
        string Search { get; set; }
        void Assign(string url);
        void Replace(string url);
        void Reload();
        string ToString();
    }
}
