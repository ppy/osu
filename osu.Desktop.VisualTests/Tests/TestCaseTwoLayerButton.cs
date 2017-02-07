// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using OpenTK.Graphics;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseTwoLayerButton : TestCase
    {
        public override string Name => @"TwoLayerButton";
        public override string Description => @"Back and skip and what not";

        public override void Reset()
        {
            base.Reset();

            Add(new BackButton());
            Add(new SkipButton());
        }
    }
}
