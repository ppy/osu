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
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Skinning.Components;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneBeatmapAttributeText : OsuTestScene
    {
        private readonly BeatmapAttributeText text;

        public TestSceneBeatmapAttributeText()
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

        [TestCase(BeatmapAttribute.CircleSize, "Circle Size: 1")]
        [TestCase(BeatmapAttribute.HPDrain, "HP Drain: 2")]
        [TestCase(BeatmapAttribute.Accuracy, "Accuracy: 3")]
        [TestCase(BeatmapAttribute.ApproachRate, "Approach Rate: 4")]
        [TestCase(BeatmapAttribute.Title, "Title: _Title")]
        [TestCase(BeatmapAttribute.Artist, "Artist: _Artist")]
        [TestCase(BeatmapAttribute.Creator, "Creator: _Creator")]
        [TestCase(BeatmapAttribute.DifficultyName, "Difficulty: _Difficulty")]
        [TestCase(BeatmapAttribute.Source, "Source: _Source")]
        [TestCase(BeatmapAttribute.RankedStatus, "Beatmap Status: Loved")]
        public void TestAttributeDisplay(BeatmapAttribute attribute, string expectedText)
        {
            AddStep($"set attribute: {attribute}", () => text.Attribute.Value = attribute);
            AddAssert("check correct text", getText, () => Is.EqualTo(expectedText));
        }

        [Test]
        public void TestChangeBeatmap()
        {
            AddStep("set title attribute", () => text.Attribute.Value = BeatmapAttribute.Title);
            AddAssert("check initial title", getText, () => Is.EqualTo("Title: _Title"));

            AddStep("change to beatmap with another title", () => Beatmap.Value = CreateWorkingBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo =
                {
                    Metadata =
                    {
                        Title = "Another"
                    }
                }
            }));

            AddAssert("check new title", getText, () => Is.EqualTo("Title: Another"));
        }

        [Test]
        public void TestWithMods()
        {
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo =
                {
                    BPM = 100,
                    Length = 30000,
                    Difficulty =
                    {
                        ApproachRate = 10,
                        CircleSize = 9.5f
                    }
                }
            }));

            test(BeatmapAttribute.BPM, new OsuModDoubleTime(), "BPM: 100", "BPM: 150");
            test(BeatmapAttribute.Length, new OsuModDoubleTime(), "Length: 00:30", "Length: 00:20");
            test(BeatmapAttribute.ApproachRate, new OsuModDoubleTime(), "Approach Rate: 10", "Approach Rate: 11");
            test(BeatmapAttribute.CircleSize, new OsuModHardRock(), "Circle Size: 9.5", "Circle Size: 10");

            void test(BeatmapAttribute attribute, Mod mod, string before, string after)
            {
                AddStep($"set attribute: {attribute}", () => text.Attribute.Value = attribute);
                AddAssert("check text is correct", getText, () => Is.EqualTo(before));

                AddStep("add DT mod", () => SelectedMods.Value = new[] { mod });
                AddAssert("check text is correct", getText, () => Is.EqualTo(after));
                AddStep("clear mods", () => SelectedMods.SetDefault());
            }
        }

        [Test]
        public void TestStarRating()
        {
            AddStep("set test ruleset", () => Ruleset.Value = new TestRuleset().RulesetInfo);
            AddStep("set star rating attribute", () => text.Attribute.Value = BeatmapAttribute.StarRating);
            AddAssert("check star rating is 0", getText, () => Is.EqualTo("Star Rating: 0.00"));

            // Adding mod
            TestMod mod = null!;
            AddStep("add mod with difficulty 1", () => SelectedMods.Value = new[] { mod = new TestMod { Difficulty = { Value = 1 } } });
            AddUntilStep("check star rating is 1", getText, () => Is.EqualTo("Star Rating: 1.00"));

            // Changing mod setting
            AddStep("change mod difficulty to 2", () => mod.Difficulty.Value = 2);
            AddUntilStep("check star rating is 2", getText, () => Is.EqualTo("Star Rating: 2.00"));
        }

        [Test]
        public void TestMaxPp()
        {
            AddStep("set test ruleset", () => Ruleset.Value = new TestRuleset().RulesetInfo);
            AddStep("set max pp attribute", () => text.Attribute.Value = BeatmapAttribute.MaxPP);
            AddAssert("check max pp is 0", getText, () => Is.EqualTo("Max PP: 0"));

            // Adding mod
            TestMod mod = null!;
            AddStep("add mod with pp 1", () => SelectedMods.Value = new[] { mod = new TestMod { Performance = { Value = 1 } } });
            AddUntilStep("check max pp is 1", getText, () => Is.EqualTo("Max PP: 1"));

            // Changing mod setting
            AddStep("change mod pp to 2", () => mod.Performance.Value = 2);
            AddUntilStep("check max pp is 2", getText, () => Is.EqualTo("Max PP: 2"));
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
