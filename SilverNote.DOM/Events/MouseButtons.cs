/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Events
{
    public static class MouseButtons
    {
        // For use with MouseEvent.button (singular)
        public const ushort LeftButton = 0;
        public const ushort MiddleButton = 1;
        public const ushort RightButton = 2;

        // For use with MouseEvent.buttons (plural)
        public const ushort None = 0;
        public const ushort LeftButtonFlag = 1;
        public const ushort RightButtonFlag = 2;
        public const ushort MiddleButtonFlag = 4;
    }
}
