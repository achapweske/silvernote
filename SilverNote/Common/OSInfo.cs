/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Runtime.InteropServices;
using System.Text;

/*
 * A collection of utilies to get information about the current operating system. 
 */
public static class OSInfo
{
    #region OSString

    public static string OSString
    {
        get
        {
            var buffer = new StringBuilder();

            buffer.Append(Name);
            buffer.Append(' ');
            buffer.Append(Edition);
            buffer.Append(' ');
            buffer.Append(ServicePack);
            buffer.Append(' ');
            buffer.AppendFormat("(x{0})", Bits);

            return buffer.ToString();
        }
    }

    #endregion

    #region Name

    private static string _Name;

    public static string Name
    {
        get
        {
            if (_Name == null)
            {
                _Name = GetName();
            }

            return _Name;
        }
    }

    private static string GetName()
    {
        var osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

        if (!GetVersionEx(ref osVersionInfo))
        {
            return String.Empty;
        }

        var osVersion = Environment.OSVersion;
        int majorVersion = osVersion.Version.Major;
        int minorVersion = osVersion.Version.Minor;

        switch (osVersion.Platform)
        {
            #region Win32Windows

            case PlatformID.Win32Windows:
                {
                    if (majorVersion == 4)
                    {
                        string csdVersion = osVersionInfo.szCSDVersion;
                        switch (minorVersion)
                        {
                            case 0:
                                if (csdVersion == "B" || csdVersion == "C")
                                    return "Windows 95 OSR2";
                                else
                                    return "Windows 95";
                            case 10:
                                if (csdVersion == "A")
                                    return "Windows 98 Second Edition";
                                else
                                    return "Windows 98";
                            case 90:
                                return "Windows Me";
                        }
                    }
                    break;
                }

            #endregion

            #region Win32NT

            case PlatformID.Win32NT:
                {
                    byte productType = osVersionInfo.wProductType;

                    switch (majorVersion)
                    {
                        case 3:
                            return "Windows NT 3.51";
                        case 4:
                            switch (productType)
                            {
                                case 1:
                                    return "Windows NT 4.0";
                                case 3:
                                    return "Windows NT 4.0 Server";
                            }
                            break;
                        case 5:
                            switch (minorVersion)
                            {
                                case 0:
                                    return "Windows 2000";
                                case 1:
                                    return "Windows XP";
                                case 2:
                                    return "Windows Server 2003";
                            }
                            break;
                        case 6:
                            switch (productType)
                            {
                                case 1:
                                    return "Windows Vista";
                                case 3:
                                    return "Windows Server 2008";
                            }
                            break;
                    }
                    break;
                }

            #endregion
        }

        return String.Empty;
    }

    #endregion NAME

    #region Edition

    private static string _Edition;

    public static string Edition
    {
        get
        {
            if (_Edition == null)
            {
                _Edition = GetEdition();
            }

            return GetEdition();
        }
    }

