/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.LS.Internal
{
    public class LSOutputImpl : LSOutput
    {
        public TextWriter CharacterStream { get; set; }
        public Stream ByteStream { get; set; }
        public string SystemId { get; set; }
        public string Encoding { get; set; }
    }
}
