// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Skinning;
using Realms;

#nullable enable

namespace osu.Game.Stores
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/WorkingBeatmaps.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class BeatmapImporter : RealmArchiveModelImporter<RealmBeatmapSet>, IDisposable
    {
        public override IEnumerable<string> HandledExtensions => new[] { ".osz" };

        protected override string[] HashableFileTypes => new[] { ".osu" };

        // protected override bool CheckLocalAvailability(RealmBeatmapSet model, System.Linq.IQueryable<RealmBeatmapSet> items)
        //     => base.CheckLocalAvailability(model, items) || (model.OnlineID > -1));

        private readonly BeatmapOnlineLookupQueue? onlineLookupQueue;

        public BeatmapImporter(RealmContextFactory contextFactory, Storage storage, BeatmapOnlineLookupQueue? onlineLookupQueue = null)
            : base(storage, contextFactory)
        {
            this.onlineLookupQueue = onlineLookupQueue;
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path).ToLowerInvariant() == ".osz";

        protected override Task Populate(RealmBeatmapSet beatmapSet, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default)
        {
            if (archive != null)
                beatmapSet.Beatmaps.AddRange(createBeatmapDifficulties(beatmapSet.Files, realm));

            foreach (RealmBeatmap b in beatmapSet.Beatmaps)
                b.BeatmapSet = beatmapSet;

            validateOnlineIds(beatmapSet, realm);

            bool hadOnlineIDs = beatmapSet.Beatmaps.Any(b => b.OnlineID > 0);

            if (onlineLookupQueue != null)
            {
                // TODO: this required `BeatmapOnlineLookupQueue` to somehow support new types.
                // await onlineLookupQueue.UpdateAsync(beatmapSet, cancellationToken).ConfigureAwait(false);
            }

            // ensure at least one beatmap was able to retrieve or keep an online ID, else drop the set ID.
            if (hadOnlineIDs && !beatmapSet.Beatmaps.Any(b => b.OnlineID > 0))
            {
                if (beatmapSet.OnlineID > 0)
                {
                    beatmapSet.OnlineID = -1;
                    LogForModel(beatmapSet, "Disassociating beatmap set ID due to loss of all beatmap IDs");
                }
            }

            return Task.CompletedTask;
        }

        protected override void PreImport(RealmBeatmapSet beatmapSet, Realm realm)
        {
            // We are about to import a new beatmap. Before doing so, ensure that no other set shares the online IDs used by the new one.
            // Note that this means if the previous beatmap is restored by the user, it will no longer be linked to its online IDs.
            // If this is ever an issue, we can consider marking as pending delete but not resetting the IDs (but care will be required for
            // beatmaps, which don't have their own `DeletePending` state).

            if (beatmapSet.OnlineID > 0)
            {
                var existingSetWithSameOnlineID = realm.All<RealmBeatmapSet>().SingleOrDefault(b => b.OnlineID == beatmapSet.OnlineID);

                if (existingSetWithSameOnlineID != null)
                {
                    existingSetWithSameOnlineID.DeletePending = true;
                    existingSetWithSameOnlineID.OnlineID = -1;

                    foreach (var b in existingSetWithSameOnlineID.Beatmaps)
                        b.OnlineID = -1;

                    LogForModel(beatmapSet, $"Found existing beatmap set with same OnlineID ({beatmapSet.OnlineID}). It will be deleted.");
                }
            }
        }

        private void validateOnlineIds(RealmBeatmapSet beatmapSet, Realm realm)
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
            List<RealmBeatmap> existingBeatmaps = new List<RealmBeatmap>();

            foreach (int id in beatmapIds)
                existingBeatmaps.AddRange(realm.All<RealmBeatmap>().Where(b => b.OnlineID == id));

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

        protected override bool CanSkipImport(RealmBeatmapSet existing, RealmBeatmapSet import)
        {
            if (!base.CanSkipImport(existing, import))
                return false;

            return existing.Beatmaps.Any(b => b.OnlineID > 0);
        }

        protected override bool CanReuseExisting(RealmBeatmapSet existing, RealmBeatmapSet import)
        {
            if (!base.CanReuseExisting(existing, import))
                return false;

            var existingIds = existing.Beatmaps.Select(b => b.OnlineID).OrderBy(i => i);
            var importIds = import.Beatmaps.Select(b => b.OnlineID).OrderBy(i => i);

            // force re-import if we are not in a sane state.
            return existing.OnlineID == import.OnlineID && existingIds.SequenceEqual(importIds);
        }

        public override string HumanisedModelName => "beatmap";

        protected override RealmBeatmapSet? CreateModel(ArchiveReader reader)
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
                beatmap = Decoder.GetDecoder<Beatmap>(stream).Decode(stream);

            return new RealmBeatmapSet
            {
                OnlineID = beatmap.BeatmapInfo.BeatmapSet?.OnlineID ?? -1,
                // Metadata = beatmap.Metadata,
                DateAdded = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Create all required <see cref="RealmBeatmap"/>s for the provided archive.
        /// </summary>
        private List<RealmBeatmap> createBeatmapDifficulties(IList<RealmNamedFileUsage> files, Realm realm)
        {
            var beatmaps = new List<RealmBeatmap>();

            foreach (var file in files.Where(f => f.Filename.EndsWith(".osu", StringComparison.OrdinalIgnoreCase)))
            {
                using (var memoryStream = new MemoryStream(Files.Store.Get(file.File.GetStoragePath()))) // we need a memory stream so we can seek
                {
                    IBeatmap decoded;
                    using (var lineReader = new LineBufferedReader(memoryStream, true))
                        decoded = Decoder.GetDecoder<Beatmap>(lineReader).Decode(lineReader);

                    string hash = memoryStream.ComputeSHA2Hash();

                    if (beatmaps.Any(b => b.Hash == hash))
                    {
                        Logger.Log($"Skipping import of {file.Filename} due to duplicate file content.", LoggingTarget.Database);
                        continue;
                    }

                    var decodedInfo = decoded.BeatmapInfo;
                    var decodedDifficulty = decodedInfo.BaseDifficulty;

                    var ruleset = realm.All<RealmRuleset>().FirstOrDefault(r => r.OnlineID == decodedInfo.RulesetID);

                    if (ruleset?.Available != true)
                    {
                        Logger.Log($"Skipping import of {file.Filename} due to missing local ruleset {decodedInfo.RulesetID}.", LoggingTarget.Database);
                        continue;
                    }

                    var difficulty = new RealmBeatmapDifficulty
                    {
                        DrainRate = decodedDifficulty.DrainRate,
                        CircleSize = decodedDifficulty.CircleSize,
                        OverallDifficulty = decodedDifficulty.OverallDifficulty,
                        ApproachRate = decodedDifficulty.ApproachRate,
                        SliderMultiplier = decodedDifficulty.SliderMultiplier,
                        SliderTickRate = decodedDifficulty.SliderTickRate,
                    };

                    var metadata = new RealmBeatmapMetadata
                    {
                        Title = decoded.Metadata.Title,
                        TitleUnicode = decoded.Metadata.TitleUnicode,
                        Artist = decoded.Metadata.Artist,
                        ArtistUnicode = decoded.Metadata.ArtistUnicode,
                        Author =
                        {
                            OnlineID = decoded.Metadata.Author.Id,
                            Username = decoded.Metadata.Author.Username
                        },
                        Source = decoded.Metadata.Source,
                        Tags = decoded.Metadata.Tags,
                        PreviewTime = decoded.Metadata.PreviewTime,
                        AudioFile = decoded.Metadata.AudioFile,
                        BackgroundFile = decoded.Metadata.BackgroundFile,
                    };

                    var beatmap = new RealmBeatmap(ruleset, difficulty, metadata)
                    {
                        Hash = hash,
                        DifficultyName = decodedInfo.DifficultyName,
                        OnlineID = decodedInfo.OnlineID ?? -1,
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

                    updateBeatmapStatistics(beatmap, decoded);

                    beatmaps.Add(beatmap);
                }
            }

            return beatmaps;
        }

        private void updateBeatmapStatistics(RealmBeatmap beatmap, IBeatmap decoded)
        {
            var rulesetInstance = ((IRulesetInfo)beatmap.Ruleset).CreateInstance();

            decoded.BeatmapInfo.Ruleset = rulesetInstance.RulesetInfo;

            // TODO: this should be done in a better place once we actually need to dynamically update it.
            beatmap.StarRating = rulesetInstance.CreateDifficultyCalculator(new DummyConversionBeatmap(decoded)).Calculate().StarRating;
            beatmap.Length = calculateLength(decoded);
            beatmap.BPM = 60000 / decoded.GetMostCommonBeatLength();
        }

        private double calculateLength(IBeatmap b)
        {
            if (!b.HitObjects.Any())
                return 0;

            var lastObject = b.HitObjects.Last();

            //TODO: this isn't always correct (consider mania where a non-last object may last for longer than the last in the list).
            double endTime = lastObject.GetEndTime();
            double startTime = b.HitObjects.First().StartTime;

            return endTime - startTime;
        }

        public void Dispose()
        {
            onlineLookupQueue?.Dispose();
        }

        /// <summary>
        /// A dummy WorkingBeatmap for the purpose of retrieving a beatmap for star difficulty calculation.
        /// </summary>
        private class DummyConversionBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap beatmap;

            public DummyConversionBeatmap(IBeatmap beatmap)
                : base(beatmap.BeatmapInfo, null)
            {
                this.beatmap = beatmap;
            }

            protected override IBeatmap GetBeatmap() => beatmap;
            protected override Texture? GetBackground() => null;
            protected override Track? GetBeatmapTrack() => null;
            protected internal override ISkin? GetSkin() => null;
            public override Stream? GetStream(string storagePath) => null;
        }
    }
}
