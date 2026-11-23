// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneSeasonProfileCard : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        private readonly Bindable<UserProfileData?> userData = new Bindable<UserProfileData?>(new UserProfileData(new APIUser(), new OsuRuleset().RulesetInfo));

        private SeasonProfileCard profileCard = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create", () => Child = new Container
            {
                AutoSizeAxes = Axes.Y,
                Width = 300,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = profileCard = new SeasonProfileCard { User = { BindTarget = userData } },
            });
        }

        [Test]
        public void TestTiers()
        {
            AddStep("set iron", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Iron)));
            AddStep("set bronze", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Bronze)));
            AddStep("set silver", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Silver)));
            AddStep("set gold", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Gold)));
            AddStep("set platinum", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Platinum)));
            AddStep("set diamond", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Radiant)));
            AddStep("set rhythm incarnate", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Lustrous)));

            AddStep("hover", () => InputManager.MoveMouseTo(profileCard));
        }

        [Test]
        public void TestNoStatistics()
        {
            AddStep("set iron", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Iron)));
            AddAssert("card is present", () => profileCard.IsPresent, () => Is.True);

            AddStep("remove statistics", () => update(u => u.CurrentSeasonStatistics = null));
            AddAssert("card is not present", () => profileCard.IsPresent, () => Is.False);
        }

        [Test]
        public void TestRulesetVariants()
        {
            AddStep("set mania 4K", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Gold, 3, 4)));
            AddAssert("4K in division name", () => profileCard.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Equals("Gold I (4K)", StringComparison.Ordinal)), () => Is.True);

            AddStep("set mania 7K", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Gold, 3, 7)));
            AddAssert("7K in division name", () => profileCard.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Equals("Gold I (7K)", StringComparison.Ordinal)), () => Is.True);

            AddStep("set no ruleset", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Gold)));
            AddAssert("no variant displayed", () => profileCard.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Equals("Gold I", StringComparison.Ordinal)), () => Is.True);

            AddStep("set osu", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Gold, 1)));
            AddAssert("no variant displayed", () => profileCard.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Equals("Gold I", StringComparison.Ordinal)), () => Is.True);

            AddStep("set taiko", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Gold, 2)));
            AddAssert("no variant displayed", () => profileCard.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Equals("Gold I", StringComparison.Ordinal)), () => Is.True);

            AddStep("set catch", () => update(u => u.CurrentSeasonStatistics = createStatistics(RankingTier.Gold, 3)));
            AddAssert("no variant displayed", () => profileCard.ChildrenOfType<IHasText>().Any(t => t.Text.ToString().Equals("Gold I", StringComparison.Ordinal)), () => Is.True);
        }

        private void update(Action<APIUser> change)
        {
            change.Invoke(userData.Value!.User);
            userData.Value = new UserProfileData(userData.Value.User, userData.Value.Ruleset);
        }

        private APISeasonStatistics createStatistics(RankingTier tier, int? rulesetID = null, int? variantID = null)
        {
            string tierName = tier switch
            {
                RankingTier.Radiant => "Diamond",
                RankingTier.Lustrous => "Rhythm Incarnate",
                _ => tier.ToString(),
            };

            string tierUrlKey = tier switch
            {
                RankingTier.Radiant => "diamond",
                RankingTier.Lustrous => "ri",
                _ => tier.ToString().ToLowerInvariant(),
            };

            return new APISeasonStatistics
            {
                Rank = 12,
                TotalScore = 53412343,
                Season = new APISeason
                {
                    Name = "osu! Beatmap Spotlights: Autumn 2026",
                    RulesetID = rulesetID,
                    VariantID = variantID,
                },
                Division = new APISeasonDivision
                {
                    Name = $"{tierName} I",
                    Tier = tier,
                    ImageUrl = $"https://assets.ppy.sh/profile-badges/spotlights-2026/autumn-2026-osu-{tierUrlKey}-1@2x.png",
                    Threshold = RNG.NextSingle(0, 1),
                },
            };
        }
    }
}
