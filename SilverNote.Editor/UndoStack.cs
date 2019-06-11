/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SilverNote.Editor
{
    public class UndoStack
    {
        #region Fields

        LinkedList<UndoItem> _UndoList = new LinkedList<UndoItem>();
        LinkedList<UndoItem> _RedoList = new LinkedList<UndoItem>();
        LinkedList<UndoItem> _ScopeList = new LinkedList<UndoItem>();

        #endregion

        #region Constructors

        public UndoStack()
        {
            Limit = 1024;
        }

        #endregion

        public int Limit { get; set; }

        public delegate void Action();

        private class UndoItem
        {
            public Action Action = null;
            public string Name = null;
            public bool IsEditing = true;
            public bool IsBeginScope = false;
            public bool IsEndScope = false;
        }

        public void BeginScope(string name = null, bool isEditing = true)
        {
            var item = new UndoItem { IsBeginScope = true, Name = name, IsEditing = isEditing };

            _ScopeList.AddFirst(item);

            if (IsUndoing)
            {
                _RedoList.AddFirst(item);
            }
            else
            {
                _UndoList.AddFirst(item);
            }
        }

        public void EndScope(string name = null, bool isEditing = true)
        {
            _ScopeList.RemoveFirst();

            if (IsUndoing)
            {
                if (_RedoList.Count > 0 && _RedoList.First().IsBeginScope)
                {
                    _RedoList.RemoveFirst();
                    return;
                }
            }
            else
            {
                if (_UndoList.Count > 0 && _UndoList.First().IsBeginScope)
                {
                    _UndoList.RemoveFirst();
                    return;
                }
            }

            var item = new UndoItem { IsEndScope = true, Name = name, IsEditing = isEditing };

            if (IsUndoing)
            {
                _RedoList.AddFirst(item);
            }
            else
            {
                _UndoList.AddFirst(item);
            }
        }

        public bool IsWithinScope
        {
            get { return _ScopeList.Count > 0; }
        }

        public void Push(Action action)
        {
            Push(new UndoItem() { Action = action });
        }

        public void Push(string name, Action action)
        {
            Push(new UndoItem() { Action = action, Name = name });
        }

        private void Push(UndoItem item)
        {
            if (!IsUndoing && !IsRedoing)
            {
                _RedoList.Clear();
            }

            if (IsUndoing)
            {
                _RedoList.AddFirst(item);
                if (_RedoList.Count > Limit)
                {
                    _RedoList.RemoveLast();
                }
            }
            else
            {
                _UndoList.AddFirst(item);
                if (_UndoList.Count > Limit)
                {
                    _UndoList.RemoveLast();
                }
            }

            if (IsEditing)
            {
                RaiseEdited();
            }
        }

        public void Clear()
        {
            _UndoList.Clear();
            _RedoList.Clear();
            _ScopeList.Clear();
        }

        public void Undo()
        {
            try
            {
                IsUndoing = true;

                int level = 0;
                while (_UndoList.Count > 0)
                {
                    UndoItem item = _UndoList.First.Value;
                    _UndoList.RemoveFirst();
                    if (item.IsEndScope)
                    {
                        if (level++ == 0)
                        {
                            BeginScope(item.Name, item.IsEditing);
                        }
                    }
                    if (item.Name != null && !item.IsBeginScope)
                    {
                        Debug.WriteLine("Undo \"" + item.Name + "\"");
                    }
                    if (item.Action != null)
                    {
                        item.Action();
                    }
                    if (item.IsBeginScope)
                    {
                        if (--level == 0)
                        {
                            EndScope(item.Name, item.IsEditing);
                        }
                    }
                    if (level <= 0 && item.IsEditing)
                    {
                        break;
                    }
                }
            }
            finally
            {
                IsUndoing = false;
            }
        }

        public bool IsUndoing { get; private set; }

        public void Redo()
        {
            try
            {
                IsRedoing = true;

                int level = 0;
                while (_RedoList.Count > 0)
                {
                    UndoItem item = _RedoList.First.Value;
                    _RedoList.RemoveFirst();
                    if (item.IsEndScope)
                    {
                        if (level++ == 0)
                        {
                            BeginScope(item.Name, item.IsEditing);
                        }
                    }
                    if (item.Name != null && !item.IsBeginScope)
                    {
                        Debug.WriteLine("Redo \"" + item.Name + "\"");
                    }
                    if (item.Action != null)
                    {
                        item.Action();
                    }
                    if (item.IsBeginScope)
                    {
                        if (--level == 0)
                        {
                            EndScope(item.Name, item.IsEditing);
                        }
                    }
                    if (level <= 0 && item.IsEditing)
                    {
                        break;
                    }
                }
            }
            finally
            {
                IsRedoing = false;
            }
        }

        public bool IsRedoing { get; private set; }

        public bool IsEditing
        {
            get
            {
                return _ScopeList.Where(item => !item.IsEditing).FirstOrDefault() == null;
            }
        }

        public void Edit()
        {
            RaiseEdited();
        }

        public event EventHandler Edited;

        protected virtual void RaiseEdited()
        {
            if (Edited != null)
            {
                Edited(this, new EventArgs());
            }
        }

    }

    public class UndoScope : IDisposable
    {
        #region Fields

        UndoStack _Stack;
        string _Name;
        bool _IsEditing;

        #endregion

        #region Constructors

        public UndoScope(UndoStack stack, string name = null, bool isEditing = true)
        {
            _Stack = stack;
            _Name = name;
            _IsEditing = isEditing;

            if (_Stack != null)
            {
                _Stack.BeginScope(Name, isEditing);
            }
        }

        #endregion

        #region Properties

        public UndoStack Stack
        {
            get { return _Stack; }
        }

        public string Name
        {
            get { return _Name; }
        }

        public bool IsEditing
        {
            get { return _IsEditing; }
        }

        #endregion

        #region Operations

        public void Push(UndoStack.Action action)
        {
            if (Stack != null)
            {
                Stack.Push(action);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (Stack != null)
            {
                Stack.EndScope(Name, IsEditing);
            }
        }

        #endregion
    }

}
