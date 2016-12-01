//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Menu;
using OpenTK.Graphics;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMenuButtonSystem : TestCase
    {
        public override string Name => @"ButtonSystem";
        public override string Description => @"Main menu button system";

        public override void Reset()
        {
            base.Reset();

            Add(new Box
            {
                ColourInfo = ColourInfo.GradientVertical(Color4.Gray, Color4.WhiteSmoke),
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
            });
            Add(new ButtonSystem());
        }
    }
}
