//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.GameModes.Testing;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Screens.Play;
using OpenTK.Graphics;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCasePlayer : TestCase
    {
        private WorkingBeatmap beatmap;
        public override string Name => @"Player";

        public override string Description => @"Showing everything to play the game.";

        [BackgroundDependencyLoader]
        private void load(BeatmapDatabase db)
        {
            beatmap = db.GetWorkingBeatmap(db.Query<BeatmapInfo>().Where(b => b.Mode == PlayMode.Osu).FirstOrDefault());
        }

        public override void Reset()
        {
            base.Reset();

            //ensure we are at offset 0
            Clock = new FramedClock();

            if (beatmap == null)
            {

                var objects = new List<HitObject>();

                int time = 1500;
                for (int i = 0; i < 50; i++)
                {
                    objects.Add(new HitCircle()
                    {
                        StartTime = time,
                        Position = new Vector2(i % 4 == 0 || i % 4 == 2 ? 0 : 512,
                        i % 4 < 2 ? 0 : 384),
                        NewCombo = i % 4 == 0
                    });

                    time += 500;
                }

                var decoder = new ConstructableBeatmapDecoder();

                Beatmap b = new Beatmap
                {
                    HitObjects = objects
                };

                decoder.Process(b);

                beatmap = new WorkingBeatmap(b);
            }

            Add(new Box
            {
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
                Colour = Color4.Gray,
            });

            Add(new Player
            {
                PreferredPlayMode = PlayMode.Osu,
                Beatmap = beatmap
            });
        }
    }
}
