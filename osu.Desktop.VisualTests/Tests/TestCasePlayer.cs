// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Screens.Play;
using OpenTK.Graphics;
using osu.Desktop.VisualTests.Beatmaps;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCasePlayer : TestCase
    {
        protected Player Player;
        private BeatmapDatabase db;

        public override string Description => @"Showing everything to play the game.";

        [BackgroundDependencyLoader]
        private void load(BeatmapDatabase db)
        {
            this.db = db;
        }

        public override void Reset()
        {
            base.Reset();

            WorkingBeatmap beatmap = null;

            var beatmapInfo = db.Query<BeatmapInfo>().FirstOrDefault(b => b.Mode == PlayMode.Osu);
            if (beatmapInfo != null)
                beatmap = db.GetWorkingBeatmap(beatmapInfo);

            if (beatmap?.Track == null)
            {
                var objects = new List<HitObject>();

                int time = 1500;
                for (int i = 0; i < 50; i++)
                {
                    objects.Add(new HitCircle
                    {
                        StartTime = time,
                        Position = new Vector2(i % 4 == 0 || i % 4 == 2 ? 0 : 512,
                        i % 4 < 2 ? 0 : 384),
                        NewCombo = i % 4 == 0
                    });

                    time += 500;
                }

                Beatmap b = new Beatmap
                {
                    HitObjects = objects,
                    BeatmapInfo = new BeatmapInfo
                    {
                        Difficulty = new BeatmapDifficulty(),
                        Metadata = new BeatmapMetadata
                        {
                            Artist = @"Unknown",
                            Title = @"Sample Beatmap",
                            Author = @"peppy",
                        }
                    }
                };

                beatmap = new TestWorkingBeatmap(b);
            }

            Add(new Box
            {
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
                Colour = Color4.Black,
            });

            Add(new PlayerLoader(Player = CreatePlayer(beatmap))
            {
                Beatmap = beatmap
            });
        }

        protected virtual Player CreatePlayer(WorkingBeatmap beatmap)
        {
            return new Player
            {
                Beatmap = beatmap
            };
        }
    }
}
