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

namespace DOM.SVG.Internal
{
    public class SVGElementInstanceImpl : SVGElementInstance
    {
        #region Fields

        SVGUseElement _CorrespondingUseElement;
        bool _Animated;

        #endregion

        #region Constructors

        public SVGElementInstanceImpl(SVGUseElement correspondingUseElement, bool animated)
        {
            _CorrespondingUseElement = correspondingUseElement;
            _Animated = animated;
        }

        #endregion

        #region SVGElementInstance

        public SVGUseElement CorrespondingUseElement
        {
            get { return _CorrespondingUseElement; }
        }

        public SVGElement CorrespondingElement
        {
            get
            {
                string href = _Animated ? CorrespondingUseElement.HRef.AnimVal : CorrespondingUseElement.HRef.BaseVal;
                if (href.StartsWith("#"))
                {
                    string id = href.Substring(1);
                    return CorrespondingUseElement.OwnerDocument.GetElementById(id) as SVGElement;
                }
                else
                {
                    return null;
                }
            }
        }

        public SVGElementInstance ParentNode
        {
            get { throw new NotImplementedException(); }
        }

        public SVGElementInstanceList ChildNodes
        {
            get { throw new NotImplementedException(); }
        }

        public SVGElementInstance FirstChild
        {
            get { throw new NotImplementedException(); }
        }

        public SVGElementInstance LastChild
        {
            get { throw new NotImplementedException(); }
        }

        public SVGElementInstance PreviousSibling
        {
            get { throw new NotImplementedException(); }
        }

        public SVGElementInstance NextSibling
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region EventTarget

        public void AddEventListener(string type, EventListener listener, bool useCapture)
        {
            throw new NotImplementedException();
        }

        public void RemoveEventListener(string type, EventListener listener, bool useCapture)
        {
            throw new NotImplementedException();
        }

        public bool DispatchEvent(Event evt)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
