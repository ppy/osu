// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    [Description("Testing chat api and overlay")]
    internal class TestCaseChatDisplay : OsuTestCase
    {
        public TestCaseChatDisplay()
        {
            Add(new ChatOverlay
            {
                State = Visibility.Visible
            });
        }
    }
}
