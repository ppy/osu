// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Skinning;
using Decoder = osu.Game.Beatmaps.Formats.Decoder;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles ef-core storage of beatmaps.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class BeatmapModelManager : ArchiveModelManager<BeatmapSetInfo, BeatmapSetFileInfo>, IBeatmapModelManager
    {
        /// <summary>
        /// Fired when a single difficulty has been hidden.
        /// </summary>
        public event Action<BeatmapInfo> BeatmapHidden;

        /// <summary>
        /// Fired when a single difficulty has been restored.
        /// </summary>
        public event Action<BeatmapInfo> BeatmapRestored;

        /// <summary>
        /// An online lookup queue component which handles populating online beatmap metadata.
        /// </summary>
        public BeatmapOnlineLookupQueue OnlineLookupQueue { private get; set; }

        /// <summary>
        /// The game working beatmap cache, used to invalidate entries on changes.
        /// </summary>
        public IWorkingBeatmapCache WorkingBeatmapCache { private get; set; }

        public override IEnumerable<string> HandledExtensions => new[] { ".osz" };

        protected override string[] HashableFileTypes => new[] { ".osu" };

        protected override string ImportFromStablePath => ".";

        protected override Storage PrepareStableStorage(StableStorage stableStorage) => stableStorage.GetSongStorage();

        private readonly BeatmapStore beatmaps;
        private readonly RulesetStore rulesets;

        public BeatmapModelManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, GameHost host = null)
            : base(storage, contextFactory, new BeatmapStore(contextFactory), host)
        {
            this.rulesets = rulesets;

            beatmaps = (BeatmapStore)ModelStore;
            beatmaps.BeatmapHidden += b => BeatmapHidden?.Invoke(b);
            beatmaps.BeatmapRestored += b => BeatmapRestored?.Invoke(b);
            beatmaps.ItemRemoved += b => WorkingBeatmapCache?.Invalidate(b);
            beatmaps.ItemUpdated += obj => WorkingBeatmapCache?.Invalidate(obj);
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == ".osz";

        protected override async Task Populate(BeatmapSetInfo beatmapSet, ArchiveReader archive, CancellationToken cancellationToken = default)
        {
            if (archive != null)
                beatmapSet.Beatmaps = createBeatmapDifficulties(beatmapSet.Files);

            foreach (BeatmapInfo b in beatmapSet.Beatmaps)
            {
                // remove metadata from difficulties where it matches the set
                if (beatmapSet.Metadata.Equals(b.Metadata))
                    b.Metadata = null;

                b.BeatmapSet = beatmapSet;
            }

            validateOnlineIds(beatmapSet);

            bool hadOnlineBeatmapIDs = beatmapSet.Beatmaps.Any(b => b.OnlineBeatmapID > 0);

            if (OnlineLookupQueue != null)
                await OnlineLookupQueue.UpdateAsync(beatmapSet, cancellationToken).ConfigureAwait(false);

            // ensure at least one beatmap was able to retrieve or keep an online ID, else drop the set ID.
            if (hadOnlineBeatmapIDs && !beatmapSet.Beatmaps.Any(b => b.OnlineBeatmapID > 0))
            {
                if (beatmapSet.OnlineBeatmapSetID != null)
                {
                    beatmapSet.OnlineBeatmapSetID = null;
                    LogForModel(beatmapSet, "Disassociating beatmap set ID due to loss of all beatmap IDs");
                }
            }
        }

        protected override void PreImport(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet.Beatmaps.Any(b => b.BaseDifficulty == null))
                throw new InvalidOperationException($"Cannot import {nameof(BeatmapInfo)} with null {nameof(BeatmapInfo.BaseDifficulty)}.");

            // check if a set already exists with the same online id, delete if it does.
            if (beatmapSet.OnlineBeatmapSetID != null)
            {
                var existingSetWithSameOnlineID = beatmaps.ConsumableItems.FirstOrDefault(b => b.OnlineBeatmapSetID == beatmapSet.OnlineBeatmapSetID);

                if (existingSetWithSameOnlineID != null)
                {
                    Delete(existingSetWithSameOnlineID);

                    // in order to avoid a unique key constraint, immediately remove the online ID from the previous set.
                    existingSetWithSameOnlineID.OnlineBeatmapSetID = null;
                    foreach (var b in existingSetWithSameOnlineID.Beatmaps)
                        b.OnlineBeatmapID = null;

                    LogForModel(beatmapSet, $"Found existing beatmap set with same OnlineBeatmapSetID ({beatmapSet.OnlineBeatmapSetID}). It has been deleted.");
                }
            }
        }

        private void validateOnlineIds(BeatmapSetInfo beatmapSet)
        {
            var beatmapIds = beatmapSet.Beatmaps.Where(b => b.OnlineBeatmapID.HasValue).Select(b => b.OnlineBeatmapID).ToList();

            // ensure all IDs are unique
            if (beatmapIds.GroupBy(b => b).Any(g => g.Count() > 1))
            {
                LogForModel(beatmapSet, "Found non-unique IDs, resetting...");
                resetIds();
                return;
            }

            // find any existing beatmaps in the database that have matching online ids
            var existingBeatmaps = QueryBeatmaps(b => beatmapIds.Contains(b.OnlineBeatmapID)).ToList();

            if (existingBeatmaps.Count > 0)
            {
                // reset the import ids (to force a re-fetch) *unless* they match the candidate CheckForExisting set.
                // we can ignore the case where the new ids are contained by the CheckForExisting set as it will either be used (import skipped) or deleted.
                var existing = CheckForExisting(beatmapSet);

                if (existing == null || existingBeatmaps.Any(b => !existing.Beatmaps.Contains(b)))
                {
                    LogForModel(beatmapSet, "Found existing import with IDs already, resetting...");
                    resetIds();
                }
            }

            void resetIds() => beatmapSet.Beatmaps.ForEach(b => b.OnlineBeatmapID = null);
        }

        /// <summary>
        /// Delete a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to hide.</param>
        public void Hide(BeatmapInfo beatmapInfo) => beatmaps.Hide(beatmapInfo);

        /// <summary>
        /// Restore a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to restore.</param>
        public void Restore(BeatmapInfo beatmapInfo) => beatmaps.Restore(beatmapInfo);

        /// <summary>
        /// Saves an <see cref="IBeatmap"/> file against a given <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to save the content against. The file referenced by <see cref="BeatmapInfo.Path"/> will be replaced.</param>
        /// <param name="beatmapContent">The <see cref="IBeatmap"/> content to write.</param>
        /// <param name="beatmapSkin">The beatmap <see cref="ISkin"/> content to write, null if to be omitted.</param>
        public virtual void Save(BeatmapInfo beatmapInfo, IBeatmap beatmapContent, ISkin beatmapSkin = null)
        {
            var setInfo = beatmapInfo.BeatmapSet;

            // Difficulty settings must be copied first due to the clone in `Beatmap<>.BeatmapInfo_Set`.
            // This should hopefully be temporary, assuming said clone is eventually removed.

            // Warning: The directionality here is important. Changes have to be copied *from* beatmapContent (which comes from editor and is being saved)
            // *to* the beatmapInfo (which is a database model and needs to receive values without the taiko slider velocity multiplier for correct operation).
            // CopyTo() will undo such adjustments, while CopyFrom() will not.
            beatmapContent.Difficulty.CopyTo(beatmapInfo.BaseDifficulty);

            // All changes to metadata are made in the provided beatmapInfo, so this should be copied to the `IBeatmap` before encoding.
            beatmapContent.BeatmapInfo = beatmapInfo;

            using (var stream = new MemoryStream())
            {
                using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    new LegacyBeatmapEncoder(beatmapContent, beatmapSkin).Encode(sw);

                stream.Seek(0, SeekOrigin.Begin);

                using (ContextFactory.GetForWrite())
                {
                    beatmapInfo = setInfo.Beatmaps.Single(b => b.ID == beatmapInfo.ID);

                    var metadata = beatmapInfo.Metadata ?? setInfo.Metadata;

                    // grab the original file (or create a new one if not found).
                    var fileInfo = setInfo.Files.SingleOrDefault(f => string.Equals(f.Filename, beatmapInfo.Path, StringComparison.OrdinalIgnoreCase)) ?? new BeatmapSetFileInfo();

                    // metadata may have changed; update the path with the standard format.
                    beatmapInfo.Path = GetValidFilename($"{metadata.Artist} - {metadata.Title} ({metadata.Author}) [{beatmapInfo.Version}].osu");

                    beatmapInfo.MD5Hash = stream.ComputeMD5Hash();

                    // update existing or populate new file's filename.
                    fileInfo.Filename = beatmapInfo.Path;

                    stream.Seek(0, SeekOrigin.Begin);
                    ReplaceFile(setInfo, fileInfo, stream);
                }
            }

            WorkingBeatmapCache?.Invalidate(beatmapInfo);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapSetInfo QueryBeatmapSet(Expression<Func<BeatmapSetInfo, bool>> query) => beatmaps.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        protected override bool CanSkipImport(BeatmapSetInfo existing, BeatmapSetInfo import)
        {
            if (!base.CanSkipImport(existing, import))
                return false;

            return existing.Beatmaps.Any(b => b.OnlineBeatmapID != null);
        }

        protected override bool CanReuseExisting(BeatmapSetInfo existing, BeatmapSetInfo import)
        {
            if (!base.CanReuseExisting(existing, import))
                return false;

            var existingIds = existing.Beatmaps.Select(b => b.OnlineBeatmapID).OrderBy(i => i);
            var importIds = import.Beatmaps.Select(b => b.OnlineBeatmapID).OrderBy(i => i);

            // force re-import if we are not in a sane state.
            return existing.OnlineBeatmapSetID == import.OnlineBeatmapSetID && existingIds.SequenceEqual(importIds);
        }

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public List<BeatmapSetInfo> GetAllUsableBeatmapSets(IncludedDetails includes = IncludedDetails.All, bool includeProtected = false) =>
            GetAllUsableBeatmapSetsEnumerable(includes, includeProtected).ToList();

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s. Note that files are not populated.
        /// </summary>
        /// <param name="includes">The level of detail to include in the returned objects.</param>
        /// <param name="includeProtected">Whether to include protected (system) beatmaps. These should not be included for gameplay playable use cases.</param>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public IEnumerable<BeatmapSetInfo> GetAllUsableBeatmapSetsEnumerable(IncludedDetails includes, bool includeProtected = false)
        {
            IQueryable<BeatmapSetInfo> queryable;

            switch (includes)
            {
                case IncludedDetails.Minimal:
                    queryable = beatmaps.BeatmapSetsOverview;
                    break;

                case IncludedDetails.AllButRuleset:
                    queryable = beatmaps.BeatmapSetsWithoutRuleset;
                    break;

                case IncludedDetails.AllButFiles:
                    queryable = beatmaps.BeatmapSetsWithoutFiles;
                    break;

                default:
                    queryable = beatmaps.ConsumableItems;
                    break;
            }

            // AsEnumerable used here to avoid applying the WHERE in sql. When done so, ef core 2.x uses an incorrect ORDER BY
            // clause which causes queries to take 5-10x longer.
            // TODO: remove if upgrading to EF core 3.x.
            return queryable.AsEnumerable().Where(s => !s.DeletePending && (includeProtected || !s.Protected));
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="includes">The level of detail to include in the returned objects.</param>
        /// <returns>Results from the provided query.</returns>
        public IEnumerable<BeatmapSetInfo> QueryBeatmapSets(Expression<Func<BeatmapSetInfo, bool>> query, IncludedDetails includes = IncludedDetails.All)
        {
            IQueryable<BeatmapSetInfo> queryable;

            switch (includes)
            {
                case IncludedDetails.Minimal:
                    queryable = beatmaps.BeatmapSetsOverview;
                    break;

                case IncludedDetails.AllButRuleset:
                    queryable = beatmaps.BeatmapSetsWithoutRuleset;
                    break;

                case IncludedDetails.AllButFiles:
                    queryable = beatmaps.BeatmapSetsWithoutFiles;
                    break;

                default:
                    queryable = beatmaps.ConsumableItems;
                    break;
            }

            return queryable.AsNoTracking().Where(query);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query) => beatmaps.Beatmaps.AsNoTracking().FirstOrDefault(query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IQueryable<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query) => beatmaps.Beatmaps.AsNoTracking().Where(query);

        public override string HumanisedModelName => "beatmap";

        protected override bool CheckLocalAvailability(BeatmapSetInfo model, IQueryable<BeatmapSetInfo> items)
            => base.CheckLocalAvailability(model, items)
               || (model.OnlineBeatmapSetID != null && items.Any(b => b.OnlineBeatmapSetID == model.OnlineBeatmapSetID));

        protected override BeatmapSetInfo CreateModel(ArchiveReader reader)
        {
            // let's make sure there are actually .osu files to import.
            string mapName = reader.Filenames.FirstOrDefault(f => f.EndsWith(".osu", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(mapName))
            {
                Logger.Log($"No beatmap files found in the beatmap archive ({reader.Name}).", LoggingTarget.Database);
                return null;
            }

            Beatmap beatmap;
            using (var stream = new LineBufferedReader(reader.GetStream(mapName)))
                beatmap = Decoder.GetDecoder<Beatmap>(stream).Decode(stream);

            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = beatmap.BeatmapInfo.BeatmapSet?.OnlineBeatmapSetID,
                Beatmaps = new List<BeatmapInfo>(),
                Metadata = beatmap.Metadata,
                DateAdded = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Create all required <see cref="BeatmapInfo"/>s for the provided archive.
        /// </summary>
        private List<BeatmapInfo> createBeatmapDifficulties(List<BeatmapSetFileInfo> files)
        {
            var beatmapInfos = new List<BeatmapInfo>();

            foreach (var file in files.Where(f => f.Filename.EndsWith(".osu", StringComparison.OrdinalIgnoreCase)))
            {
                using (var raw = Files.Store.GetStream(file.FileInfo.StoragePath))
                using (var ms = new MemoryStream()) // we need a memory stream so we can seek
                using (var sr = new LineBufferedReader(ms))
                {
                    raw.CopyTo(ms);
                    ms.Position = 0;

                    var decoder = Decoder.GetDecoder<Beatmap>(sr);
                    IBeatmap beatmap = decoder.Decode(sr);

                    string hash = ms.ComputeSHA2Hash();

                    if (beatmapInfos.Any(b => b.Hash == hash))
                        continue;

                    beatmap.BeatmapInfo.Path = file.Filename;
                    beatmap.BeatmapInfo.Hash = hash;
                    beatmap.BeatmapInfo.MD5Hash = ms.ComputeMD5Hash();

                    var ruleset = rulesets.GetRuleset(beatmap.BeatmapInfo.RulesetID);
                    beatmap.BeatmapInfo.Ruleset = ruleset;

                    // TODO: this should be done in a better place once we actually need to dynamically update it.
                    beatmap.BeatmapInfo.StarDifficulty = ruleset?.CreateInstance().CreateDifficultyCalculator(new DummyConversionBeatmap(beatmap)).Calculate().StarRating ?? 0;
                    beatmap.BeatmapInfo.Length = calculateLength(beatmap);
                    beatmap.BeatmapInfo.BPM = 60000 / beatmap.GetMostCommonBeatLength();

                    beatmapInfos.Add(beatmap.BeatmapInfo);
                }
            }

            return beatmapInfos;
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
            protected override Texture GetBackground() => null;
            protected override Track GetBeatmapTrack() => null;
            protected internal override ISkin GetSkin() => null;
            public override Stream GetStream(string storagePath) => null;
        }
    }

    /// <summary>
    /// The level of detail to include in database results.
    /// </summary>
    public enum IncludedDetails
    {
        /// <summary>
        /// Only include beatmap difficulties and set level metadata.
        /// </summary>
        Minimal,

        /// <summary>
        /// Include all difficulties, rulesets, difficulty metadata but no files.
        /// </summary>
        AllButFiles,

        /// <summary>
        /// Include everything except ruleset. Used for cases where we aren't sure the ruleset is present but still want to consume the beatmap.
        /// </summary>
        AllButRuleset,

        /// <summary>
        /// Include everything.
        /// </summary>
        All
    }
}
