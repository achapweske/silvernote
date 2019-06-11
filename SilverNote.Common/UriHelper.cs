/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SilverNote.Common
{
    public static class UriHelper
    {
        #region Parsing

        public const string URI_PATTERN = 
              @"^(?:(?<scheme>[^:/\?#]+):)?" 
            + @"(?://(?<authority>[^/\?#]*))?"
            + @"(?<path>[^\?#]*)"
            + @"(?:\?(?<query>[^#]*))?"
            + @"(?:#(?<fragment>.*))?";

        private static Regex _Regex;

        public static Regex Regex
        {
            get
            {
                if (_Regex == null)
                {
                    _Regex = new Regex(URI_PATTERN);
                }

                return _Regex;
            }
        }

        public static string Scheme(string uri)
        {
            var match = Regex.Match(uri);
            
            return match.Groups["scheme"].Value;
        }

        public static string Authority(string uri)
        {
            var match = Regex.Match(uri);

            return match.Groups["authority"].Value;
        }

        public static string Path(string uri)
        {
            var match = Regex.Match(uri);

            return match.Groups["path"].Value;
        }

        public static string Query(string uri)
        {
            var match = Regex.Match(uri);

            return match.Groups["query"].Value;
        }

        public static string Fragment(string uri)
        {
            var match = Regex.Match(uri);

            return match.Groups["fragment"].Value;
        }

        public static string[] PathSegments(string uri)
        {
            string path = Path(uri);

            return Segments(path);
        }

        public static int PathSegmentCount(string uri)
        {
            string path = Path(uri);

            return SegmentCount(path);
        }

        public static string[] Segments(string path)
        {
            return path.Trim('/').Split('/');
        }

        public static int SegmentCount(string path)
        {
            string[] segments = Segments(path);

            return segments.Length;
        }

        #endregion

        public static Uri Combine(Uri baseUri, Uri uri)
        {
            string result = Combine(baseUri.ToString(), uri.ToString());

            return new Uri(result);
        }

        public static string Combine(string baseUri, string uri)
        {
            // If uri is empty, return baseURI

            if (uri.Length == 0)
            {
                return baseUri;
            }

            // If uri is absolute, return uri

            if (uri.Contains(':'))
            {
                return uri;
            }

            // Remove query and parameter portions of baseUri

            int index = baseUri.IndexOfAny("?#".ToCharArray());
            if (index != -1)
            {
                baseUri = baseUri.Remove(index);
            }

            // If uri is an absolute path, remove path portion of baseUri

            if (uri.StartsWith("/", StringComparison.Ordinal))
            {
                index = baseUri.IndexOf("//");
                if (index != -1)
                {
                    index = baseUri.IndexOf('/', index + 2);
                }
                else
                {
                    index = baseUri.IndexOf('/');
                }

                if (index != -1 && index < baseUri.Length - 1)
                {
                    baseUri = baseUri.Remove(index + 1);
                }

                uri = uri.Remove(0, 1);
            }

            if (!baseUri.EndsWith("/") || baseUri.EndsWith("://"))
            {
                baseUri += "/";
            }

            return baseUri + uri;
        }

        /// <summary>
        /// Append a relative path to the given URI
        /// </summary>
        public static Uri AppendPath(Uri uri, string path)
        {
            if (uri == null)
            {
                throw new NullReferenceException();
            }

            string buffer = uri.GetLeftPart(UriPartial.Path);
            
            if (!buffer.EndsWith("/"))
            {
                buffer = buffer + "/";
            }
            buffer = buffer + path.TrimStart('/');

            return new Uri(buffer);
        }

        /// <summary>
        /// Append a query parameter to the given URI
        /// </summary>
        public static Uri SetQueryParameter(Uri uri, string name, string value)
        {
            string buffer = uri.ToString();

            int index = buffer.LastIndexOf('?');
            if (index == -1)
            {
                buffer += "?";
                index = buffer.Length - 1;
            }

            if (index < buffer.Length - 1)
            {
                buffer += "&";
            }

            buffer += HttpUtility.UrlEncode(name);
            buffer += "=";
            buffer += HttpUtility.UrlEncode(value);

            return new Uri(buffer);
        }

        public static string GetLocalPath(Uri uri)
        {
            string localPath = uri.LocalPath;

            if (localPath.StartsWith("/"))
            {
                localPath = "." + localPath;
            }

            localPath = localPath.Replace("/", "\\");

            return localPath;
        }

        public static byte[] GetUriData(string uri)
        {
            // data:[<mediatype>][;base64],<data>

            int startIndex = uri.IndexOf(',');
            if (startIndex == -1)
            {
                return null;
            }

            string base64 = uri.Substring(startIndex + 1);

            return System.Convert.FromBase64String(base64);
        }
    }
}
