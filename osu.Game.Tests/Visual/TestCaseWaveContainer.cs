// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

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
