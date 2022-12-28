// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;
using osu.Framework;

namespace osu.Game.IO
{
    internal static class HardLinkHelper
    {
        public static bool CheckAvailability(string testDestinationPath, string testSourcePath)
        {
            // We can support other operating systems quite easily in the future.
            // Let's handle the most common one for now, though.
            if (RuntimeInfo.OS != RuntimeInfo.Platform.Windows)
                return false;

            const string test_filename = "_hard_link_test";

            testDestinationPath = Path.Combine(testDestinationPath, test_filename);
            testSourcePath = Path.Combine(testSourcePath, test_filename);

            cleanupFiles();

            try
            {
                File.WriteAllText(testSourcePath, string.Empty);

                // Test availability by creating an arbitrary hard link between the source and destination paths.
                return CreateHardLink(testDestinationPath, testSourcePath, IntPtr.Zero);
            }
            catch
            {
                return false;
            }
            finally
            {
                cleanupFiles();
            }

            void cleanupFiles()
            {
                try
                {
                    File.Delete(testDestinationPath);
                    File.Delete(testSourcePath);
                }
                catch
                {
                }
            }
        }

        // For future use (to detect if a file is a hard link with other references existing on disk).
        public static int GetFileLinkCount(string filePath)
        {
            int result = 0;
            SafeFileHandle handle = CreateFile(filePath, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Archive, IntPtr.Zero);

            ByHandleFileInformation fileInfo;

            if (GetFileInformationByHandle(handle, out fileInfo))
                result = (int)fileInfo.NumberOfLinks;
            CloseHandle(handle);

            return result;
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out ByHandleFileInformation lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(SafeHandle hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct ByHandleFileInformation
        {
            public readonly uint FileAttributes;
            public readonly FILETIME CreationTime;
            public readonly FILETIME LastAccessTime;
            public readonly FILETIME LastWriteTime;
            public readonly uint VolumeSerialNumber;
            public readonly uint FileSizeHigh;
            public readonly uint FileSizeLow;
            public readonly uint NumberOfLinks;
            public readonly uint FileIndexHigh;
            public readonly uint FileIndexLow;
        }
    }
}
