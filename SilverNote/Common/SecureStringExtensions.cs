/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace SilverNote.Common
{
    public static class SecureStringExtensions
    {
        /// <summary>
        /// Compare two SecureString objects to determine if they represent the same string
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Equals(SecureString a, SecureString b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            else if (a == null || b == null)
            {
                return false;
            }

            if (a.Length != b.Length)  
            {  
                return false;  
            }  

            IntPtr bstrA = IntPtr.Zero;  
            IntPtr bstrB = IntPtr.Zero;  

            try 
            {  
                bstrA = Marshal.SecureStringToBSTR(a);  
                bstrB = Marshal.SecureStringToBSTR(b);

                for (int i = 0; i < a.Length; i++)
                {
                    if (Marshal.ReadInt16(bstrA, i*2) != Marshal.ReadInt16(bstrB, i*2))
                    {
                        return false;
                    }
                }

                return true;
            }  
            finally 
            {  
                if (bstrA != IntPtr.Zero)  
                {  
                    Marshal.ZeroFreeBSTR(bstrA);  
                }  

                if (bstrB != IntPtr.Zero)  
                {  
                    Marshal.ZeroFreeBSTR(bstrB);  
                }  
            }  
        }
    }
}
