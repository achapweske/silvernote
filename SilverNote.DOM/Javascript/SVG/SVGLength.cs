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
    public class SVGLength : ObjectInstance
    {
        private DOM.SVG.SVGLength _Owner;

        public SVGLength(DOM.SVG.SVGLength owner, ObjectInstance prototype)
            : base(prototype)
        {
            _Owner = owner;
            PopulateFields();
            PopulateFunctions();
        }

        [JSProperty(Name = "value")]
        public double Value
        {
            get { return _Owner.Value;  }
            set { _Owner.Value = value;  }
        }
    }
}
