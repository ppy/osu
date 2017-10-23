// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseChatDisplay : OsuTestCase
    {
        public override string Description => @"Testing chat api and overlay";

        public TestCaseChatDisplay()
        {
            Add(new ChatOverlay
            {
                State = Visibility.Visible
            });
        }
    }
}
