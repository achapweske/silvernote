/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace SilverNote.Common
{
    /// <summary>
    /// Utilities for getting file extension association information
    /// </summary>
    public class FileAssociation
    {
        #region Static Methods

        /// <summary>
        /// Get all file associations for the given file extension +/- action
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IList<FileAssociation> GetAssociations(string fileExtension, string action = null)
        {
            var results = new List<FileAssociation>();

            foreach (var exeName in GetOpenWithList(Registry.CurrentUser, HKCU_FILE_EXTS, fileExtension))
            {
                results.Add(new FileAssociation(fileExtension, exeName, action, true));
            }

            foreach (var progid in GetOpenWithProgids(Registry.CurrentUser, HKCU_FILE_EXTS, fileExtension))
            {
                results.Add(new FileAssociation(fileExtension, progid, action, false));
            }

            foreach (var exeName in GetOpenWithList(Registry.ClassesRoot, "", fileExtension))
            {
                results.Add(new FileAssociation(fileExtension, exeName, action, true));
            }

            foreach (var progid in GetOpenWithProgids(Registry.ClassesRoot, "", fileExtension))
            {
                results.Add(new FileAssociation(fileExtension, progid, action, false));
            }

            results.RemoveAll(r => String.IsNullOrEmpty(r.FriendlyAppName));
            results.RemoveAll(r => String.IsNullOrEmpty(r.AppCommand));
            results.RemoveAll(r => results.Any(s => s != r && s.AppCommand == r.AppCommand));

            return results;
        }

        #endregion

        #region Constructors

        public FileAssociation()
        {

        }

        public FileAssociation(string fileExtension, string progid, string action, bool isExeName = false)
        {
            Win32.AssocF assocFlags = Win32.AssocF.ASSOCF_NONE;

            if (isExeName)
            {
                assocFlags = Win32.AssocF.ASSOCF_INIT_BYEXENAME;
            }

            FileExtension = fileExtension;
            FriendlyAppName = Win32.AssocQueryString(assocFlags, Win32.AssocStr.ASSOCSTR_FRIENDLYAPPNAME, progid, action);
            DefaultIcon = Win32.AssocQueryString(assocFlags, Win32.AssocStr.ASSOCSTR_DEFAULTICON, progid, action);
            Executable = Win32.AssocQueryString(assocFlags, Win32.AssocStr.ASSOCSTR_EXECUTABLE, progid, action);
            AppCommand = Win32.AssocQueryString(assocFlags, Win32.AssocStr.ASSOCSTR_COMMAND, progid, action);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The file extension
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Friendly name of the executable file
        /// </summary>
        public string FriendlyAppName { get; set; }

        /// <summary>
        /// Default icon resource path
        /// </summary>
        public string DefaultIcon { get; set; }

        /// <summary>
        /// Executable from a shell verb command string
        /// </summary>
        public string Executable { get; set; }

        /// <summary>
        /// Command string associated with a shell verb
        /// </summary>
        public string AppCommand { get; set; }

        /// <summary>
        /// Get the Icon for the Executable
        /// </summary>
        public Image DefaultIconImage
        {
            get
            {
                if (String.IsNullOrEmpty(Executable))
                {
                    return null;
                }

                ImageSource bitmap = Images.GetFileIcon(Executable);
                if (bitmap == null)
                {
                    return null;
                }
                
                return new Image { Source = bitmap };
            }
        }

        #endregion

        #region Implementation

        private const string HKCU_FILE_EXTS = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts";

        private static IEnumerable<string> GetOpenWithList(RegistryKey root, string baseKey, string fileExtension)
        {
            var results = new List<string>();

            baseKey = baseKey.TrimEnd('\\') + "\\" + fileExtension + "\\OpenWithList";

            using (RegistryKey key = root.OpenSubKey(baseKey))
            {
                if (key != null)
                {
                    string mruList = (string)key.GetValue("MRUList");
                    if (mruList != null)
                    {
                        foreach (char mru in mruList.ToString())
                        {
                            object name = key.GetValue(mru.ToString());
                            if (name != null)
                            {
                                results.Add(name.ToString());
                            }
                        }
                    }
                }
            }

            return results;
        }

        private static IEnumerable<string> GetOpenWithProgids(RegistryKey root, string baseKey, string fileExtension)
        {
            var results = new List<string>();

            baseKey = baseKey.TrimEnd('\\') + "\\" + fileExtension + "\\OpenWithProgids";

            using (RegistryKey key = root.OpenSubKey(baseKey))
            {
                if (key != null)
                {
                    foreach (string progid in key.GetValueNames())
                    {
                        results.Add(progid);
                    }
                }
            }

            return results;
        }

        #endregion
    }
}
