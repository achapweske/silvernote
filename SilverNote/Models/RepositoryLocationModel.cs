/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace SilverNote.Models
{
    [XmlType("LoginModel")]
    public class RepositoryLocationModel : ModelBase
    {
        #region Fields

        string _Alias;
        string _Uri;
        string _Username;
        SecureString _Password;

        #endregion

        #region Constructors

        public RepositoryLocationModel()
        {

        }

        public RepositoryLocationModel(string filePath)
        {
            Alias = filePath;
            FilePath = filePath;
        }

        #endregion

        #region Properties

        public string Alias
        {
            get 
            { 
                return _Alias; 
            }
            set
            {
                _Alias = value;
                RaisePropertyChanged("Alias");
            }
        }

        public string Uri 
        {
            get 
            { 
                return _Uri; 
            }
            set
            {
                _Uri = value;
                RaisePropertyChanged("Uri");
            }
        }

        public string Username
        {
            get 
            { 
                return _Username; 
            }
            set
            {
                _Username = value;
                RaisePropertyChanged("Username");
            }
        }

        public SecureString Password
        {
            get 
            { 
                return _Password; 
            }
            set
            {
                _Password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string SavedPassword { get; set; }

        [XmlIgnore]
        public string FilePath
        {
            get { return FilePathFromUri(Uri); }
            set { Uri = UriFromFilePath(value); }
        }

        #endregion

        #region Implementation

        private static string FilePathFromUri(string uri)
        {
            return FilePathFromUri(new Uri(uri));
        }

        private static string FilePathFromUri(Uri uri)
        {
            string filePath = HttpUtility.UrlDecode(uri.AbsolutePath);
            filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
            filePath = filePath.Trim(Path.DirectorySeparatorChar);
            return filePath;
        }

        private static string UriFromFilePath(string filePath)
        {
            return new Uri(filePath).ToString();
        }

        #endregion
    }

    public class RepositoryLocationCollection : List<RepositoryLocationModel>
    {
        public RepositoryLocationCollection()
        {

        }

        public RepositoryLocationCollection(IEnumerable<RepositoryLocationModel> collection)
            : base(collection)
        {

        }
    }

    public class LoginCollection : List<RepositoryLocationModel>
    {
        public LoginCollection()
        {

        }

        public LoginCollection(IEnumerable<RepositoryLocationModel> collection)
            : base(collection)
        {

        }
    }
}
