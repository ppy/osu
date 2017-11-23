// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Menu;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [Description("main menu")]
    internal class TestCaseMenuButtonSystem : OsuTestCase
    {
        public TestCaseMenuButtonSystem()
        {
            Add(new Box
            {
                Colour = ColourInfo.GradientVertical(Color4.Gray, Color4.WhiteSmoke),
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
            });
            Add(new ButtonSystem());
        }
    }
}
