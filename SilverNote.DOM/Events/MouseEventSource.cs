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
using DOM.Internal;

namespace DOM.Events
{
    public class MouseEventSource : UIEventSource
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
        INodeSource _RelatedTarget;

        #endregion

        #region Properties

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

        public INodeSource RelatedTarget
        {
            get { return _RelatedTarget; }
        }

        #endregion

        #region Operations

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
                            INodeSource relatedTargetArg)
        {
            InitMouseEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg, screenXArg, screenYArg, clientXArg, clientYArg, ctrlKeyArg, altKeyArg, shiftKeyArg, metaKeyArg, buttonArg, 0, relatedTargetArg);
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
                            INodeSource relatedTargetArg)
        {
            InitUIEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg);

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

        #region IEventSource

        public override void Dispatch(Node node)
        {
            DocumentBase document = (DocumentBase)node.OwnerDocument;
            if (document != null && 
                (document.HasEventListeners(Type, true) || document.HasEventListeners(Type, false)))
            {
                EventTarget relatedTarget = null;
                if (RelatedTarget != null)
                {
                    relatedTarget = document.GetNode(RelatedTarget) as EventTarget;
                }

                MouseEvent evt = (MouseEvent)document.CreateEvent(EventTypes.MouseEvents);
                evt.InitMouseEvent(Type, Bubbles, Cancelable, View, Detail, ScreenX, ScreenY, ClientX, ClientY, CtrlKey, AltKey, ShiftKey, MetaKey, Button, Buttons, relatedTarget);
                if (!node.DispatchEvent(evt))
                {
                    Cancel();
                }
            }
        }


        #endregion
    }
}
