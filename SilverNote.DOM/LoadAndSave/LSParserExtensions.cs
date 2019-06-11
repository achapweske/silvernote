/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DOM.LS
{
    public static class LSParserExtensions
    {
        public static Document ParseStream(this LSParser parser, Stream stream)
        {
            LSInput input = DOMFactory.CreateLSInput();
            input.ByteStream = stream;
            return parser.Parse(input);
        }

        public static Document ParseReader(this LSParser parser, TextReader reader)
        {
            LSInput input = DOMFactory.CreateLSInput();
            input.CharacterStream = reader;
            return parser.Parse(input);
        }

        public static Document ParseString(this LSParser parser, string str)
        {
            using (var reader = new StringReader(str))
            {
                return parser.ParseReader(reader);
            }
        }
    }
}
