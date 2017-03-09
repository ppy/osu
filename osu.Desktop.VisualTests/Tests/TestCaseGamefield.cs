// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Modes.Catch.UI;
using osu.Game.Modes.Mania.UI;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.Taiko.UI;
using OpenTK;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseGamefield : TestCase
    {
        public override string Description => @"Showing hitobjects and what not.";

        public override void Reset()
        {
            base.Reset();

            var objects = new List<HitObject>();

            int time = 1500;
            for (int i = 0; i < 100; i++)
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

            var decoder = new ConstructableBeatmapDecoder();

            Beatmap beatmap = new Beatmap
            {
                HitObjects = objects,
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BaseDifficulty
                    {
                        ApproachRate = 5,
                        CircleSize = 5,
                        DrainRate = 5,
                        OverallDifficulty = 5,
                        SliderMultiplier = 1.4,
                        SliderTickRate = 1
                    },
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        Author = @"peppy",
                    }
                }
            };

            decoder.Process(beatmap);

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
