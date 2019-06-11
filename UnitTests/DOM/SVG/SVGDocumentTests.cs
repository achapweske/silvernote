/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Diagnostics;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DOM;
using DOM.SVG;
using DOM.LS;

namespace UnitTests
{
    [TestClass]
    public class SVGDocumentTests
    {
        [TestMethod]
        public void TestSaveXML()
        {
            SVGDocument document = DOMFactory.CreateSVGDocument();
            var comment = document.CreateComment("Created with SilverNote v1.0.0000.00000 (http://www.silver-note.com/)");
            document.InsertBefore(comment, document.RootElement);
            var line = document.CreateElementNS(SVGElements.NAMESPACE, SVGElements.LINE);
            line.SetAttributeNS(null, SVGAttributes.STROKE_WIDTH, "2");
            line.SetAttributeNS(null, SVGAttributes.STROKE, "rgb(0,0,0)");
            line.SetAttributeNS(null, SVGAttributes.FILL, "none");
            line.SetAttributeNS(null, SVGAttributes.X1, "0");
            line.SetAttributeNS(null, SVGAttributes.Y1, "8");
            line.SetAttributeNS(null, SVGAttributes.X2, "24");
            line.SetAttributeNS(null, SVGAttributes.Y2, "8");
            document.RootElement.AppendChild(line);
            
            var loadAndSave = (DOMImplementationLS)document.Implementation;
            var serializer = loadAndSave.CreateLSSerializer();
            string output = serializer.WriteToString(document);

            string expected = 
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>" +
                @"<!--Created with SilverNote v1.0.0000.00000 (http://www.silver-note.com/)-->" +
                @"<svg version=""1.1"" xmlns=""http://www.w3.org/2000/svg"">" +
                    @"<line stroke-width=""2"" stroke=""rgb(0,0,0)"" fill=""none"" x1=""0"" y1=""8"" x2=""24"" y2=""8"" />" +
                @"</svg>";

            Assert.AreEqual(expected, output);
        }

       

    }
}
