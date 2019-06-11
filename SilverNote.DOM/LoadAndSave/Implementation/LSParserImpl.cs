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
using System.Xml;
using DOM.Internal;

namespace DOM.LS.Internal
{
    public class LSParserImpl : LSParser
    {
        #region Fields

        LSParserConfig _Config = new LSParserConfig();

        #endregion

        #region Constructors

        #endregion

        #region Properties

        public DOMConfiguration Config
        {
            get { return _Config; }
        }
        
        public LSParserFilter Filter { get; set; }

        public bool Async
        {
            get { return false; }
        }

        public bool Busy 
        {
            get { return false; }
        }

        #endregion

        #region Methods

        public Document ParseURI(string uri)
        {
            var input = new LSInputImpl();
            input.SystemId = uri;
            return Parse(input);
        }

        public Document Parse(LSInput input)
        {
            Node result = ReadAllNodes(input);
            if (result.NodeType != NodeType.DOCUMENT_NODE)
            {
                throw new Exception();
            }

            return (Document)result;
        }
        
        public Node ParseWithContext(LSInput input, NodeContext context, ActionTypes action)
        {
            Node result = ReadAllNodes(input, context.OwnerDocument);

            switch (action)
            {
                case ActionTypes.ACTION_APPEND_AS_CHILDREN:
                case ActionTypes.ACTION_INSERT_AFTER:
                case ActionTypes.ACTION_INSERT_BEFORE:
                case ActionTypes.ACTION_REPLACE:
                    throw new NotImplementedException();
                case ActionTypes.ACTION_REPLACE_CHILDREN:
                    // "Replace all the children of the context node with the 
                    // result of the parse operation. For this action to work, 
                    // the context node must be an Element, a Document, or a 
                    // DocumentFragment.
                    break;
                default:
                    break;
            }

            return result;
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation

        Node ReadAllNodes(LSInput input, Document ownerDocument = null)
        {
            var settings = new XmlReaderSettings();

            if (_Config.DisallowDoctype)
            {
                settings.DtdProcessing = DtdProcessing.Prohibit;
            }
            else
            {
                settings.DtdProcessing = DtdProcessing.Parse;
            }

            settings.XmlResolver = null;

            // "The LSParser will use the LSInput object to determine how to 
            // read data. The LSParser will look at the different inputs 
            // specified in the LSInput in the following order to know which 
            // one to read from, the first one that is not null and not an 
            // empty string will be used:
            //
            //    LSInput.characterStream
            //    LSInput.byteStream
            //    LSInput.stringData
            //    LSInput.systemId
            //    LSInput.publicId
            //
            // http://www.w3.org/TR/DOM-Level-3-LS/load-save.html#LS-LSInput

            if (input.CharacterStream != null)
            {
                using (var reader = XmlReader.Create(input.CharacterStream, settings))
                {
                    return ReadAll(reader, ownerDocument);
                }
            }
            else if (input.ByteStream != null)
            {
                using (var reader = XmlReader.Create(input.ByteStream, settings))
                {
                    return ReadAll(reader, ownerDocument);
                }
            }
            else if (!String.IsNullOrEmpty(input.StringData))
            {
                using (var stream = new StringReader(input.StringData))
                {
                    using (var reader = XmlReader.Create(stream, settings))
                    {
                        return ReadAll(reader, ownerDocument);
                    }
                }
            }
            else if (!String.IsNullOrEmpty(input.SystemId))
            {
                throw new NotImplementedException();
            }
            else if (!String.IsNullOrEmpty(input.PublicId))
            {
                throw new NotImplementedException();
            }
            else
            {
                ReportError(DOMErrorSeverityType.SEVERITY_FATAL_ERROR, "no-input-specified", "No input specified");
                return null;
            }
        }

        Node ReadAll(XmlReader reader, Document ownerDocument)
        {
            if (ownerDocument != null)
            {
                return ReadDocumentFragment(reader, ownerDocument);
            }
            else
            {
                return ReadDocument(reader);
            }
        }

        Node ReadNode(XmlReader reader, Document ownerDocument)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Attribute:
                    return ReadAttribute(reader, ownerDocument);
                case XmlNodeType.CDATA:
                    return ReadCData(reader, ownerDocument);
                case XmlNodeType.Comment:
                    return ReadComment(reader, ownerDocument);
                case XmlNodeType.Document:
                    return ReadDocument(reader);
                case XmlNodeType.DocumentFragment:
                    return ReadDocumentFragment(reader, ownerDocument);
                case XmlNodeType.DocumentType:
                    return ReadDocumentType(reader, ownerDocument);
                case XmlNodeType.Element:
                    return ReadElement(reader, ownerDocument);
                case XmlNodeType.Text:
                    return ReadText(reader, ownerDocument);
                case XmlNodeType.EndElement:
                case XmlNodeType.EndEntity:
                case XmlNodeType.Entity:
                case XmlNodeType.EntityReference:
                case XmlNodeType.None:
                case XmlNodeType.Notation:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Whitespace:
                case XmlNodeType.XmlDeclaration:
                default:
                    reader.Skip();
                    return null;
            }
        }


