// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;
using osu.Game.Overlays.Chat.Tabs;

namespace osu.Game.Tests.Visual.Online
{
    [Description("Testing chat api and overlay")]
    public class TestCaseChatDisplay : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChatOverlay),
            typeof(ChatLine),
            typeof(DrawableChannel),
            typeof(ChannelSelectorTabItem),
            typeof(ChannelTabControl),
            typeof(ChannelTabItem),
            typeof(PrivateChannelTabItem),
            typeof(TabCloseButton)
        };

        [Cached]
        private readonly ChannelManager channelManager = new ChannelManager();

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                channelManager,
                new ChatOverlay { State = Visibility.Visible }
            };
        }
    }
}
