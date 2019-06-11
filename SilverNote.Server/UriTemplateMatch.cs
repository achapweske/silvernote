/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Server
{
    public class UriTemplateMatch
    {
        public UriTemplateMatch()
        {
            BoundVariables = new NameValueCollection();
        }

        public NameValueCollection BoundVariables { get; set; }
    }
}
