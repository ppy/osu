// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Tests.Visual;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseManiaHitObjects : OsuTestCase
    {
        public TestCaseManiaHitObjects()
        {
            Note note1 = new Note();
            Note note2 = new Note();
            HoldNote holdNote = new HoldNote { StartTime = 1000 };

            note1.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            note2.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            holdNote.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                // Imagine that the containers containing the drawable notes are the "columns"
                Children = new Drawable[]
                {
                    new Container
                    {
                        Name = "Normal note column",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Width = 50,
                        Children = new[]
                        {
                            new Container
                            {
                                Name = "Timing section",
                                RelativeSizeAxes = Axes.Both,
                                RelativeChildSize = new Vector2(1, 10000),
                                Children = new[]
                                {
                                    new DrawableNote(note1, ManiaAction.Key1)
                                    {
                                        Y = 5000,
                                        LifetimeStart = double.MinValue,
                                        LifetimeEnd = double.MaxValue,
                                        AccentColour = Color4.Red
                                    },
                                    new DrawableNote(note2, ManiaAction.Key1)
                                    {
                                        Y = 6000,
                                        LifetimeStart = double.MinValue,
                                        LifetimeEnd = double.MaxValue,
                                        AccentColour = Color4.Red
                                    }
                                }
                            }
                        }
                    },
                    new Container
                    {
                        Name = "Hold note column",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Width = 50,
                        Children = new[]
                        {
                            new Container
                            {
                                Name = "Timing section",
                                RelativeSizeAxes = Axes.Both,
                                RelativeChildSize = new Vector2(1, 10000),
                                Children = new[]
                                {
                                    new DrawableHoldNote(holdNote, ManiaAction.Key1)
                                    {
                                        Y = 5000,
                                        Height = 1000,
                                        LifetimeStart = double.MinValue,
                                        LifetimeEnd = double.MaxValue,
                                        AccentColour = Color4.Red,
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}
