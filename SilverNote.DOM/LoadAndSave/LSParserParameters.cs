/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.LS
{
    public static class LSParserParameters
    {
        public const string CHARSET_OVERRIDES_XML_ENCODING = "charset-overrides-xml-encoding";
        public const string DISALLOW_DOCTYPE = "disallow-doctype";
        public const string IGNORE_UNKNOWN_CHARACTER_DENORMALIZATION= "ignore-unknown-character-denormalizations";
        public const string INFOSET = "infoset";
        public const string NAMESPACES = "namespaces";
        public const string RESOURCE_RESOLVER = "resource-resolver";
        public const string SUPPORTED_MEDIA_TYPES_ONLY = "supported-media-types-only";
        public const string VALIDATE = "validate";
        public const string VALIDATE_IF_SCHEMA = "validate-if-schema";
        public const string WELL_FORMED = "well-formed";
    }
}
