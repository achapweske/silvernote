using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class DocumentConfig : DOMConfigurationBase
    {
        #region Fields

        bool _CanonicalForm;
        bool _CDataSections;
        bool _ElementContentWhitespace;
        bool _Entities;
        bool _Namespaces;
        bool _NamespaceDeclarations;
        bool _NormalizeCharacters;
        bool _WellFormed;

        #endregion

        #region Constructors

        public DocumentConfig()
        {
            CanonicalForm = false;
            CDataSections = true;
            CheckCharacterNormalization = false;
            Comments = true;
            DataTypeNormalization = false;
            ElementContentWhitespace = true;
            Entities = true;
            ErrorHandler = null;
            Namespaces = true;
            NamespaceDeclarations = true;
            NormalizeCharacters = false;
            SchemaLocation = null;
            SchemaType = null;
            SplitCDataSections = true;
            Validate = false;
            ValidateIfSchema = false;
            WellFormed = true;
        }

        #endregion

        #region Properties

        public bool CanonicalForm
        {
            get
            {
                return _CanonicalForm;
            }
            set
            {
                if (value)
                {
                    Entities = NormalizeCharacters = CDataSections = false;
                    Namespaces = NamespaceDeclarations = WellFormed = ElementContentWhitespace = true;
                }

                _CanonicalForm = value;
            }
        }
        public bool CDataSections
        {
            get
            {
                return _CDataSections;
            }
            set
            {
                if (value != _CDataSections)
                {
                    _CDataSections = value;
                    _CanonicalForm = false;
                }
            }
        }
        public bool CheckCharacterNormalization { get; set; }
        public bool Comments { get; set; }
        public bool DataTypeNormalization { get; set; }
        public bool ElementContentWhitespace
        {
            get
            {
                return _ElementContentWhitespace;
            }
            set
            {
                if (value != _ElementContentWhitespace)
                {
                    _ElementContentWhitespace = value;
                    _CanonicalForm = false;
                }
            }
        }
        public bool Entities
        {
            get 
            { 
                return _Entities; 
            }
            set
            {
                if (value != _Entities)
                {
                    _Entities = value;
                    _CanonicalForm = false;
                }
            }
        }
        public DOMErrorHandler ErrorHandler { get; set; }
        public bool Namespaces
        {
            get
            {
                return _Namespaces;
            }
            set
            {
                if (value != _Namespaces)
                {
                    _Namespaces = value;
                    _CanonicalForm = false;
                }
            }
        }
        public bool NamespaceDeclarations
        {
            get
            {
                return _NamespaceDeclarations;
            }
            set
            {
                if (value != _NamespaceDeclarations)
                {
                    _NamespaceDeclarations = value;
                    _CanonicalForm = false;
                }
            }
        }
        public bool NormalizeCharacters
        {
            get 
            { 
                return _NormalizeCharacters; 
            }
            set
            {
                if (value != _NormalizeCharacters)
                {
                    _NormalizeCharacters = value;
                    _CanonicalForm = false;
                }
            }
        }
        public string SchemaLocation { get; set; }
        public string SchemaType { get; set; }
        public bool SplitCDataSections { get; set; }
        public bool Validate { get; set; }
        public bool ValidateIfSchema { get; set; }
        public bool WellFormed
        {
            get
            {
                return _WellFormed;
            }
            set
            {
                if (value != _WellFormed)
                {
                    _WellFormed = value;
                    _CanonicalForm = false;
                }
            }
        }
        public bool Infoset
        {
            get
            {
                return !(ValidateIfSchema && Entities && DataTypeNormalization && CDataSections) && (NamespaceDeclarations && WellFormed && ElementContentWhitespace && Comments && Namespaces);
            }
            set
            {
                if (value)
                {
                    ValidateIfSchema = Entities = DataTypeNormalization = CDataSections = false;
                    NamespaceDeclarations = WellFormed = ElementContentWhitespace = Comments = Namespaces = true;
                }
            }
        }

        #endregion

        #region DOMConfiguration

        public override object GetParameter(string name)
        {
            switch (name.ToLower())
            {
                case DOMConfigurationParameters.CANONICAL_FORM:
                    return CanonicalForm;
                case DOMConfigurationParameters.CDATA_SECTIONS:
                    return CDataSections;
                case DOMConfigurationParameters.CHECK_CHARACTER_NORMALIZATION:
                    return CheckCharacterNormalization;
                case DOMConfigurationParameters.COMMENTS:
                    return Comments;
                case DOMConfigurationParameters.DATATYPE_NORMALIZATION:
                    return DataTypeNormalization;
                case DOMConfigurationParameters.ELEMENT_CONTENT_WHITESPACE:
                    return ElementContentWhitespace;
                case DOMConfigurationParameters.ENTITIES:
                    return Entities;
                case DOMConfigurationParameters.ERROR_HANDLER:
                    return ErrorHandler;
                case DOMConfigurationParameters.INFOSET:
                    return Infoset;
                case DOMConfigurationParameters.NAMESPACES:
                    return Namespaces;
                case DOMConfigurationParameters.NAMESPACE_DECLARATIONS:
                    return NamespaceDeclarations;
                case DOMConfigurationParameters.NORMALIZE_CHARACTERS:
                    return NormalizeCharacters;
                case DOMConfigurationParameters.SCHEMA_LOCATION:
                    return SchemaLocation;
                case DOMConfigurationParameters.SCHEMA_TYPE:
                    return SchemaType;
                case DOMConfigurationParameters.SPLIT_CDATA_SECTIONS:
                    return SplitCDataSections;
                case DOMConfigurationParameters.VALIDATE:
                    return Validate;
                case DOMConfigurationParameters.VALIDATE_IF_SCHEMA:
                    return ValidateIfSchema;
                case DOMConfigurationParameters.WELL_FORMED:
                    return WellFormed;
                default:
                    return base.GetParameter(name);
            }
        }

        public override void SetParameter(string name, object value)
        {
            switch (name.ToLower())
            {
                case DOMConfigurationParameters.CANONICAL_FORM:
                    CanonicalForm = (bool)value;
                    break;
                case DOMConfigurationParameters.CDATA_SECTIONS:
                    CDataSections = (bool)value;
                    break;
                case DOMConfigurationParameters.CHECK_CHARACTER_NORMALIZATION:
                    CheckCharacterNormalization = (bool)value;
                    break;
                case DOMConfigurationParameters.COMMENTS:
                    Comments = (bool)value;
                    break;
                case DOMConfigurationParameters.DATATYPE_NORMALIZATION:
                    DataTypeNormalization = (bool)value;
                    break;
                case DOMConfigurationParameters.ELEMENT_CONTENT_WHITESPACE:
                    ElementContentWhitespace = (bool)value;
                    break;
                case DOMConfigurationParameters.ENTITIES:
                    Entities = (bool)value;
                    break;
                case DOMConfigurationParameters.ERROR_HANDLER:
                    ErrorHandler = (DOMErrorHandler)value;
                    break;
                case DOMConfigurationParameters.INFOSET:
                    Infoset = (bool)value;
                    break;
                case DOMConfigurationParameters.NAMESPACES:
                    Namespaces = (bool)value;
                    break;
                case DOMConfigurationParameters.NAMESPACE_DECLARATIONS:
                    NamespaceDeclarations = (bool)value;
                    break;
                case DOMConfigurationParameters.NORMALIZE_CHARACTERS:
                    NormalizeCharacters = (bool)value;
                    break;
                case DOMConfigurationParameters.SCHEMA_LOCATION:
                    SchemaLocation = (string)value;
                    break;
                case DOMConfigurationParameters.SCHEMA_TYPE:
                    SchemaType = (string)value;
                    break;
                case DOMConfigurationParameters.SPLIT_CDATA_SECTIONS:
                    SplitCDataSections = (bool)value;
                    break;
                case DOMConfigurationParameters.VALIDATE:
                    Validate = (bool)value;
                    break;
                case DOMConfigurationParameters.VALIDATE_IF_SCHEMA:
                    ValidateIfSchema = (bool)value;
                    break;
                case DOMConfigurationParameters.WELL_FORMED:
                    WellFormed = (bool)value;
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
                case DOMConfigurationParameters.CANONICAL_FORM:
                case DOMConfigurationParameters.CDATA_SECTIONS:
                case DOMConfigurationParameters.CHECK_CHARACTER_NORMALIZATION:
                case DOMConfigurationParameters.COMMENTS:
                case DOMConfigurationParameters.DATATYPE_NORMALIZATION:
                case DOMConfigurationParameters.ELEMENT_CONTENT_WHITESPACE:
                case DOMConfigurationParameters.ENTITIES:
                case DOMConfigurationParameters.ERROR_HANDLER:
                case DOMConfigurationParameters.INFOSET:
                case DOMConfigurationParameters.NAMESPACES:
                case DOMConfigurationParameters.NAMESPACE_DECLARATIONS:
                case DOMConfigurationParameters.NORMALIZE_CHARACTERS:
                case DOMConfigurationParameters.SCHEMA_LOCATION:
                case DOMConfigurationParameters.SCHEMA_TYPE:
                case DOMConfigurationParameters.SPLIT_CDATA_SECTIONS:
                case DOMConfigurationParameters.VALIDATE:
                case DOMConfigurationParameters.VALIDATE_IF_SCHEMA:
                case DOMConfigurationParameters.WELL_FORMED:
                    return true;
                default:
                    return base.CanSetParameter(name, value);
            }
        }

        #endregion
    }
}
