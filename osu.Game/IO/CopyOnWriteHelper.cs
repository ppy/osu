// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;

namespace osu.Game.IO
{
    internal static class CopyOnWriteHelper
    {
        public static bool CheckAvailability(string testDestinationPath, string testSourcePath)
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 26100))
            {
                // Starting from Windows 11 24H2, the CopyFile syscall will perform CoW cloning on ReFS drives.

                // Attention: Windows supports mounting volume into folders. Don't detect volumn from the volume letter of path.
                if (!HardLinkHelper.GetVolumeInformationForDirectory(testSourcePath, out uint sourceVolume, out uint flags))
                    return false;

                if (!HardLinkHelper.GetVolumeInformationForDirectory(testDestinationPath, out uint destinationVolume, out _))
                    return false;

                if (sourceVolume != destinationVolume)
                    return false;

                return (flags & HardLinkHelper.FILE_SUPPORTS_HARD_LINKS) != 0;
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
    }
}
