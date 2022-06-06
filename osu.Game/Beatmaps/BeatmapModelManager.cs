// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Skinning;
using osu.Game.Stores;
using osu.Game.Overlays.Notifications;

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

        public static readonly string[] VIDEO_EXTENSIONS = { ".mp4", ".mov", ".avi", ".flv" };

        public BeatmapModelManager(RealmAccess realm, Storage storage, BeatmapOnlineLookupQueue? onlineLookupQueue = null)
            : base(realm, storage, onlineLookupQueue)
        {
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == ".osz";

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

        /// <summary>
        /// Delete videos from a list of beatmaps.
        /// This will post notifications tracking progress.
        /// </summary>
        public void DeleteVideos(List<BeatmapSetInfo> items, bool silent = false)
        {
            if (items.Count == 0) return;

            var notification = new ProgressNotification
            {
                Progress = 0,
                Text = $"Preparing to delete all {HumanisedModelName} videos...",
                CompletionText = "No videos found to delete!",
                State = ProgressNotificationState.Active,
            };

            if (!silent)
                PostNotification?.Invoke(notification);

            int i = 0;
            int deleted = 0;

            foreach (var b in items)
            {
                if (notification.State == ProgressNotificationState.Cancelled)
                    // user requested abort
                    return;

                var video = b.Files.FirstOrDefault(f => VIDEO_EXTENSIONS.Any(ex => f.Filename.EndsWith(ex, StringComparison.Ordinal)));

                if (video != null)
                {
                    DeleteFile(b, video);
                    deleted++;
                    notification.CompletionText = $"Deleted {deleted} {HumanisedModelName} video(s)!";
                }

                notification.Text = $"Deleting videos from {HumanisedModelName}s ({deleted} deleted)";

                notification.Progress = (float)++i / items.Count;
            }

            notification.State = ProgressNotificationState.Completed;
        }
    }
}
