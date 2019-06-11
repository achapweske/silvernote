/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;

namespace DOM.SVG.Internal
{
    public class SVGPaintImpl : SVGColorImpl, SVGPaint
    {
        #region Fields

        SVGPaintType _PaintType;
        string _Uri;
        string _CssText;

        #endregion

        #region Constructor

        public SVGPaintImpl(SVGPaintType paintType)
            : this(paintType, null, null, null)
        {

        }

        public SVGPaintImpl(SVGPaintType paintType, RGBColor rgbColor)
            : this(paintType, null, rgbColor, null)
        {

        }

        public SVGPaintImpl(SVGPaintType paintType, RGBColor rgbColor, SVGICCColor iccColor)
            : this(paintType, null, rgbColor, iccColor)
        {

        }

        public SVGPaintImpl(SVGPaintType paintType, string uri)
            : this(paintType, uri, null, null)
        {

        }

        public SVGPaintImpl(SVGPaintType paintType, string uri, RGBColor rgbColor)
            : this(paintType, uri, rgbColor, null)
        {

        }

        public SVGPaintImpl(SVGPaintType paintType, string uri, RGBColor rgbColor, SVGICCColor iccColor)
            : base((SVGColorType)paintType, rgbColor, iccColor)
        {
            _PaintType = paintType;
            _Uri = uri;
        }

        protected SVGPaintImpl(CSSValueType cssValueType)
            : base(cssValueType)
        {

        }

        #endregion

        #region SVGPaint

        public virtual SVGPaintType PaintType
        {
            get { return _PaintType; }
        }

        public virtual string Uri
        {
            get { return _Uri; }
        }

        public virtual void SetUri(string uri)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        public virtual void SetPaint(SVGPaintType paintType, string uri, string rgbColor, string iccColor)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        #endregion

        #region CSSValue

        public override string CssText
        {
            get
            {
                if (_CssText == null)
                {
                    _CssText = SVGFormatter.FormatPaint(this);
                }

                return _CssText;
            }
            set
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }
        }

        #endregion

        #region Extensions

        new public static readonly SVGPaint Inherit = new SVGPaintImpl(CSSValueType.CSS_INHERIT);
        public static readonly SVGPaint None = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_NONE);
        new public static readonly SVGPaint CurrentColor = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_CURRENTCOLOR);

        #endregion
    }
}
