/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SilverNote.Server
{
    public struct RequestRoute : IComparable<RequestRoute>
    {
        public RequestRoute(RequestMethod method, UriTemplate uri, RequestHandler handler)
        {
            Method = method;
            Uri = uri;
            Handler = handler;
        }

        public RequestRoute(RequestMethod method, string uri, RequestHandler handler)
        {
            Method = method;
            Uri = new UriTemplate(uri);
            Handler = handler;
        }

        public RequestMethod Method;
        public UriTemplate Uri;
        public RequestHandler Handler;

        #region IComparable

        public int CompareTo(RequestRoute other)
        {
            // The route with the greatest number of segments is matched first.
            // For routes with the same number of segments, the one that ends 
            // in a constant (NOT a variable) is matched first.

            string path1 = UriHelper.Path(this.Uri.ToString());
            string path2 = UriHelper.Path(other.Uri.ToString());

            int weight1 = 0;

            if (path1.Length > 0)
            {
                weight1 = UriHelper.SegmentCount(path1) * 2;
            }

            if (!path1.EndsWith("}"))
            {
                weight1 += 1;
            }

            int weight2 = 0;

            if (path2.Length > 0)
            {
                weight2 = UriHelper.SegmentCount(path2) * 2;
            }

            if (!path2.EndsWith("}"))
            {
                weight2 += 1;
            }

            return weight2 - weight1;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return Method.ToString() + " " + Uri.ToString();
        }

        #endregion
    };

    public delegate Response RequestHandler(Request query);

    public class RequestRouter
    {
        public RequestRouter(Uri prefix = null)
        {
            Prefix = prefix;
            Routes = new List<RequestRoute>();
        }

        public Uri Prefix { get; set; }

        public List<RequestRoute> Routes { get; private set; }

        public void AddRoutes(object target)
        {
            Type type = target.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                object[] attributes = method.GetCustomAttributes(typeof(RequestTarget), true);
                foreach (RequestTarget attribute in attributes)
                {
                    RequestHandler handler = (RequestHandler)Delegate.CreateDelegate(typeof(RequestHandler), target, method.Name);
                    Routes.Add(new RequestRoute(attribute.Method, attribute.Uri, handler));
                }
            }

            Routes.Sort();
        }

        public Response Route(Request request)
        {
            foreach (RequestRoute route in Routes)
            {
                if (request.Method != RequestMethod.Any && request.Method != route.Method)
                {
                    continue;
                }

                UriTemplateMatch results = route.Uri.Match(Prefix, request.Uri);
                if (results == null)
                {
                    continue;
                }

                request.Params.Add(results.BoundVariables);

                return route.Handler(request);
            }

            string message = String.Format("No route for \"{0}\"", request.Uri);

            return Response.NotFound(message);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RequestTarget : System.Attribute
    {
        public RequestTarget(RequestMethod method, UriTemplate uri)
        {
            Method = method;
            Uri = uri;
        }

        public RequestTarget(RequestMethod method, string uri)
        {
            Method = method;
            Uri = new UriTemplate(uri);
        }

        public RequestMethod Method;
        public UriTemplate Uri;
    }
}
