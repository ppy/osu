// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning.Components;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneScoreAttributeText : OsuTestScene
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

        private ScoreAttributeText text = null!;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("set up score", () =>
            {
                var scoreInfo = TestResources.CreateTestScoreInfo(new OsuRuleset().RulesetInfo);
                scoreInfo.Date = new DateTimeOffset(2026, 10, 16, 9, 30, 0, TimeSpan.FromHours(2));
                scoreInfo.Mods =
                [
                    new OsuModHidden(),
                    new OsuModDoubleTime { SpeedChange = { Value = 1.3 } },
                ];
                scoreInfo.PP = 134;
                score.Value = scoreInfo;
            });
            AddStep("create component", () =>
            {
                Child = text = new ScoreAttributeText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                };
            });
        }

        [TestCase(ScoreAttribute.Username, "Played by: peppy")]
        [TestCase(ScoreAttribute.MaxCombo, "Max Combo: 999x")]
        [TestCase(ScoreAttribute.Accuracy, "Accuracy: 95.00%")]
        [TestCase(ScoreAttribute.Date, "Date: 10/16/2026 09:30:00")]
        [TestCase(ScoreAttribute.PP, "Performance: 134pp")]
        [TestCase(ScoreAttribute.Rank, "Rank: S")]
        [TestCase(ScoreAttribute.Mods, "Mods: HD, DT (Speed change: 1.30x)")]
        public void TestAttributeDisplay(ScoreAttribute attribute, string expectedText)
        {
            AddStep($"set attribute: {attribute}", () => text.Attribute.Value = attribute);
            AddAssert("check correct text", getText, () => Is.EqualTo(expectedText));
        }

        [Test]
        public void TestTotalScoreDisplayRespectsScoringMode()
        {
            AddStep("set total score attribute", () => text.Attribute.Value = ScoreAttribute.TotalScore);
            AddStep("set standardised scoring", () => configManager.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised));
            AddAssert("check correct text", getText, () => Is.EqualTo("Total Score: 284,537"));
            AddStep("set classic scoring", () => configManager.SetValue(OsuSetting.ScoreDisplayMode, ScoringMode.Classic));
            AddAssert("check correct text", getText, () => Is.EqualTo("Total Score: 8,766,110"));
        }

        [Test]
        public void TestAccessToStatistics()
        {
            AddStep("set template", () => text.Template.Value = @"{LargeTickHitLabel}: {LargeTickHit} / {LargeTickHitMax}");
            AddAssert("check correct text", getText, () => Is.EqualTo("slider tick: 100 / 150"));
        }

        private string getText() => text.ChildrenOfType<SpriteText>().Single().Text.ToString();
    }
}
