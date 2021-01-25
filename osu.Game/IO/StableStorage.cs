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
        private const string stable_songs_path = "Songs";

        private readonly DesktopGameHost host;
        private readonly string songsPath;

        public StableStorage(string path, DesktopGameHost host)
            : base(path, host)
        {
            this.host = host;
            songsPath = locateSongsDirectory();
        }

        /// <summary>
        /// Returns a <see cref="Storage"/> pointing to the osu-stable Songs directory.
        /// </summary>
        public Storage GetSongStorage() => new DesktopStorage(songsPath, host);

        private string locateSongsDirectory()
        {
            var configFile = GetStream(GetFiles(".", "osu!.*.cfg").First());
            var textReader = new StreamReader(configFile);

            var songsDirectoryPath = Path.Combine(BasePath, stable_songs_path);

            while (!textReader.EndOfStream)
            {
                var line = textReader.ReadLine();

                if (line?.StartsWith("BeatmapDirectory", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var directory = line.Split('=')[1].TrimStart();
                    if (Path.IsPathFullyQualified(directory))
                        songsDirectoryPath = directory;

                    break;
                }
            }

            return songsDirectoryPath;
        }
    }
}
