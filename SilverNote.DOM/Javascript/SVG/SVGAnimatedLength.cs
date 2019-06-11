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
    public class SVGAnimatedLength : ObjectInstance
    {
        private readonly DOM.SVG.SVGAnimatedLength _Owner;
        private SVGLength _BaseVal;
        private SVGLength _AnimVal;

        public SVGAnimatedLength(DOM.SVG.SVGAnimatedLength owner, ObjectInstance prototype)
            : base(prototype)
        {
            _Owner = owner;
            PopulateFields();
            PopulateFunctions();
        }

        [JSProperty(Name = "animVal")]
        public SVGLength AnimVal
        {
            get { return _AnimVal ?? (_AnimVal = new SVGLength(_Owner.AnimVal, Prototype)); }
        }

        [JSProperty(Name = "baseVal")]
        public SVGLength BaseVal
        {
            get { return _BaseVal ?? (_BaseVal = new SVGLength(_Owner.BaseVal, Prototype));  }
        }
    }
}
