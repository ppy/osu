// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public partial class TestSceneDrawableTaikoMascot : TaikoSkinnableTestScene
    {
        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        private TaikoScoreProcessor scoreProcessor = null!;

        private IEnumerable<DrawableTaikoMascot> mascots => this.ChildrenOfType<DrawableTaikoMascot>();

        private IEnumerable<DrawableTaikoMascot> animatedMascots =>
            mascots.Where(mascot => mascot.ChildrenOfType<TextureAnimation>().All(animation => animation.FrameCount > 0));

        private IEnumerable<TaikoPlayfield> playfields => this.ChildrenOfType<TaikoPlayfield>();

        [SetUp]
        public void SetUp()
        {
            scoreProcessor = new TaikoScoreProcessor();
        }

        [Test]
        public void TestStateAnimations()
        {
            AddStep("set beatmap", () => setBeatmap());

            AddStep("clear state", () => SetContents(_ => new TaikoMascotAnimation(TaikoMascotAnimationState.Clear)));
            AddStep("idle state", () => SetContents(_ => new TaikoMascotAnimation(TaikoMascotAnimationState.Idle)));
            AddStep("kiai state", () => SetContents(_ => new TaikoMascotAnimation(TaikoMascotAnimationState.Kiai)));
            AddStep("fail state", () => SetContents(_ => new TaikoMascotAnimation(TaikoMascotAnimationState.Fail)));
        }

        [Test]
        public void TestInitialState()
        {
            AddStep("set beatmap", () => setBeatmap());

            AddStep("create mascot", () => SetContents(_ => new DrawableTaikoMascot { RelativeSizeAxes = Axes.Both }));

            AddAssert("mascot initially idle", () => allMascotsIn(TaikoMascotAnimationState.Idle));
        }

        [Test]
        public void TestClearStateTransition()
        {
            AddStep("set beatmap", () => setBeatmap());

            AddStep("create mascot", () => SetContents(_ => new DrawableTaikoMascot { RelativeSizeAxes = Axes.Both }));

            AddStep("set clear state", () => mascots.ForEach(mascot => mascot.State.Value = TaikoMascotAnimationState.Clear));
            AddStep("miss", () => mascots.ForEach(mascot => mascot.LastResult.Value = new Judgement(new Hit(), new TaikoJudgementInfo()) { Type = HitResult.Miss }));
            AddAssert("skins with animations remain in clear state", () => animatedMascotsIn(TaikoMascotAnimationState.Clear));
            AddUntilStep("state reverts to fail", () => allMascotsIn(TaikoMascotAnimationState.Fail));

            AddStep("set clear state again", () => mascots.ForEach(mascot => mascot.State.Value = TaikoMascotAnimationState.Clear));
            AddAssert("skins with animations change to clear", () => animatedMascotsIn(TaikoMascotAnimationState.Clear));
        }

        [Test]
        public void TestIdleState()
        {
            prepareDrawableRulesetAndBeatmap(false);

            var hit = new Hit();
            assertStateAfterResult(new Judgement(hit, new TaikoJudgementInfo()) { Type = HitResult.Great }, TaikoMascotAnimationState.Idle);
            assertStateAfterResult(new Judgement(new Hit.StrongNestedHit(hit), new TaikoStrongJudgementInfo()) { Type = HitResult.IgnoreMiss }, TaikoMascotAnimationState.Idle);
        }

        [Test]
        public void TestKiaiState()
        {
            prepareDrawableRulesetAndBeatmap(true);

            assertStateAfterResult(new Judgement(new Hit(), new TaikoJudgementInfo()) { Type = HitResult.Ok }, TaikoMascotAnimationState.Kiai);
            assertStateAfterResult(new Judgement(new Hit(), new TaikoStrongJudgementInfo()) { Type = HitResult.IgnoreMiss }, TaikoMascotAnimationState.Kiai);
            assertStateAfterResult(new Judgement(new Hit(), new TaikoJudgementInfo()) { Type = HitResult.Miss }, TaikoMascotAnimationState.Fail);
        }

        [Test]
        public void TestMissState()
        {
            prepareDrawableRulesetAndBeatmap(false);

            assertStateAfterResult(new Judgement(new Hit(), new TaikoJudgementInfo()) { Type = HitResult.Great }, TaikoMascotAnimationState.Idle);
            assertStateAfterResult(new Judgement(new Hit(), new TaikoJudgementInfo()) { Type = HitResult.Miss }, TaikoMascotAnimationState.Fail);
            assertStateAfterResult(new Judgement(new Hit(), new TaikoJudgementInfo()) { Type = HitResult.Ok }, TaikoMascotAnimationState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestClearStateOnComboMilestone(bool kiai)
        {
            prepareDrawableRulesetAndBeatmap(kiai);

            AddRepeatStep("reach 49 combo", () => applyNewResult(new Judgement(new Hit(), new TaikoJudgementInfo()) { Type = HitResult.Great }), 49);

            assertStateAfterResult(new Judgement(new Hit(), new TaikoJudgementInfo()) { Type = HitResult.Ok }, TaikoMascotAnimationState.Clear);
        }

        [TestCase(true, TaikoMascotAnimationState.Kiai)]
        [TestCase(false, TaikoMascotAnimationState.Idle)]
        public void TestClearStateOnClearedSwell(bool kiai, TaikoMascotAnimationState expectedStateAfterClear)
        {
            prepareDrawableRulesetAndBeatmap(kiai);

            assertStateAfterResult(new Judgement(new Swell(), new TaikoSwellJudgementInfo()) { Type = HitResult.Great }, TaikoMascotAnimationState.Clear);
            AddUntilStep($"state reverts to {expectedStateAfterClear.ToString().ToLowerInvariant()}", () => allMascotsIn(expectedStateAfterClear));
        }

        private void setBeatmap(bool kiai = false)
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint { BeatLength = 90 });

            if (kiai)
                controlPointInfo.Add(0, new EffectControlPoint { KiaiMode = true });

            Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject> { new Hit { Type = HitType.Centre } },
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = "Unknown",
                        Title = "Sample Beatmap",
                        Author = { Username = "Craftplacer" },
                    },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
                ControlPointInfo = controlPointInfo
            });

            scoreProcessor.ApplyBeatmap(Beatmap.Value.Beatmap);
        }

        private void prepareDrawableRulesetAndBeatmap(bool kiai)
        {
            AddStep("set beatmap", () => setBeatmap(kiai));

            AddStep("create drawable ruleset", () =>
            {
                SetContents(_ =>
                {
                    var ruleset = new TaikoRuleset();
                    return new DrawableTaikoRuleset(ruleset, Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo));
                });
            });

            AddUntilStep("wait for track to be loaded", () => MusicController.TrackLoaded);
            AddStep("start track", () => MusicController.CurrentTrack.Restart());
            AddUntilStep("wait for track started", () => MusicController.IsPlaying);
        }

        private void assertStateAfterResult(Judgement judgement, TaikoMascotAnimationState expectedState)
        {
            TaikoMascotAnimationState[] mascotStates = null!;

            AddStep($"{judgement.Type.ToString().ToLowerInvariant()} result for {judgement.JudgementInfo.GetType().Name.Humanize(LetterCasing.LowerCase)}",
                () =>
                {
                    applyNewResult(judgement);
                    // store the states as soon as possible, so that the delay between steps doesn't incorrectly fail the test
                    // due to not checking if the state changed quickly enough.
                    Schedule(() => mascotStates = animatedMascots.Select(mascot => mascot.State.Value).ToArray());
                });

            AddAssert($"state is {expectedState.ToString().ToLowerInvariant()}", () => mascotStates.Distinct(), () => Is.EquivalentTo(new[] { expectedState }));
        }

        private void applyNewResult(Judgement judgement)
        {
            scoreProcessor.ApplyResult(judgement);

            foreach (var playfield in playfields)
            {
                var hit = new DrawableTestHit(new Hit(), judgement.Type);
                playfield.Add(hit);

                playfield.OnNewResult(hit, judgement);
            }

            foreach (var mascot in mascots)
            {
                mascot.LastResult.Value = judgement;
            }
        }

        private bool allMascotsIn(TaikoMascotAnimationState state) => mascots.All(d => d.State.Value == state);
        private bool animatedMascotsIn(TaikoMascotAnimationState state) => animatedMascots.Any(d => d.State.Value == state);
    }
}
