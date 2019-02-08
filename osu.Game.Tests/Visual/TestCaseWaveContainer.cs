﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseWaveContainer : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            WaveContainer container;
            Add(container = new WaveContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(400),
                FirstWaveColour = colours.Red,
                SecondWaveColour = colours.Green,
                ThirdWaveColour = colours.Blue,
                FourthWaveColour = colours.Pink,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        TextSize = 20,
                        Text = @"Wave Container",
                    },
                },
            });

            AddStep(@"show", container.Show);
            AddStep(@"hide", container.Hide);
        }
    }
}
