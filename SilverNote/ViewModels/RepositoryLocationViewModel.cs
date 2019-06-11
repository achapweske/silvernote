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
using System.IO;
using System.Security;
using System.Windows.Data;
using SilverNote.Common;
using SilverNote.Models;

namespace SilverNote.ViewModels
{
    public class RepositoryLocationViewModel : ViewModelBase<RepositoryLocationModel, RepositoryLocationViewModel>
    {
        #region Properties

        public string DisplayName
        {
            get
            {
                var result = new StringBuilder();
                string filePath = FilePath;
                if (!String.IsNullOrEmpty(filePath))
                {
                    result.Append(filePath);
                }
                else
                {
                    result.Append(Uri);
                }
                
                Uri uri;
                if (System.Uri.TryCreate(Uri, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (!String.IsNullOrEmpty(uri.Host))
                    {
                        result.AppendFormat(" ({0})", uri.Host);
                    }
                }

                return result.ToString();
            }
        }

        public string Alias
        {
            get { return Model.Alias; }
            set { Model.Alias = value; }
        }

        public string Uri
        {
            get { return Model.Uri; }
            set { Model.Uri = value; }
        }

        public string Username
        {
            get { return Model.Username; }
            set { Model.Username = value; }
        }

        public SecureString Password
        {
            get { return Model.Password; }
            set { Model.Password = value; }
        }

        public string FilePath
        {
            get 
            {
                string filePath = Model.FilePath;
                if (filePath.EndsWith("\\.repository"))
                {
                    filePath = filePath.Remove(filePath.Length - "\\.repository".Length);
                }
                return filePath; 
            }
            set 
            { 
                Model.FilePath = value; 
            }
        }

        #endregion

    }

    [ValueConversion(typeof(RepositoryLocationModel), typeof(RepositoryLocationViewModel))]
    public class RepositoryLocationViewModelConverter : ViewModelConverter<RepositoryLocationModel, RepositoryLocationViewModel>
    {

    }

    [ValueConversion(typeof(IEnumerable<RepositoryLocationModel>), typeof(IEnumerable<RepositoryLocationViewModel>))]
    public class RepositoryLocationViewModelsConverter : ViewModelsConverter<RepositoryLocationModel, RepositoryLocationViewModel>
    {

    }

}
