/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DOM.CSS;
using DOM.CSS.Internal;
using DOM.Events;

namespace DOM.Helpers
{
    public class Query : IEnumerable<Element>
    {
        #region Fields

        Element[] _Elements;

        #endregion

        #region Constructors

        protected Query()
        {
            _Elements = new Element[0];
        }

        public Query(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException();
            }

            _Elements = new [] { element };
        }

        public Query(Document document)
        {
            if (document.DocumentElement != null)
            {
                _Elements = new[] { document.DocumentElement };
            }
            else
            {
                _Elements = new Element[0];
            }
        }

        public Query(string selector, Node context)
        {
            _Elements = QuerySelectorAll(context, selector);
        }

        #endregion

        #region Properties

        public static readonly Query Empty = new Query();

        public int Length
        {
            get { return _Elements.Length; }
        }

        #endregion

        #region Operations

        public Element this[int index]
        {
            get { return _Elements[index]; }
        }

        #endregion

        #region Events

        public Query Bind(string eventType, EventDelegate callback)
        {
            foreach (Element element in this.ToArray())
            {
                element.AddEventListener(eventType, new EventHandler(callback), false);
            }

            return this;
        }

        public Query Bind(string eventType, MouseEventDelegate callback)
        {
            foreach (Element element in this.ToArray())
            {
                element.AddEventListener(eventType, new MouseEventHandler(callback), false);
            }

            return this;
        }

        public Query Unbind(string eventType, EventDelegate callback)
        {
            foreach (Element element in this.ToArray())
            {
                element.RemoveEventListener(eventType, new EventHandler(callback), false);
            }

            return this;
        }

        public Query Unbind(string eventType, MouseEventDelegate callback)
        {
            foreach (Element element in this.ToArray())
            {
                element.RemoveEventListener(eventType, new MouseEventHandler(callback), false);
            }

            return this;
        }

        public Query MouseDown(MouseEventDelegate handler)
        {
            return Bind(MouseEventTypes.MouseDown, handler);
        }

        public Query MouseUp(MouseEventDelegate handler)
        {
            return Bind(MouseEventTypes.MouseUp, handler);
        }

        public Query MouseMove(MouseEventDelegate handler)
        {
            return Bind(MouseEventTypes.MouseMove, handler);
        }

        #endregion

        #region IEnumerable

        public IEnumerator<Element> GetEnumerator()
        {
            foreach (var element in _Elements)
            {
                yield return element;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation

        static Element[] QuerySelectorAll(Node context, string selector)
        {
            NodeList results;

            if (context is Element)
            {
                results = ((Element)context).QuerySelectorAll(selector);
            }
            else if (context is Document)
            {
                results = ((Document)context).QuerySelectorAll(selector);
            }
            else if (context is DocumentFragment)
            {
                results = ((DocumentFragment)context).QuerySelectorAll(selector);
            }
            else
            {
                return new Element[0];
            }

            return results.OfType<Element>().ToArray();
        }

        #endregion
    }



}
