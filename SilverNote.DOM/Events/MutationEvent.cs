/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Events
{
    public interface MutationEvent : Event
    {
        Node RelatedNode { get; }
        string PrevValue { get; }
        string NewValue { get; }
        string AttrName { get; }
        AttrChangeType AttrChange { get; }
        void InitMutationEvent(string typeArg, 
                                bool canBubbleArg, 
                                bool cancelableArg, 
                                Node relatedNodeArg, 
                                string prevValueArg, 
                                string newValueArg, 
                                string attrNameArg, 
                                AttrChangeType attrChangeArg);
    }
}
