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
    public class LSParserConfig : DOMConfigurationBase
    {
        #region Constructors

        public LSParserConfig()
        {
            CharsetOverridesXmlEncoding = true;
            DisallowDoctype = false;
            IgnoreUnknownCharacterDenormalization = true;
            Infoset = true;
            Namespaces = true;
            ResourceResolver = null;
            SupportedMediaTypesOnly = false;
            Validate = false;
            ValidateIfSchema = false;
            WellFormed = true;
        }

        #endregion

        #region Properties

        public bool CharsetOverridesXmlEncoding { get; set; }
        public bool DisallowDoctype { get; set; }
        public bool IgnoreUnknownCharacterDenormalization { get; set; }
        public bool Infoset { get; set; }
        public bool Namespaces { get; set; }
        public LSResourceResolver ResourceResolver { get; set; }
        public bool SupportedMediaTypesOnly { get; set; }
        public bool Validate { get; set; }
        public bool ValidateIfSchema { get; set; }
        public bool WellFormed { get; set; }

        #endregion

        #region DOMConfiguration

        public override object GetParameter(string name)
        {
            switch (name.ToLower())
            {
                case LSParserParameters.CHARSET_OVERRIDES_XML_ENCODING:
                    return CharsetOverridesXmlEncoding;
                case LSParserParameters.DISALLOW_DOCTYPE:
                    return DisallowDoctype;
                case LSParserParameters.IGNORE_UNKNOWN_CHARACTER_DENORMALIZATION:
                    return IgnoreUnknownCharacterDenormalization;
                case LSParserParameters.INFOSET:
                    return Infoset;
                case LSParserParameters.NAMESPACES:
                    return Namespaces;
                case LSParserParameters.RESOURCE_RESOLVER:
                    return ResourceResolver;
                case LSParserParameters.SUPPORTED_MEDIA_TYPES_ONLY:
                    return SupportedMediaTypesOnly;
                case LSParserParameters.VALIDATE:
                    return Validate;
                case LSParserParameters.VALIDATE_IF_SCHEMA:
                    return ValidateIfSchema;
                case LSParserParameters.WELL_FORMED:
                    return WellFormed;
                default:
                    return base.GetParameter(name);
            }
        }

        public override void SetParameter(string name, object value)
        {
            switch (name.ToLower())
            {
                case LSParserParameters.CHARSET_OVERRIDES_XML_ENCODING:
                case LSParserParameters.DISALLOW_DOCTYPE:
                case LSParserParameters.IGNORE_UNKNOWN_CHARACTER_DENORMALIZATION:
                case LSParserParameters.INFOSET:
                case LSParserParameters.NAMESPACES:
                case LSParserParameters.RESOURCE_RESOLVER:
                case LSParserParameters.SUPPORTED_MEDIA_TYPES_ONLY:
                case LSParserParameters.VALIDATE:
                case LSParserParameters.VALIDATE_IF_SCHEMA:
                case LSParserParameters.WELL_FORMED:
                default:
                    base.SetParameter(name, value);
                    break;
            }
        }

        public override bool CanSetParameter(string name, object value)
        {
            switch (name.ToLower())
            {
                case LSParserParameters.CHARSET_OVERRIDES_XML_ENCODING:
                case LSParserParameters.DISALLOW_DOCTYPE:
                case LSParserParameters.IGNORE_UNKNOWN_CHARACTER_DENORMALIZATION:
                case LSParserParameters.INFOSET:
                case LSParserParameters.NAMESPACES:
                case LSParserParameters.RESOURCE_RESOLVER:
                case LSParserParameters.SUPPORTED_MEDIA_TYPES_ONLY:
                case LSParserParameters.VALIDATE:
                case LSParserParameters.VALIDATE_IF_SCHEMA:
                case LSParserParameters.WELL_FORMED:
                    return true;
                default:
                    return base.CanSetParameter(name, value);
            }
        }

        #endregion
    }
}
