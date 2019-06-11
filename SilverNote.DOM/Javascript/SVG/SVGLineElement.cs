/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jurassic;
using Jurassic.Library;

namespace DOM.Javascript.SVG
{
    public class SVGLineElement : SVGElement
    {
        DOM.SVG.SVGLineElement _Element;
        SVGAnimatedLength _X1;
        SVGAnimatedLength _Y1;
        SVGAnimatedLength _X2;
        SVGAnimatedLength _Y2;

        public SVGLineElement(ScriptEngine engine, DOM.SVG.SVGLineElement element)
            : base(new SVGElement(engine, element), element)
        {
            _Element = element;
            PopulateFields();
            PopulateFunctions();
        }

        [JSProperty(Name = "x1")]
        public SVGAnimatedLength X1
        {
            get { return _X1 ?? (_X1 = new SVGAnimatedLength(_Element.X1, Prototype));  }
        }

        [JSProperty(Name = "y1")]
        public SVGAnimatedLength Y1
        {
            get { return _Y1 ?? (_Y1 = new SVGAnimatedLength(_Element.Y1, Prototype)); }
        }

        [JSProperty(Name = "x2")]
        public SVGAnimatedLength X2
        {
            get { return _X2 ?? (_X2 = new SVGAnimatedLength(_Element.X2, Prototype)); }
        }

        [JSProperty(Name = "y2")]
        public SVGAnimatedLength Y2
        {
            get { return _Y2 ?? (_Y2 = new SVGAnimatedLength(_Element.Y2, Prototype)); }
        }

    }
}
