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
    public static class MutationEventTypes
    {
        public const string DOMSubtreeModified = "DOMSubtreeModified";
        public const string DOMNodeInserted = "DOMNodeInserted";
        public const string DOMNodeRemoved = "DOMNodeRemoved";
        public const string DOMNodeRemovedFromDocument = "DOMNodeRemovedFromDocument";
        public const string DOMNodeInsertedIntoDocument = "DOMNodeInsertedIntoDocument";
        public const string DOMAttrModified = "DOMAttrModified";
        public const string DOMCharacterDataModified = "DOMCharacterDataModified";
    }
}
