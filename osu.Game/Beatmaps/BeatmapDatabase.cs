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

            deletePending();
        }

        public void Update(BeatmapSetInfo setInfo) => Connection.Update(setInfo);

        public void Import(IEnumerable<BeatmapSetInfo> beatmapSets)
        {
            lock (Connection)
            {
                Connection.BeginTransaction();

                foreach (var s in beatmapSets)
                {
                    Connection.InsertOrReplaceWithChildren(s, true);
                    BeatmapSetAdded?.Invoke(s);
                }

                Connection.Commit();
            }
        }

        public bool Delete(BeatmapSetInfo set)
        {
            if (set.DeletePending) return false;

            set.DeletePending = true;
            Connection.Update(set);

            BeatmapSetRemoved?.Invoke(set);
            return true;
        }

        public bool Undelete(BeatmapSetInfo set)
        {
            if (!set.DeletePending) return false;

            set.DeletePending = false;
            Connection.Update(set);

            BeatmapSetAdded?.Invoke(set);
            return true;
        }

        private void deletePending()
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