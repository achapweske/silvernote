/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SilverNote.Common
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _Execute;
        private readonly Action<object> _ExecuteWithParameter;
        private readonly Predicate<object> _CanExecute;

        public DelegateCommand(Action execute, Predicate<object> canExecute = null)
        {
            _Execute = execute;
            _CanExecute = canExecute;
        }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _ExecuteWithParameter = execute;
            _CanExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            if (_Execute != null)
            {
                _Execute();
            }
            else if (_ExecuteWithParameter != null)
            {
                _ExecuteWithParameter(parameter);
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_CanExecute != null)
            {
                return _CanExecute(parameter);
            }
            else
            {
                return true;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class NullCommand : ICommand
    {
        private static NullCommand _Value = null;

        public static NullCommand Value
        {
            get
            {
                if (_Value == null)
                {
                    _Value = new NullCommand();
                }
                return _Value;
            }
        }

        public void Execute(object parameter)
        {

        }

        public bool CanExecute(object parameter)
        {
            return false;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
