/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using DOM.Internal;
using DOM.CSS;

namespace DOM.LS.Internal
{
    public class LSSerializerImpl : LSSerializer
    {
        #region Fields

        LSSerializerConfig _Config = new LSSerializerConfig();

        #endregion

        #region Constructors

        public LSSerializerImpl()
        {

        }

        #endregion

        #region DOMSerializer

        public DOMConfiguration Config
        {
            get { return _Config; }
        }
        
        public string NewLine { get; set; }
        
        public LSSerializerFilter Filter { get; set; }

        public string WriteToString(Node node)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                LSOutput destination = new LSOutputImpl
                {
                    CharacterStream = writer
                };

                Write(node, destination);

                return writer.ToString();
            }
        }

        public bool WriteURI(Node node, string uri)
        {
            LSOutput destination = new LSOutputImpl
            {
                SystemId = uri
            };

            return Write(node, destination);
        }

        public bool Write(Node node, LSOutput destination)
        {
            var settings = new XmlWriterSettings();

            settings.OmitXmlDeclaration = !_Config.XmlDeclaration;
            settings.Indent = _Config.FormatPrettyPrint;

            string encodingName = DetermineEncoding(node, destination);
            try
            {
                settings.Encoding = Encoding.GetEncoding(encodingName);
            }
            catch
            {
                ReportError(DOMErrorSeverityType.SEVERITY_FATAL_ERROR, "unsupported-encoding", "Unsupported encoding");
                return false;
            }

            // "The DOMSerializer will look at the different outputs specified
            // in the DOMOutput in the following order to know which one to 
            // output to, the first one that data can be output to will be used:
            //
            //   DOMOutput.characterStream
            //   DOMOutput.byteStream
            //   DOMOutput.systemId
            //
            // http://www.w3.org/TR/2003/WD-DOM-Level-3-LS-20030619/load-save.html#LS-DOMOutput

            if (destination.CharacterStream != null)
            {
                // XmlWriter ignores the specified encoding when writing its declaration

                if (!settings.OmitXmlDeclaration)
                {
                    settings.OmitXmlDeclaration = true;
                    string decl = String.Format("<?xml version=\"{0}\" encoding=\"{1}\" standalone=\"{2}\"?>", "1.0", encodingName, "no");
                    destination.CharacterStream.Write(decl);
                }

                using (var writer = XmlWriter.Create(destination.CharacterStream, settings))
                {
                    return WriteNode(node, writer);
                }
            }
            else if (destination.ByteStream != null)
            {
                using (var writer = XmlWriter.Create(destination.ByteStream, settings))
                {
                    return WriteNode(node, writer);
                }
            }
            else if (!String.IsNullOrEmpty(destination.SystemId))
            {
                throw new NotImplementedException();
            }
            else
            {
                ReportError(DOMErrorSeverityType.SEVERITY_FATAL_ERROR, "no-output-specified", "No output specified");
                return false;
            }
        }

        #endregion

        #region Implementation

        string DetermineEncoding(Node node, LSOutput destination)
        {
            // "When writing to a DOMOutput, the encoding is found by looking at 
            // the encoding information that is reachable through the DOMOutput 
            // and the item to be written (or its owner document) in this order:
            //
            // DOMOutput.encoding,
            // Document.actualEncoding,
            // Document.xmlEncoding.
            //
            // If no encoding is reachable through the above properties, a default 
            // encoding of "UTF-8" will be used. If the specified encoding is not 
            // supported an "unsupported-encoding" error is raised."
            //
            // http://www.w3.org/TR/2003/WD-DOM-Level-3-LS-20030619/load-save.html#LS-DOMSerializer

            if (!String.IsNullOrEmpty(destination.Encoding))
            {
                return destination.Encoding;
            }

            Document document = node.OwnerDocument ?? node as Document;

            if (document != null && !String.IsNullOrEmpty(document.XmlEncoding))
            {
                return document.XmlEncoding;
            }

            return "UTF-8";
        }

        string SetDeclaredEncoding(string str, string encoding)
        {
            if (!str.StartsWith("<?xml"))
            {
                return str;
            }

            int declarationEnd = str.IndexOf("?>");
            if (declarationEnd == -1)
            {
                return str;
            }

            int encodingStart = str.IndexOf("encoding=\"", 0, declarationEnd);
            if (encodingStart == -1)
            {
                return str;
            }
            encodingStart += "encoding=\"".Length;

            int encodingEnd = str.IndexOf('\"', encodingStart, declarationEnd - encodingStart);
            if (encodingEnd == -1)
            {
                return str;
            }

            return str.Remove(encodingStart, encodingEnd - encodingStart).Insert(encodingStart, encoding);
        }

        bool WriteNode(Node node, XmlWriter writer)
        {
            switch (node.NodeType)
            {
                case NodeType.DOCUMENT_NODE:
                    return WriteDocument((Document)node, writer);
                case NodeType.DOCUMENT_TYPE_NODE:
                    return WriteDocumentType((DocumentType)node, writer);
                case NodeType.ELEMENT_NODE:
                    return WriteElement((Element)node, writer);
                case NodeType.TEXT_NODE:
                    return WriteText((Text)node, writer);
                case NodeType.COMMENT_NODE:
                    return WriteComment((Comment)node, writer);
                case NodeType.PROCESSING_INSTRUCTION_NODE:
                    return WriteProcessorInstruction((ProcessingInstruction)node, writer);
                case NodeType.CDATA_SECTION_NODE:
                    return WriteCDataSection((CDATASection)node, writer);
                default:
                    return false;
            }
        }

        bool WriteDocument(Document document, XmlWriter writer)
        {
            writer.WriteStartDocument(document.XmlStandalone);

            if (document.HasChildNodes())
            {
                foreach (var node in document.ChildNodes)
                {
                    WriteNode(node, writer);
                }
            }

            writer.WriteEndDocument();

            return true;
        }

        bool WriteDocumentType(DocumentType docType, XmlWriter writer)
        {
            writer.WriteDocType(docType.Name, docType.PublicId, docType.SystemId, docType.InternalSubset);
            return true;
        }

        bool WriteElement(Element element, XmlWriter writer)
        {
            if (element.LocalName != null)
            {
                writer.WriteStartElement(element.Prefix ?? "", element.LocalName, element.NamespaceURI);
            }
            else
            {
                writer.WriteStartElement(element.NodeName);
            }

            if (element is CSS.Internal.CSSElement)
            {
                element.UpdateStyle(false);
            }

            if (element.HasAttributes())
            {
                foreach (Attr attr in element.Attributes)
                {
                    if (attr.LocalName != null)
                    {
                        writer.WriteAttributeString(attr.Prefix, attr.LocalName, attr.NamespaceURI, attr.Value);
                    }
                    else
                    {
                        writer.WriteAttributeString(attr.Name, attr.Value);
                    }
                }
            }

            if (element.HasChildNodes())
            {
                foreach (var node in element.ChildNodes)
                {
                    WriteNode(node, writer);
                }
            }

            writer.WriteEndElement();

            return true;
        }

        bool WriteText(Text text, XmlWriter writer)
        {
            writer.WriteString(text.Data);
            return true;
        }

        bool WriteComment(Comment comment, XmlWriter writer)
        {
            writer.WriteComment(comment.Data);
            return true;
        }

        bool WriteProcessorInstruction(ProcessingInstruction pi, XmlWriter writer)
        {
            writer.WriteProcessingInstruction(pi.Target, pi.Data);
            return true;
        }

        bool WriteCDataSection(CDATASection cdata, XmlWriter writer)
        {
            writer.WriteCData(cdata.Data);
            return true;
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
