// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public partial class TestSceneDifficultyIcon : OsuTestScene
    {
        private FillFlowContainer fill = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Child = fill = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                Width = 300,
                Direction = FillDirection.Full,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [Test]
        public void CreateDifficultyIcon()
        {
            DifficultyIcon difficultyIcon = null!;

            AddRepeatStep("create difficulty icon", () =>
            {
                var rulesetInfo = new OsuRuleset().RulesetInfo;
                var beatmapInfo = new TestBeatmap(rulesetInfo).BeatmapInfo;

                beatmapInfo.Difficulty.ApproachRate = RNG.Next(0, 10);
                beatmapInfo.Difficulty.CircleSize = RNG.Next(0, 10);
                beatmapInfo.Difficulty.OverallDifficulty = RNG.Next(0, 10);
                beatmapInfo.Difficulty.DrainRate = RNG.Next(0, 10);
                beatmapInfo.StarRating = RNG.NextSingle(0, 10);
                beatmapInfo.BPM = RNG.Next(60, 300);

                fill.Add(difficultyIcon = new DifficultyIcon(beatmapInfo, rulesetInfo)
                {
                    Scale = new Vector2(2),
                    ShowTooltip = true,
                    ShowExtendedTooltip = true
                });
            }, 10);

            AddStep("hide extended tooltip", () => difficultyIcon.ShowExtendedTooltip = false);

            AddStep("hide tooltip", () => difficultyIcon.ShowTooltip = false);

            AddStep("show tooltip", () => difficultyIcon.ShowTooltip = true);

            AddStep("show extended tooltip", () => difficultyIcon.ShowExtendedTooltip = true);
        }
    }
}
