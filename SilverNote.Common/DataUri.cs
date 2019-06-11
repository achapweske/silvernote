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

namespace SilverNote.Common
{
    /// <summary>
    /// RFC 2397
    /// 
    /// dataurl    := "data:" [ mediatype ] [ ";base64" ] "," data
    /// mediatype  := [ type "/" subtype ] *( ";" parameter )
    /// data       := *urlchar
    /// parameter  := attribute "=" value
    /// </summary>
    public class DataUri
    {
        #region Constructors

        public DataUri()
        {
            Parameters = new NameValueCollection();
        }

        #endregion

        #region Properties

        public string Type { get; set; }

        public string Subtype { get; set; }

        public string MediaType
        {
            get
            {
                if (Type != null)
                {
                    if (Subtype != null)
                    {
                        return Type + '/' + Subtype;
                    }
                    else
                    {
                        return Type;
                    }
                }

                return null;
            }
        }

        public NameValueCollection Parameters { get; set; }

        public string Encoding { get; set; }

        public string Data { get; set; }

        public byte[] DataBytes
        {
            get { return Convert.FromBase64String(Data); }
            set { Data = Convert.ToBase64String(value); }
        }

        #endregion

        #region Static Methods

        public static bool TryParse(string uri, out DataUri result)
        {
            // dataurl    := "data:" [ mediatype ] [ ";base64" ] "," data
            // mediatype  := [ type "/" subtype ] *( ";" parameter )
            // data       := *urlchar
            // parameter  := attribute "=" value 

            result = null;

            // "data:"

            if (!uri.StartsWith("data:"))
            {
                return false;
            }

            uri = uri.Substring("data:".Length);

            int index = uri.LastIndexOf(',');
            if (index == -1)
            {
                return false;
            }

            result = new DataUri();

            // data

            if (index + 1 < uri.Length)
            {
                result.Data = uri.Substring(index + 1);
                result.Data = Uri.UnescapeDataString(result.Data);
            }

            uri = uri.Remove(index);

            // [ ";base64" ]

            if (uri.EndsWith(";base64"))
            {
                result.Encoding = "base64";
                uri = uri.Remove(uri.Length - ";base64".Length);
            }

            string[] parameters = uri.Split(',');
            int i = 0;

            // [ type "/" subtype ]
            if (parameters.Length > 0 && !parameters[i].Contains('='))
            {
                string[] tokens = parameters[0].Split('/');
                if (tokens.Length >= 2)
                {
                    result.Type = tokens[0];
                    result.Subtype = tokens[1];
                    i++;
                }
            }

            // *( ";" parameter )
            for (; i < parameters.Length; i++)
            {
                // attribute "=" value
                string[] tokens = parameters[i].Split('=');
                if (tokens.Length >= 2)
                {
                    result.Parameters.Add(tokens[0], tokens[1]);
                }
            }

            return true;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            // dataurl    := "data:" [ mediatype ] [ ";base64" ] "," data
            // mediatype  := [ type "/" subtype ] *( ";" parameter )
            // data       := *urlchar
            // parameter  := attribute "=" value 

            var buffer = new StringBuilder();

            buffer.Append("data:");

            if (!String.IsNullOrEmpty(Type))
            {
                buffer.Append(Type);
                buffer.Append('/');
                buffer.Append(Subtype);
            }
            
            for (int i = 0; i < Parameters.Count; i++)
            {
                buffer.Append(';');
                buffer.Append(Parameters.GetKey(i));
                buffer.Append('=');
                buffer.Append(Parameters.GetValues(i).Last());
            }

            if (!String.IsNullOrEmpty(Encoding))
            {
                buffer.Append(';');
                buffer.Append(Encoding);
            }

            buffer.Append(',');

            string data = Data;
            if (Encoding != "base64")
            {
                data = Uri.EscapeDataString(data);
            }
            buffer.Append(data);

            return buffer.ToString();
        }

        #endregion
    }
}
