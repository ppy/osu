// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Extended;
using osu.Game.Tests.Beatmaps.IO;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneExtendedBeatmapInfoPanel : OsuTestScene
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Resolved]
        private OsuGameBase osu { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private BeatmapSetInfo importedSet = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("import set", () => importedSet = BeatmapImportHelper.LoadOszIntoOsu(osu).GetResultSafely());
            AddStep("set score", () => setScore(0));

            AddStep("create component", () => Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.9f,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new BeatmapInfoPanel(),
                ]
            });
        }

        [Test]
        public void TestAppearance()
        {
            foreach (var status in Enum.GetValues<BeatmapOnlineStatus>())
                AddStep($"{status} status", () => setScore(0, s => s.BeatmapInfo?.Status = status));

            for (int i = 0; i < 4; ++i)
            {
                int ruleset = i;
                AddStep($"ruleset {ruleset}", () => setScore(ruleset));
            }

            AddStep("with HR", () => setScore(0, s => s.Mods = [new OsuModHardRock()]));
            AddStep("with DA", () => setScore(0, s => s.Mods =
            [
                new OsuModDifficultyAdjust
                {
                    CircleSize = { Value = 3 },
                    OverallDifficulty = { Value = 9.3f },
                }
            ]));
            AddStep("with TP", () => setScore(0, s => s.Mods = [new OsuModTargetPractice()]));

            AddStep("very long metadata", () => setScore(0, s =>
            {
                s.BeatmapInfo?.Metadata.Artist = "very very very very very very very very very very very very long artist.";
                s.BeatmapInfo?.Metadata.Title = "very very very very very very very very very very very very long title.";
                s.BeatmapInfo?.DifficultyName = "very very very very very very very very very very very very long difficulty name.";
            }));

            AddStep("random local beatmap", () =>
            {
                var beatmapSet = beatmaps.GetAllUsableBeatmapSets().OrderBy(_ => RNG.NextSingle()).First();
                var beatmap = beatmapSet.Beatmaps.First();
                Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap);
                score.Value = new ScoreInfo
                {
                    BeatmapInfo = beatmap,
                    Ruleset = beatmap.Ruleset,
                };
            });
        }

        private void setScore(int rulesetId, Action<ScoreInfo>? setUp = null)
        {
            var ruleset = rulesets.GetRuleset(rulesetId)!;

            var beatmap = importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == rulesetId);
            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap);

            var newScore = new ScoreInfo
            {
                BeatmapInfo = beatmap,
                Ruleset = ruleset,
            };
            setUp?.Invoke(newScore);

            score.Value = newScore;
        }
    }
}
