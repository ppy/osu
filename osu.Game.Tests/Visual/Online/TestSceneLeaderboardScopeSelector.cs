// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.BeatmapSet;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneLeaderboardScopeSelector : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneLeaderboardScopeSelector()
        {
            Bindable<BeatmapLeaderboardScope> scope = new Bindable<BeatmapLeaderboardScope>();

            Add(new LeaderboardScopeSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = { BindTarget = scope }
            });

            AddStep(@"Select global", () => scope.Value = BeatmapLeaderboardScope.Global);
            AddStep(@"Select country", () => scope.Value = BeatmapLeaderboardScope.Country);
            AddStep(@"Select friend", () => scope.Value = BeatmapLeaderboardScope.Friend);
        }
    }
}
