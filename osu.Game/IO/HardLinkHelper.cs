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

            if (OperatingSystem.IsWindows())
            {
                // Attention: Windows supports mounting volume into folders. Don't detect volume from the volume letter of path.
                if (!GetVolumeInformationForDirectory(testSourcePath, out uint sourceVolume, out uint flags))
                    return false;

                if (!GetVolumeInformationForDirectory(testDestinationPath, out uint destinationVolume, out _))
                    return false;

                if (sourceVolume != destinationVolume)
                    return false;

                return (flags & FILE_SUPPORTS_HARD_LINKS) != 0;
            }

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
                    using (SafeFileHandle handle = File.OpenHandle(filePath))
                    {
                        if (GetFileInformationByHandle(handle, out var fileInfo))
                            result = (int)fileInfo.NumberOfLinks;
                    }

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

        // https://learn.microsoft.com/windows/win32/fileio/obtaining-a-handle-to-a-directory
        // The flag is required when opening directory as a handle. It's not accepted by File.OpenHandle.
        public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            FileMode dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        public const uint FILE_SUPPORTS_HARD_LINKS = 0x00400000;
        public const uint FILE_SUPPORTS_BLOCK_REFCOUNTING = 0x08000000;

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool GetVolumeInformationByHandle(
            SafeFileHandle hFile,
            IntPtr lpVolumeNameBuffer,
            uint nVolumeNameSize,
            out uint lpVolumeSerialNumber,
            out uint lpMaximumComponentLength,
            out uint lpFileSystemFlags,
            IntPtr lpFileSystemNameBuffer,
            uint nFileSystemNameSize);

        public static bool GetVolumeInformationForDirectory(string directoryPath, out uint volumeSerialNumber, out uint fileSystemFlags)
        {
            using var handle = CreateFile(directoryPath, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, (FileAttributes)FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);

            if (handle.IsInvalid)
            {
                volumeSerialNumber = 0;
                fileSystemFlags = 0;
                return false;
            }

            return GetVolumeInformationByHandle(handle, IntPtr.Zero, 0, out volumeSerialNumber, out _, out fileSystemFlags, IntPtr.Zero, 0);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out ByHandleFileInformation lpFileInformation);

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
