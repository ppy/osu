// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class LeaderboardScopeSelector : GradientLineTabControl<BeatmapLeaderboardScope>
    {
        protected override bool AddEnumEntriesAutomatically => false;

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
    }
}