        Document ReadDocument(XmlReader reader)
        {
            string xmlVersion = null;
            string xmlEncoding = null;
            string xmlStandalone = null;
            Document buffer = new DocumentBase();

            reader.Read();

            while (reader.NodeType != XmlNodeType.Element)
            {
                if (reader.NodeType == XmlNodeType.XmlDeclaration)
                {
                    xmlVersion = reader.GetAttribute("version");
                    xmlEncoding = reader.GetAttribute("encoding");
                    xmlStandalone = reader.GetAttribute("standalone");
                    reader.Skip();
                }
                else
                {
                    Node node = ReadNode(reader, buffer);
                    if (node != null)
                    {
                        buffer.AppendChild(node);
                    }
                }
            }

            // Name

            string namespaceURI = reader.NamespaceURI;
            string qualifiedName = FormatQualifiedName(reader.Prefix, reader.LocalName);

            Document result = DOMFactory.CreateDocument(namespaceURI, qualifiedName, null);

            if (xmlEncoding != null && result is DocumentBase)
                ((DocumentBase)result).SetEncoding(xmlEncoding);
            if (xmlVersion != null)
                result.XmlVersion = xmlVersion;
            if (xmlStandalone != null)
                result.XmlStandalone = (xmlStandalone == "yes");

            if (result.DocumentElement != null)
            {
                result.RemoveChild(result.DocumentElement);
            }

            foreach (var child in buffer.ChildNodes.ToArray())
            {
                buffer.RemoveChild(child);
                result.AdoptNode(child);
                result.AppendChild(child);
            }

            // Content

            while (reader.NodeType != XmlNodeType.None)
            {
                Node child = ReadNode(reader, result);
                if (child != null)
                {
                    result.AppendChild(child);
                }
            }

            return result;
        }

        DocumentType ReadDocumentType(XmlReader reader, Document ownerDocument)
        {
            string qualifiedName = reader.Name;
            string publicId = reader.GetAttribute("PUBLIC");
            string systemId = reader.GetAttribute("SYSTEM");

            reader.Read();

            return DOMFactory.CreateDocumentType(qualifiedName, publicId, systemId);
        }

        DocumentFragment ReadDocumentFragment(XmlReader reader, Document ownerDocument)
        {
            DocumentFragment result = ownerDocument.CreateDocumentFragment();

            reader.Read();

            while (reader.NodeType != XmlNodeType.None)
            {
                Node child = ReadNode(reader, ownerDocument);
                if (child != null)
                {
                    result.AppendChild(child);
                }
            }

            return result;
        }

        Element ReadElement(XmlReader reader, Document ownerDocument)
        {
            Element result;

            // Name

            if (!String.IsNullOrEmpty(reader.LocalName))
            {
                string qualifiedName = FormatQualifiedName(reader.Prefix, reader.LocalName);
                result = ownerDocument.CreateElementNS(reader.NamespaceURI, qualifiedName);
            }
            else
            {
                result = ownerDocument.CreateElement(reader.Name);
            }

            // Attributes 

            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    Attr attr = ReadAttribute(reader, ownerDocument);

                    if (!String.IsNullOrEmpty(attr.LocalName))
                    {
                        result.SetAttributeNodeNS(attr);
                    }
                    else
                    {
                        result.SetAttributeNode(attr);
                    }

                } while (reader.MoveToNextAttribute());
            }

            reader.MoveToElement();

            // Children

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return result;
            }

            reader.ReadStartElement();

            while (reader.NodeType != XmlNodeType.None)
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.ReadEndElement();
                    break;
                }

                Node child = ReadNode(reader, ownerDocument);
                if (child != null)
                {
                    result.AppendChild(child);
                }
            }

            return result;
        }

        Attr ReadAttribute(XmlReader reader, Document ownerDocument)
        {
            Attr result;

            if (!String.IsNullOrEmpty(reader.NamespaceURI))
            {
                string qualifiedName = FormatQualifiedName(reader.Prefix, reader.LocalName);
                result = ownerDocument.CreateAttributeNS(reader.NamespaceURI, qualifiedName);
            }
            else
            {
                result = ownerDocument.CreateAttribute(reader.Name);
            }

            result.Value = reader.Value;

            return result;
        }

        CDATASection ReadCData(XmlReader reader, Document ownerDocument)
        {
            var result = ownerDocument.CreateCDATASection(reader.Value);
            reader.Read();
            return result;
        }

        Comment ReadComment(XmlReader reader, Document ownerDocument)
        {
            var result = ownerDocument.CreateComment(reader.Value);
            reader.Read();
            return result;
        }

        Text ReadText(XmlReader reader, Document ownerDocument)
        {
            var result = ownerDocument.CreateTextNode(reader.Value);
            reader.Read();
            return result;
        }



        static string FormatQualifiedName(string prefix, string localName)
        {
            if (!String.IsNullOrEmpty(prefix))
            {
                return prefix + ":" + localName;
            }
            else
            {
                return localName;
            }
        }

        void ReportError(DOMErrorSeverityType severity, string type, string message)
        {
            ReportError(new DOMErrorBase(severity, type, message));
        }

        void ReportError(DOMError error)
        {
            var errorHandler = Config.GetParameter(DOMParameters.ERROR_HANDLER) as DOMErrorHandler;
            if (errorHandler != null)
            {
                errorHandler.HandleError(error);
            }
        }

        #endregion
    }
}
