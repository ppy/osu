// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat.Tabs
{
    public class ChannelSelectorTabItem : ChannelTabItem
    {
        public override bool IsRemovable => false;

        public override bool IsSwitchable => false;

        protected override bool IsBoldWhenActive => false;

        public ChannelSelectorTabItem()
            : base(new ChannelSelectorTabChannel())
        {
            Depth = float.MaxValue;
            Width = 45;

            Icon.Alpha = 0;

            Text.Font = Text.Font.With(size: 45);
            Text.Truncate = false;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            BackgroundInactive = colour.Gray2;
            BackgroundActive = colour.Gray3;
        }

        public class ChannelSelectorTabChannel : Channel
        {
            public ChannelSelectorTabChannel()
            {
                Name = "+";
            }
        }
    }
}
