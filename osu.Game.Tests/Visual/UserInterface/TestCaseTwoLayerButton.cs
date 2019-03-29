// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.UserInterface
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
