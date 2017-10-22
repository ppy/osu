// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/BeatmapSets to the database backing
    /// </summary>
    public class BeatmapStore : DatabaseBackedStore
    {
        public event Action<BeatmapSetInfo> BeatmapSetAdded;
        public event Action<BeatmapSetInfo> BeatmapSetRemoved;

        public event Action<BeatmapInfo> BeatmapHidden;
        public event Action<BeatmapInfo> BeatmapRestored;

        public BeatmapStore(Func<OsuDbContext> factory)
            : base(factory)
        {
        }

        protected override void Prepare(bool reset = false)
        {
            if (reset)
            {
                var context = GetContext();

                // https://stackoverflow.com/a/10450893
                context.Database.ExecuteSqlCommand("DELETE FROM BeatmapMetadata");
                context.Database.ExecuteSqlCommand("DELETE FROM BeatmapDifficulty");
                context.Database.ExecuteSqlCommand("DELETE FROM BeatmapSetInfo");
                context.Database.ExecuteSqlCommand("DELETE FROM BeatmapSetFileInfo");
                context.Database.ExecuteSqlCommand("DELETE FROM BeatmapInfo");
            }
        }

        /// <summary>
        /// Add a <see cref="BeatmapSetInfo"/> to the database.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to add.</param>
        public void Add(BeatmapSetInfo beatmapSet)
        {
            var context = GetContext();

            context.BeatmapSetInfo.Attach(beatmapSet);
            context.SaveChanges();

            BeatmapSetAdded?.Invoke(beatmapSet);
        }

        /// <summary>
        /// Delete a <see cref="BeatmapSetInfo"/> from the database.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to delete.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapSetInfo.DeletePending"/> was changed.</returns>
        public bool Delete(BeatmapSetInfo beatmapSet)
        {
            var context = GetContext();

            if (beatmapSet.DeletePending) return false;

            beatmapSet.DeletePending = true;
            context.Update(beatmapSet);
            context.SaveChanges();

            BeatmapSetRemoved?.Invoke(beatmapSet);
            return true;
        }

        /// <summary>
        /// Restore a previously deleted <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to restore.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapSetInfo.DeletePending"/> was changed.</returns>
        public bool Undelete(BeatmapSetInfo beatmapSet)
        {
            var context = GetContext();

            if (!beatmapSet.DeletePending) return false;

            beatmapSet.DeletePending = false;
            context.Update(beatmapSet);
            context.SaveChanges();

            BeatmapSetAdded?.Invoke(beatmapSet);
            return true;
        }

        /// <summary>
        /// Hide a <see cref="BeatmapInfo"/> in the database.
        /// </summary>
        /// <param name="beatmap">The beatmap to hide.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapInfo.Hidden"/> was changed.</returns>
        public bool Hide(BeatmapInfo beatmap)
        {
            var context = GetContext();

            if (beatmap.Hidden) return false;

            beatmap.Hidden = true;
            context.Update(beatmap);
            context.SaveChanges();

            BeatmapHidden?.Invoke(beatmap);
            return true;
        }

        /// <summary>
        /// Restore a previously hidden <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmap">The beatmap to restore.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapInfo.Hidden"/> was changed.</returns>
        public bool Restore(BeatmapInfo beatmap)
        {
            var context = GetContext();

            if (!beatmap.Hidden) return false;

            beatmap.Hidden = false;
            context.Update(beatmap);
            context.SaveChanges();

            BeatmapRestored?.Invoke(beatmap);
            return true;
        }

        public override void Cleanup()
        {
            var context = GetContext();

            var purgeable = context.BeatmapSetInfo.Where(s => s.DeletePending && !s.Protected)
                   .Include(s => s.Beatmaps).ThenInclude(b => b.Metadata)
                   .Include(s => s.Beatmaps).ThenInclude(b => b.BaseDifficulty)
                   .Include(s => s.Metadata);

            // metadata is M-N so we can't rely on cascades
            context.BeatmapMetadata.RemoveRange(purgeable.Select(s => s.Metadata));
            context.BeatmapMetadata.RemoveRange(purgeable.SelectMany(s => s.Beatmaps.Select(b => b.Metadata).Where(m => m != null)));

            // todo: we can probably make cascades work here with a FK in BeatmapDifficulty. just make to make it work correctly.
            context.BeatmapDifficulty.RemoveRange(purgeable.SelectMany(s => s.Beatmaps.Select(b => b.BaseDifficulty)));

            // cascades down to beatmaps.
            context.BeatmapSetInfo.RemoveRange(purgeable);
            context.SaveChanges();
        }

        public IEnumerable<BeatmapSetInfo> BeatmapSets => GetContext().BeatmapSetInfo
                                                                      .Include(s => s.Metadata)
                                                                      .Include(s => s.Beatmaps).ThenInclude(s => s.Ruleset)
                                                                      .Include(s => s.Beatmaps).ThenInclude(b => b.BaseDifficulty)
                                                                      .Include(s => s.Beatmaps).ThenInclude(b => b.Metadata)
                                                                      .Include(s => s.Files).ThenInclude(f => f.FileInfo);

        public IEnumerable<BeatmapInfo> Beatmaps => GetContext().BeatmapInfo
                                                                .Include(b => b.BeatmapSet).ThenInclude(s => s.Metadata)
                                                                .Include(b => b.Metadata)
                                                                .Include(b => b.Ruleset)
                                                                .Include(b => b.BaseDifficulty);
    }
}
