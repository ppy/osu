//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using System.Collections.Generic;
using osu.Desktop.Platform;
using osu.Game.Database;
using osu.Game.GameModes.Play;
using SQLiteNetExtensions.Extensions;
using osu.Framework;
using osu.Game;

namespace osu.Desktop.Tests
{
    class TestCasePlaySongSelect : TestCase
    {
        private BeatmapDatabase db;
        private TestStorage storage;

        public override string Name => @"Song Select";
        public override string Description => @"with fake data";

        public override void Reset()
        {
            base.Reset();

            if (db == null)
            {
                storage = new TestStorage(@"TestCasePlaySongSelect");
                db = new BeatmapDatabase(storage);

                var sets = new List<BeatmapSetInfo>();

                for (int i = 0; i < 100; i += 10)
                    sets.Add(createTestBeatmapSet(i));

                db.Import(sets);
            }

            Add(new PlaySongSelect(db));
        }

        private BeatmapSetInfo createTestBeatmapSet(int i)
        {
            return new BeatmapSetInfo
            {
                BeatmapSetID = 1234 + i,
                Hash = "d8e8fca2dc0f896fd7cb4cb0031ba249",
                Path = string.Empty,
                Metadata = new BeatmapMetadata
                {
                    BeatmapSetID = 1234 + i,
                    Artist = "MONACA",
                    Title = "Black Song",
                    Author = "Some Guy",
                },
                Beatmaps = new List<BeatmapInfo>(new[]
                {
                    new BeatmapInfo
                    {
                        BeatmapID = 1234 + i,
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
                        BeatmapID = 1235 + i,
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
                        BeatmapID = 1236 + i,
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
