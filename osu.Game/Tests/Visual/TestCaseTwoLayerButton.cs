// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseTwoLayerButton : OsuTestCase
    {
        public override string Description => @"Mostly back button";

        public TestCaseTwoLayerButton()
        {
            Add(new BackButton());
        }
    }
}
