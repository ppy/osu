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
            AddStep("Centre hit", () => SetContents(_ => createDrawableHitAtCurrentTime()));
            AddStep("Centre hit (strong)", () => SetContents(_ => createDrawableHitAtCurrentTime(true)));
            AddStep("Rim hit", () => SetContents(_ => createDrawableHitAtCurrentTime(rim: true)));
            AddStep("Rim hit (strong)", () => SetContents(_ => createDrawableHitAtCurrentTime(true, true)));
        }

        private DrawableHit createDrawableHitAtCurrentTime(bool strong = false, bool rim = false)
        {
            var drawable = DrawableHit.CreateConcrete(createHitAtCurrentTime(strong, rim));
            drawable.Anchor = Anchor.Centre;
            drawable.Origin = Anchor.Centre;
            return drawable;
        }

        private Hit createHitAtCurrentTime(bool strong = false, bool rim = false)
        {
            Hit hit = rim ? new HitRim() : new HitCentre();
            hit.IsStrong = strong;
            hit.StartTime = Time.Current + 3000;

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
