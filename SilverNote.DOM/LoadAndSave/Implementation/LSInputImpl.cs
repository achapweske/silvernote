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
    public class LSInputImpl : LSInput
    {
        public TextReader CharacterStream { get; set; }
        public Stream ByteStream { get; set; }
        public string StringData { get; set; }
        public string SystemId { get; set; }
        public string Encoding { get; set; }
        public string PublicId { get; set; }
        public string BaseURI { get; set; }
        public bool Certified { get; set; }
    }
}
