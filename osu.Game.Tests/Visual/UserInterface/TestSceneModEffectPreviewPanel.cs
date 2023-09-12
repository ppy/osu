// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModEffectPreviewPanel : OsuTestScene
    {
        [Cached(typeof(BeatmapDifficultyCache))]
        private TestBeatmapDifficultyCache difficultyCache = new TestBeatmapDifficultyCache();

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private Container content = null!;
        protected override Container<Drawable> Content => content;

        private BeatmapAttributesDisplay panel = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            base.Content.AddRange(new Drawable[]
            {
                difficultyCache,
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both
                }
            });
        }

        [Test]
        public void TestDisplay()
        {
            OsuModDifficultyAdjust difficultyAdjust = new OsuModDifficultyAdjust();
            OsuModDoubleTime doubleTime = new OsuModDoubleTime();

            AddStep("create display", () => Child = panel = new BeatmapAttributesDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddStep("set beatmap", () =>
            {
                var beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo)
                {
                    BeatmapInfo =
                    {
                        BPM = 120
                    }
                };

                Ruleset.Value = beatmap.BeatmapInfo.Ruleset;
                panel.BeatmapInfo.Value = beatmap.BeatmapInfo;
            });

            AddSliderStep("change star rating", 0, 10d, 5, stars =>
            {
                if (panel.IsNotNull())
                    previewStarRating(stars);
            });
            AddStep("preview ridiculously high SR", () => previewStarRating(1234));

            AddStep("add DA to mods", () => SelectedMods.Value = new[] { difficultyAdjust });

            AddSliderStep("change AR", 0, 10f, 5, ar =>
            {
                if (panel.IsNotNull())
                    difficultyAdjust.ApproachRate.Value = ar;
            });
            AddSliderStep("change CS", 0, 10f, 5, cs =>
            {
                if (panel.IsNotNull())
                    difficultyAdjust.CircleSize.Value = cs;
            });
            AddSliderStep("change HP", 0, 10f, 5, hp =>
            {
                if (panel.IsNotNull())
                    difficultyAdjust.DrainRate.Value = hp;
            });
            AddSliderStep("change OD", 0, 10f, 5, od =>
            {
                if (panel.IsNotNull())
                    difficultyAdjust.OverallDifficulty.Value = od;
            });

            AddStep("add DT to mods", () => SelectedMods.Value = new Mod[] { difficultyAdjust, doubleTime });
            AddSliderStep("change rate", 1.01d, 2d, 1.5d, rate =>
            {
                if (panel.IsNotNull())
                    doubleTime.SpeedChange.Value = rate;
            });

            AddToggleStep("toggle collapsed", collapsed => panel.Collapsed.Value = collapsed);
        }

        private void previewStarRating(double stars)
        {
            difficultyCache.Difficulty = new StarDifficulty(stars, 0);
            panel.BeatmapInfo.TriggerChange();
        }

        private partial class TestBeatmapDifficultyCache : BeatmapDifficultyCache
        {
            public StarDifficulty? Difficulty { get; set; }

            public override Task<StarDifficulty?> GetDifficultyAsync(IBeatmapInfo beatmapInfo, IRulesetInfo? rulesetInfo = null, IEnumerable<Mod>? mods = null,
                                                                     CancellationToken cancellationToken = default)
                => Task.FromResult(Difficulty);
        }
    }
}
