/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace SilverNote.Models
{
    public static class IDGenerator
    {
        #region Fields

        static bool _IsSettingsDirty = false;

        #endregion

        #region Operations

        /// <summary>
        /// Get the next available Note ID
        /// </summary>
        public static Int64 NextID()
        {
            if (Settings.Default.ClientID == 0)
            {
                Settings.Default.ClientID = RandomID();
            }

            Int32 id = Settings.Default.ResourceID++;
            _IsSettingsDirty = true;

            if (App.Current.MainWindow != null)
            {
                App.Current.MainWindow.Dispatcher.BeginInvoke(
                    new Action(SaveSettings),
                    DispatcherPriority.Background
                );
            }
            else
            {
                SaveSettings();
            }

            return ((Int64)Settings.Default.ClientID << 32) | (UInt32)id;
        }

        #endregion

        #region Implementation

        public static int RandomID()
        {
            return new Random().Next();
        }

        private static void SaveSettings()
        {
            if (_IsSettingsDirty)
            {
                try
                {
                    Settings.Default.Save();
                }
                catch (Exception e)
                {
                    Debug.Assert(false, e.Message);
                }
                _IsSettingsDirty = false;
            }
        }

        #endregion
    }

}
