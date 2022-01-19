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

        public BeatmapModelManager(RealmContextFactory contextFactory, Storage storage, BeatmapOnlineLookupQueue? onlineLookupQueue = null)
            : base(contextFactory, storage, onlineLookupQueue)
        {
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == ".osz";

        /// <summary>
        /// Saves an <see cref="IBeatmap"/> file against a given <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to save the content against. The file referenced by <see cref="BeatmapInfo.Path"/> will be replaced.</param>
        /// <param name="beatmapContent">The <see cref="IBeatmap"/> content to write.</param>
        /// <param name="beatmapSkin">The beatmap <see cref="ISkin"/> content to write, null if to be omitted.</param>
        public virtual void Save(BeatmapInfo beatmapInfo, IBeatmap beatmapContent, ISkin? beatmapSkin = null)
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
            return $"{metadata.Artist} - {metadata.Title} ({metadata.Author}) [{beatmapInfo.DifficultyName}].osu".GetValidArchiveContentFilename();
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo? QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query)
        {
            using (var context = ContextFactory.CreateContext())
                return context.All<BeatmapInfo>().FirstOrDefault(query)?.Detach();
        }

        public void Update(BeatmapSetInfo item)
        {
            using (var realm = ContextFactory.CreateContext())
            {
                var existing = realm.Find<BeatmapSetInfo>(item.ID);
                realm.Write(r => item.CopyChangesToRealm(existing));
            }
        }
    }
}
