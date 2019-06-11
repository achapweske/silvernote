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
using System.Threading;
using System.Reflection;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using SilverNote.Common;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using SilverNote.Data.Models;
using Newtonsoft.Json;

namespace SilverNote.Server
{
    public class Request : INotifyPropertyChanged
    {
        #region Fields

        object _Content;
        string _ContentString;
        Response _Response;
        long _RoundTripTime;

        #endregion

        #region Constructors

        public Request()
        {
            Context = SynchronizationContext.Current;
            Method = RequestMethod.Get;
            Params = new RequestParams();
        }

        #endregion

        #region Properties

        public SynchronizationContext Context { get; set; }

        public object Args { get; set; }

        public RequestMethod Method { get; set; }

        public Uri Uri { get; set; }

        public RequestParams Params { get; set; }

        public string ContentType { get; set; }

        public string ContentString
        {
            get
            {
                if (_ContentString == null && _Content != null)
                {
                    _ContentString = EncodeContent(_Content, ContentType);
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
            get
            {
                return _Content;
            }
            set
            {
                _Content = value;
            }
        }

        public T Content<T>()
        {
            if (_Content == null && _ContentString != null)
            {
                _Content = Response.DecodeContent(_ContentString, ContentType, typeof(T));
            }
            return (T)_Content;
        }

        public Response Response
        {
            get
            {
                return _Response;
            }
            private set
            {
                _Response = value;
                RaisePropertyChanged("Response");
            }
        }

        public long RoundTripTime
        {
            get
            {
                return _RoundTripTime;
            }
            set
            {
                _RoundTripTime = value;
                RaisePropertyChanged("RoundTripTime");
            }
        }

        #endregion

        #region Operations

        public delegate void ResponseHandler(Request request, Response response);

        public event ResponseHandler AsyncCompleted;

        public event ResponseHandler Completed;

        public void Complete(Response response)
        {
            Response = response;

            var asyncCompleted = AsyncCompleted;
            if (asyncCompleted != null)
            {
                asyncCompleted(this, response);
            }

            Synchronize(() =>
            {
                var completed = Completed;
                if (completed != null)
                {
                    completed(this, response);
                }
            });
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                Synchronize(() =>
                {
                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        var e = new PropertyChangedEventArgs(propertyName);
                        handler(this, e);
                    }
                });
            }
        }

        #endregion

        #region Implementation

        protected void Synchronize(Action action)
        {
            if (Context == null || Context.Equals(SynchronizationContext.Current))
            {
                action();
            }
            else
            {
                Context.Post((state) => { action(); }, null);
            }
        }

        #region Content Encoding

        /// <summary>
        /// Convert an object to the given MIME type
        /// </summary>
        /// <param name="content">Object to be encoded</param>
        /// <param name="mimeType">MIME type to encode to</param>
        /// <returns>Encoded content</returns>
        public static string EncodeContent(object content, string mimeType, bool pretty = false)
        {
            mimeType = mimeType ?? "text/plain";

            switch (mimeType)
            {
                case "application/json":
                    return EncodeContentToJson(content);
                case "text/xml":
                    return EncodeContentToXml(content);
                case "text/plain":
                    return content.ToString();
                default:
                    Debug.WriteLine("Unknown content type: \"" + mimeType + "\"");
                    return null;
            }
        }

        public static string EncodeContentToJson(object content, bool pretty = false)
        {
            pretty = true;

            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = pretty ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None,
                    DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };

                return JsonConvert.SerializeObject(content, settings);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Encode an object as text/xml
        /// </summary>
        /// <param name="content">Object to be encoded</param>
        /// <returns>An XML document</returns>
        public static string EncodeContentToXml(object content, bool pretty = false)
        {
            var serializer = new XmlSerializer(content.GetType());
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (var stream = new System.IO.MemoryStream())
            {
                var settings = new XmlWriterSettings()
                {
                    Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    Indent = pretty
                };
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    serializer.Serialize(writer, content, namespaces);
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        #endregion

        #endregion

        #region Object

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append(Method);

            if (Uri != null)
            {
                buffer.AppendLine(" " + Uri);
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
            else if (Content<object>() != null)
            {
                buffer.AppendLine(Content<object>().ToString());
            }
            else
            {
                buffer.AppendLine("<No Content>");
            }

            return buffer.ToString();
        }

        #endregion
    }

    public class RequestResult : IAsyncResult
    {
        public RequestResult(Request request, AsyncCallback callback, object state)
        {
            Request = request;
            request.AsyncCompleted += Request_AsyncCompleted;
            request.Completed += Request_Completed;
            AsyncCallback = callback;
            AsyncState = state;
            _AsyncWaitHandle = new ManualResetEvent(false);
        }

        public Request Request { get; private set; }

        public void Request_AsyncCompleted(Request request, Response response)
        {
            IsCompleted = true;

            _AsyncWaitHandle.Set();
        }

        public void Request_Completed(Request request, Response response)
        {
            if (AsyncCallback != null)
            {
                AsyncCallback(this);
            }
        }

        #region IAsyncResult

        private ManualResetEvent _AsyncWaitHandle;

        public WaitHandle AsyncWaitHandle
        {
            get { return _AsyncWaitHandle; }
        }

        public bool CompletedSynchronously { get; private set; }

        public bool IsCompleted { get; private set; }

        public AsyncCallback AsyncCallback { get; set; }

        public object AsyncState { get; set; }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _AsyncWaitHandle.Dispose();
        }

        #endregion

    }
}
