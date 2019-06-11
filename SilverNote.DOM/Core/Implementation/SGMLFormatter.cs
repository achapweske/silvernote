using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public static class SGMLFormatter
    {
        public static string FormatDocumentType(DocumentType docType)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatDocumentType(writer, docType);
                return writer.ToString();
            }
        }

        public static void FormatDocumentType(TextWriter writer, DocumentType docType)
        {
            writer.Write("<!DOCTYPE ");

            writer.Write(docType.Name);

            if (!String.IsNullOrEmpty(docType.PublicId))
            {
                writer.Write(" PUBLIC");
            }
            else if (!String.IsNullOrEmpty(docType.SystemId))
            {
                writer.Write(" SYSTEM");
            }

            if (!String.IsNullOrEmpty(docType.PublicId))
            {
                writer.Write(" \"");
                writer.Write(docType.PublicId);
                writer.Write('\"');
            }

            if (!String.IsNullOrEmpty(docType.SystemId))
            {
                writer.Write(" \"");
                writer.Write(docType.SystemId);
                writer.Write('\"');
            }

            writer.Write(">");
        }
    }
}
