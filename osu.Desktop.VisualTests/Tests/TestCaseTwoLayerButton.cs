// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTwoLayerButton : TestCase
    {
        public override string Description => @"Mostly back button";

        public TestCaseTwoLayerButton()
        {
            Add(new BackButton());
        }
    }
}
