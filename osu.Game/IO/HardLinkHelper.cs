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
            // For simplicity, only support desktop operating systems for now.
            if (!RuntimeInfo.IsDesktop)
                return false;

            const string test_filename = "_hard_link_test";

            testDestinationPath = Path.Combine(testDestinationPath, test_filename);
            testSourcePath = Path.Combine(testSourcePath, test_filename);

            cleanupFiles();

            try
            {
                File.WriteAllText(testSourcePath, string.Empty);

                // Test availability by creating an arbitrary hard link between the source and destination paths.
                return TryCreateHardLink(testDestinationPath, testSourcePath);
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

        /// <summary>
        /// Attempts to create a hard link from <paramref name="sourcePath"/> to <paramref name="destinationPath"/>,
        /// using platform-specific native methods.
        /// </summary>
        /// <remarks>
        /// Hard links are only available on desktop platforms.
        /// </remarks>
        /// <returns>Whether the hard link was successfully created.</returns>
        public static bool TryCreateHardLink(string destinationPath, string sourcePath)
        {
            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    return CreateHardLink(destinationPath, sourcePath, IntPtr.Zero);

                case RuntimeInfo.Platform.Linux:
                case RuntimeInfo.Platform.macOS:
                    return link(sourcePath, destinationPath) == 0;

                default:
                    return false;
            }
        }

        // For future use (to detect if a file is a hard link with other references existing on disk).
        public static int GetFileLinkCount(string filePath)
        {
            int result = 0;

            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    SafeFileHandle handle = CreateFile(filePath, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Archive, IntPtr.Zero);

                    ByHandleFileInformation fileInfo;

                    if (GetFileInformationByHandle(handle, out fileInfo))
                        result = (int)fileInfo.NumberOfLinks;
                    CloseHandle(handle);
                    break;

                case RuntimeInfo.Platform.Linux:
                case RuntimeInfo.Platform.macOS:
                    if (stat(filePath, out var statbuf) == 0)
                        result = (int)statbuf.st_nlink;

                    break;
            }

            return result;
        }

        #region Windows native methods

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

        #endregion

        #region Linux native methods

#pragma warning disable IDE1006 // Naming rule violation

        [DllImport("libc", SetLastError = true)]
        public static extern int link(string oldpath, string newpath);

        [DllImport("libc", SetLastError = true)]
        private static extern int stat(string pathname, out Stat statbuf);

        // ReSharper disable once InconsistentNaming
        // Struct layout is likely non-portable across unices. Tread with caution.
        [StructLayout(LayoutKind.Sequential)]
        private struct Stat
        {
            public readonly long st_dev;
            public readonly long st_ino;
            public readonly long st_nlink;
            public readonly int st_mode;
            public readonly int st_uid;
            public readonly int st_gid;
            public readonly long st_rdev;
            public readonly long st_size;
            public readonly long st_blksize;
            public readonly long st_blocks;
            public readonly Timespec st_atim;
            public readonly Timespec st_mtim;
            public readonly Timespec st_ctim;
        }

        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        private struct Timespec
        {
            public readonly long tv_sec;
            public readonly long tv_nsec;
        }

#pragma warning restore IDE1006

        #endregion
    }
}
