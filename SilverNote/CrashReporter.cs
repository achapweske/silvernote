/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace SilverNote
{
    /// <summary>
    /// A collection of utilities for creating and sending crash reports
    /// </summary>
    public static class CrashReporter
    {
        public const string ServiceURI = "";        // e.g., http://silver-note.com/crash-reports

        /// <summary>
        /// Send the given crash report to the default crash reporting service
        /// </summary>
        /// <param name="report"></param>
        public static void ReportCrash(string report)
        {
            if (String.IsNullOrEmpty(ServiceURI))
            {
                return;
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "text/plain";
                    client.UploadString(ServiceURI, report);
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine("Crash report failed: " + error.Message);
            }
        }

        public static string CreateReport(Exception e)
        {
            // Exception

            string result = FormatException(e);

            // Operating System

            result += OSInfo.OSString + "\n";

            // Culture

            result += CultureInfo.CurrentCulture.EnglishName + "\n";

            // Assemblies

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                result += assembly.ToString() + "\n";
            }

            return result;
        }

        static string FormatException(Exception e)
        {
            // Message 

            string result = e.Message + "\n\n";

            // Stack Trace

            if (!String.IsNullOrEmpty(e.StackTrace))
            {
                result += e.StackTrace + "\n\n";
            }

            // Inner Exception

            if (e.InnerException != null)
            {
                result += "Inner exception: \n\n";

                result += FormatException(e.InnerException);
            }

            return result;
        }
    }
}
