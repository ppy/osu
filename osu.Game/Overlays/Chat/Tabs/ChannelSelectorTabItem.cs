// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat.Tabs
{
    public class ChannelSelectorTabItem : ChannelTabItem
    {
        public override bool IsRemovable => false;

        public override bool IsSwitchable => false;

        public ChannelSelectorTabItem(Channel value) : base(value)
        {
            Depth = float.MaxValue;
            Width = 45;

            Icon.Alpha = 0;

            Text.TextSize = 45;
            TextBold.TextSize = 45;
        }

        [BackgroundDependencyLoader]
        private new void load(OsuColour colour)
        {
            BackgroundInactive = colour.Gray2;
            BackgroundActive = colour.Gray3;
        }
    }
}
