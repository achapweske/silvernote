/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Reflection;
using System.Text;

namespace SilverNote.Commands
{
    public class NShortcut : INotifyPropertyChanged
    {
        #region Fields

        string _Gesture;
        string _Command;
        string _Parameter;

        #endregion

        #region Constructors

        public NShortcut()
        {

        }

        public NShortcut(string gesture, string command, string parameter = null)
        {
            Gesture = gesture;
            Command = command;
            Parameter = parameter;
        }

        #endregion

        #region Properties

        public string Gesture
        {
            get
            {
                return _Gesture;
            }
            set
            {
                _Gesture = value;
                RaisePropertyChanged("Gesture");
            }
        }

        public string Command
        {
            get
            {
                return _Command;
            }
            set
            {
                _Command = value;
                RaisePropertyChanged("Command");
            }
        }

        public string Parameter
        {
            get
            {
                return _Parameter;
            }
            set
            {
                _Parameter = value;
                RaisePropertyChanged("Parameter");
            }
        }


        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return String.Format("NShortcut {{ Gesture = {0}, Command = {1} }}", Gesture, Command);
        }

        #endregion
    }


}
