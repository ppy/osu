// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Tests
{
    /// <summary>
    /// Taiko doesn't output any samples. They are all handled externally by <see cref="DrumSamplePlayer"/>.
    /// </summary>
    [HeadlessTest]
    public partial class TestSceneSampleOutput : TestSceneTaikoPlayer
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            string[] expectedSampleNames =
            {
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
            };

            var actualSampleNames = new List<string>();

            // due to pooling we can't access all samples right away due to object re-use,
            // so we need to collect as we go.
            AddStep("collect sample names", () => Player.DrawableRuleset.Playfield.NewResult += (dho, _) =>
            {
                if (!(dho is DrawableHit h))
                    return;

                actualSampleNames.Add(string.Join(',', h.GetSamples().Select(s => s.Name)));
            });

            AddUntilStep("all samples collected", () => actualSampleNames.Count == expectedSampleNames.Length);

            AddAssert("samples are correct", () => actualSampleNames, () => Is.EqualTo(expectedSampleNames));
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TaikoBeatmapConversionTest().GetBeatmap("sample-to-type-conversions");
    }
}
