// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneDrawableHit : TaikoSkinnableTestScene
    {
        [Test]
        public void TestHits()
        {
            AddStep("Centre hit", () => SetContents(_ => new DrawableHit(createHitAtCurrentTime())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Centre hit (strong)", () => SetContents(_ => new DrawableHit(createHitAtCurrentTime(true))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Rim hit", () => SetContents(_ => new DrawableHit(createHitAtCurrentTime(rim: true))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Rim hit (strong)", () => SetContents(_ => new DrawableHit(createHitAtCurrentTime(true, true))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));
        }

        [Test]
        public void TestHitAnimationSlow()
        {
            AddStep("Reset combo", () => ScoreProcessor.Combo.Value = 0);

            AddRepeatStep("Increase combo", () => ScoreProcessor.Combo.Value++, 50);

            TestHits();
        }

        [Test]
        public void TestHitAnimationFast()
        {
            AddStep("Reset combo", () => ScoreProcessor.Combo.Value = 0);

            AddRepeatStep("Increase combo", () => ScoreProcessor.Combo.Value++, 150);

            TestHits();
        }

        private Hit createHitAtCurrentTime(bool strong = false, bool rim = false)
        {
            var hit = new Hit
            {
                Type = rim ? HitType.Rim : HitType.Centre,
                IsStrong = strong,
                StartTime = Time.Current + 3000,
            };

            hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return hit;
        }
    }
}
