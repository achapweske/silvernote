/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.SVG
{
    public static class SVGHelper
    {
        public static string GetDefinitionId(SVGDocument document, SVGElement element)
        {
            SVGDefsElement defs = (SVGDefsElement)document.GetElementsByTagNameNS(SVGElements.NAMESPACE, SVGElements.DEFS)[0];
            if (defs == null)
            {
                return null;
            }

            foreach (SVGElement def in defs.ChildNodes.OfType<SVGElement>())
            {
                element.ID = def.ID;

                if (def.IsEqualNode(element))
                {
                    return def.ID;
                }
            }

            return null;
        }

        public static string AddDefinition(SVGDocument document, SVGElement element)
        {
            SVGDefsElement defs = (SVGDefsElement)document.GetElementsByTagNameNS(SVGElements.NAMESPACE, SVGElements.DEFS)[0];
            if (defs == null)
            {
                defs = (SVGDefsElement)document.CreateElementNS(SVGElements.NAMESPACE, SVGElements.DEFS);
                document.RootElement.InsertBefore(defs, document.RootElement.FirstChild);
            }

            string id = RandomID();
            while (document.GetElementById(id) != null)
            {
                id = RandomID();
            }
            element.ID = id;

            defs.AppendChild(element);

            return id;
        }

        private static string RandomID()
        {
            return System.IO.Path.GetRandomFileName().Replace(".", "");
        }
    }
}
