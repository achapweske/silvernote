using SilverNote.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Clipart
{
    public static class ClipartLoader
    {
        public static void LoadClipart(RepositoryModel repository)
        {
            foreach (string groupName in GetEmbeddedClipartGroupNames())
            {
                var group = repository.CreateClipartGroup(groupName.Replace('_', ' '));

                var items = GetEmbeddedClipartItemsPaths(groupName);
                foreach (string itemPath in items)
                {
                    string itemName = GetEmbeddedClipartItemName(itemPath);
                    string itemData = GetEmbeddedClipartItemData(itemPath);

                    group.CreateClipart(itemName, itemData);
                }
            }
        }

        static string[] GetEmbeddedResourcePaths()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceNames();
        }

        const string CLIPART_FOLDER_PATH = "SilverNote.Clipart.";

        static string[] GetEmbeddedClipartGroupNames()
        {
            var result = new List<string>();

            foreach (string name in GetEmbeddedResourcePaths())
            {
                if (name.StartsWith(CLIPART_FOLDER_PATH))
                {
                    string groupName = name.Substring(CLIPART_FOLDER_PATH.Length);
                    int endIndex = groupName.IndexOf('.');
                    if (endIndex != -1)
                    {
                        groupName = groupName.Remove(endIndex);
                    }

                    if (!result.Contains(groupName))
                    {
                        result.Add(groupName);
                    }
                }
            }

            return result.ToArray();
        }

        static string[] GetEmbeddedClipartItemsPaths(string groupName)
        {
            return (from name in GetEmbeddedResourcePaths()
                    where name.StartsWith(CLIPART_FOLDER_PATH + groupName + ".")
                    select name).ToArray();
        }

        static string GetEmbeddedClipartItemName(string path)
        {
            // strip folder path
            string name = path.Substring(CLIPART_FOLDER_PATH.Length);
            // strip group name
            int startIndex = name.IndexOf('.') + 1;
            name = name.Substring(startIndex);
            // strip file extension
            int endIndex = name.LastIndexOf('.');
            if (endIndex != -1)
            {
                name = name.Remove(endIndex);
            }
            return name.Trim();
        }

        static string GetEmbeddedClipartItemData(string path)
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();

            using (var stream = assembly.GetManifestResourceStream(path))
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
