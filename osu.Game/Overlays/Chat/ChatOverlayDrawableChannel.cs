// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class ChatOverlayDrawableChannel : DrawableChannel
    {
        public ChatOverlayDrawableChannel(Channel channel)
            : base(channel)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // TODO: Remove once DrawableChannel & ChatLine padding is fixed
            ChatLineFlow.Padding = new MarginPadding(0);
        }

        protected override DaySeparator CreateDaySeparator(DateTimeOffset time) => new ChatOverlayDaySeparator(time);

        private class ChatOverlayDaySeparator : DaySeparator
        {
            public ChatOverlayDaySeparator(DateTimeOffset time)
                : base(time)
            {
                // TODO: Remove once DrawableChannel & ChatLine padding is fixed
                Padding = new MarginPadding { Horizontal = 15 };
            }
        }
    }
}
