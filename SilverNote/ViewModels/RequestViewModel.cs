/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using SilverNote.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SilverNote.ViewModels
{
    public class RequestViewModel : ViewModelBase<Request, RequestViewModel>
    {
        #region Fields

        ResponseViewModel _Response;

        #endregion

        #region Constructors

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        #endregion

        #region Properties

        public RequestMethod Method
        {
            get { return Model.Method; }
        }

        public Uri Uri
        {
            get { return Model.Uri; }
        }

        public string Resource
        {
            get { return Model.Uri.Segments.LastOrDefault(); }
        }

        public string ContentType
        {
            get { return Model.ContentType; }
        }

        public object Content
        {
            get { return Model.ContentObject; }
        }

        public string ContentString
        {
            get 
            {
                if (ContentType == "text/xml")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(Model.ContentString);
                    using (var writer = new StringWriter())
                    {
                        doc.Save(writer);
                        return writer.ToString();
                    }
                }
                else
                {
                    return Model.ContentString;
                }
            }
        }

        public string Summary
        {
            get
            {
                var buffer = new StringBuilder();

                buffer.Append(Method);
                buffer.AppendLine(" " + Uri);

                if (ContentType != null)
                {
                    buffer.AppendLine("Content-Type: " + ContentType);
                }

                if (ContentString != null)
                {
                    buffer.AppendLine(ContentString);
                }

                return buffer.ToString();
            }
        }

        public ResponseViewModel Response
        {
            get
            {
                if (_Response == null)
                {
                    if (Model.Response != null)
                    {
                        _Response = new ResponseViewModel(Model.Response);
                    }
                }
                return _Response;
            }
        }

        public long RoundTripTime
        {
            get { return Model.RoundTripTime; }
        }

        #endregion

    }
}
