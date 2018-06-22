// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual
{
    [Description("mostly back button")]
    public class TestCaseTwoLayerButton : OsuTestCase
    {
        public TestCaseTwoLayerButton()
        {
            Add(new BackButton());
        }
    }
}
