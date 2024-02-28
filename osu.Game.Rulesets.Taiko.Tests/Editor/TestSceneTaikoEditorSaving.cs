// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Tests.Visual;
using SharpCompress.Archives.Zip;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    public partial class TestSceneTaikoEditorSaving : EditorSavingTestScene
    {
        protected override Ruleset CreateRuleset() => new TaikoRuleset();

        [TestCase(null)]
        [TestCase(1f)]
        [TestCase(2f)]
        [TestCase(2.4f)]
        public void TestTaikoSliderMultiplierInExport(float? multiplier)
        {
            if (multiplier.HasValue)
                AddStep("Set slider multiplier", () => EditorBeatmap.Difficulty.SliderMultiplier = multiplier.Value);

            SaveEditor();
            AddStep("export beatmap", () => Game.BeatmapManager.Export(EditorBeatmap.BeatmapInfo.BeatmapSet!).WaitSafely());

            AddAssert("check slider multiplier correct in file", () =>
            {
                string export = LocalStorage.GetFiles("exports").First();

                using (var stream = LocalStorage.GetStream(export))
                using (var zip = ZipArchive.Open(stream))
                {
                    using (var osuStream = zip.Entries.First().OpenEntryStream())
                    using (var reader = new StreamReader(osuStream))
                    {
                        string? line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("SliderMultiplier", StringComparison.Ordinal))
                            {
                                return float.Parse(line.Split(':', StringSplitOptions.TrimEntries).Last(), provider: CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }

                return 0;
            }, () => Is.EqualTo(multiplier ?? new BeatmapDifficulty().SliderMultiplier).Within(Precision.FLOAT_EPSILON));
        }

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
