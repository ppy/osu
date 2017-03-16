// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Desktop.VisualTests.Platform;
using osu.Framework.Screens.Testing;
using osu.Framework.MathUtils;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCasePlaySongSelect : TestCase
    {
        private BeatmapDatabase db, oldDb;
        private TestStorage storage;
        private PlaySongSelect songSelect;

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

            Add(songSelect = new PlaySongSelect());

            AddButton(@"Sort by Artist", delegate { songSelect.FilterControl.Sort = SortMode.Artist; });
            AddButton(@"Sort by Title", delegate { songSelect.FilterControl.Sort = SortMode.Title; });
            AddButton(@"Sort by Author", delegate { songSelect.FilterControl.Sort = SortMode.Author; });
            AddButton(@"Sort by Difficulty", delegate { songSelect.FilterControl.Sort = SortMode.Difficulty; });
        }

        protected override void Dispose(bool isDisposing)
        {
            if (oldDb != null)
            {
                Dependencies.Cache(oldDb, true);
                db = null;
            }

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
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = "MONACA " + RNG.Next(0, 9),
                    Title = "Black Song " + RNG.Next(0, 9),
                    Author = "Some Guy " + RNG.Next(0, 9),
                },
                Beatmaps = new List<BeatmapInfo>(new[]
                {
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1234 + i,
                        Mode = PlayMode.Osu,
                        Path = "normal.osu",
                        Version = "Normal",
                        Difficulty = new BeatmapDifficulty
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
                        Difficulty = new BeatmapDifficulty
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
                        Difficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 7,
                        }
                    },
                }),
            };
        }
    }
}
