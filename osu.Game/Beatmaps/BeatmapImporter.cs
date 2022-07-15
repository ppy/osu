// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using Realms;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/WorkingBeatmaps.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class BeatmapImporter : RealmArchiveModelImporter<BeatmapSetInfo>
    {
        public override IEnumerable<string> HandledExtensions => new[] { ".osz" };

        protected override string[] HashableFileTypes => new[] { ".osu" };

        public Action<BeatmapSetInfo>? ProcessBeatmap { private get; set; }

        public BeatmapImporter(Storage storage, RealmAccess realm)
            : base(storage, realm)
        {
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path).ToLowerInvariant() == ".osz";

        protected override void Populate(BeatmapSetInfo beatmapSet, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default)
        {
            if (archive != null)
                beatmapSet.Beatmaps.AddRange(createBeatmapDifficulties(beatmapSet, realm));

            foreach (BeatmapInfo b in beatmapSet.Beatmaps)
            {
                b.BeatmapSet = beatmapSet;

                // ensure we aren't trying to add a new ruleset to the database
                // this can happen in tests, mostly
                if (!b.Ruleset.IsManaged)
                    b.Ruleset = realm.Find<RulesetInfo>(b.Ruleset.ShortName) ?? throw new ArgumentNullException(nameof(b.Ruleset));
            }

            validateOnlineIds(beatmapSet, realm);

            bool hadOnlineIDs = beatmapSet.Beatmaps.Any(b => b.OnlineID > 0);

            // TODO: this may no longer be valid as we aren't doing an online population at this point.
            // ensure at least one beatmap was able to retrieve or keep an online ID, else drop the set ID.
            if (hadOnlineIDs && !beatmapSet.Beatmaps.Any(b => b.OnlineID > 0))
            {
                if (beatmapSet.OnlineID > 0)
                {
                    beatmapSet.OnlineID = -1;
                    LogForModel(beatmapSet, "Disassociating beatmap set ID due to loss of all beatmap IDs");
                }
            }
        }

        protected override void PreImport(BeatmapSetInfo beatmapSet, Realm realm)
        {
            // We are about to import a new beatmap. Before doing so, ensure that no other set shares the online IDs used by the new one.
            // Note that this means if the previous beatmap is restored by the user, it will no longer be linked to its online IDs.
            // If this is ever an issue, we can consider marking as pending delete but not resetting the IDs (but care will be required for
            // beatmaps, which don't have their own `DeletePending` state).

            if (beatmapSet.OnlineID > 0)
            {
                // OnlineID should really be unique, but to avoid catastrophic failure let's iterate just to be sure.
                foreach (var existingSetWithSameOnlineID in realm.All<BeatmapSetInfo>().Where(b => b.OnlineID == beatmapSet.OnlineID))
                {
                    existingSetWithSameOnlineID.DeletePending = true;
                    existingSetWithSameOnlineID.OnlineID = -1;

                    foreach (var b in existingSetWithSameOnlineID.Beatmaps)
                        b.OnlineID = -1;

                    LogForModel(beatmapSet, $"Found existing beatmap set with same OnlineID ({beatmapSet.OnlineID}). It will be disassociated and marked for deletion.");
                }
            }
        }

        protected override void PostImport(BeatmapSetInfo model, Realm realm)
        {
            base.PostImport(model, realm);

            ProcessBeatmap?.Invoke(model);
        }

        private void validateOnlineIds(BeatmapSetInfo beatmapSet, Realm realm)
        {
            var beatmapIds = beatmapSet.Beatmaps.Where(b => b.OnlineID > 0).Select(b => b.OnlineID).ToList();

            // ensure all IDs are unique
            if (beatmapIds.GroupBy(b => b).Any(g => g.Count() > 1))
            {
                LogForModel(beatmapSet, "Found non-unique IDs, resetting...");
                resetIds();
                return;
            }

            // find any existing beatmaps in the database that have matching online ids
            List<BeatmapInfo> existingBeatmaps = new List<BeatmapInfo>();

            foreach (int id in beatmapIds)
                existingBeatmaps.AddRange(realm.All<BeatmapInfo>().Where(b => b.OnlineID == id));

            if (existingBeatmaps.Any())
            {
                // reset the import ids (to force a re-fetch) *unless* they match the candidate CheckForExisting set.
                // we can ignore the case where the new ids are contained by the CheckForExisting set as it will either be used (import skipped) or deleted.

                var existing = CheckForExisting(beatmapSet, realm);

                if (existing == null || existingBeatmaps.Any(b => !existing.Beatmaps.Contains(b)))
                {
                    LogForModel(beatmapSet, "Found existing import with online IDs already, resetting...");
                    resetIds();
                }
            }

            void resetIds() => beatmapSet.Beatmaps.ForEach(b => b.OnlineID = -1);
        }

        protected override bool CanSkipImport(BeatmapSetInfo existing, BeatmapSetInfo import)
        {
            if (!base.CanSkipImport(existing, import))
                return false;

            return existing.Beatmaps.Any(b => b.OnlineID > 0);
        }

        protected override bool CanReuseExisting(BeatmapSetInfo existing, BeatmapSetInfo import)
        {
            if (!base.CanReuseExisting(existing, import))
                return false;

            var existingIds = existing.Beatmaps.Select(b => b.OnlineID).OrderBy(i => i);
            var importIds = import.Beatmaps.Select(b => b.OnlineID).OrderBy(i => i);

            // force re-import if we are not in a sane state.
            return existing.OnlineID == import.OnlineID && existingIds.SequenceEqual(importIds);
        }

        protected override void UndeleteForReuse(BeatmapSetInfo existing)
        {
            base.UndeleteForReuse(existing);
            existing.DateAdded = DateTimeOffset.UtcNow;
        }

        public override string HumanisedModelName => "beatmap";

        protected override BeatmapSetInfo? CreateModel(ArchiveReader reader)
        {
            // let's make sure there are actually .osu files to import.
            string? mapName = reader.Filenames.FirstOrDefault(f => f.EndsWith(".osu", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(mapName))
            {
                Logger.Log($"No beatmap files found in the beatmap archive ({reader.Name}).", LoggingTarget.Database);
                return null;
            }

            Beatmap beatmap;

            using (var stream = new LineBufferedReader(reader.GetStream(mapName)))
            {
                if (stream.PeekLine() == null)
                {
                    Logger.Log($"No content found in first .osu file of beatmap archive ({reader.Name} / {mapName})", LoggingTarget.Database);
                    return null;
                }

                beatmap = Decoder.GetDecoder<Beatmap>(stream).Decode(stream);
            }

            return new BeatmapSetInfo
            {
                OnlineID = beatmap.BeatmapInfo.BeatmapSet?.OnlineID ?? -1,
                // Metadata = beatmap.Metadata,
                DateAdded = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Create all required <see cref="BeatmapInfo"/>s for the provided archive.
        /// </summary>
        private List<BeatmapInfo> createBeatmapDifficulties(BeatmapSetInfo beatmapSet, Realm realm)
        {
            var beatmaps = new List<BeatmapInfo>();

            foreach (var file in beatmapSet.Files.Where(f => f.Filename.EndsWith(".osu", StringComparison.OrdinalIgnoreCase)))
            {
                using (var memoryStream = new MemoryStream(Files.Store.Get(file.File.GetStoragePath()))) // we need a memory stream so we can seek
                {
                    IBeatmap decoded;

                    using (var lineReader = new LineBufferedReader(memoryStream, true))
                    {
                        if (lineReader.PeekLine() == null)
                        {
                            LogForModel(beatmapSet, $"No content found in beatmap file {file.Filename}.");
                            continue;
                        }

                        decoded = Decoder.GetDecoder<Beatmap>(lineReader).Decode(lineReader);
                    }

                    string hash = memoryStream.ComputeSHA2Hash();

                    if (beatmaps.Any(b => b.Hash == hash))
                    {
                        LogForModel(beatmapSet, $"Skipping import of {file.Filename} due to duplicate file content.");
                        continue;
                    }

                    var decodedInfo = decoded.BeatmapInfo;
                    var decodedDifficulty = decodedInfo.Difficulty;

                    var ruleset = realm.All<RulesetInfo>().FirstOrDefault(r => r.OnlineID == decodedInfo.Ruleset.OnlineID);

                    if (ruleset?.Available != true)
                    {
                        LogForModel(beatmapSet, $"Skipping import of {file.Filename} due to missing local ruleset {decodedInfo.Ruleset.OnlineID}.");
                        continue;
                    }

                    var difficulty = new BeatmapDifficulty
                    {
                        DrainRate = decodedDifficulty.DrainRate,
                        CircleSize = decodedDifficulty.CircleSize,
                        OverallDifficulty = decodedDifficulty.OverallDifficulty,
                        ApproachRate = decodedDifficulty.ApproachRate,
                        SliderMultiplier = decodedDifficulty.SliderMultiplier,
                        SliderTickRate = decodedDifficulty.SliderTickRate,
                    };

                    var metadata = new BeatmapMetadata
                    {
                        Title = decoded.Metadata.Title,
                        TitleUnicode = decoded.Metadata.TitleUnicode,
                        Artist = decoded.Metadata.Artist,
                        ArtistUnicode = decoded.Metadata.ArtistUnicode,
                        Author =
                        {
                            OnlineID = decoded.Metadata.Author.OnlineID,
                            Username = decoded.Metadata.Author.Username
                        },
                        Source = decoded.Metadata.Source,
                        Tags = decoded.Metadata.Tags,
                        PreviewTime = decoded.Metadata.PreviewTime,
                        AudioFile = decoded.Metadata.AudioFile,
                        BackgroundFile = decoded.Metadata.BackgroundFile,
                    };

                    var beatmap = new BeatmapInfo(ruleset, difficulty, metadata)
                    {
                        Hash = hash,
                        DifficultyName = decodedInfo.DifficultyName,
                        OnlineID = decodedInfo.OnlineID,
                        AudioLeadIn = decodedInfo.AudioLeadIn,
                        StackLeniency = decodedInfo.StackLeniency,
                        SpecialStyle = decodedInfo.SpecialStyle,
                        LetterboxInBreaks = decodedInfo.LetterboxInBreaks,
                        WidescreenStoryboard = decodedInfo.WidescreenStoryboard,
                        EpilepsyWarning = decodedInfo.EpilepsyWarning,
                        SamplesMatchPlaybackRate = decodedInfo.SamplesMatchPlaybackRate,
                        DistanceSpacing = decodedInfo.DistanceSpacing,
                        BeatDivisor = decodedInfo.BeatDivisor,
                        GridSize = decodedInfo.GridSize,
                        TimelineZoom = decodedInfo.TimelineZoom,
                        MD5Hash = memoryStream.ComputeMD5Hash(),
                    };

                    beatmaps.Add(beatmap);
                }
            }

            return beatmaps;
        }
    }
}
