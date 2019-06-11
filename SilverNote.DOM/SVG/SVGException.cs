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
    public class SVGException : Exception
    {
        #region ExceptionCode

        public const ushort SVG_WRONG_TYPE_ERR = 0;
        public const ushort SVG_INVALID_VALUE_ERR = 1;
        public const ushort SVG_MATRIX_NOT_INVERTABLE = 2;

        #endregion

        public SVGException(ushort code)
            : base(GetMessage(code))
        {
            Code = code;
        }

        public SVGException(ushort code, string message)
            : base(message)
        {
            Code = code;
        }

        public SVGException(ushort code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }

        public ushort Code { get; set; }

        private static string GetMessage(ushort code)
        {
            switch (code)
            {
                case SVG_WRONG_TYPE_ERR:
                    return "Wrong type";
                case SVG_INVALID_VALUE_ERR:
                    return "Invalid value";
                case SVG_MATRIX_NOT_INVERTABLE:
                    return "Matrix not invertable";
                default:
                    return "An unknown has occurred";
            }
        }
    }
}
