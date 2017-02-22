// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework;
using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Threading;
using osu.Game;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using OpenTK;
using osu.Framework.Allocation;
using osu.Game.Online.Chat.Drawables;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseChatDisplay : TestCase
    {
        private ScheduledDelegate messageRequest;

        public override string Name => @"Chat";
        public override string Description => @"Testing chat api and overlay";

        public override void Reset()
        {
            base.Reset();

            Add(new ChatOverlay()
            {
                State = Visibility.Visible
            });
        }
    }
}
