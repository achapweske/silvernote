/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Views;

namespace DOM.Events.Internal
{
    public class MouseEventImpl : UIEventImpl, MouseEvent
    {
        #region Fields

        int _ScreenX;
        int _ScreenY;
        int _ClientX;
        int _ClientY;
        bool _CtrlKey;
        bool _ShiftKey;
        bool _AltKey;
        bool _MetaKey;
        ushort _Button;
        ushort _Buttons;
        EventTarget _RelatedTarget;

        #endregion

        #region MouseEvent

        public int ScreenX
        {
            get { return _ScreenX; }
        }

        public int ScreenY
        {
            get { return _ScreenY; }
        }

        public int ClientX
        {
            get { return _ClientX; }
        }

        public int ClientY
        {
            get { return _ClientY; }
        }

        public bool CtrlKey
        {
            get { return _CtrlKey; }
        }

        public bool ShiftKey
        {
            get { return _ShiftKey; }
        }

        public bool AltKey
        {
            get { return _AltKey; }
        }

        public bool MetaKey
        {
            get { return _MetaKey; }
        }

        public ushort Button
        {
            get { return _Button; }
        }

        public ushort Buttons
        {
            get { return _Buttons; }
        }

        public EventTarget RelatedTarget
        {
            get { return _RelatedTarget; }
        }

        public void InitMouseEvent(string typeArg,
                            bool canBubbleArg,
                            bool cancelableArg,
                            AbstractView viewArg,
                            int detailArg,
                            int screenXArg,
                            int screenYArg,
                            int clientXArg,
                            int clientYArg,
                            bool ctrlKeyArg,
                            bool altKeyArg,
                            bool shiftKeyArg,
                            bool metaKeyArg,
                            ushort buttonArg,
                            EventTarget relatedTargetArg)
        {
            InitMouseEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg, screenXArg, screenXArg, clientXArg, clientYArg, ctrlKeyArg, altKeyArg, shiftKeyArg, metaKeyArg, buttonArg, 0, relatedTargetArg);
        }

        public void InitMouseEvent(string typeArg,
                            bool canBubbleArg,
                            bool cancelableArg,
                            AbstractView viewArg,
                            int detailArg,
                            int screenXArg,
                            int screenYArg,
                            int clientXArg,
                            int clientYArg,
                            bool ctrlKeyArg,
                            bool altKeyArg,
                            bool shiftKeyArg,
                            bool metaKeyArg,
                            ushort buttonArg,
                            ushort buttonsArg,
                            EventTarget relatedTargetArg)
        {
            base.InitUIEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg);

            _ScreenX = screenXArg;
            _ScreenY = screenYArg;
            _ClientX = clientXArg;
            _ClientY = clientYArg;
            _CtrlKey = ctrlKeyArg;
            _AltKey = altKeyArg;
            _ShiftKey = shiftKeyArg;
            _MetaKey = metaKeyArg;
            _Button = buttonArg;
            _Buttons = buttonsArg;
            _RelatedTarget = relatedTargetArg;
        }

        #endregion
    }
}
