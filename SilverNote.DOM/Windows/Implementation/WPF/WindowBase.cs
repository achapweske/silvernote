/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using DOM.Views;

namespace DOM.Windows.Internal.WPF
{
    public class WindowBase : DOM.Windows.Internal.WindowBase
    {
        #region Constructors

        public WindowBase()
        {

        }

        public WindowBase(DocumentView document)
            : base(document)
        {
        }

        #endregion

        #region Window

        public override int SetTimeout(TimerListener listener, int milliseconds)
        {
            return CreateTimer(listener, milliseconds, oneShot: true);
        }

        public override void ClearTimeout(int timerID)
        {
            ClearTimer(timerID);
        }

        public override int SetInterval(TimerListener listener, int milliseconds)
        {
            return CreateTimer(listener, milliseconds, oneShot: false);
        }

        public override void ClearInterval(int timerID)
        {
            ClearTimeout(timerID);
        }

        public override void Alert(string message)
        {
            MessageBox.Show(message);
        }

        #endregion

        #region Implementation

        struct TimerInfo
        {
            public int ID;
            public DispatcherTimer Timer;
            public TimerListener Listener;
            public bool OneShot;
        }

        Dictionary<int, TimerInfo> _Timers = new Dictionary<int, TimerInfo>();
        int _NextTimerID = 1;

        private int CreateTimer(TimerListener listener, int milliseconds, bool oneShot)
        {
            var info = new TimerInfo
            {
                Listener = listener,
                OneShot = oneShot
            };

            while (_Timers.ContainsKey(_NextTimerID))
            {
                _NextTimerID++;
            }
            info.ID = _NextTimerID++;

            info.Timer = new DispatcherTimer();
            info.Timer.Tag = info;
            info.Timer.Interval = TimeSpan.FromMilliseconds(milliseconds);
            info.Timer.Tick += Timer_Tick;

            _Timers.Add(info.ID, info);

            info.Timer.Start();

            return info.ID;
        }

        public void ClearTimer(int timerID)
        {
            TimerInfo info;
            if (_Timers.TryGetValue(timerID, out info))
            {
                _Timers.Remove(timerID);
                info.Timer.Stop();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var timer = (DispatcherTimer)sender;
            var info = (TimerInfo)timer.Tag;

            if (_Timers.ContainsKey(info.ID))
            {
                if (info.OneShot)
                {
                    _Timers.Remove(info.ID);
                    timer.Stop();
                }
                info.Listener();
            }
        }

        #endregion

    }
}
