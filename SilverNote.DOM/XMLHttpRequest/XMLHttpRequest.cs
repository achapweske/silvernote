/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Events;

namespace DOM.XHR
{
    public interface XMLHttpRequest
    {
        EventHandler OnReadyStateChange { get; set; }
        ReadyStates ReadyState { get; }
        void Open(string method, string url, bool async = true, string user = null, string password = null);
        void SetRequestHeader(string header, string value);
        int Timeout { get; set; }
        bool WithCredentials { get; set; }
        XMLHttpRequestUpload Upload { get; }
        void Send(object data = null);
        void Abort();
        ushort status { get; }
        string StatusText { get; }
        string GetResponseHeader(string header);
        string GetAllResponseHeaders();
        void OverrideMimeType(string mime);
        XMLHttpRequestResponseType ResponseType { get; set; }
        object Response { get; }
        string ResponseText { get; }
        Document ResponseXML { get; }
    }
}
