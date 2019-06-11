/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SilverNote.Common;

namespace SilverNote.Server
{
    public class UriTemplate
    {
        public UriTemplate()
        {
            Uri = null;
        }

        public UriTemplate(Uri uri)
        {
            Uri = uri;
        }

        public UriTemplate(string uri)
        {
            Uri = new Uri(uri, UriKind.Relative);
        }

        public Uri Uri { get; set; }

        public UriTemplateMatch Match(Uri baseUri, Uri candidateUri)
        {
            Uri templateUri = UriHelper.Combine(baseUri, Uri);

            return MatchTemplate(templateUri, candidateUri);
        }

        public static UriTemplateMatch MatchTemplate(Uri templateUri, Uri candidateUri)
        {
            // Compare scheme://host

            if (candidateUri.Scheme != templateUri.Scheme ||
                candidateUri.Host != templateUri.Host)
            {
                return null;
            }

            // Compare path segments

            if (candidateUri.Segments.Length < templateUri.Segments.Length)
            {
                return null;
            }

            for (int i = 0; i < templateUri.Segments.Length; i++)
            {
                var templateSegment = templateUri.Segments[i].TrimEnd('/');
                var candidateSegment = candidateUri.Segments[i].TrimEnd('/');

                if (candidateSegment != templateSegment)
                {
                    if (GetVariableName(templateSegment) == null)
                    {
                        return null;
                    }
                }
            }

            // We have a match

            UriTemplateMatch match = new UriTemplateMatch();

            // Parse query string

            HttpUtility.ParseQueryString ( 
                candidateUri.Query, 
                Encoding.UTF8, 
                match.BoundVariables
            );

            // Parse template variables

            for (int i = 0; i < templateUri.Segments.Length; i++)
            {
                string name = GetVariableName(templateUri.Segments[i]);
                if (name != null)
                {
                    string value = candidateUri.Segments[i];
                    value = TrimSegment(value);
                    match.BoundVariables.Add(name, value);
                }
            }

            return match;
        }

        public static string TrimSegment(string segment)
        {
            if (segment.Length > 0 && segment.Last() == '/')
            {
                segment = segment.Substring(0, segment.Length - 1);
            }

            return segment;
        }

        public static string GetVariableName(string segment)
        {
            segment = TrimSegment(segment);

            if (segment.Length == 0)
            {
                return null;
            }

            if (segment.First() == '{' && segment.Last() == '}')
            {
                return segment.Substring(1, segment.Length - 2);
            }

            string escapedStart = Uri.HexEscape('{');
            string escapedEnd = Uri.HexEscape('}');
            if (segment.StartsWith(escapedStart) && segment.EndsWith(escapedEnd))
            {
                return segment.Substring(escapedStart.Length, segment.Length - escapedStart.Length - escapedEnd.Length);
            }

            return null;
        }

        #region Object

        public override string ToString()
        {
            return Uri.ToString();
        }

        #endregion
    }
}
