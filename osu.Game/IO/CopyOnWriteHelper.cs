// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace osu.Game.IO
{
    internal static class CopyOnWriteHelper
    {
        public static bool CheckAvailability(string testDestinationPath, string testSourcePath)
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 26100))
            {
                // Starting from Windows 11 24H2, the CopyFile syscall will perform CoW cloning on ReFS drives.

                const string test_filename = "_cow_test";

                testDestinationPath = Path.Combine(testDestinationPath, test_filename);
                testSourcePath = Path.Combine(testSourcePath, test_filename);

                try
                {
                    using var sourceFileHandle = File.OpenHandle(testSourcePath, FileMode.CreateNew, FileAccess.Write, FileShare.Write, FileOptions.DeleteOnClose);
                    using var destinationFileHandle = File.OpenHandle(testDestinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.Write, FileOptions.DeleteOnClose);

                    // Attention: Windows supports mounting volume into folders. Don't detect volumn from the volume letter of path.
                    if (!GetVolumeInformationByHandle(sourceFileHandle, IntPtr.Zero, 0, out uint sourceVolume, out _, out uint flags, IntPtr.Zero, 0))
                        return false;

                    if (!GetVolumeInformationByHandle(destinationFileHandle, IntPtr.Zero, 0, out uint destinationVolume, out _, out _, IntPtr.Zero, 0))
                        return false;

                    if (sourceVolume != destinationVolume)
                        return false;

                    return (flags & FILE_SUPPORTS_BLOCK_REFCOUNTING) != 0;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public static bool TryCloneFile(string destinationPath, string sourcePath)
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 26100))
            {
                // The CopyFile syscall will do all the cloning stuff.
                File.Copy(sourcePath, destinationPath);
                return true;
            }

            return false;
        }

        #region Windows native methods

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

        #endregion
    }
}
