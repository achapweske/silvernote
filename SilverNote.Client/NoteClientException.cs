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
    public class NoteClientException : Exception
    {
        public NoteClientException(Response response, Exception innerException = null)
            : this(response.StatusCode, response.StatusDescription, innerException)
        {
            if (!String.IsNullOrEmpty(response.ContentString))
            {
                _Message = response.ContentString;
            }
        }

        public NoteClientException(HttpStatusCode statusCode, string message, Exception innerException = null)
            : base(message, innerException)
        {
            _StatusCode = statusCode;
            _Message = message;
        }

        private HttpStatusCode _StatusCode;

        public HttpStatusCode StatusCode
        {
            get { return _StatusCode; }
        }

        private string _Message;

        public override string Message
        {
            get { return _Message; }
        }
    }
}
