/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM
{
    /// <summary>
    /// http://www.w3.org/TR/2003/WD-DOM-Level-3-Core-20030609/core.html#DOMConfiguration
    /// </summary>
    public static class DOMParameters
    {
        public static readonly string CANONICAL_FORM = "canonical-form";
        public static readonly string CDATA_SECTIONS = "cdata-sections";
        public static readonly string CHECK_CHARACTER_NORMALIZATION = "check-character-normalization";
        public static readonly string COMMENTS = "comments";
        public static readonly string DATATYPE_NORMALIZATION = "datatype-normalization";
        public static readonly string ENTITIES = "entities";
        public static readonly string ERROR_HANDLER = "error-handler";
        public static readonly string INFOSET = "infoset";
        public static readonly string NAMESPACES = "namespaces";
        public static readonly string NAMESPACE_DECLARATIONS = "namespace-declarations";
        public static readonly string NORMALIZE_CHARACTERS = "normalize-characters";
        public static readonly string SCHEMA_LOCATION = "schema-location";
        public static readonly string SCHEMA_TYPE = "schema-type";
        public static readonly string SPLIT_CDATA_SECTIONS = "split-cdata-sections";
        public static readonly string VALIDATE = "validate";
        public static readonly string VALIDATE_IF_SCHEMA = "validate-if-schema";
        public static readonly string WELL_FORMED = "well-formed";
        public static readonly string WHITESPACEC_IN_ELEMENT_CONTENT = "whitespace-in-element-content";
    }
}
