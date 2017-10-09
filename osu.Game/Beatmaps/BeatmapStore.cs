// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public BeatmapStore(OsuDbContext connection)
            : base(connection)
        {
        }

        protected override Type[] ValidTypes => new[]
        {
            typeof(BeatmapSetInfo),
            typeof(BeatmapInfo),
            typeof(BeatmapMetadata),
            typeof(BeatmapDifficulty),
        };

        protected override void Prepare(bool reset = false)
        {
            if (reset)
            {
                // https://stackoverflow.com/a/10450893
                Connection.Database.ExecuteSqlCommand("DELETE FROM BeatmapMetadata");
                Connection.Database.ExecuteSqlCommand("DELETE FROM BeatmapDifficulty");
                Connection.Database.ExecuteSqlCommand("DELETE FROM BeatmapSetInfo");
                Connection.Database.ExecuteSqlCommand("DELETE FROM BeatmapSetFileInfo");
                Connection.Database.ExecuteSqlCommand("DELETE FROM BeatmapInfo");
            }
        }

        protected override void StartupTasks()
        {
            base.StartupTasks();
            cleanupPendingDeletions();
        }

        /// <summary>
        /// Add a <see cref="BeatmapSetInfo"/> to the database.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to add.</param>
        public void Add(BeatmapSetInfo beatmapSet)
        {
            Connection.BeatmapSetInfo.Update(beatmapSet);
            Connection.SaveChanges();

            BeatmapSetAdded?.Invoke(beatmapSet);
        }

        /// <summary>
        /// Delete a <see cref="BeatmapSetInfo"/> from the database.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to delete.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapSetInfo.DeletePending"/> was changed.</returns>
        public bool Delete(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet.DeletePending) return false;

            beatmapSet.DeletePending = true;
            Connection.BeatmapSetInfo.Remove(beatmapSet);
            Connection.SaveChanges();

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
            if (!beatmapSet.DeletePending) return false;

            beatmapSet.DeletePending = false;
            Connection.BeatmapSetInfo.Update(beatmapSet);

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
            if (beatmap.Hidden) return false;

            beatmap.Hidden = true;
            Connection.BeatmapInfo.Update(beatmap);

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
            if (!beatmap.Hidden) return false;

            beatmap.Hidden = false;
            Connection.BeatmapInfo.Update(beatmap);

            BeatmapRestored?.Invoke(beatmap);
            return true;
        }

        private void cleanupPendingDeletions()
        {
            Connection.BeatmapSetInfo.RemoveRange(Connection.BeatmapSetInfo.Where(b => b.DeletePending && !b.Protected));
        }

        public BeatmapSetInfo QueryBeatmapSet(Func<BeatmapSetInfo, bool> query)
        {
            return Connection.BeatmapSetInfo
                .Include(b => b.Metadata)
                .Include(b => b.Beatmaps).ThenInclude(b => b.Ruleset)
                .Include(b => b.Beatmaps).ThenInclude(b => b.Difficulty)
                .Include(b => b.Files).ThenInclude(f => f.FileInfo)
                .FirstOrDefault(query);
        }

        public List<BeatmapSetInfo> QueryBeatmapSets(Expression<Func<BeatmapSetInfo, bool>> query)
        {
            return Connection.BeatmapSetInfo
                .Include(b => b.Metadata)
                .Include(b => b.Beatmaps).ThenInclude(b => b.Ruleset)
                .Include(b => b.Beatmaps).ThenInclude(b => b.Difficulty)
                .Include(b => b.Files).ThenInclude(f => f.FileInfo)
                .Where(query).ToList();
        }

        public BeatmapInfo QueryBeatmap(Func<BeatmapInfo, bool> query)
        {
            return Connection.BeatmapInfo
                .Include(b => b.BeatmapSet)
                .Include(b => b.Metadata)
                .Include(b => b.Ruleset)
                .Include(b => b.Difficulty)
                .FirstOrDefault(query);
        }

        public List<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query)
        {
            return Connection.BeatmapInfo
                .Include(b => b.BeatmapSet)
                .Include(b => b.Metadata)
                .Include(b => b.Ruleset)
                .Include(b => b.Difficulty)
                .Where(query).ToList();
        }
    }
}