    private static string GetEdition()
    {
        OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
        if (!GetVersionEx(ref osVersionInfo))
        {
            return String.Empty;
        }

        OperatingSystem osVersion = Environment.OSVersion;
        int majorVersion = osVersion.Version.Major;
        int minorVersion = osVersion.Version.Minor;
        byte productType = osVersionInfo.wProductType;
        short suiteMask = osVersionInfo.wSuiteMask;

        #region VERSION 4

        if (majorVersion == 4)
        {
            if (productType == VER_NT_WORKSTATION)
            {
                // Windows NT 4.0 Workstation
                return "Workstation";
            }
            else if (productType == VER_NT_SERVER)
            {
                if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                {
                    // Windows NT 4.0 Server Enterprise
                    return "Enterprise Server";
                }
                else
                {
                    // Windows NT 4.0 Server
                    return "Standard Server";
                }
            }
        }

        #endregion VERSION 4

        #region VERSION 5

        else if (majorVersion == 5)
        {
            if (productType == VER_NT_WORKSTATION)
            {
                if ((suiteMask & VER_SUITE_PERSONAL) != 0)
                {
                    // Windows XP Home Edition
                    return "Home";
                }
                else
                {
                    // Windows XP / Windows 2000 Professional
                    return "Professional";
                }
            }
            else if (productType == VER_NT_SERVER)
            {
                if (minorVersion == 0)
                {
                    if ((suiteMask & VER_SUITE_DATACENTER) != 0)
                    {
                        // Windows 2000 Datacenter Server
                        return "Datacenter Server";
                    }
                    else if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                    {
                        // Windows 2000 Advanced Server
                        return "Advanced Server";
                    }
                    else
                    {
                        // Windows 2000 Server
                        return "Server";
                    }
                }
                else
                {
                    if ((suiteMask & VER_SUITE_DATACENTER) != 0)
                    {
                        // Windows Server 2003 Datacenter Edition
                        return "Datacenter";
                    }
                    else if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                    {
                        // Windows Server 2003 Enterprise Edition
                        return "Enterprise";
                    }
                    else if ((suiteMask & VER_SUITE_BLADE) != 0)
                    {
                        // Windows Server 2003 Web Edition
                        return "Web Edition";
                    }
                    else
                    {
                        // Windows Server 2003 Standard Edition
                        return "Standard";
                    }
                }
            }
        }

        #endregion VERSION 5

        #region VERSION 6

        else if (majorVersion == 6)
        {
            int edition;
            if (!GetProductInfo(majorVersion, minorVersion, osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor, out edition))
            {
                return String.Empty;
            }

            switch (edition)
            {
                case PRODUCT_BUSINESS:
                    return "Business";
                case PRODUCT_BUSINESS_N:
                    return "Business N";
                case PRODUCT_CLUSTER_SERVER:
                    return "HPC Edition";
                case PRODUCT_DATACENTER_SERVER:
                    return "Datacenter Server";
                case PRODUCT_DATACENTER_SERVER_CORE:
                    return "Datacenter Server (core installation)";
                case PRODUCT_ENTERPRISE:
                    return "Enterprise";
                case PRODUCT_ENTERPRISE_N:
                    return "Enterprise N";
                case PRODUCT_ENTERPRISE_SERVER:
                    return "Enterprise Server";
                case PRODUCT_ENTERPRISE_SERVER_CORE:
                    return "Enterprise Server (core installation)";
                case PRODUCT_ENTERPRISE_SERVER_CORE_V:
                    return "Enterprise Server without Hyper-V (core installation)";
                case PRODUCT_ENTERPRISE_SERVER_IA64:
                    return "Enterprise Server for Itanium-based Systems";
                case PRODUCT_ENTERPRISE_SERVER_V:
                    return "Enterprise Server without Hyper-V";
                case PRODUCT_HOME_BASIC:
                    return "Home Basic";
                case PRODUCT_HOME_BASIC_N:
                    return "Home Basic N";
                case PRODUCT_HOME_PREMIUM:
                    return "Home Premium";
                case PRODUCT_HOME_PREMIUM_N:
                    return "Home Premium N";
                case PRODUCT_HYPERV:
                    return "Microsoft Hyper-V Server";
                case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                    return "Windows Essential Business Management Server";
                case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                    return "Windows Essential Business Messaging Server";
                case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                    return "Windows Essential Business Security Server";
                case PRODUCT_SERVER_FOR_SMALLBUSINESS:
                    return "Windows Essential Server Solutions";
                case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                    return "Windows Essential Server Solutions without Hyper-V";
                case PRODUCT_SMALLBUSINESS_SERVER:
                    return "Windows Small Business Server";
                case PRODUCT_STANDARD_SERVER:
                    return "Standard Server";
                case PRODUCT_STANDARD_SERVER_CORE:
                    return "Standard Server (core installation)";
                case PRODUCT_STANDARD_SERVER_CORE_V:
                    return "Standard Server without Hyper-V (core installation)";
                case PRODUCT_STANDARD_SERVER_V:
                    return "Standard Server without Hyper-V";
                case PRODUCT_STARTER:
                    return "Starter";
                case PRODUCT_STORAGE_ENTERPRISE_SERVER:
                    return "Enterprise Storage Server";
                case PRODUCT_STORAGE_EXPRESS_SERVER:
                    return "Express Storage Server";
                case PRODUCT_STORAGE_STANDARD_SERVER:
                    return "Standard Storage Server";
                case PRODUCT_STORAGE_WORKGROUP_SERVER:
                    return "Workgroup Storage Server";
                case PRODUCT_UNDEFINED:
                    return "Unknown product";
                case PRODUCT_ULTIMATE:
                    return "Ultimate";
                case PRODUCT_ULTIMATE_N:
                    return "Ultimate N";
                case PRODUCT_WEB_SERVER:
                    return "Web Server";
                case PRODUCT_WEB_SERVER_CORE:
                    return "Web Server (core installation)";
            }
        }

        #endregion VERSION 6

        return String.Empty;
    }

