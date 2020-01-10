// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays.Rankings;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsSpotlightSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SpotlightSelector),
        };

        protected override bool UseOnlineAPI => true;

        public TestSceneRankingsSpotlightSelector()
        {
            SpotlightSelector selector;

            Add(selector = new SpotlightSelector());

            AddStep("Fetch spotlights", selector.FetchSpotlights);
        }
    }
}
