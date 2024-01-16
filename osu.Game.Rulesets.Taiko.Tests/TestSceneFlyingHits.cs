// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public partial class TestSceneFlyingHits : DrawableTaikoRulesetTestScene
    {
        [TestCase(HitType.Centre)]
        [TestCase(HitType.Rim)]
        public void TestFlyingHits(HitType hitType)
        {
            DrawableFlyingHit flyingHit = null;

            AddStep("add flying hit", () =>
            {
                addFlyingHit(hitType);

                // flying hits all land in one common scrolling container (and stay there for rewind purposes),
                // so we need to manually get the latest one.
                flyingHit = this.ChildrenOfType<DrawableFlyingHit>().MaxBy(h => h.HitObject.StartTime);
            });

            AddAssert("hit type is correct", () => flyingHit.HitObject.Type == hitType);
        }

        private void addFlyingHit(HitType hitType)
        {
            var tick = new DrumRollTick(new DrumRoll()) { HitWindows = HitWindows.Empty, StartTime = DrawableRuleset.Playfield.Time.Current };

            DrawableDrumRollTick h;
            DrawableRuleset.Playfield.Add(h = new DrawableDrumRollTick(tick) { JudgementType = hitType });
            ((TaikoPlayfield)DrawableRuleset.Playfield).OnNewResult(h, new Judgement(tick, new TaikoDrumRollTickJudgementCriteria()) { Type = HitResult.Great });
        }
    }
}
