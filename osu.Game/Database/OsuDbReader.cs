// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.IO.Legacy;

namespace osu.Game.Database
{
    /// <summary>
    /// Reads beatmap metadata from an osu!stable <c>osu!.db</c> file.
    /// See https://github.com/ppy/osu/wiki/Legacy-database-file-structure for the format specification.
    /// </summary>
    public static class OsuDbReader
    {
        private const string database_name = "osu!.db";

        /// <summary>
        /// Attempts to read the <c>osu!.db</c> file from the given stable storage and returns a mapping
        /// of beatmap folder name (relative to Songs) to the earliest "last modification time" among all
        /// beatmaps in that folder.  This is the closest approximation to "date added" available in the
        /// stable database.
        /// </summary>
        /// <param name="stableStorage">The root of the osu!stable installation.</param>
        /// <returns>
        /// A dictionary keyed by folder name (case-insensitive) with the corresponding date, or
        /// <see langword="null"/> if the database could not be read.
        /// </returns>
        public static Dictionary<string, DateTimeOffset>? ReadDateAddedByFolder(Storage stableStorage)
        {
            if (!stableStorage.Exists(database_name))
                return null;

            try
            {
                using var stream = stableStorage.GetStream(database_name);
                return ReadDateAddedByFolder(stream);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to read {database_name}: {e.Message}", LoggingTarget.Database, LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Reads a mapping of beatmap folder name to earliest "last modification time" from the given
        /// <c>osu!.db</c> stream.  Exposed for testing.
        /// </summary>
        public static Dictionary<string, DateTimeOffset>? ReadDateAddedByFolder(Stream stream)
        {
            try
            {
                return readDateAddedByFolder(stream);
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to parse {database_name}: {e.Message}", LoggingTarget.Database, LogLevel.Error);
                return null;
            }
        }

        private static Dictionary<string, DateTimeOffset> readDateAddedByFolder(Stream stream)
        {
            var result = new Dictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);

            using var sr = new SerializationReader(stream);

            int version = sr.ReadInt32();
            sr.ReadInt32(); // folder count
            sr.ReadBoolean(); // account unlocked
            sr.ReadDateTime(); // date account will be unlocked
            sr.ReadString(); // player name

            int beatmapCount = sr.ReadInt32();

            // Sanity check: prevent reading corrupted files with unreasonable beatmap counts.
            // A typical large collection has ~50k beatmaps; anything beyond 1 million is suspicious.
            if (beatmapCount < 0 || beatmapCount > 1_000_000)
            {
                Logger.Log($"Suspicious beatmap count ({beatmapCount}) in {database_name}, aborting parse.", LoggingTarget.Database, LogLevel.Error);
                return result;
            }

            for (int i = 0; i < beatmapCount; i++)
            {
                // Size in bytes of the beatmap entry – only present before version 20191106.
                if (version < 20191106)
                    sr.ReadInt32();

                sr.ReadString(); // artist name
                sr.ReadString(); // artist name unicode
                sr.ReadString(); // song title
                sr.ReadString(); // song title unicode
                sr.ReadString(); // creator name
                sr.ReadString(); // difficulty name
                sr.ReadString(); // audio file name
                sr.ReadString(); // MD5 hash
                sr.ReadString(); // .osu file name
                sr.ReadByte();   // ranked status
                sr.ReadInt16();  // hit circles
                sr.ReadInt16();  // sliders
                sr.ReadInt16();  // spinners

                long ticks = sr.ReadInt64();

                // Validate ticks to prevent ArgumentOutOfRangeException.
                // Corrupted or malformed osu!.db files may contain invalid values.
                DateTimeOffset? lastModified = null;

                if (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks)
                {
                    lastModified = new DateTimeOffset(new DateTime(ticks, DateTimeKind.Utc));
                }
                else
                {
                    Logger.Log($"Invalid ticks value ({ticks}) in {database_name}, skipping beatmap entry.", LoggingTarget.Database, LogLevel.Debug);
                }

                // Approach rate, circle size, HP drain, overall difficulty
                if (version < 20140609)
                {
                    sr.ReadByte();
                    sr.ReadByte();
                    sr.ReadByte();
                    sr.ReadByte();
                }
                else
                {
                    sr.ReadSingle();
                    sr.ReadSingle();
                    sr.ReadSingle();
                    sr.ReadSingle();
                }

                sr.ReadDouble(); // slider velocity

                if (version >= 20140609)
                {
                    // Star rating pairs for each ruleset (osu!, taiko, catch, mania)
                    for (int r = 0; r < 4; r++)
                    {
                        int pairCount = sr.ReadInt32();

                        // Sanity check: prevent reading corrupted data with unreasonable pair counts.
                        // If we encounter this, the stream is likely corrupted and we cannot safely continue.
                        if (pairCount < 0 || pairCount > 100_000)
                        {
                            Logger.Log($"Suspicious star rating pair count ({pairCount}) in {database_name}, aborting parse.", LoggingTarget.Database, LogLevel.Error);
                            return result;
                        }

                        for (int p = 0; p < pairCount; p++)
                        {
                            if (version >= 20250107)
                            {
                                // Int-Float pair: 0x08, Int, 0x0c, Float
                                sr.ReadByte();
                                sr.ReadInt32();
                                sr.ReadByte();
                                sr.ReadSingle();
                            }
                            else
                            {
                                // Int-Double pair: 0x08, Int, 0x0d, Double
                                sr.ReadByte();
                                sr.ReadInt32();
                                sr.ReadByte();
                                sr.ReadDouble();
                            }
                        }
                    }
                }

                sr.ReadInt32(); // drain time
                sr.ReadInt32(); // total time
                sr.ReadInt32(); // preview time

                int timingPointCount = sr.ReadInt32();

                // Sanity check: prevent reading corrupted data with unreasonable timing point counts.
                // If we encounter this, the stream is likely corrupted and we cannot safely continue.
                if (timingPointCount < 0 || timingPointCount > 100_000)
                {
                    Logger.Log($"Suspicious timing point count ({timingPointCount}) in {database_name}, aborting parse.", LoggingTarget.Database, LogLevel.Error);
                    return result;
                }

                for (int t = 0; t < timingPointCount; t++)
                {
                    sr.ReadDouble(); // BPM
                    sr.ReadDouble(); // offset
                    sr.ReadBoolean(); // inherited
                }

                sr.ReadInt32(); // difficulty ID
                sr.ReadInt32(); // beatmap ID
                sr.ReadInt32(); // thread ID
                sr.ReadByte();  // grade osu!
                sr.ReadByte();  // grade taiko
                sr.ReadByte();  // grade catch
                sr.ReadByte();  // grade mania
                sr.ReadInt16(); // local offset
                sr.ReadSingle(); // stack leniency
                sr.ReadByte();  // gameplay mode
                sr.ReadString(); // song source
                sr.ReadString(); // song tags
                sr.ReadInt16(); // online offset
                sr.ReadString(); // title font
                sr.ReadBoolean(); // unplayed
                sr.ReadInt64(); // last played time
                sr.ReadBoolean(); // is osz2

                string folderName = sr.ReadString() ?? string.Empty;

                sr.ReadInt64(); // last checked against repo
                sr.ReadBoolean(); // ignore beatmap sound
                sr.ReadBoolean(); // ignore beatmap skin
                sr.ReadBoolean(); // disable storyboard
                sr.ReadBoolean(); // disable video
                sr.ReadBoolean(); // visual override

                if (version < 20140609)
                    sr.ReadInt16(); // unknown

                sr.ReadInt32(); // last modification time (duplicate)
                sr.ReadByte();  // mania scroll speed

                if (string.IsNullOrEmpty(folderName) || !lastModified.HasValue)
                    continue;

                // Keep the earliest modification time across all difficulties in the same folder,
                // as that best represents when the set was first added.
                if (!result.TryGetValue(folderName, out var existing) || lastModified.Value < existing)
                    result[folderName] = lastModified.Value;
            }

            return result;
        }
    }
}
