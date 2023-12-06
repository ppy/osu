// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    public partial class TestSceneTaikoEditorSaving : EditorSavingTestScene
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
                return Precision.AlmostEquals(EditorBeatmap.Difficulty.SliderMultiplier, 2);
            }
        }
    }
}
