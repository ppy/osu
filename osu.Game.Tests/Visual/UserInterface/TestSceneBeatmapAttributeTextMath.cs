// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Skinning.Components;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneBeatmapAttributeTextMath : OsuTestScene
    {
        private readonly BeatmapAttributeText text;
        public TestSceneBeatmapAttributeTextMath()
        {
            Child = text = new BeatmapAttributeText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            SelectedMods.SetDefault();
            Ruleset.Value = new OsuRuleset().RulesetInfo;
            Beatmap.Value = CreateWorkingBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo =
                {
                    BPM = 100,
                    DifficultyName = "_Difficulty",
                    Status = BeatmapOnlineStatus.Loved,
                    Metadata =
                    {
                        Title = "_Title",
                        TitleUnicode = "_Title",
                        Artist = "_Artist",
                        ArtistUnicode = "_Artist",
                        Author = new RealmUser { Username = "_Creator" },
                        Source = "_Source",
                    },
                    Difficulty =
                    {
                        CircleSize = 1,
                        DrainRate = 2,
                        OverallDifficulty = 3,
                        ApproachRate = 4,
                    }
                }
            });
        });

        [TestCase("{CircleSize+1}", "2")]
        [TestCase("{CircleSize+-1}", "0")]
        [TestCase("{CircleSize-1}", "0")]
        [TestCase("{-CircleSize}", "-1")]
        [TestCase("{ {-CircleSize}", "{ -1")]
        [TestCase("{(-CircleSize)}", "-1")]
        [TestCase("{-(CircleSize)}", "-1")]
        [TestCase("{(-Circleize)}", "{(-Circleize)}")]
        [TestCase("{4+ApproachRate/4}", "5")]
        [TestCase("{(4+ApproachRate)/4}", "2")]
        [TestCase("{-CircleSize} {CircleSize+0} {CircleSize+}", "-1 1 {CircleSize+}")]
        [TestCase("{-CircleSize} {CircleSize +0} {CircleSize+0}", "-1 {CircleSize +0} 1")]
        [TestCase("{()()()()}", "{()()()()}")]
        [TestCase("{-()()()()}", "{-()()()()}")]
        [TestCase("{()+()+()+()}", "{()+()+()+()}")]
        [TestCase("{(ApproachRate)(ApproachRate)(ApproachRate)(ApproachRate)}", "256")]
        [TestCase("{(ApproachRate-(ApproachRate*ApproachRate*ApproachRate))}", "-60")]
        [TestCase("{1/0}", "{1/0}")]
        [TestCase("{1%0}", "{1%0}")]
        [TestCase("{(-1)+1*1*1+(-1-1)}", "-2")]
        [TestCase("{4%3}", "1")]
        [TestCase("{4%1.5}", "{4%1.5}")]
        [TestCase("{-1}", "-1")]
        [TestCase("{(-1)}", "-1")]
        [TestCase("{4(CircleSize)}", "4")]
        [TestCase("{(ApproachRate)(ApproachRate)}", "16")]
        [TestCase("(ApproachRate-CircleSize)(Accuracy)={(ApproachRate-CircleSize)(Accuracy)}", "(ApproachRate-CircleSize)(Accuracy)=9")]
        public void TestAttributeMathDisplay(string inputText, string expectedText)
        {
            AddStep($"set text: \"{inputText}\"", () => text.Template.Value = inputText);
            AddAssert("check correct text", getText, () => Is.EqualTo(expectedText));
        }

        private string getText() => text.ChildrenOfType<SpriteText>().Single().Text.ToString();
        private class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => new[]
            {
                new TestMod()
            };

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap)
                => new OsuRuleset().CreateBeatmapConverter(beatmap);

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap)
                => new TestDifficultyCalculator(new TestRuleset().RulesetInfo, beatmap);

            public override PerformanceCalculator CreatePerformanceCalculator()
                => new TestPerformanceCalculator(new TestRuleset());

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
                => null!;

            public override string Description => string.Empty;
            public override string ShortName => string.Empty;
        }

        private class TestDifficultyCalculator : DifficultyCalculator
        {
            public TestDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
                : base(ruleset, beatmap)
            {
            }

            protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
                => new DifficultyAttributes(mods, mods.OfType<TestMod>().SingleOrDefault()?.Difficulty.Value ?? 0);

            protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
                => Array.Empty<DifficultyHitObject>();

            protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
                => Array.Empty<Skill>();
        }

        private class TestPerformanceCalculator : PerformanceCalculator
        {
            public TestPerformanceCalculator(Ruleset ruleset)
                : base(ruleset)
            {
            }

            protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
                => new PerformanceAttributes { Total = score.Mods.OfType<TestMod>().SingleOrDefault()?.Performance.Value ?? 0 };
        }

        private class TestMod : Mod
        {
            [SettingSource("difficulty")]
            public BindableDouble Difficulty { get; } = new BindableDouble(0);

            [SettingSource("performance")]
            public BindableDouble Performance { get; } = new BindableDouble(0);

            [JsonConstructor]
            public TestMod()
            {
            }

            public override string Name => string.Empty;
            public override LocalisableString Description => string.Empty;
            public override double ScoreMultiplier => 1.0;
            public override string Acronym => "Test";
        }
    }
}
