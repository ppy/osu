// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading.Tasks;
using osu.Framework.Logging;

namespace osu.Game.IO
{
    public static class DiskUsage
    {
        /// <summary>
        /// 500 MiB
        /// </summary>
        private const long required_space_default = 512L * 1024L * 1024L;

        /// <summary>
        /// Checks if the available free space on the drive containing the path is sufficient for normal operation.
        /// This method is blocking, and <see cref="EnsureSufficientSpaceAsync"/> should be preferred in IO-bound scenarios.
        /// </summary>
        /// <param name="checkPath">A path to a file or directory in which the drive's disk space should be checked.</param>
        /// <param name="requiredSpace">The amount of space to ensure is available in bytes. Defaults to <see cref="required_space_default"/>.</param>
        public static void EnsureSufficientSpace(string checkPath, long requiredSpace = required_space_default)
        {
            if (!Directory.Exists(checkPath))
                throw new DirectoryNotFoundException($"The directory '{checkPath}' does not exist or could not be found.");

            string validPath = Path.GetFullPath(checkPath);

            if (string.IsNullOrEmpty(validPath))
                throw new IOException($"The directory '{checkPath}' is not a valid path.");

            var activeDriveInfo = new DriveInfo(validPath);

            long availableFreeSpace = activeDriveInfo.AvailableFreeSpace;

#if DEBUG
            Logger.Log($"Available disk space for ({validPath}): {availableFreeSpace / 1048576L} MiB");
#endif

            if (availableFreeSpace < requiredSpace)
                throw new IOException($"Insufficient disk space available! Required: {requiredSpace / 1048576L} MiB | Available: {availableFreeSpace / 1048576L} MiB");
        }

        public static Task EnsureSufficientSpaceAsync(string checkDirectory, long requiredSpace = required_space_default)
        {
            return Task.Run(() => EnsureSufficientSpace(checkDirectory, requiredSpace));
        }
    }
}
