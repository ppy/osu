// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    public class TestSceneTaikoEditorSaving : EditorSavingTestScene
    {
        protected override Ruleset CreateRuleset() => new TaikoRuleset();

        [Test]
        public void TestTaikoSliderMultiplier()
        {
            AddStep("Set slider multiplier", () => EditorBeatmap.Difficulty.SliderMultiplier = 2);

            SaveEditor();

            AddAssert("Beatmap has correct slider multiplier", assertTaikoSliderMulitplier);

            ReloadEditorToSameBeatmap();

            AddAssert("Beatmap still has correct slider multiplier", assertTaikoSliderMulitplier);

            bool assertTaikoSliderMulitplier()
            {
                // we can only assert value correctness on TaikoMultiplierAppliedDifficulty, because that is the final difficulty converted taiko beatmaps use.
                // therefore, ensure that we have that difficulty type by calling .CopyFrom(), which is a no-op if the type is already correct.
                var taikoDifficulty = new TaikoBeatmapConverter.TaikoMultiplierAppliedDifficulty();
                taikoDifficulty.CopyFrom(EditorBeatmap.Difficulty);
                return Precision.AlmostEquals(taikoDifficulty.SliderMultiplier, 2);
            }
        }
    }
}
