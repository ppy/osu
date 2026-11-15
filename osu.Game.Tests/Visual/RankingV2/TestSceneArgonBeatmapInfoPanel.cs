// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Argon;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneArgonBeatmapInfoPanel : OsuTestScene
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Cached(typeof(BeatmapDifficultyCache))]
        private readonly TestBeatmapDifficultyCache difficultyCache = new TestBeatmapDifficultyCache();

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private ArgonBeatmapInfoPanel panel = null!;
        private SkinnableTestScene.OutlineBox? outline;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set score", () => setScore());

            AddStep("create component", () => Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    panel = new ArgonBeatmapInfoPanel(),
                    outline = new SkinnableTestScene.OutlineBox(),
                ],
            });
        }

        [Test]
        public void TestAppearance()
        {
            AddToggleStep("toggle outline", t => outline?.Alpha = t ? 1 : 0);
            AddStep("begin animation", () => panel.StartAnimating(Time.Current));
            AddStep("cancel animation", () => panel.FinishAnimating());

            foreach (var status in Enum.GetValues<BeatmapOnlineStatus>())
                AddStep($"{status} status", () => setScore(s => s.BeatmapInfo?.Status = status));

            for (int i = 0; i < 4; ++i)
            {
                int ruleset = i;
                AddStep($"ruleset {i}", () => setScore(s => s.Ruleset = rulesets.GetRuleset(ruleset)!));
            }

            AddStep("very long metadata", () => setScore(s =>
            {
                s.BeatmapInfo?.Metadata.Artist = "very very very very very very very very very very very very long artist.";
                s.BeatmapInfo?.Metadata.Title = "very very very very very very very very very very very very long title.";
                s.BeatmapInfo?.DifficultyName = "very very very very very very very very very very very very long difficulty name.";
            }));
            AddSliderStep("change star rating", 0f, 15f, 5.55f, stars =>
            {
                if (difficultyCache.IsNull() || panel.IsNull()) return;

                difficultyCache.ComputeDifficulty = _ => new StarDifficulty(stars, maxCombo: 1234);
                setScore();
            });
            AddSliderStep("change width", 50, 1000, ArgonBeatmapInfoPanel.WIDTH, width =>
            {
                if (panel.IsNotNull())
                    panel.Width = width;
            });
        }

        private void setScore(Action<ScoreInfo>? setUp = null)
        {
            var ruleset = rulesets.GetRuleset(0)!;

            var newScore = new ScoreInfo
            {
                BeatmapInfo = CreateBeatmap(rulesets.GetRuleset(0)).BeatmapInfo,
                Ruleset = ruleset,
            };
            setUp?.Invoke(newScore);

            score.Value = newScore;
        }

        private partial class TestBeatmapDifficultyCache : BeatmapDifficultyCache
        {
            public Func<DifficultyCacheLookup, StarDifficulty>? ComputeDifficulty { get; set; }

            protected override Task<StarDifficulty?> ComputeValueAsync(DifficultyCacheLookup lookup, CancellationToken token = default)
            {
                return Task.FromResult<StarDifficulty?>(ComputeDifficulty?.Invoke(lookup) ?? new StarDifficulty(5.55, 0));
            }
        }
    }
}
