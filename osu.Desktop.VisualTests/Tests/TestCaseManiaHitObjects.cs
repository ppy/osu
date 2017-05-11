// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using OpenTK.Graphics;
using OpenTK;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseManiaHitObjects : TestCase
    {
        public override void Reset()
        {
            base.Reset();

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
                                RelativeCoordinateSpace = new Vector2(1, 10000),
                                Children = new[]
                                {
                                    new DrawableNote(new Note
                                    {
                                        StartTime = 5000
                                    })
                                    {
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
                                RelativeCoordinateSpace = new Vector2(1, 10000),
                                Children = new[]
                                {
                                    new DrawableHoldNote(new HoldNote
                                    {
                                        StartTime = 5000,
                                        Duration = 1000
                                    })
                                    {
                                        AccentColour = Color4.Red
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
