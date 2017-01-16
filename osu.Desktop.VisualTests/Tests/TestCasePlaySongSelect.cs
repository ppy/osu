//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Desktop.VisualTests.Platform;
using osu.Framework.GameModes.Testing;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Screens.Select;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCasePlaySongSelect : TestCase
    {
        private BeatmapDatabase db, oldDb;
        private TestStorage storage;

        public override string Name => @"Song Select";
        public override string Description => @"with fake data";

        public override void Reset()
        {
            base.Reset();
            oldDb = Dependencies.Get<BeatmapDatabase>();
            if (db == null)
            {
                storage = new TestStorage(@"TestCasePlaySongSelect");
                db = new BeatmapDatabase(storage);
                Dependencies.Cache(db, true);

                var sets = new List<BeatmapSetInfo>();

                for (int i = 0; i < 100; i += 10)
                    sets.Add(createTestBeatmapSet(i));

                db.Import(sets);
            }
            Add(new PlaySongSelect());
        }
        
        protected override void Dispose(bool isDisposing)
        {
            if (oldDb != null)
                Dependencies.Cache(oldDb, true);
            base.Dispose(isDisposing);
        }

        private BeatmapSetInfo createTestBeatmapSet(int i)
        {
            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = 1234 + i,
                Hash = "d8e8fca2dc0f896fd7cb4cb0031ba249",
                Path = string.Empty,
                Metadata = new BeatmapMetadata
                {
                    OnlineBeatmapSetID = 1234 + i,
                    Artist = "MONACA",
                    Title = "Black Song",
                    Author = "Some Guy",
                },
                Beatmaps = new List<BeatmapInfo>(new[]
                {
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1234 + i,
                        Mode = PlayMode.Osu,
                        Path = "normal.osu",
                        Version = "Normal",
                        BaseDifficulty = new BaseDifficulty
                        {
                            OverallDifficulty = 3.5f,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1235 + i,
                        Mode = PlayMode.Osu,
                        Path = "hard.osu",
                        Version = "Hard",
                        BaseDifficulty = new BaseDifficulty
                        {
                            OverallDifficulty = 5,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1236 + i,
                        Mode = PlayMode.Osu,
                        Path = "insane.osu",
                        Version = "Insane",
                        BaseDifficulty = new BaseDifficulty
                        {
                            OverallDifficulty = 7,
                        }
                    },
                }),
            };
        }
    }
}
