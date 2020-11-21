// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    /// <summary>
    /// Taiko has some interesting rules for legacy mappings.
    /// </summary>
    [HeadlessTest]
    public class TestSceneSampleOutput : TestSceneTaikoPlayer
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddAssert("has correct samples", () =>
            {
                var names = Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableHit>().Select(h => string.Join(',', h.GetSamples().Select(s => s.Name)));

                var expected = new[]
                {
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    HitSampleInfo.HIT_FINISH,
                    HitSampleInfo.HIT_WHISTLE,
                    HitSampleInfo.HIT_WHISTLE,
                    HitSampleInfo.HIT_WHISTLE,
                };

                return names.SequenceEqual(expected);
            });
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TaikoBeatmapConversionTest().GetBeatmap("sample-to-type-conversions");
    }
}
