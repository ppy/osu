// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Menu;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseButtonSystem : OsuTestCase
    {
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
        }
    }
}
