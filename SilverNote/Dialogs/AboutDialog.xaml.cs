/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        public string ProductName
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.ProductName;
            }
        }

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string Copyright
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.LegalCopyright;
            }
        }

        public string OperatingSystem
        {
            get 
            {
                string str = Environment.OSVersion.ToString();

                str = str.Replace("Microsoft", "");

                if (Environment.Is64BitOperatingSystem)
                {
                    str += " (x64)";
                }
                else
                {
                    str += " (x32)";
                }

                return str.Trim();
            }
        }
    }
}
