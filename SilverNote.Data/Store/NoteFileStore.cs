/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security;
using System.Xml.Serialization;
using System.Diagnostics;
using SilverNote.Data.Models;
using System.Collections.Specialized;

namespace SilverNote.Data.Store
{
    public class NoteFileStore : NoteStore
    {
        #region Static Methods

        new public static NoteStore Create(Uri uri, string username, SecureString password, bool autoCreate)
        {
            if (uri.Scheme == Uri.UriSchemeFile)
            {
                return new NoteFileStore(uri.LocalPath);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Fields

        readonly string _RootPath;

        #endregion

        #region Constructors

        public NoteFileStore(string rootPath)
        {
            _RootPath = rootPath;

            _ClipartGroups.Add(1, Path.Combine(ClipartPath, "Lines (favorites)"));
            _ClipartGroups.Add(2, Path.Combine(ClipartPath, "Markers (favorites)"));
        }

        #endregion

        #region Properties

        public string RootPath
        {
            get { return _RootPath; }
        }

        #endregion

        #region Repository

        public override RepositoryDataModel GetRepository()
        {
            return new RepositoryDataModel
            {
                Notebooks = new NotebookDataModel[0]
            };
        }

        public override bool SetRepository(RepositoryDataModel repository)
        {
            return false;
        }

        #endregion

        #region Clipart

        private string ClipartPath
        {
            get { return Path.Combine(RootPath, "SilverNote", "Clipart"); }
        }

        public override ClipartGroupDataModel[] GetClipartGroups()
        {
            if (!Directory.Exists(ClipartPath))
            {
                return new ClipartGroupDataModel[0];
            }

            var groupPaths = Directory.EnumerateDirectories(ClipartPath);

            var results = groupPaths.Select(dirPath =>
                new ClipartGroupDataModel
                {
                    // Exclude "Items" field
                    ID = GetClipartGroupID(dirPath),
                    Name = GetClipartGroupName(dirPath),
                    CreatedAt = Directory.GetCreationTime(dirPath),
                    ModifiedAt = Directory.GetLastWriteTime(dirPath),
                    ViewedAt = Directory.GetLastAccessTime(dirPath)
                }
            );

            return results.ToArray();
        }

        public override Int64 CreateClipartGroup(ClipartGroupDataModel group)
        {
            if (group.ID == 0)
            {
                throw new Exception("0 is not a valid group ID");
            }

            string groupPath = Path.Combine(ClipartPath, group.ID.ToString());
            Directory.CreateDirectory(groupPath);
            SetClipartGroupPath(group.ID, groupPath);
            SetClipartGroup(group.ID, group);

            return group.ID;
        }

        public override bool DeleteClipartGroup(long groupID, bool purge = false)
        {
            string groupPath = GetClipartGroupPath(groupID);
            if (groupPath != null)
            {
                return DeleteClipartGroup(groupID, groupPath);
            }
            else
            {
                return false;
            }
        }

        private bool DeleteClipartGroup(Int64 groupID, string groupPath)
        {
            if (!Directory.Exists(groupPath))
            {
                return false;
            }

            Directory.Delete(groupPath);
            return true;
        }

        public override ClipartGroupDataModel GetClipartGroup(Int64 groupID)
        {
            string groupPath = GetClipartGroupPath(groupID);
            if (groupPath != null)
            {
                return GetClipartGroup(groupID, groupPath);
            }
            else
            {
                return null;
            }
        }

        private ClipartGroupDataModel GetClipartGroup(Int64 groupID, string groupPath)
        {
            return new ClipartGroupDataModel
            {
                ID = groupID,
                Name = GetClipartGroupName(groupPath),
                Items = GetClipartItems(groupID, groupPath),
                CreatedAt = Directory.GetCreationTime(groupPath),
                ModifiedAt = Directory.GetLastWriteTime(groupPath),
                ViewedAt = Directory.GetLastAccessTime(groupPath)
            };
        }

        public override bool SetClipartGroup(Int64 groupID, ClipartGroupDataModel group)
        {
            if (group.Name != null)
            {
                return SetClipartGroupName(groupID, group.Name, group.NameModifiedAt);
            }
            else
            {
                return true;
            }
        }

        public override bool SetClipartGroupName(Int64 groupID, string newName, DateTime modifiedAt)
        {
            string oldDirPath = GetClipartGroupPath(groupID);
            if (oldDirPath == null)
            {
                return false;   // Not found
            }

            string oldName = GetClipartGroupName(oldDirPath);
            if (newName == oldName)
            {
                return true;    // No change
            }

            string newDirPath = Path.Combine(ClipartPath, newName);
            Directory.Move(oldDirPath, newDirPath);
            SetClipartGroupPath(groupID, newDirPath);

            return true;
        }

        public override string GetClipartGroupName(Int64 groupID)
        {
            string groupPath = GetClipartGroupPath(groupID);
            if (groupPath != null)
            {
                return GetClipartGroupName(groupPath);
            }
            else
            {
                return null;
            }
        }

        private string GetClipartGroupName(string groupPath)
        {
            return Path.GetFileName(groupPath);
        }

        public override ClipartDataModel[] GetClipartItems(Int64 groupID)
        {
            string groupPath = GetClipartGroupPath(groupID);
            if (groupPath != null)
            {
                return GetClipartItems(groupID, groupPath);
            }
            else
            {
                return null;    // Not found
            }
        }

        private ClipartDataModel[] GetClipartItems(Int64 groupID, string groupPath)
        {
            if (!Directory.Exists(groupPath))
            {
                return null;
            }

            var filePaths = Directory.EnumerateFiles(groupPath);

            var results = filePaths.Select(filePath =>
                new ClipartDataModel
                {
                    // Exclude "Data" field
                    GroupID = groupID,
                    ID = GetClipartItemID(filePath),
                    Name = GetClipartName(filePath),
                    CreatedAt = File.GetCreationTime(filePath),
                    ModifiedAt = File.GetLastWriteTime(filePath),
                    ViewedAt = File.GetLastAccessTime(filePath)
                }
            );

            return results.ToArray();
        }

        public override Int64 CreateClipart(Int64 groupID, ClipartDataModel clipart)
        {
            if (clipart.ID == 0)
            {
                throw new Exception("0 is not a valid clipart item ID");
            }

            string groupPath = GetClipartGroupPath(groupID);
            if (groupPath == null)
            {
                return 0;   // Group not found
            }

            string clipartPath = Path.Combine(groupPath, clipart.ID.ToString());
            File.Create(clipartPath).Dispose();
            SetClipartItemPath(clipart.ID, clipartPath);
            SetClipart(groupID, clipart.ID, clipart);

            return clipart.ID;
        }

        public override bool DeleteClipart(Int64 groupID, Int64 clipartID, bool purge)
        {
            string filePath = GetClipartItemPath(clipartID);
            if (filePath != null)
            {
                return PurgeClipart(groupID, clipartID, filePath);
            }
            else
            {
                return false;
            }
        }

        private bool PurgeClipart(Int64 groupID, Int64 clipartID, string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            File.Delete(filePath);
            return true;
        }

        public override ClipartDataModel GetClipart(Int64 groupID, Int64 clipartID)
        {
            string filePath = GetClipartItemPath(clipartID);
            if (filePath != null)
            {
                return GetClipart(groupID, clipartID, filePath);
            }
            else
            {
                return null;
            }
        }

        private ClipartDataModel GetClipart(Int64 groupID, Int64 clipartID, string filePath)
        {
            var clipart = new ClipartDataModel
            {
                ID = clipartID,
                GroupID = groupID,
                Name = GetClipartName(filePath),
                Data = GetClipartData(filePath),
                CreatedAt = File.GetCreationTime(filePath),
                ModifiedAt = File.GetLastWriteTime(filePath),
                ViewedAt = File.GetLastAccessTime(filePath)
            };

            return clipart;
        }

        public override string GetClipartName(long groupID, long clipartID)
        {
            string filePath = GetClipartItemPath(clipartID);
            if (filePath != null)
            {
                return GetClipartName(filePath);
            }
            else
            {
                return null;
            }
        }

        private string GetClipartName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public override string GetClipartData(long groupID, long clipartID)
        {
            string filePath = GetClipartItemPath(clipartID);
            if (filePath != null)
            {
                return GetClipartData(filePath);
            }
            else
            {
                return null;
            }
        }

        private string GetClipartData(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public override bool SetClipart(Int64 groupID, Int64 clipartID, ClipartDataModel clipart)
        {
            if (!ContainsClipart(groupID, clipartID))
            {
                return false;
            }

            if (clipart.Name != null)
            {
                SetClipartName(groupID, clipartID, clipart.Name, clipart.NameModifiedAt);
            }

            if (clipart.Data != null)
            {
                SetClipartData(groupID, clipartID, clipart.Data, clipart.ModifiedAt);
            }

            return true;
        }

        public override bool ContainsClipart(long groupID, long clipartID)
        {
            return GetClipartItemPath(clipartID) != null;
        }

        public override bool SetClipartName(Int64 groupID, Int64 clipartID, string newName, DateTime modifiedAt)
        {
            string dirPath = GetClipartGroupPath(groupID);
            if (dirPath == null)
            {
                return false;   // Group not found
            }

            string oldFilePath = GetClipartItemPath(clipartID);
            if (oldFilePath == null)
            {
                return false;   // Clipart not found
            }

            string newFilePath = Path.Combine(dirPath, newName) + ".svg";
            if (newFilePath == oldFilePath)
            {
                return true;    // No change
            }

            for (int i = 1; File.Exists(newFilePath); i++)
            {
                string tempName = String.Format("{0} ({1}).svg", newName, i);
                newFilePath = Path.Combine(dirPath, tempName);
            }

            File.Move(oldFilePath, newFilePath);
            SetClipartItemPath(clipartID, newFilePath);

            return true;
        }

        public override bool SetClipartData(Int64 groupID, Int64 clipartID, string data, DateTime modifiedAt)
        {
            string filePath = GetClipartItemPath(clipartID);
            if (filePath == null)
            {
                return false;   // Not found
            }

            File.WriteAllText(filePath, data);
            if (modifiedAt != default(DateTime))
            {
                File.SetLastWriteTime(filePath, modifiedAt);
            }
        
            return true;
        }

        #endregion

        #region Implementation

        Int64 _NextID = 0x1000;

        // Map group IDs to directory paths
        Dictionary<Int64, string> _ClipartGroups = new Dictionary<Int64, string>();

        string GetClipartGroupPath(Int64 id)
        {
            string result;
            if (_ClipartGroups.TryGetValue(id, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        void SetClipartGroupPath(Int64 id, string dirPath)
        {
            _ClipartGroups[id] = dirPath;
        }

        Int64 GetClipartGroupID(string dirPath)
        {
            Int64 id;
            var group = _ClipartGroups.FirstOrDefault(entry => entry.Value == dirPath);
            if (group.Value == dirPath)
            {
                id = group.Key;
            }
            else
            {
                id = GetID(dirPath);
                SetClipartGroupPath(id, dirPath);
            }

            return id;
        }

        // Map clipart item IDs to directory paths
        Dictionary<Int64, string> _ClipartItems = new Dictionary<Int64, string>();

        string GetClipartItemPath(Int64 id)
        {
            string result;
            if (_ClipartItems.TryGetValue(id, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        void SetClipartItemPath(Int64 id, string dirPath)
        {
            _ClipartItems[id] = dirPath;
        }

        Int64 GetClipartItemID(string dirPath)
        {
            Int64 id;

            var Item = _ClipartItems.FirstOrDefault(entry => entry.Value == dirPath);
            if (Item.Value == dirPath)
            {
                id = Item.Key;
            }
            else
            {
                id = _NextID++;
                SetClipartItemPath(id, dirPath);
            }

            return id;
        }

        Int64 GetID(string dirPath)
        {
            /*
            string idPath = Directory.EnumerateFiles(dirPath, "*.id", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (idPath == null)
            {
                return 0;
            }

            string idString = Path.GetFileNameWithoutExtension(idPath);

            Int64 id;
            if (!Int64.TryParse(idString, out id))
            {
                return 0;
            }
            */

            return _NextID++;
        }

        #endregion

    }
}
