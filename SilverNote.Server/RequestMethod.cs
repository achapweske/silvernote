/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Server
{
    public enum RequestMethod
    {
        Any,
        Get,
        Put,
        Post,
        Patch,
        Delete,
        Head,
        None
    }

    public class RequestMethodHelper
    {
        public static RequestMethod Parse(string str)
        {
            RequestMethod result;
            if (!TryParse(str, out result))
            {
                throw new FormatException();
            }
            return result;
        }

        public static RequestMethod Parse(string str, RequestMethod defaultValue = RequestMethod.None)
        {
            RequestMethod result;
            if (!TryParse(str, out result))
            {
                return defaultValue;
            }
            return result;
        }

        public static bool TryParse(string str, out RequestMethod result)
        {
            switch (str.ToUpper())
            {
                case "GET":
                    result = RequestMethod.Get;
                    return true;
                case "PUT":
                    result = RequestMethod.Put;
                    return true;
                case "POST":
                    result = RequestMethod.Post;
                    return true;
                case "PATCH":
                    result = RequestMethod.Patch;
                    return true;
                case "DELETE":
                    result = RequestMethod.Delete;
                    return true;
                case "HEAD":
                    result = RequestMethod.Head;
                    return true;
                default:
                    result = RequestMethod.None;
                    return false;
            }
        }
    }
}
