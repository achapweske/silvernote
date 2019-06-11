/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SilverNote.Server
{
    public class RequestParams : NameValueCollection
    {
        public RequestParams()
        {

        }

        public RequestParams(NameValueCollection nvc)
            : base(nvc)
        {

        }

        public void Set(string name, object value)
        {
            base.Set(name, value.ToString());
        }

        public string GetString(string name, string defaultValue = "")
        {
            string[] values = GetValues(name);

            if (values != null && values.Count() > 0)
            {
                return HttpUtility.UrlDecode(values.Last());
            }
            else
            {
                return defaultValue;
            }
        }

        public int GetInt(string name, int defaultValue = default(int))
        {
            string value = GetString(name);
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            int result;
            if (!int.TryParse(value, out result))
            {
                return defaultValue;
            }

            return result;
        }

        public Int64 GetInt64(string name, Int64 defaultValue = default(Int64))
        {
            string value = GetString(name);
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            Int64 result;
            if (!Int64.TryParse(value, out result))
            {
                return defaultValue;
            }

            return result;
        }

        public bool GetBool(string name, bool defaultValue = default(bool))
        {
            string value = GetString(name);
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            value = value.ToLower();

            switch (value)
            {
                case "true":
                case "1":
                    return true;
                case "false":
                case "0":
                    return false;
                default:
                    return defaultValue;
            }
        }

        public DateTime GetDateTime(string name, DateTime defaultValue = default(DateTime))
        {
            string value = GetString(name);
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            DateTime result;
            if (!DateTime.TryParse(value, out result))
            {
                return defaultValue;
            }

            return result;
        }
    }

}
