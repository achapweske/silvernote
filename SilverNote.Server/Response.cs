/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Newtonsoft.Json;
using SilverNote.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Server
{
    public class Response
    {
        #region Static Methods

        public static Response Ok(object content, string type)
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.OK;
            response.StatusDescription = "OK";
            if (content is string)
                response.ContentString = (string)content;
            else
                response.ContentObject = content;
            response.ContentType = type;
            return response;
        }

        public static Response NoContent()
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.NoContent;
            response.StatusDescription = "No Content";
            return response;
        }

        public static Response Created(Uri uri, object newObject)
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.Created;
            response.StatusDescription = "Created";
            response.Headers.Add("Location", uri.ToString());
            response.ContentObject = newObject;
            response.ContentType = "text/xml";
            return response;
        }

        public static Response BadRequest(string content = "Bad Request", string type = "text/plain")
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.BadRequest;
            response.StatusDescription = "Bad Request";
            response.ContentString = content;
            response.ContentType = type;
            return response;
        }

        public static Response NotFound(string content = "Not Found", string type = "text/plain")
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.NotFound;
            response.StatusDescription = "Not Found";
            response.ContentString = content;
            response.ContentType = type;
            return response;
        }

        public static Response Forbidden(string content = "Forbidden", string type = "text/plain")
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.Forbidden;
            response.StatusDescription = "Forbidden";
            response.ContentString = content;
            response.ContentType = type;
            return response;
        }

        public static Response Gone(string content = "Gone", string type = "text/plain")
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.Gone;
            response.StatusDescription = "Gone";
            response.ContentString = content;
            response.ContentType = type;
            return response;
        }

        public static Response NotImplemented(string content = "Not Implemented", string type = "text/plain")
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.NotImplemented;
            response.StatusDescription = "Not Implemented";
            response.ContentString = content;
            response.ContentType = type;
            return response;
        }

        public static Response UnsupportedMediaType(string content = "Unsupported Media Type", string type = "text/plain")
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.UnsupportedMediaType;
            response.StatusDescription = "Unsupported Media Type";
            response.ContentString = content;
            response.ContentType = type;
            return response;
        }

        public static Response InternalServerError(string content = "Internal Server Error", string type = "text/plain")
        {
            Response response = new Response();
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.StatusDescription = "Internal Server Error";
            response.ContentString = content;
            response.ContentType = type;
            return response;
        }

        #endregion

        #region Fields

        object _Content;
        string _ContentString;

        #endregion

        #region Constructors

        public Response()
        {
            StatusCode = HttpStatusCode.OK;
            StatusDescription = "";
            Headers = new NameValueCollection();
            Timestamp = DateTime.UtcNow;
            ContentObject = null;
            ContentType = null;
        }

        public Response(HttpStatusCode statusCode, string statusDescription)
            : this()
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }

        public Response(HttpStatusCode statusCode, string statusDescription, object content, string contentType = "text/plain")
            : this(statusCode, statusDescription)
        {
            ContentType = contentType;
            ContentObject = content;
        }

        #endregion

        #region Properties

        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccess
        {
            get { return (int)StatusCode >= 200 && (int)StatusCode < 300; }
        }

        public bool IsRedirect
        {
            get { return (int)StatusCode >= 300 && (int)StatusCode < 400; }
        }

        public bool IsClientError
        {
            get { return (int)StatusCode >= 400 && (int)StatusCode < 500; }
        }

        public bool IsServerError
        {
            get { return (int)StatusCode >= 500; }
        }

        public string StatusDescription { get; set; }

        public NameValueCollection Headers { get; set; }

        public DateTime Timestamp { get; set; }

        public string ContentType { get; set; }

        public string ContentString
        {
            get
            {
                if (_ContentString == null && _Content != null)
                {
                    _ContentString = Request.EncodeContent(_Content, ContentType);
                }
                return _ContentString;
            }
            set
            {
                _ContentString = value;
            }
        }

        public object ContentObject
        {
            get { return _Content; }
            set { _Content = value; }
        }

        public object Content<T>()
        {
            if (_Content == null && _ContentString != null)
            {
                _Content = DecodeContent(_ContentString, ContentType, typeof(T));
            }
            return _Content;
        }

        public T ContentOrDefault<T>() where T : class, new()
        {
            T result = Content<T>() as T;
            if (result == null)
            {
                result = new T();
            }
            return result;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Decode an object from the given MIME type
        /// </summary>
        /// <param name="content">Content to be decoded</param>
        /// <param name="mimeType">MIME type to decode from</param>
        /// <returns>Decoded object</returns>
        public static object DecodeContent(string content, string mimeType, Type resultType)
        {
            mimeType = mimeType ?? "text/plain";

            switch (mimeType)
            {
                case "application/json":
                    return DecodeJsonContent(content, resultType);
                case "text/xml":
                    return DecodeXmlContent(content, resultType);
                case "text/plain":
                    return content;
                default:
                    Debug.WriteLine("Unknown content type: \"" + mimeType + "\"");
                    return null;
            }
        }

        public static object DecodeJsonContent(string content, Type type)
        {
            try
            {
                return JsonConvert.DeserializeObject(content, type);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Decode an object from a text/xml string
        /// </summary>
        /// <param name="content">XML document to be decoded</param>
        /// <param name="type">Object type</param>
        /// <returns>Decoded object</returns>
        public static object DecodeXmlContent(string content, Type type)
        {
            try
            {
                using (var reader = new System.IO.StringReader(content))
                {
                    var serializer = new XmlSerializer(type);

                    return serializer.Deserialize(reader);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append((int)StatusCode);

            if (StatusDescription != null)
            {
                buffer.AppendLine(" " + StatusDescription);
            }
            else
            {
                buffer.AppendLine(" <null>");
            }

            if (ContentType != null)
            {
                buffer.AppendLine("Content-Type: " + ContentType);
            }

            if (ContentString != null)
            {
                buffer.AppendLine(ContentString);
            }
            else if (ContentObject != null)
            {
                buffer.AppendLine(ContentObject.ToString());
            }
            else
            {
                buffer.AppendLine("<No Content>");
            }

            return buffer.ToString();
        }

        #endregion
    }

}
