/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Events.Internal
{
    /// <summary>
    /// The Event interface is used to provide contextual information about an event to the handler processing the event.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Events-20001113/events.html#Events-Event
    /// </summary>
    public class EventBase : Event
    {
        #region Fields

        string _Type;
        EventTarget _Target;
        EventTarget _CurrentTarget;
        EventPhaseType _EventPhase;
        bool _Bubbles;
        bool _Cancelable;
        bool _StopPropagation;
        bool _PreventDefault;

        #endregion

        #region Constructors

        #endregion

        #region Event

        /// <summary>
        /// The name of the event (case-insensitive). The name must be an XML name.
        /// </summary>
        public string Type
        {
            get { return _Type; }
        }

        /// <summary>
        /// Used to indicate the EventTarget to which the event was originally dispatched.
        /// </summary>
        public EventTarget Target
        {
            get { return _Target; }
        }

        /// <summary>
        /// Used to indicate the EventTarget whose EventListeners are currently being processed.
        /// </summary>
        public EventTarget CurrentTarget
        {
            get { return _CurrentTarget; }
        }

        /// <summary>
        /// Used to indicate which phase of event flow is currently being evaluated.
        /// </summary>
        public EventPhaseType EventPhase
        {
            get { return _EventPhase; }
        }

        /// <summary>
        /// Used to indicate whether or not an event is a bubbling event.
        /// </summary>
        public bool Bubbles
        {
            get { return _Bubbles; }
        }

        /// <summary>
        /// Used to indicate whether or not an event can have its default action prevented.
        /// </summary>
        public bool Cancelable
        {
            get { return _Cancelable; }
        }

        /// <summary>
        /// Used to specify the time (in milliseconds relative to the epoch) at which the event was created.
        /// </summary>
        public long Timestamp
        {
            get { return 0; }
        }

        /// <summary>
        /// The initEvent method is used to initialize the value of an Event created through the DocumentEvent interface. This method may only be called before the Event has been dispatched via the dispatchEvent method, though it may be called multiple times during that phase if necessary. If called multiple times the final invocation takes precedence. If called from a subclass of Event interface only the values specified in the initEvent method are modified, all other attributes are left unchanged.
        /// </summary>
        /// <param name="eventType">Specifies the event type.</param>
        /// <param name="canBubble">Specifies whether or not the event can bubble.</param>
        /// <param name="cancelable">Specifies whether or not the event's default action can be prevented.</param>
        public void InitEvent(string eventType, bool canBubble, bool cancelable)
        {
            _Type = eventType;
            _Bubbles = canBubble;
            _Cancelable = cancelable;
        }

        public void StopPropagation()
        {
            _StopPropagation = true;
        }

        public void PreventDefault()
        {
            if (Cancelable)
            {
                _PreventDefault = true;
            }
        }

        #endregion

        #region Implementation

        internal delegate void DispatchDelegate(EventBase evt);

        internal bool Dispatch(EventTarget[] targets, DispatchDelegate callback)
        {
            _Target = targets.First();
            var ancestors = targets.Skip(1);

            // CAPTURING_PHASE

            _EventPhase = EventPhaseType.CAPTURING_PHASE;

            foreach (var target in ancestors.Reverse())
            {
                _CurrentTarget = target;

                callback(this);

                if (_StopPropagation)
                {
                    break;
                }
            }

            // AT_TARGET

            _EventPhase = EventPhaseType.AT_TARGET;
            _CurrentTarget = _Target;
            callback(this);

            // BUBBLING_PHASE

            if (Bubbles)
            {
                _EventPhase = EventPhaseType.BUBBLING_PHASE;

                foreach (var target in ancestors)
                {
                    _CurrentTarget = target;

                    callback(this);

                    if (_StopPropagation)
                    {
                        break;
                    }
                }
            }

            return !_PreventDefault;
        }

        #endregion

    }
}
