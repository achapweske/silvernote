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
    public interface DOMImplementation
    {
        bool HasFeature(string feature, string version);
        object GetFeature(string feature, string version);
        DocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId);
        Document CreateDocument(string namespaceURI, string qualifiedName, DocumentType docType);
    }
}
