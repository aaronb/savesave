using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace savesave
{
    class Link
    {

        class Native
        {

            public enum SYMBOLIC_LINK_FLAG
            {
                FILE = 0,
                DIRECTORY = 1
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct WIN32_FIND_DATA
            {
                public uint dwFileAttributes;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
                public uint nFileSizeHigh;
                public uint nFileSizeLow;

                public uint dwReserved0;
                public uint dwReserved1;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string cFileName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
                public string cAlternateFileName;
            }

            public enum ReparseTagType : uint
            {
                IO_REPARSE_TAG_MOUNT_POINT = (0xA0000003),
                IO_REPARSE_TAG_HSM = (0xC0000004),
                IO_REPARSE_TAG_SIS = (0x80000007),
                IO_REPARSE_TAG_DFS = (0x8000000A),
                IO_REPARSE_TAG_SYMLINK = (0xA000000C),
                IO_REPARSE_TAG_DFSR = (0x80000012),
            }

            public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool CreateSymbolicLink(string SymlinkFileName, string TargetFileName, SYMBOLIC_LINK_FLAG Flags);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FindClose(IntPtr hChangeHandle);
        }

        public static void CreateSymbolicLink(string path, string target)
        {
            Native.SYMBOLIC_LINK_FLAG flags;
            var attr = File.GetAttributes(target);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                flags = Native.SYMBOLIC_LINK_FLAG.DIRECTORY;
            } else {
                flags = Native.SYMBOLIC_LINK_FLAG.FILE;
            }

            bool ok = Native.CreateSymbolicLink(path, target, flags);

            if (!ok) {
                throw new System.ComponentModel.Win32Exception();
            }
        }

        public static bool IsSymbolicLink(string path)
        {
            // https://msdn.microsoft.com/en-us/library/aa363940(v=VS.85).aspx

            var attr = File.GetAttributes(path);
            if ((attr & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint) {
                return false;
            }

            Native.WIN32_FIND_DATA filedata;
            IntPtr handle = Native.FindFirstFile(path, out filedata);
            if (handle == Native.INVALID_HANDLE_VALUE) {
                throw new System.ComponentModel.Win32Exception();
            }

            bool ok = Native.FindClose(handle);
            if (!ok) {
                throw new System.ComponentModel.Win32Exception();
            }

            var reparse_tag = (Native.ReparseTagType)filedata.dwReserved0;
            if (reparse_tag != Native.ReparseTagType.IO_REPARSE_TAG_SYMLINK) {
                return false;
            }

            return true;
        }
    }
}
