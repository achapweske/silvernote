/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using SilverNote.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SilverNote.ViewModels
{
    public class ResponseViewModel : ObservableObject
    {
        #region Fields

        Response _Model;

        #endregion

        #region Constructors

        public ResponseViewModel(Response response)
        {
            _Model = response;
        }

        #endregion

        #region Properties

        public Response Model
        {
            get { return _Model; }
        }

        public int StatusCode
        {
            get { return (int)_Model.StatusCode; }
        }

        public string StatusDescription
        {
            get { return _Model.StatusDescription; }
        }

        public string ContentType
        {
            get { return _Model.ContentType; }
        }

        public object Content
        {
            get { return _Model.ContentObject; }
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

                buffer.Append((int)StatusCode);

                if (StatusDescription != null)
                {
                    buffer.AppendLine(" " + StatusDescription);
                }

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

        #endregion
    }
}
