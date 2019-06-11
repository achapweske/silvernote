/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace DOM.CSS.Internal
{
    public class CSSRectBase : CSSRect
    {
        #region Fields

        CSSPrimitiveValue _Top;
        CSSPrimitiveValue _Right;
        CSSPrimitiveValue _Bottom;
        CSSPrimitiveValue _Left;

        #endregion

        #region Constructors

        public CSSRectBase()
        {
            _Top = new CSSPrimitiveValueBase(0, CSSPrimitiveType.CSS_DIMENSION);
            _Right = new CSSPrimitiveValueBase(0, CSSPrimitiveType.CSS_DIMENSION);
            _Bottom = new CSSPrimitiveValueBase(0, CSSPrimitiveType.CSS_DIMENSION);
            _Left = new CSSPrimitiveValueBase(0, CSSPrimitiveType.CSS_DIMENSION);
        }

        public CSSRectBase(CSSPrimitiveValue top, CSSPrimitiveValue right, CSSPrimitiveValue bottom, CSSPrimitiveValue left)
        {
            _Top = top;
            _Right = right;
            _Bottom = bottom;
            _Left = left;
        }

        public CSSRectBase(double top, double right, double bottom, double left)
        {
            _Top = new CSSPrimitiveValueBase(top, CSSPrimitiveType.CSS_DIMENSION);
            _Right = new CSSPrimitiveValueBase(right, CSSPrimitiveType.CSS_DIMENSION);
            _Bottom = new CSSPrimitiveValueBase(bottom, CSSPrimitiveType.CSS_DIMENSION);
            _Left = new CSSPrimitiveValueBase(left, CSSPrimitiveType.CSS_DIMENSION);
        }

        #endregion

        #region CSSRect

        public CSSPrimitiveValue Top
        {
            get { return _Top; }
        }

        public CSSPrimitiveValue Right
        {
            get { return _Right; }
        }

        public CSSPrimitiveValue Bottom
        {
            get { return _Bottom; }
        }

        public CSSPrimitiveValue Left
        {
            get { return _Left; }
        }

        #endregion

        #region Extensions

        public static CSSRectBase FromRect(Rect fromRect)
        {
            return new CSSRectBase(fromRect.Top, fromRect.Right, fromRect.Bottom, fromRect.Left);
        }

        public Rect ToRect()
        {
            double left = Left == CSSValues.Auto ? 0 : Left.GetFloatValue(CSSPrimitiveType.CSS_PX);
            double top = Top == CSSValues.Auto ? 0 : Top.GetFloatValue(CSSPrimitiveType.CSS_PX);
            double right = Right == CSSValues.Auto ? 0 : Right.GetFloatValue(CSSPrimitiveType.CSS_PX);
            double bottom = Bottom == CSSValues.Auto ? 0 : Bottom.GetFloatValue(CSSPrimitiveType.CSS_PX);

            double x = Math.Min(left, right);
            double y = Math.Min(top, bottom);
            double width = Math.Abs(right - left);
            double height = Math.Abs(bottom - top);

            return new Rect(x, y, width, height);
        }

        #endregion

        #region Parsing

        public static CSSRectBase Parse(string str)
        {
            CSSRectBase result;
            if (TryParse(str, out result))
            {
                return result;
            }
            else
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        public static bool TryParse(string str, out CSSRectBase result)
        {
            str = str.ToLower().Trim();

            if (!str.StartsWith("rect("))
            {
                result = null;
                return false;
            }

            str = str.Remove(0, 5);

            if (str.EndsWith(")"))
            {
                str = str.Remove(str.Length - 1);
            }

            // spec says items are separated by ',' and for compatibility we also
            // allow separation by whitespace

            string[] values = str.Split(", \t".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            if (values.Length != 4)
            {
                result = null;
                return false;
            }

            CSSPrimitiveValueBase top, right, bottom, left;

            if (!CSSPrimitiveValueBase.TryParse(values[0], out top) ||
                !CSSPrimitiveValueBase.TryParse(values[1], out right) ||
                !CSSPrimitiveValueBase.TryParse(values[2], out bottom) ||
                !CSSPrimitiveValueBase.TryParse(values[3], out left))
            {
                result = null;
                return false;
            }

            result = new CSSRectBase(top, right, bottom, left);
            return true;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CSSFormatter.FormatRect(this);
        }

        #endregion
    }
}
