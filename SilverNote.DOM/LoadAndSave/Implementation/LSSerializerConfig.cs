/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;

namespace DOM.LS.Internal
{
    public class LSSerializerConfig : DOMConfigurationBase
    {
        #region Constructors

        public LSSerializerConfig()
        {
            CanonicalForm = false;
            DiscardDefaultContent = true;
            FormatPrettyPrint = false;
            IgnoreUnknownCharacterDenormalization = true;
            NormalizeCharacters = true;
            XmlDeclaration = true;
        }

        #endregion

        #region Properties

        public bool CanonicalForm { get; set; }
        public bool DiscardDefaultContent { get; set; }
        public bool FormatPrettyPrint { get; set; }
        public bool IgnoreUnknownCharacterDenormalization { get; set; }
        public bool NormalizeCharacters { get; set; }
        public bool XmlDeclaration { get; set; }

        #endregion

        #region DOMConfiguration

        public override object GetParameter(string name)
        {
            switch (name.ToLower())
            {
                case LSSerializerParameters.CANONICAL_FORM:
                    return CanonicalForm;
                case LSSerializerParameters.DISCARD_DEFAULT_CONTENT:
                    return DiscardDefaultContent;
                case LSSerializerParameters.FORMAT_PRETTY_PRINT:
                    return FormatPrettyPrint;
                case LSSerializerParameters.IGNORE_UNKNOWN_CHARACTER_DENORMALIZATION:
                    return IgnoreUnknownCharacterDenormalization;
                case LSSerializerParameters.NORMALIZE_CHARACTERS:
                    return NormalizeCharacters;
                case LSSerializerParameters.XML_DECLARATION:
                    return XmlDeclaration;
                default:
                    return base.GetParameter(name);
            }
        }

        public override void SetParameter(string name, object value)
        {
            switch (name.ToLower())
            {
                case LSSerializerParameters.CANONICAL_FORM:
                    CanonicalForm = (bool)value;
                    break;
                case LSSerializerParameters.DISCARD_DEFAULT_CONTENT:
                    DiscardDefaultContent = (bool)value;
                    break;
                case LSSerializerParameters.FORMAT_PRETTY_PRINT:
                    FormatPrettyPrint = (bool)value;
                    break;
                case LSSerializerParameters.IGNORE_UNKNOWN_CHARACTER_DENORMALIZATION:
                    IgnoreUnknownCharacterDenormalization = (bool)value;
                    break;
                case LSSerializerParameters.NORMALIZE_CHARACTERS:
                    NormalizeCharacters = (bool)value;
                    break;
                case LSSerializerParameters.XML_DECLARATION:
                    XmlDeclaration = (bool)value;
                    break;
                default:
                    base.SetParameter(name, value);
                    break;
            }
        }

        public override bool CanSetParameter(string name, object value)
        {
            switch (name.ToLower())
            {
                case LSSerializerParameters.CANONICAL_FORM:
                case LSSerializerParameters.DISCARD_DEFAULT_CONTENT:
                case LSSerializerParameters.FORMAT_PRETTY_PRINT:
                case LSSerializerParameters.IGNORE_UNKNOWN_CHARACTER_DENORMALIZATION:
                case LSSerializerParameters.NORMALIZE_CHARACTERS:
                case LSSerializerParameters.XML_DECLARATION:
                    return true;
                default:
                    return base.CanSetParameter(name, value);
            }
        }

        #endregion
    }
}
