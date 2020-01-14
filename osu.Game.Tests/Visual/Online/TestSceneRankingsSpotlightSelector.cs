// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Rankings;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsSpotlightSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SpotlightSelector),
        };

        public TestSceneRankingsSpotlightSelector()
        {
            SpotlightSelector selector;

            Add(selector = new SpotlightSelector());

            var spotlights = new[]
            {
                new APISpotlight { Name = "Spotlight 1" },
                new APISpotlight { Name = "Spotlight 2" },
                new APISpotlight { Name = "Spotlight 3" },
            };

            AddStep("Load spotlights", () => selector.Spotlights = spotlights);
            AddStep("Load info", () => selector.UpdateInfo(new APISpotlight
            {
                StartDate = DateTimeOffset.Now,
                EndDate = DateTimeOffset.Now,
                ParticipantCount = 15155151,
            }, 18));
        }
    }
}
