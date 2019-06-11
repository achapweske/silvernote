/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SilverNote.Common
{
    public class Resource
    {
        /// <summary>
        /// Get the content of a text file embedded as a resource
        /// </summary>
        /// <param name="path">path of the resource to be read</param>
        /// <returns></returns>
        public static string ReadText(string path)
        {
            var assembly = Assembly.GetEntryAssembly();

            using (var stream = assembly.GetManifestResourceStream(path))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
