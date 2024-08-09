// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using NUnit.Framework;
using osu.Framework.Testing;
using osuTK.Graphics;
using osu.Game.Skinning.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using System.Threading;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using System.Collections.Immutable;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneBeatmapAttributeText : OsuTestScene
    {
        private BeatmapAttributeText beatmapAttributeText = null!;

        private TestBeatmapDifficultyCache difficultyCache = null!;

        private IBeatmap easyBeatmap = null!;
        private IBeatmap hardBeatmap = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs<BeatmapDifficultyCache>(difficultyCache = new TestBeatmapDifficultyCache());
            easyBeatmap = initBeatmap("Easy Beatmap", 120, 3);
            hardBeatmap = initBeatmap("Hard Beatmap", 200, 7);

            AddRange(new Drawable[]
            {
                difficultyCache,
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Gray,
                },
                beatmapAttributeText = new BeatmapAttributeText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            });
        }

        [SetUp]
        public void SetUp()
        {
            Ruleset.Value = new TestOsuRuleset().RulesetInfo;

            Beatmap.Value = CreateWorkingBeatmap(easyBeatmap);
            SelectedMods.Value = Array.Empty<Mod>();

            beatmapAttributeText.Template.Value = "{Label}: {Value}";
            beatmapAttributeText.Attribute.Value = BeatmapAttribute.MaxPerformance;
        }

        [SetUpSteps]
        public void SetUpSteps() => AddUntilStep("Check correctness after setup", () => beatmapAttributeText.Text.ToString() == "Max Performance: 300pp");

        [Test]
        public void TestComponentSettingsChange()
        {
            AddStep("Change attribute to star rating", () => beatmapAttributeText.Attribute.Value = BeatmapAttribute.StarRating);
            AddUntilStep("Check for star rating correctness", () => beatmapAttributeText.Text.ToString() == "Star Rating: 3.00");

            AddStep("Change template", () => beatmapAttributeText.Template.Value = "{Value}*");
            AddUntilStep("Check for template update", () => beatmapAttributeText.Text.ToString() == "3.00*");
        }

        [Test]
        public void TestBeatmapChange()
        {
            AddStep("Select hard beatmap", () => Beatmap.Value = CreateWorkingBeatmap(hardBeatmap));
            AddUntilStep("Check for pp amount update", () => beatmapAttributeText.Text.ToString() == "Max Performance: 700pp");

            AddStep("Change attribute to title", () => beatmapAttributeText.Attribute.Value = BeatmapAttribute.Title);
            AddUntilStep("Check for title correctness", () => beatmapAttributeText.Text.ToString() == "Title: Hard Beatmap");

            AddStep("Select easy beatmap", () => Beatmap.Value = CreateWorkingBeatmap(easyBeatmap));
            AddUntilStep("Check for title update", () => beatmapAttributeText.Text.ToString() == "Title: Easy Beatmap");
        }

        [Test]
        public void TestModChange()
        {
            OsuModDoubleTime dt = new OsuModDoubleTime();

            AddStep("Select DT", delegate
            {
                dt.SpeedChange.SetDefault();
                SelectedMods.Value = SelectedMods.Value.Append(dt).ToImmutableList();
            });
            AddUntilStep("Check for pp amount update", () => beatmapAttributeText.Text.ToString() == "Max Performance: 450pp");

            AddStep("Change DT rate to 2.0", () => dt.SpeedChange.Value = 2.0);
            AddUntilStep("Check for pp amount update", () => beatmapAttributeText.Text.ToString() == "Max Performance: 600pp");

            AddStep("Select HD", () => SelectedMods.Value = SelectedMods.Value.Append(new OsuModHidden()).ToImmutableList());
            AddUntilStep("Check for pp amount update", () => beatmapAttributeText.Text.ToString() == "Max Performance: 669pp");

            AddStep("Change attribute to star rating", () => beatmapAttributeText.Attribute.Value = BeatmapAttribute.StarRating);
            AddUntilStep("Check for star rating correctness", () => beatmapAttributeText.Text.ToString() == "Star Rating: 6.00");

            AddStep("Change attribute to BPM", () => beatmapAttributeText.Attribute.Value = BeatmapAttribute.BPM);
            AddUntilStep("Check for label update to BPM", () => beatmapAttributeText.Text.ToString() == "BPM: 240");

            AddStep("Change DT rate to 1.2", () => dt.SpeedChange.Value = 1.2);
            AddUntilStep("Check for BPM update", () => beatmapAttributeText.Text.ToString() == "BPM: 144");

            AddStep("Select hard beatmap", () => Beatmap.Value = CreateWorkingBeatmap(hardBeatmap));
            AddUntilStep("Check for BPM update", () => beatmapAttributeText.Text.ToString() == "BPM: 240");
        }

        [Test]
        public void TestRulesetChange()
        {
            AddStep("Change ruleset to Test Mania", () => Ruleset.Value = new TestManiaRuleset().RulesetInfo);
            AddUntilStep("Check for pp amount update", () => beatmapAttributeText.Text.ToString() == "Max Performance: 333pp");

            AddStep("Select hard beatmap", () => Beatmap.Value = CreateWorkingBeatmap(hardBeatmap));
            AddUntilStep("Check for pp amount update", () => beatmapAttributeText.Text.ToString() == "Max Performance: 733pp");

            AddStep("Change attribute to star rating", () => beatmapAttributeText.Attribute.Value = BeatmapAttribute.StarRating);
            AddUntilStep("Check for star rating correctness", () => beatmapAttributeText.Text.ToString() == "Star Rating: 7.30");

            AddStep("Change attribute to max performance", () => beatmapAttributeText.Attribute.Value = BeatmapAttribute.MaxPerformance);
            AddStep("Change ruleset to Test Osu", () => Ruleset.Value = new TestOsuRuleset().RulesetInfo);
            AddStep("Change ruleset to Test Mania", () => Ruleset.Value = new TestManiaRuleset().RulesetInfo);
            AddStep("Change ruleset to Test Osu", () => Ruleset.Value = new TestOsuRuleset().RulesetInfo);
            AddUntilStep("Check for pp correctness", () => beatmapAttributeText.Text.ToString() == "Max Performance: 700pp");
        }

        private IBeatmap initBeatmap(string name, double bpm, float difficulty)
        {
            IBeatmap beatmap = CreateBeatmap(Ruleset.Value);

            beatmap.Metadata.Title = name;
            beatmap.BeatmapInfo.BPM = bpm;
            beatmap.BeatmapInfo.Difficulty.OverallDifficulty = difficulty;

            return beatmap;
        }

        private partial class TestBeatmapDifficultyCache : BeatmapDifficultyCache
        {
            // Use simple star rating formula that's sensitive to mods and ruleset
            protected override Task<StarDifficulty?> ComputeValueAsync(DifficultyCacheLookup lookup, CancellationToken token = default)
            {
                double starRating = lookup.BeatmapInfo.Difficulty.OverallDifficulty;

                ModDoubleTime? DT = lookup.OrderedMods.OfType<ModDoubleTime>().SingleOrDefault();
                if (DT != null) starRating *= DT.SpeedChange.Value;

                starRating += (double)lookup.Ruleset.OnlineID / 10;

                var attributes = new DifficultyAttributes();
                attributes.StarRating = starRating;
                attributes.Mods = lookup.OrderedMods;

                // Use this as ruleset ID field
                attributes.MaxCombo = lookup.Ruleset.OnlineID;

                return Task.FromResult<StarDifficulty?>(new StarDifficulty(attributes));
            }
        }

        private partial class TestPerformanceCalculator : PerformanceCalculator
        {
            public TestPerformanceCalculator(Ruleset ruleset) : base(ruleset)
            {
            }

            // Simple pp formula giving value depending on star rating and HD presence
            protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
            {
                // Be sure that correct rulesets are used
                Assert.AreEqual(Ruleset.RulesetInfo.OnlineID, score.Ruleset.OnlineID);
                Assert.AreEqual(Ruleset.RulesetInfo.OnlineID, attributes.MaxCombo);

                var result = new PerformanceAttributes();
                result.Total = attributes.StarRating * 100;

                result.Total += Ruleset.RulesetInfo.OnlineID;

                if (attributes.Mods.Any(m => m is ModHidden))
                    result.Total += 69;

                return result;
            }
        }

        private partial class TestOsuRuleset : OsuRuleset
        {
            public override string Description => "osu test";
            public override string ShortName => "osu test";
            public override PerformanceCalculator CreatePerformanceCalculator() => new TestPerformanceCalculator(this);
        }

        private partial class TestManiaRuleset : ManiaRuleset
        {
            public override string Description => "mania test";
            public override string ShortName => "mania test";
            public override PerformanceCalculator CreatePerformanceCalculator() => new TestPerformanceCalculator(this);
        }
    }
}