    #endregion EDITION

    #region Service Pack

    private static string _ServicePack;

    public static string ServicePack
    {
        get
        {
            if (_ServicePack == null)
            {
                _ServicePack = GetServicePack();
            }

            return _ServicePack;
        }
    }

    private static string GetServicePack()
    {
        var osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

        if (GetVersionEx(ref osVersionInfo))
        {
            return osVersionInfo.szCSDVersion;
        }
        else
        {
            return String.Empty;
        }
    }


    #endregion SERVICE PACK

    #region Bits

    static public int Bits
    {
        get
        {
            return IntPtr.Size * 8;
        }
    }

    #endregion BITS

    #region P/Invoke

    #region GET
    #region PRODUCT INFO
    [DllImport("Kernel32.dll")]
    internal static extern bool GetProductInfo(
        int osMajorVersion,
        int osMinorVersion,
        int spMajorVersion,
        int spMinorVersion,
        out int edition);
    #endregion PRODUCT INFO

    #region VERSION
    [DllImport("kernel32.dll")]
    private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);
    #endregion VERSION
    #endregion GET

    #region OSVERSIONINFOEX
    [StructLayout(LayoutKind.Sequential)]
    private struct OSVERSIONINFOEX
    {
        public int dwOSVersionInfoSize;
        public int dwMajorVersion;
        public int dwMinorVersion;
        public int dwBuildNumber;
        public int dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
        public short wServicePackMajor;
        public short wServicePackMinor;
        public short wSuiteMask;
        public byte wProductType;
        public byte wReserved;
    }
    #endregion OSVERSIONINFOEX

    #region PRODUCT
    private const int PRODUCT_UNDEFINED = 0x00000000;
    private const int PRODUCT_ULTIMATE = 0x00000001;
    private const int PRODUCT_HOME_BASIC = 0x00000002;
    private const int PRODUCT_HOME_PREMIUM = 0x00000003;
    private const int PRODUCT_ENTERPRISE = 0x00000004;
    private const int PRODUCT_HOME_BASIC_N = 0x00000005;
    private const int PRODUCT_BUSINESS = 0x00000006;
    private const int PRODUCT_STANDARD_SERVER = 0x00000007;
    private const int PRODUCT_DATACENTER_SERVER = 0x00000008;
    private const int PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
    private const int PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
    private const int PRODUCT_STARTER = 0x0000000B;
    private const int PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
    private const int PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
    private const int PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
    private const int PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
    private const int PRODUCT_BUSINESS_N = 0x00000010;
    private const int PRODUCT_WEB_SERVER = 0x00000011;
    private const int PRODUCT_CLUSTER_SERVER = 0x00000012;
    private const int PRODUCT_HOME_SERVER = 0x00000013;
    private const int PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
    private const int PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
    private const int PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
    private const int PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
    private const int PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
    private const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019;
    private const int PRODUCT_HOME_PREMIUM_N = 0x0000001A;
    private const int PRODUCT_ENTERPRISE_N = 0x0000001B;
    private const int PRODUCT_ULTIMATE_N = 0x0000001C;
    private const int PRODUCT_WEB_SERVER_CORE = 0x0000001D;
    private const int PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
    private const int PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
    private const int PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
    private const int PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
    private const int PRODUCT_STANDARD_SERVER_V = 0x00000024;
    private const int PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;
    private const int PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
    private const int PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
    private const int PRODUCT_HYPERV = 0x0000002A;
    #endregion PRODUCT

    #region VERSIONS
    private const int VER_NT_WORKSTATION = 1;
    private const int VER_NT_DOMAIN_CONTROLLER = 2;
    private const int VER_NT_SERVER = 3;
    private const int VER_SUITE_SMALLBUSINESS = 1;
    private const int VER_SUITE_ENTERPRISE = 2;
    private const int VER_SUITE_TERMINAL = 16;
    private const int VER_SUITE_DATACENTER = 128;
    private const int VER_SUITE_SINGLEUSERTS = 256;
    private const int VER_SUITE_PERSONAL = 512;
    private const int VER_SUITE_BLADE = 1024;
    #endregion VERSIONS

    #endregion

}
