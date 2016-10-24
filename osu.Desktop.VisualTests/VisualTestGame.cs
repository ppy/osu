// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Cursor;
using osu.Game.Database;
using osu.Game;
using osu.Framework.Desktop.Platform;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using osu.Game.GameModes.Play;
using SQLiteNetExtensions.Extensions;

namespace osu.Framework.VisualTests
{
    class VisualTestGame : OsuGameBase
    {
        private void InsertTestMap(int i)
        {
            var beatmapSet = new BeatmapSetInfo
            {
                BeatmapSetID = 1234 + i,
                Hash = "d8e8fca2dc0f896fd7cb4cb0031ba249",
                Path = "/foo/bar/baz",
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
            BeatmapDatabase.Connection.InsertWithChildren(beatmapSet, true);
        }
    
        public override void Load(BaseGame game)
        {
            (Host.Storage as DesktopStorage).InMemorySQL = true;
            base.Load(game);
            for (int i = 0; i < 100; i += 10)
                InsertTestMap(i);
            Add(new TestBrowser());
        }
    }
}
