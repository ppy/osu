// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Screens.Menu;
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

        public BeatmapDatabase(SQLiteConnection connection) : base(connection)
        {
        }

        protected override Type[] ValidTypes => new[] {
            typeof(BeatmapSetInfo),
            typeof(BeatmapInfo),
            typeof(BeatmapMetadata),
            typeof(BeatmapDifficulty),
        };

        protected override void Prepare(bool reset = false)
        {
            Connection.CreateTable<BeatmapMetadata>();
            Connection.CreateTable<BeatmapDifficulty>();
            Connection.CreateTable<BeatmapSetInfo>();
            Connection.CreateTable<BeatmapInfo>();

            if (reset)
            {
                Connection.DropTable<BeatmapMetadata>();
                Connection.DropTable<BeatmapDifficulty>();
                Connection.DropTable<BeatmapSetInfo>();
                Connection.DropTable<BeatmapInfo>();
            }

            deletePending();
        }

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

        public void Delete(IEnumerable<BeatmapSetInfo> beatmapSets)
        {
            foreach (var s in beatmapSets)
            {
                s.DeletePending = true;
                Update(s, false);
                BeatmapSetRemoved?.Invoke(s);
            }
        }

        private void deletePending()
        {
            foreach (var b in GetAllWithChildren<BeatmapSetInfo>(b => b.DeletePending))
            {
                if (b.Hash == Intro.MENU_MUSIC_BEATMAP_HASH)
                    // this is a bit hacky, but will do for now.
                    continue;

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