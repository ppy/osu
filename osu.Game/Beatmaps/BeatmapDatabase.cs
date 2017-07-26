// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Logging;
using osu.Game.Database;
using SQLite.Net;
using SQLiteNetExtensions.Extensions;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/BeatmapSets to the database backing
    /// </summary>
    public class BeatmapDatabase : DatabaseStore
    {
        public event Action<BeatmapSetInfo> BeatmapSetAdded;
        public event Action<BeatmapSetInfo> BeatmapSetRemoved;

        public BeatmapDatabase(SQLiteConnection connection)
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

            cleanupPendingDeletions();
        }

        /// <summary>
        /// Add a <see cref="BeatmapSetInfo"/> to the database.
        /// </summary>
        /// <param name="beatmapSet">The beatmap to add.</param>
        public void Add(BeatmapSetInfo beatmapSet)
        {
            Connection.InsertOrReplaceWithChildren(beatmapSet, true);
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
        public bool Undelete(BeatmapSetInfo set)
        {
            if (!set.DeletePending) return false;

            set.DeletePending = false;
            Connection.Update(set);

            BeatmapSetAdded?.Invoke(set);
            return true;
        }

        private void cleanupPendingDeletions()
        {
            foreach (var b in GetAllWithChildren<BeatmapSetInfo>(b => b.DeletePending && !b.Protected))
            {
                try
                {
                    foreach (var i in b.Beatmaps)
                    {
                        if (i.Metadata != null) Connection.Delete(i.Metadata);
                        if (i.Difficulty != null) Connection.Delete(i.Difficulty);

                        Connection.Delete(i);
                    }

                    if (b.Metadata != null) Connection.Delete(b.Metadata);
                    Connection.Delete(b);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $@"Could not delete beatmap {b}");
                }
            }

            //this is required because sqlite migrations don't work, initially inserting nulls into this field.
            //see https://github.com/praeclarum/sqlite-net/issues/326
            Connection.Query<BeatmapSetInfo>("UPDATE BeatmapSetInfo SET DeletePending = 0 WHERE DeletePending IS NULL");
        }
    }
}
