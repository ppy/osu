// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Rulesets.Osu;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneUserProfileMatchmakingStatsDisplay : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly Bindable<UserProfileData?> userProfileData = new Bindable<UserProfileData?>(new UserProfileData(new APIUser(), new OsuRuleset().RulesetInfo));

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create", () =>
            {
                Clear();
                Add(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background2,
                });
                Add(new MatchmakingStatsDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1f),
                    User = { BindTarget = userProfileData },
                });
            });

            AddStep("set stats", () => userProfileData.Value = new UserProfileData(new APIUser
            {
                MatchmakingStatistics =
                [
                    new APIUserMatchmakingStatistics
                    {
                        Plays = 10,
                        FirstPlacements = 8,
                        Rank = 1000,
                        Rating = 2000,
                        TotalPoints = 500,
                        Pool =
                        {
                            Name = "Active Pool"
                        }
                    },
                    new APIUserMatchmakingStatistics
                    {
                        Plays = 5,
                        FirstPlacements = 4,
                        Rank = 500,
                        Rating = 1000,
                        TotalPoints = 250,
                        Pool =
                        {
                            Name = "Inactive Pool"
                        }
                    },
                    new APIUserMatchmakingStatistics
                    {
                        Rating = 1500,
                        IsRatingProvisional = true,
                        Pool =
                        {
                            Name = "Provisional"
                        }
                    }
                ]
            }, new OsuRuleset().RulesetInfo));

            AddStep("clear stats", () => userProfileData.Value = null);
        }
    }
}
