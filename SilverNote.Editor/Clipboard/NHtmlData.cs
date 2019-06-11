/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace SilverNote.Editor
{
    public class NHtmlData
    {
        public NHtmlData()
        {

        }

        public NHtmlData(IDataObject obj)
        {
            if (obj.GetDataPresent(DataFormats.Html))
            {
                string data = obj.GetData(DataFormats.Html) as string;
                if (data != null)
                {
                    Parse(data);
                }
            }
        }

        public NHtmlData(string data)
        {
            Parse(data);
        }

        public string Version { get; set; }

        public string SourceURL { get; set; }

        public int FragmentOffset { get; set; }

        public int FragmentLength { get; set; }

        public string Html { get; set; }

        private void Parse(string data)
        {
            // StartHTML
            string startHTMLPattern = @"^StartHTML:\s*(\d+)\r\n";
            var match = Regex.Match(data, startHTMLPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success || match.Groups.Count < 2)
            {
                return;
            }
            int startHTML;
            if (!int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out startHTML))
            {
                return;
            }

            // EndHTML
            string endHTMLPattern = @"^EndHTML:\s*(\d+)\r\n";
            match = Regex.Match(data, endHTMLPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success || match.Groups.Count < 2)
            {
                return;
            }
            int endHTML;
            if (!int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out endHTML))
            {
                return;
            }

            // StartFragment
            string startFragmentPattern = @"^StartFragment:\s*(\d+)\r\n";
            match = Regex.Match(data, startFragmentPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success || match.Groups.Count < 2)
            {
                return;
            }
            int startFragment;
            if (!int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out startFragment))
            {
                return;
            }

            // EndFragment
            string endFragmentPattern = @"^EndFragment:\s*(\d+)\r\n";
            match = Regex.Match(data, endFragmentPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success || match.Groups.Count < 2)
            {
                return;
            }
            int endFragment;
            if (!int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out endFragment))
            {
                return;
            }

            // SourceURL
            string sourceURLPattern = @"^SourceURL:\s*([^\r\n]+)\r\n";
            match = Regex.Match(data, sourceURLPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (match.Success && match.Groups.Count >= 2)
            {
                SourceURL = match.Groups[1].Value;
            }

            // HTML
            startHTML = Math.Max(0, Math.Min(data.Length, startHTML));
            endHTML = Math.Max(0, Math.Min(data.Length, endHTML));
            Html = data.Substring(startHTML, endHTML - startHTML);

            startFragment = Math.Max(0, Math.Min(data.Length, startFragment));
            endFragment = Math.Max(0, Math.Min(data.Length, endFragment));
            FragmentOffset = startFragment - startHTML;
            FragmentLength = endFragment - startFragment;
        }

        private string Format()
        {
            var buffer = new StringBuilder();
            if (!String.IsNullOrEmpty(Version))
            {
                buffer.AppendLine("Version:" + Version);
            }
            else
            {
                buffer.AppendLine("Version:0.9");
            }
            buffer.AppendLine("StartHTML:<<<<<<<1");
            buffer.AppendLine("EndHTML:<<<<<<<2");
            buffer.AppendLine("StartFragment:<<<<<<<3");
            buffer.AppendLine("EndFragment:<<<<<<<4");
            buffer.AppendLine("SourceURL:" + SourceURL);
            int startHTML = buffer.Length;
            buffer.Append(Html);
            buffer.Replace("<body>", "<body>" + "\r\n<!--StartFragment-->\r\n");
            buffer.Replace("</body>", "\r\n<!--EndFragment-->\r\n" + "</body>");
            int endHTML = buffer.Length;

            string bufferString = buffer.ToString();
            int startFragment = bufferString.IndexOf("<!--StartFragment-->") + "<!--StartFragment-->".Length;
            if (startFragment == -1)
            {
                startFragment = startHTML;
            }
            int endFragment = bufferString.IndexOf("<!--EndFragment-->");
            if (endFragment == -1)
            {
                endFragment = endHTML;
            }

            buffer.Replace("<<<<<<<1", startHTML.ToString("D8"));
            buffer.Replace("<<<<<<<2", endHTML.ToString("D8"));
            buffer.Replace("<<<<<<<3", startFragment.ToString("D8"));
            buffer.Replace("<<<<<<<4", endFragment.ToString("D8"));

            return buffer.ToString();
        }

        public override string ToString()
        {
            return Format();
        }
    }
}
