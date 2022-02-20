// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
<<<<<<< HEAD
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
=======
>>>>>>> master
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Skinning;
using osu.Game.Stores;

#nullable enable

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    public class BeatmapModelManager : BeatmapImporter
    {
        /// <summary>
        /// The game working beatmap cache, used to invalidate entries on changes.
        /// </summary>
        public IWorkingBeatmapCache? WorkingBeatmapCache { private get; set; }

        public override IEnumerable<string> HandledExtensions => new[] { ".osz" };

        protected override string[] HashableFileTypes => new[] { ".osu" };

<<<<<<< HEAD
        private ReplayGainManager replayGainManager;
        private readonly BeatmapStore beatmaps;
        private readonly RulesetStore rulesets;

        public BeatmapModelManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, GameHost host = null, ReplayGainManager replayGainManager = null)
            : base(storage, contextFactory, new BeatmapStore(contextFactory), host)
        {
            this.rulesets = rulesets;
            this.replayGainManager = replayGainManager;
            beatmaps = (BeatmapStore)ModelStore;
            beatmaps.BeatmapHidden += b => BeatmapHidden?.Invoke(b);
            beatmaps.BeatmapRestored += b => BeatmapRestored?.Invoke(b);
            beatmaps.ItemRemoved += b => WorkingBeatmapCache?.Invalidate(b);
            beatmaps.ItemUpdated += obj => WorkingBeatmapCache?.Invalidate(obj);
=======
        public BeatmapModelManager(RealmAccess realm, Storage storage, BeatmapOnlineLookupQueue? onlineLookupQueue = null)
            : base(realm, storage, onlineLookupQueue)
        {
>>>>>>> master
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == ".osz";

<<<<<<< HEAD
        protected override async Task Populate(BeatmapSetInfo beatmapSet, ArchiveReader archive, CancellationToken cancellationToken = default)
        {
            if (archive != null)
                beatmapSet.Beatmaps.AddRange(createBeatmapDifficulties(beatmapSet.Files));

            foreach (BeatmapInfo b in beatmapSet.Beatmaps)
            {
                // remove metadata from difficulties where it matches the set
                if (beatmapSet.Metadata.Equals(b.Metadata))
                    b.Metadata = null;

                b.BeatmapSet = beatmapSet;
            }

            foreach (BeatmapInfo b in beatmapSet.Beatmaps)
            {
                if (replayGainManager != null && b.ReplayGainInfo == null)
                {
                    ReplayGainInfo info = replayGainManager.generateReplayGainInfo(b, beatmapSet);
                    await replayGainManager.saveReplayGainInfo(info, b).ConfigureAwait(false);
                    beatmapSet = replayGainManager.PopulateSet(b, beatmapSet);
                }
            }

            validateOnlineIds(beatmapSet);

            bool hadOnlineIDs = beatmapSet.Beatmaps.Any(b => b.OnlineID > 0);

            if (OnlineLookupQueue != null)
                await OnlineLookupQueue.UpdateAsync(beatmapSet, cancellationToken).ConfigureAwait(false);

            // ensure at least one beatmap was able to retrieve or keep an online ID, else drop the set ID.
            if (hadOnlineIDs && !beatmapSet.Beatmaps.Any(b => b.OnlineID > 0))
            {
                if (beatmapSet.OnlineID != null)
                {
                    beatmapSet.OnlineID = null;
                    LogForModel(beatmapSet, "Disassociating beatmap set ID due to loss of all beatmap IDs");
                }
            }
        }

        protected override void PreImport(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet.Beatmaps.Any(b => b.BaseDifficulty == null))
                throw new InvalidOperationException($"Cannot import {nameof(BeatmapInfo)} with null {nameof(BeatmapInfo.BaseDifficulty)}.");

            // check if a set already exists with the same online id, delete if it does.
            if (beatmapSet.OnlineID != null)
            {
                var existingSetWithSameOnlineID = beatmaps.ConsumableItems.FirstOrDefault(b => b.OnlineID == beatmapSet.OnlineID);

                if (existingSetWithSameOnlineID != null)
                {
                    Delete(existingSetWithSameOnlineID);

                    // in order to avoid a unique key constraint, immediately remove the online ID from the previous set.
                    existingSetWithSameOnlineID.OnlineID = null;
                    foreach (var b in existingSetWithSameOnlineID.Beatmaps)
                        b.OnlineID = null;

                    LogForModel(beatmapSet, $"Found existing beatmap set with same OnlineBeatmapSetID ({beatmapSet.OnlineID}). It has been deleted.");
                }
            }
        }

        private void validateOnlineIds(BeatmapSetInfo beatmapSet)
        {
            var beatmapIds = beatmapSet.Beatmaps.Where(b => b.OnlineID.HasValue).Select(b => b.OnlineID).ToList();

            // ensure all IDs are unique
            if (beatmapIds.GroupBy(b => b).Any(g => g.Count() > 1))
            {
                LogForModel(beatmapSet, "Found non-unique IDs, resetting...");
                resetIds();
                return;
            }

            // find any existing beatmaps in the database that have matching online ids
            var existingBeatmaps = QueryBeatmaps(b => beatmapIds.Contains(b.OnlineID)).ToList();

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

            void resetIds() => beatmapSet.Beatmaps.ForEach(b => b.OnlineID = null);
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

=======
>>>>>>> master
        /// <summary>
        /// Saves an <see cref="IBeatmap"/> file against a given <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to save the content against. The file referenced by <see cref="BeatmapInfo.Path"/> will be replaced.</param>
        /// <param name="beatmapContent">The <see cref="IBeatmap"/> content to write.</param>
        /// <param name="beatmapSkin">The beatmap <see cref="ISkin"/> content to write, null if to be omitted.</param>
        public void Save(BeatmapInfo beatmapInfo, IBeatmap beatmapContent, ISkin? beatmapSkin = null)
        {
            var setInfo = beatmapInfo.BeatmapSet;
            Debug.Assert(setInfo != null);

            // Difficulty settings must be copied first due to the clone in `Beatmap<>.BeatmapInfo_Set`.
            // This should hopefully be temporary, assuming said clone is eventually removed.

            // Warning: The directionality here is important. Changes have to be copied *from* beatmapContent (which comes from editor and is being saved)
            // *to* the beatmapInfo (which is a database model and needs to receive values without the taiko slider velocity multiplier for correct operation).
            // CopyTo() will undo such adjustments, while CopyFrom() will not.
            beatmapContent.Difficulty.CopyTo(beatmapInfo.Difficulty);

            // All changes to metadata are made in the provided beatmapInfo, so this should be copied to the `IBeatmap` before encoding.
            beatmapContent.BeatmapInfo = beatmapInfo;

            using (var stream = new MemoryStream())
            {
                using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    new LegacyBeatmapEncoder(beatmapContent, beatmapSkin).Encode(sw);

                stream.Seek(0, SeekOrigin.Begin);

                // AddFile generally handles updating/replacing files, but this is a case where the filename may have also changed so let's delete for simplicity.
                var existingFileInfo = setInfo.Files.SingleOrDefault(f => string.Equals(f.Filename, beatmapInfo.Path, StringComparison.OrdinalIgnoreCase));
                string targetFilename = getFilename(beatmapInfo);

                // ensure that two difficulties from the set don't point at the same beatmap file.
                if (setInfo.Beatmaps.Any(b => b.ID != beatmapInfo.ID && string.Equals(b.Path, targetFilename, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"{setInfo.GetDisplayString()} already has a difficulty with the name of '{beatmapInfo.DifficultyName}'.");

                if (existingFileInfo != null)
                    DeleteFile(setInfo, existingFileInfo);

                beatmapInfo.MD5Hash = stream.ComputeMD5Hash();
                beatmapInfo.Hash = stream.ComputeSHA2Hash();

                AddFile(setInfo, stream, getFilename(beatmapInfo));
                Update(setInfo);
            }

            WorkingBeatmapCache?.Invalidate(beatmapInfo);
        }

        private static string getFilename(BeatmapInfo beatmapInfo)
        {
            var metadata = beatmapInfo.Metadata;
            return $"{metadata.Artist} - {metadata.Title} ({metadata.Author.Username}) [{beatmapInfo.DifficultyName}].osu".GetValidArchiveContentFilename();
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo? QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query)
        {
            return Realm.Run(realm => realm.All<BeatmapInfo>().FirstOrDefault(query)?.Detach());
        }

        public void Update(BeatmapSetInfo item)
        {
            Realm.Write(r =>
            {
                var existing = r.Find<BeatmapSetInfo>(item.ID);
                item.CopyChangesToRealm(existing);
            });
        }
    }
}
