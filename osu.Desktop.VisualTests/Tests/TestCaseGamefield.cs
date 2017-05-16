// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Taiko.UI;
using System.Collections.Generic;
using osu.Desktop.VisualTests.Beatmaps;
using osu.Framework.Allocation;
using osu.Game.Beatmaps.Timing;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseGamefield : TestCase
    {
        private RulesetDatabase rulesets;

        public override string Description => @"Showing hitobjects and what not.";

        [BackgroundDependencyLoader]
        private void load(RulesetDatabase rulesets)
        {
            this.rulesets = rulesets;
        }

        public override void Reset()
        {
            base.Reset();

            List<HitObject> objects = new List<HitObject>();

            int time = 500;
            for (int i = 0; i < 100; i++)
            {
                objects.Add(new HitCircle
                {
                    StartTime = time,
                    Position = new Vector2(RNG.Next(0, (int)OsuPlayfield.BASE_SIZE.X), RNG.Next(0, (int)OsuPlayfield.BASE_SIZE.Y)),
                    Scale = RNG.NextSingle(0.5f, 1.0f),
                });

                time += RNG.Next(50, 500);
            }

            TimingInfo timing = new TimingInfo();
            timing.ControlPoints.Add(new ControlPoint
            {
                BeatLength = 200
            });

            WorkingBeatmap beatmap = new TestWorkingBeatmap(new Beatmap
            {
                HitObjects = objects,
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Ruleset = rulesets.Query<RulesetInfo>().First(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        Author = @"peppy",
                    },
                },
                TimingInfo = timing
            });

            Add(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    //ensure we are at offset 0
                    Clock = new FramedClock(),
                    Children = new Drawable[]
                    {
                        new OsuHitRenderer(beatmap)
                        {
                            Scale = new Vector2(0.5f),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft
                        },
                        new TaikoHitRenderer(beatmap)
                        {
                            Scale = new Vector2(0.5f),
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight
                        },
                        new CatchHitRenderer(beatmap)
                        {
                            Scale = new Vector2(0.5f),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft
                        },
                        new ManiaHitRenderer(beatmap)
                        {
                            Scale = new Vector2(0.5f),
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight
                        }
                    }
                }
            });
        }
    }
}
