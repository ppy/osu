// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Menu;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestCaseButtonSystem : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ButtonSystem),
            typeof(ButtonArea),
            typeof(Button)
        };

        public TestCaseButtonSystem()
        {
            OsuLogo logo;
            ButtonSystem buttons;

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = ColourInfo.GradientVertical(Color4.Gray, Color4.WhiteSmoke),
                    RelativeSizeAxes = Axes.Both,
                },
                buttons = new ButtonSystem(),
                logo = new OsuLogo()
            };

            buttons.SetOsuLogo(logo);

            foreach (var s in Enum.GetValues(typeof(ButtonSystemState)).OfType<ButtonSystemState>().Skip(1))
                AddStep($"State to {s}", () => buttons.State = s);
        }
    }
}
