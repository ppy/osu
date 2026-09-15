// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Skinning;
using osu.Game.Skinning.Select;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneLegacyFooterUser : OsuTestScene
    {
        [Cached(typeof(LocalUserStatisticsProvider))]
        private readonly TestUserStatisticsProvider userStatisticsProvider = new TestUserStatisticsProvider();

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 100),
                    Colour = Color4.Black,
                },
                new SkinProvidingContainer(skins.DefaultClassicSkin)
                {
                    Child = new LegacyFooterUser
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                },
            };

            updateStatistics();
        });

        [Test]
        public void TestDisplay()
        {
            AddStep("rank #123456", () => updateStatistics(s => s.GlobalRank = 123456));
            AddStep("rank #200000", () => updateStatistics(s => s.GlobalRank = 200000));
            AddStep("rank #100000", () => updateStatistics(s => s.GlobalRank = 100000));
            AddStep("rank #50000", () => updateStatistics(s => s.GlobalRank = 50000));
            AddStep("rank #1000", () => updateStatistics(s => s.GlobalRank = 1000));
            AddStep("rank #10", () => updateStatistics(s => s.GlobalRank = 10));
            AddStep("rank #1", () => updateStatistics(s => s.GlobalRank = 1));
            AddStep("level 0%", () => updateStatistics(s => s.Level.Progress = 0));
            AddStep("level 25%", () => updateStatistics(s => s.Level.Progress = 25));
            AddStep("level 50%", () => updateStatistics(s => s.Level.Progress = 50));
            AddStep("level 75%", () => updateStatistics(s => s.Level.Progress = 75));
            AddStep("level 99%", () => updateStatistics(s => s.Level.Progress = 99));
            AddStep("osu ruleset", () => Ruleset.Value = new OsuRuleset().RulesetInfo);
            AddStep("taiko ruleset", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("catch ruleset", () => Ruleset.Value = new CatchRuleset().RulesetInfo);
            AddStep("mania ruleset", () => Ruleset.Value = new ManiaRuleset().RulesetInfo);
        }

        private void updateStatistics(Action<UserStatistics>? apply = null)
        {
            var statistics = getStats(apply);

            foreach (var ruleset in rulesets.AvailableRulesets)
                userStatisticsProvider.UpdateStatistics(statistics, ruleset);
        }

        private UserStatistics? currentStatistics;

        private UserStatistics getStats(Action<UserStatistics>? apply = null)
        {
            currentStatistics ??= new UserStatistics
            {
                User = new APIUser { Username = "Boat", Id = 3 },
                PP = 7272,
                Accuracy = 98.76,
                Level = { Current = 45, Progress = 56 },
                GlobalRank = 123456,
            };

            apply?.Invoke(currentStatistics);
            return currentStatistics;
        }

        public partial class TestUserStatisticsProvider : LocalUserStatisticsProvider
        {
            public new void UpdateStatistics(UserStatistics newStatistics, RulesetInfo ruleset, Action<UserStatisticsUpdate>? callback = null)
                => base.UpdateStatistics(newStatistics, ruleset, callback);
        }
    }
}
