// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
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

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly SpotlightSelector selector;

        public TestSceneRankingsSpotlightSelector()
        {
            Add(selector = new SpotlightSelector());
        }

        [Test]
        public void TestVisibility()
        {
            AddStep("Toggle Visibility", selector.ToggleVisibility);
        }

        [Test]
        public void TestLocalSpotlights()
        {
            var spotlights = new[]
            {
                new APISpotlight
                {
                    Name = "Spotlight 1",
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now,
                },
                new APISpotlight
                {
                    Name = "Spotlight 2",
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now,
                },
                new APISpotlight
                {
                    Name = "Spotlight 3",
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now,
                },
            };

            AddStep("load spotlights", () => selector.Spotlights = spotlights);
            AddStep("change to spotlight 3", () => selector.Current.Value = spotlights[2]);
        }

        [Test]
        public void TestOnlineSpotlights()
        {
            List<APISpotlight> spotlights = null;

            AddStep("retrieve spotlights", () =>
            {
                var req = new GetSpotlightsRequest();
                req.Success += res => spotlights = res.Spotlights;

                api.Perform(req);
            });

            AddStep("set spotlights", () =>
            {
                if (spotlights != null)
                    selector.Spotlights = spotlights;
            });
        }
    }
}
