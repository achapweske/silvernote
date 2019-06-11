/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SilverNote.Common
{
    public static class ProcessHelper
    {
        /// <summary>
        /// Open the given URL in a web browser.
        /// </summary>
        public static bool StartBrowser(string url)
        {
            try
            {
                Process.Start(url);
                return true;
            }
            catch (Win32Exception)
            {
                // This occurs if firefox is the default browser. 
                // The browser launches fine, so the exception can be ignored
                return true;
            }
            catch
            {
                // Process.Start() can fail randomly. Try opening URL using IE below...
            }

            try
            {
                Process.Start(new ProcessStartInfo("IExplore.exe", url));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
