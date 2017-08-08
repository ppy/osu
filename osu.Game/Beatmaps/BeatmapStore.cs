// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Database;
using SQLite.Net;
using SQLiteNetExtensions.Extensions;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/BeatmapSets to the database backing
    /// </summary>
    public class BeatmapStore : DatabaseBackedStore
    {
        public event Action<BeatmapSetInfo> BeatmapSetAdded;
        public event Action<BeatmapSetInfo> BeatmapSetRemoved;

        /// <summary>
        /// The current version of this store. Used for migrations (see <see cref="PerformMigration(int, int)"/>).
        /// The initial version is 1.
        /// </summary>
        protected override int StoreVersion => 2;

        public BeatmapStore(SQLiteConnection connection)
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
                Connection.DropTable<BeatmapMetadata>();
                Connection.DropTable<BeatmapDifficulty>();
                Connection.DropTable<BeatmapSetInfo>();
                Connection.DropTable<BeatmapSetFileInfo>();
                Connection.DropTable<BeatmapInfo>();
            }

            Connection.CreateTable<BeatmapMetadata>();
            Connection.CreateTable<BeatmapDifficulty>();
            Connection.CreateTable<BeatmapSetInfo>();
            Connection.CreateTable<BeatmapSetFileInfo>();
            Connection.CreateTable<BeatmapInfo>();
        }

        protected override void StartupTasks()
        {
            base.StartupTasks();
            cleanupPendingDeletions();
        }

        /// <summary>
        /// Perform migrations between two store versions.
        /// </summary>
        /// <param name="currentVersion">The current store version. This will be zero on a fresh database initialisation.</param>
        /// <param name="targetVersion">The target version which we are migrating to (equal to the current <see cref="StoreVersion"/>).</param>
        protected override void PerformMigration(int currentVersion, int targetVersion)
        {
            base.PerformMigration(currentVersion, targetVersion);

            while (currentVersion++ < targetVersion)
            {
                switch (currentVersion)
                {
                    case 1:
                    case 2:
                        // cannot migrate; breaking underlying changes.
                        Reset();
                        break;
                }
            }
        }

        /// <summary>
        /// Add a <see cref="BeatmapSetInfo"/> to the database.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to add.</param>
        public void Add(BeatmapSetInfo beatmapSet)
        {
            Connection.RunInTransaction(() =>
            {
                Connection.InsertOrReplaceWithChildren(beatmapSet, true);
            });

            BeatmapSetAdded?.Invoke(beatmapSet);
        }

        /// <summary>
        /// Delete a <see cref="BeatmapSetInfo"/> to the database.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to delete.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapSetInfo.DeletePending"/> was changed.</returns>
        public bool Delete(BeatmapSetInfo beatmapSet)
        {
            if (beatmapSet.DeletePending) return false;

            beatmapSet.DeletePending = true;
            Connection.Update(beatmapSet);

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
            Connection.Update(beatmapSet);

            BeatmapSetAdded?.Invoke(beatmapSet);
            return true;
        }

        private void cleanupPendingDeletions()
        {
            Connection.RunInTransaction(() =>
            {
                foreach (var b in QueryAndPopulate<BeatmapSetInfo>(b => b.DeletePending && !b.Protected))
                    Connection.Delete(b, true);
            });
        }
    }
}
