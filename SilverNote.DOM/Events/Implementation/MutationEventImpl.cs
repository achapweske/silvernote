/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Events.Internal
{
    public class MutationEventImpl : EventBase, MutationEvent
    {
        #region Fields

        Node _RelatedNode;
        string _PrevValue;
        string _NewValue;
        string _AttrName;
        AttrChangeType _AttrChange;

        #endregion

        #region MutationEvent

        public Node RelatedNode
        {
            get { return _RelatedNode; }
        }

        public string PrevValue
        {
            get { return _PrevValue; }
        }

        public string NewValue
        {
            get { return _NewValue; }
        }

        public string AttrName
        {
            get { return _AttrName; }
        }

        public AttrChangeType AttrChange
        {
            get { return _AttrChange; }
        }

        public void InitMutationEvent(string type, bool canBubble, bool cancelable, Node relatedNode, string prevValue, string newValue, string attrName, AttrChangeType attrChange)
        {
            base.InitEvent(type, canBubble, cancelable);

            _RelatedNode = relatedNode;
            _PrevValue = prevValue;
            _NewValue = newValue;
            _AttrName = attrName;
            _AttrChange = attrChange;
        }

        #endregion
    }
}
