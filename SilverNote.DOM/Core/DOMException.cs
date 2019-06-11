/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM
{
    public class DOMException : Exception
    {
        #region ExceptionCode

        public const ushort  INDEX_SIZE_ERR                 = 1;
        public const ushort  DOMSTRING_SIZE_ERR             = 2;
        public const ushort  HIERARCHY_REQUEST_ERR          = 3;
        public const ushort  WRONG_DOCUMENT_ERR             = 4;
        public const ushort  INVALID_CHARACTER_ERR          = 5;
        public const ushort  NO_DATA_ALLOWED_ERR            = 6;
        public const ushort  NO_MODIFICATION_ALLOWED_ERR    = 7;
        public const ushort  NOT_FOUND_ERR                  = 8;
        public const ushort  NOT_SUPPORTED_ERR              = 9;
        public const ushort  INUSE_ATTRIBUTE_ERR            = 10;
        // Introduced in DOM Level 2:
        public const ushort  INVALID_STATE_ERR              = 11;
        // Introduced in DOM Level 2:
        public const ushort  SYNTAX_ERR                     = 12;
        // Introduced in DOM Level 2:
        public const ushort  INVALID_MODIFICATION_ERR       = 13;
        // Introduced in DOM Level 2:
        public const ushort  NAMESPACE_ERR                  = 14;
        // Introduced in DOM Level 2:
        public const ushort  INVALID_ACCESS_ERR             = 15;

        #endregion

        public DOMException(ushort code)
            : base(GetMessage(code))
        {
            Code = code;
        }

        public DOMException(ushort code, string message)
            : base(message)
        {
            Code = code;
        }

        public DOMException(ushort code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }

        public ushort Code { get; set; }

        private static string GetMessage(ushort code)
        {
            switch (code)
            {
                case INDEX_SIZE_ERR:
                    return "Index out of range";
                case DOMSTRING_SIZE_ERR:
                    return "String size error";
                case HIERARCHY_REQUEST_ERR:
                    return "Hierarchy request error";
                case WRONG_DOCUMENT_ERR:
                    return "Wrong document";
                case INVALID_CHARACTER_ERR:
                    return "Invalid character";
                case NO_DATA_ALLOWED_ERR:
                    return "No data allowed";
                case NO_MODIFICATION_ALLOWED_ERR:
                    return "No modification allowed";
                case NOT_FOUND_ERR:
                    return "Item not found";
                case NOT_SUPPORTED_ERR:
                    return "Feature not supported";
                case INUSE_ATTRIBUTE_ERR:
                    return "The specified attribute is in use by another element";
                case INVALID_STATE_ERR:
                    return "Invalid state error";
                case SYNTAX_ERR:
                    return "Syntax error";
                case INVALID_MODIFICATION_ERR:
                    return "Invalid modification";
                case NAMESPACE_ERR:
                    return "Namespace error";
                case INVALID_ACCESS_ERR:
                    return "Invalid access";
                default:
                    return "An unknown has occurred";
            }
        }
    }
}
