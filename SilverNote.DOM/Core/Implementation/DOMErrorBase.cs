using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class DOMErrorBase : DOMError
    {
        #region Fields

        DOMErrorSeverityType _Severity;
        string _Message;
        string _Type;
        object _RelatedException;
        object _RelatedData;
        DOMLocator _Location;

        #endregion

        #region Constructors

        public DOMErrorBase()
        {

        }

        public DOMErrorBase(DOMErrorSeverityType severity, string type, string message)
            : this(severity, type, message, null, null, null)
        {

        }

        public DOMErrorBase(DOMErrorSeverityType severity, string type, string message, object relatedException, object relatedData, DOMLocator location)
        {
            _Severity = severity;
            _Message = message;
            _Type = type;
            _RelatedException = relatedException;
            _RelatedData = relatedData;
            _Location = location;
        }

        #endregion

        #region DOMError

        public virtual DOMErrorSeverityType Severity
        {
            get { return _Severity; }
        }

        public virtual string Message
        {
            get { return _Message; }
        }

        public virtual string Type
        {
            get { return _Type; }
        }

        public virtual object RelatedException
        {
            get { return _RelatedException; }
        }

        public virtual object RelatedData
        {
            get { return _RelatedData; }
        }

        public virtual DOMLocator Location
        {
            get { return _Location; }
        }


        #endregion
    }
}
