// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.BeatmapSet;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneLeaderboardScopeSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LeaderboardScopeSelector),
        };

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
