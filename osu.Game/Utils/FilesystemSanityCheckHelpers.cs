// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;

namespace osu.Game.Utils
{
    public static class FilesystemSanityCheckHelpers
    {
        /// <summary>
        /// Returns whether <paramref name="path"/> is potentially susceptible to path traversal style attacks.
        /// </summary>
        public static bool IncursPathTraversalRisk(string path)
            => path.Contains("../", StringComparison.Ordinal) || path.Contains("..\\", StringComparison.Ordinal) || Path.IsPathRooted(path);

        /// <summary>
        /// Returns whether <paramref name="child"/> is a subdirectory (direct or nested) of <paramref name="parent"/>.
        /// </summary>
        public static bool IsSubDirectory(string parent, string child)
        {
            // `Path.GetFullPath()` invocations are required to fully resolve the paths to unambiguous downwards-traversal-only paths.
            var parentInfo = new DirectoryInfo(Path.GetFullPath(parent));
            var childInfo = new DirectoryInfo(Path.GetFullPath(child));

            while (childInfo != null)
            {
                if (parentInfo.FullName == childInfo.FullName)
                    return true;

                childInfo = childInfo.Parent;
            }

            return false;
        }
    }
}
