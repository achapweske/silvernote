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
    public interface LSInput
    {
        TextReader CharacterStream { get; set; }
        Stream ByteStream { get; set; }
        string StringData { get; set; }
        string SystemId { get; set; }
        string Encoding { get; set; }
        string PublicId { get; set; }
        string BaseURI { get; set; }
        bool Certified { get; set; }
    }
}
