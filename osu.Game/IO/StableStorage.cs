// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Platform;

namespace osu.Game.IO
{
    /// <summary>
    /// A storage pointing to an osu-stable installation.
    /// Provides methods for handling installations with a custom Song folder location.
    /// </summary>
    public class StableStorage : DesktopStorage
    {
        private const string stable_default_songs_path = "Songs";

        private readonly DesktopGameHost host;
        private readonly Lazy<string> songsPath;

        public StableStorage(string path, DesktopGameHost host)
            : base(path, host)
        {
            this.host = host;
            songsPath = new Lazy<string>(locateSongsDirectory);
        }

        /// <summary>
        /// Returns a <see cref="Storage"/> pointing to the osu-stable Songs directory.
        /// </summary>
        public Storage GetSongStorage() => new DesktopStorage(songsPath.Value, host);

        private string locateSongsDirectory()
        {
            var songsDirectoryPath = Path.Combine(BasePath, stable_default_songs_path);

            var configFile = GetFiles(".", "osu!.*.cfg").SingleOrDefault();

            if (configFile == null)
                return songsDirectoryPath;

            using (var stream = GetStream(configFile))
            using (var textReader = new StreamReader(stream))
            {
                string line;

                while ((line = textReader.ReadLine()) != null)
                {
                    if (line.StartsWith("BeatmapDirectory", StringComparison.OrdinalIgnoreCase))
                    {
                        var directory = line.Split('=')[1].TrimStart();
                        if (Path.IsPathFullyQualified(directory))
                            songsDirectoryPath = directory;

                        break;
                    }
                }
            }

            return songsDirectoryPath;
        }
    }
}
