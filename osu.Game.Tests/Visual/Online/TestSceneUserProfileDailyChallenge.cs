// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
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
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneUserProfileDailyChallenge : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly Bindable<UserProfileData?> userProfileData = new Bindable<UserProfileData?>(new UserProfileData(new APIUser(), new OsuRuleset().RulesetInfo));

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        private DailyChallengeStatsDisplay display = null!;

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
                Add(display = new DailyChallengeStatsDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1f),
                    User = { BindTarget = userProfileData },
                });
            });

            AddStep("set local user", () => update(s => s.UserID = API.LocalUser.Value.Id));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("daily", 0, 999, 2, v => update(s => s.DailyStreakCurrent = v));
            AddSliderStep("daily best", 0, 999, 2, v => update(s => s.DailyStreakBest = v));
            AddSliderStep("weekly", 0, 250, 1, v => update(s => s.WeeklyStreakCurrent = v));
            AddSliderStep("weekly best", 0, 250, 1, v => update(s => s.WeeklyStreakBest = v));
            AddSliderStep("top 10%", 0, 999, 0, v => update(s => s.Top10PercentPlacements = v));
            AddSliderStep("top 50%", 0, 999, 0, v => update(s => s.Top50PercentPlacements = v));
            AddSliderStep("playcount", 0, 1500, 1, v => update(s => s.PlayCount = v));
        }

        [Test]
        public void TestStates()
        {
            AddStep("played today", () => update(s => s.LastUpdate = DateTimeOffset.UtcNow.Date));
            AddStep("played yesterday", () => update(s => s.LastUpdate = DateTimeOffset.UtcNow.Date.AddDays(-1)));
            AddStep("change to non-local user", () => update(s => s.UserID = API.LocalUser.Value.Id + 1000));

            AddStep("hover", () => InputManager.MoveMouseTo(display));
        }

        private void update(Action<APIUserDailyChallengeStatistics> change)
        {
            change.Invoke(userProfileData.Value!.User.DailyChallengeStatistics);
            userProfileData.Value = new UserProfileData(userProfileData.Value.User, userProfileData.Value.Ruleset);
        }

        [Test]
        public void TestPlayCountRankingTier()
        {
            AddAssert("1 before silver", () => DailyChallengeStatsTooltip.TierForPlayCount(29) == RankingTier.Bronze);
            AddAssert("first silver", () => DailyChallengeStatsTooltip.TierForPlayCount(30) == RankingTier.Silver);
        }
    }
}
