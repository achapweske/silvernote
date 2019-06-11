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

namespace DOM.LS
{
    public interface LSOutput
    {
        TextWriter CharacterStream { get; set; }
        Stream ByteStream { get; set; }
        string SystemId { get; set; }
        string Encoding { get; set; }
    }
}
