/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SilverNote.Client
{
    public class NoteClientErrorEventArgs : EventArgs
    {
        public NoteClientErrorEventArgs(Request request, Response response)
            : this(response.StatusCode, response.StatusDescription)
        {
            Request = request;

            if (!String.IsNullOrEmpty(response.ContentString))
            {
                Message = response.ContentString;
            }
        }

        public NoteClientErrorEventArgs(HttpStatusCode statusCode, string message = "")
        {
            StatusCode = statusCode;
            Message = message;
        }

        public Request Request { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}
