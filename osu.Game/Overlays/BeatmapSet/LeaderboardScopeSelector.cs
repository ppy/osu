// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osuTK.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class LeaderboardScopeSelector : GradientLineTabControl<BeatmapLeaderboardScope>
    {
        protected override bool AddEnumEntriesAutomatically => false;

        protected override TabItem<BeatmapLeaderboardScope> CreateTabItem(BeatmapLeaderboardScope value) => new ScopeSelectorTabItem(value);

        public LeaderboardScopeSelector()
        {
            AddItem(BeatmapLeaderboardScope.Global);
            AddItem(BeatmapLeaderboardScope.Country);
            AddItem(BeatmapLeaderboardScope.Friend);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Blue;
            LineColour = Color4.Gray;
        }

        private class ScopeSelectorTabItem : PageTabItem
        {
            public ScopeSelectorTabItem(BeatmapLeaderboardScope value)
                : base(value)
            {
                Text.Font = OsuFont.GetFont(size: 16);
            }

            protected override bool OnHover(HoverEvent e)
            {
                Text.FadeColour(AccentColour);

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                Text.FadeColour(Color4.White);
            }
        }
    }
}
