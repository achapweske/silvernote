/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SilverNote
{
    public static class LookupHelper
    {
        public static void Lookup(string command, string phrase)
        {
            if (command.IndexOf("%phrase%") != -1)
            {
                // Replace %phrase% variable

                if (!String.IsNullOrWhiteSpace(phrase))
                {
                    // A phrase is provided - do the replacement

                    Uri uri;
                    if (Uri.TryCreate(command, UriKind.Absolute, out uri))
                    {
                        // if command is a URI, phrase needs to be escaped
                        phrase = Uri.EscapeUriString(phrase);
                    }

                    command = command.Replace("%phrase%", phrase);
                }
                else
                {
                    // A phrase is not provided

                    Uri uri;
                    if (Uri.TryCreate(command, UriKind.Absolute, out uri))
                    {
                        command = uri.GetLeftPart(UriPartial.Authority);                    
                    }
                }
            }

            // Now actually execute the command

            Process.Start(command);
        }
    }

    public class LookupService
    {
        public string Name { get; set; }
        public string Command { get; set; }
    }

    public class LookupCollection : List<LookupService>
    {
        public LookupCollection()
        {

        }

        public LookupCollection(IEnumerable<LookupService> collection)
            : base(collection)
        {

        }
    }
}
