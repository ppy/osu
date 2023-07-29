// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Screens.Play;
using osu.Game.Tests.Gameplay;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public partial class TestSceneDrawableHit : TaikoSkinnableTestScene
    {
        [Cached]
        private GameplayState gameplayState = TestGameplayState.Create(new TaikoRuleset());

        [Test]
        public void TestHits([Values] bool withKiai)
        {
            AddStep("Create beatmap", () => setUpBeatmap(withKiai));
            addHitSteps();
        }

        [Test]
        public void TestHitAnimationSlow()
        {
            AddStep("Create beatmap", () => setUpBeatmap(false));

            AddStep("Set 50 combo", () => gameplayState.ScoreProcessor.Combo.Value = 50);
            addHitSteps();
            AddStep("Reset combo", () => gameplayState.ScoreProcessor.Combo.Value = 0);
        }

        [Test]
        public void TestHitAnimationFast()
        {
            AddStep("Create beatmap", () => setUpBeatmap(false));

            AddStep("Set 150 combo", () => gameplayState.ScoreProcessor.Combo.Value = 150);
            addHitSteps();
            AddStep("Reset combo", () => gameplayState.ScoreProcessor.Combo.Value = 0);
        }

        private void addHitSteps()
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

        private void setUpBeatmap(bool withKiai)
        {
            var controlPointInfo = new ControlPointInfo();

            controlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });

            if (withKiai)
                controlPointInfo.Add(0, new EffectControlPoint { KiaiMode = true });

            Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                ControlPointInfo = controlPointInfo
            });

            Beatmap.Value.Track.Start();
        }
    }
}
