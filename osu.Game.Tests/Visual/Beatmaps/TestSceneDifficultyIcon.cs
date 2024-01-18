// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public partial class TestSceneDifficultyIcon : OsuTestScene
    {
        [Test]
        public void CreateDifficultyIcon()
        {
            DifficultyIcon difficultyIcon = null;

            AddStep("create difficulty icon", () =>
            {
                Child = difficultyIcon = new DifficultyIcon(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo, new OsuRuleset().RulesetInfo)
                {
                    ShowTooltip = true,
                    ShowExtendedTooltip = true
                };
            });

            AddStep("hide extended tooltip", () => difficultyIcon.ShowExtendedTooltip = false);

            AddStep("hide tooltip", () => difficultyIcon.ShowTooltip = false);

            AddStep("show tooltip", () => difficultyIcon.ShowTooltip = true);

            AddStep("show extended tooltip", () => difficultyIcon.ShowExtendedTooltip = true);
        }
    }
}
