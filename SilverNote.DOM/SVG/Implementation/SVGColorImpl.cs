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
using DOM.CSS.Internal;

namespace DOM.SVG.Internal
{
    public class SVGColorImpl : CSSValueBase, SVGColor
    {
        #region Fields

        SVGColorType _ColorType;
        RGBColor _RgbColor;
        SVGICCColor _IccColor;

        #endregion

        #region Constructors

        public SVGColorImpl(SVGColorType colorType)
            : this(colorType, null, null)
        {

        }

        public SVGColorImpl(SVGColorType colorType, RGBColor rgbColor)
            : this(colorType, rgbColor, null)
        {

        }

        public SVGColorImpl(SVGColorType colorType, RGBColor rgbColor, SVGICCColor iccColor)
            : this(CSSValueType.CSS_CUSTOM)
        {
            _ColorType = colorType;
            _RgbColor = rgbColor;
            _IccColor = iccColor;
        }

        protected SVGColorImpl(CSSValueType cssValueType)
            : base(cssValueType)
        {
            _ColorType = SVGColorType.SVG_COLORTYPE_UNKNOWN;
            _RgbColor = null;
            _IccColor = null;
        }

        #endregion

        #region SVGColor

        public virtual SVGColorType ColorType
        {
            get { return _ColorType; }
        }

        public virtual RGBColor RgbColor
        {
            get { return _RgbColor; }
        }

        public virtual SVGICCColor IccColor
        {
            get { return _IccColor; }
        }

        public virtual void SetRGBColor(string rgbColor)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        public virtual void SetRGBColorICCColor(string rgbColor, string iccColor)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        public virtual void SetColor(SVGColorType colorType, string rgbColor, string iccColor)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        #endregion

        #region Extensions

        public static readonly SVGColor Inherit = new SVGColorImpl(CSSValueType.CSS_INHERIT);
        public static readonly SVGColor CurrentColor = new SVGColorImpl(SVGColorType.SVG_COLORTYPE_CURRENTCOLOR);

        #endregion
    }
}
