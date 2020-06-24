// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Framework.Platform;

namespace osu.Game.IO
{
    /// <summary>
    /// A <see cref="WrappedStorage"/> that is migratable to different locations.
    /// </summary>
    public abstract class MigratableStorage : WrappedStorage
    {
        public virtual string[] IgnoreDirectories => Array.Empty<string>();
        public virtual string[] IgnoreFiles => Array.Empty<string>();

        protected MigratableStorage(Storage storage, string subPath = null)
            : base(storage, subPath)
        {
        }

        public abstract void Migrate(string newLocation);

        protected void DeleteRecursive(DirectoryInfo target, bool topLevelExcludes = true)
        {
            foreach (System.IO.FileInfo fi in target.GetFiles())
            {
                if (topLevelExcludes && IgnoreFiles.Contains(fi.Name))
                    continue;

                AttemptOperation(() => fi.Delete());
            }

            foreach (DirectoryInfo dir in target.GetDirectories())
            {
                if (topLevelExcludes && IgnoreDirectories.Contains(dir.Name))
                    continue;

                AttemptOperation(() => dir.Delete(true));
            }

            if (target.GetFiles().Length == 0 && target.GetDirectories().Length == 0)
                AttemptOperation(target.Delete);
        }

        protected void CopyRecursive(DirectoryInfo source, DirectoryInfo destination, bool topLevelExcludes = true)
        {
            // based off example code https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo
            if (!destination.Exists)
                Directory.CreateDirectory(destination.FullName);

            foreach (System.IO.FileInfo fi in source.GetFiles())
            {
                if (topLevelExcludes && IgnoreFiles.Contains(fi.Name))
                    continue;

                AttemptOperation(() => fi.CopyTo(Path.Combine(destination.FullName, fi.Name), true));
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (topLevelExcludes && IgnoreDirectories.Contains(dir.Name))
                    continue;

                CopyRecursive(dir, destination.CreateSubdirectory(dir.Name), false);
            }
        }

        /// <summary>
        /// Attempt an IO operation multiple times and only throw if none of the attempts succeed.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="attempts">The number of attempts (250ms wait between each).</param>
        protected static void AttemptOperation(Action action, int attempts = 10)
        {
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception)
                {
                    if (attempts-- == 0)
                        throw;
                }

                Thread.Sleep(250);
            }
        }
    }
}
