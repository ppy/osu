// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
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
    public class TestSceneDrawableTaikoMascot : TaikoSkinnableTestScene
    {
        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        private TaikoScoreProcessor scoreProcessor;

        private IEnumerable<DrawableTaikoMascot> mascots => this.ChildrenOfType<DrawableTaikoMascot>();
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

            AddStep("clear state", () => SetContents(() => new TaikoMascotAnimation(TaikoMascotAnimationState.Clear)));
            AddStep("idle state", () => SetContents(() => new TaikoMascotAnimation(TaikoMascotAnimationState.Idle)));
            AddStep("kiai state", () => SetContents(() => new TaikoMascotAnimation(TaikoMascotAnimationState.Kiai)));
            AddStep("fail state", () => SetContents(() => new TaikoMascotAnimation(TaikoMascotAnimationState.Fail)));
        }

        [Test]
        public void TestInitialState()
        {
            AddStep("create mascot", () => SetContents(() => new DrawableTaikoMascot { RelativeSizeAxes = Axes.Both }));

            AddAssert("mascot initially idle", () => allMascotsIn(TaikoMascotAnimationState.Idle));
        }

        [Test]
        public void TestClearStateTransition()
        {
            AddStep("set beatmap", () => setBeatmap());

            AddStep("create mascot", () => SetContents(() => new DrawableTaikoMascot { RelativeSizeAxes = Axes.Both }));

            AddStep("set clear state", () => mascots.ForEach(mascot => mascot.State.Value = TaikoMascotAnimationState.Clear));
            AddStep("miss", () => mascots.ForEach(mascot => mascot.LastResult.Value = new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Miss }));
            AddAssert("skins with animations remain in clear state", () => someMascotsIn(TaikoMascotAnimationState.Clear));
            AddUntilStep("state reverts to fail", () => allMascotsIn(TaikoMascotAnimationState.Fail));

            AddStep("set clear state again", () => mascots.ForEach(mascot => mascot.State.Value = TaikoMascotAnimationState.Clear));
            AddAssert("skins with animations change to clear", () => someMascotsIn(TaikoMascotAnimationState.Clear));
        }

        [Test]
        public void TestIdleState()
        {
            AddStep("set beatmap", () => setBeatmap());

            createDrawableRuleset();

            assertStateAfterResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Great }, TaikoMascotAnimationState.Idle);
            assertStateAfterResult(new JudgementResult(new StrongHitObject(), new TaikoStrongJudgement()) { Type = HitResult.Miss }, TaikoMascotAnimationState.Idle);
        }

        [Test]
        public void TestKiaiState()
        {
            AddStep("set beatmap", () => setBeatmap(true));

            createDrawableRuleset();

            assertStateAfterResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Good }, TaikoMascotAnimationState.Kiai);
            assertStateAfterResult(new JudgementResult(new Hit(), new TaikoStrongJudgement()) { Type = HitResult.Miss }, TaikoMascotAnimationState.Kiai);
            assertStateAfterResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Miss }, TaikoMascotAnimationState.Fail);
        }

        [Test]
        public void TestMissState()
        {
            AddStep("set beatmap", () => setBeatmap());

            createDrawableRuleset();

            assertStateAfterResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Great }, TaikoMascotAnimationState.Idle);
            assertStateAfterResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Miss }, TaikoMascotAnimationState.Fail);
            assertStateAfterResult(new JudgementResult(new DrumRoll(), new TaikoDrumRollJudgement()) { Type = HitResult.Great }, TaikoMascotAnimationState.Fail);
            assertStateAfterResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Good }, TaikoMascotAnimationState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestClearStateOnComboMilestone(bool kiai)
        {
            AddStep("set beatmap", () => setBeatmap(kiai));

            createDrawableRuleset();

            AddRepeatStep("reach 49 combo", () => applyNewResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Great }), 49);

            assertStateAfterResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Good }, TaikoMascotAnimationState.Clear);
        }

        [TestCase(true, TaikoMascotAnimationState.Kiai)]
        [TestCase(false, TaikoMascotAnimationState.Idle)]
        public void TestClearStateOnClearedSwell(bool kiai, TaikoMascotAnimationState expectedStateAfterClear)
        {
            AddStep("set beatmap", () => setBeatmap(kiai));

            createDrawableRuleset();

            assertStateAfterResult(new JudgementResult(new Swell(), new TaikoSwellJudgement()) { Type = HitResult.Great }, TaikoMascotAnimationState.Clear);
            AddUntilStep($"state reverts to {expectedStateAfterClear.ToString().ToLower()}", () => allMascotsIn(expectedStateAfterClear));
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
                    BaseDifficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = "Unknown",
                        Title = "Sample Beatmap",
                        AuthorString = "Craftplacer",
                    },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
                ControlPointInfo = controlPointInfo
            });

            scoreProcessor.ApplyBeatmap(Beatmap.Value.Beatmap);
        }

        private void createDrawableRuleset()
        {
            AddUntilStep("wait for beatmap to be loaded", () => Beatmap.Value.Track.IsLoaded);

            AddStep("create drawable ruleset", () =>
            {
                Beatmap.Value.Track.Start();

                SetContents(() =>
                {
                    var ruleset = new TaikoRuleset();
                    return new DrawableTaikoRuleset(ruleset, Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo));
                });
            });
        }

        private void assertStateAfterResult(JudgementResult judgementResult, TaikoMascotAnimationState expectedState)
        {
            AddStep($"{judgementResult.Type.ToString().ToLower()} result for {judgementResult.Judgement.GetType().Name.Humanize(LetterCasing.LowerCase)}",
                () => applyNewResult(judgementResult));

            AddAssert($"state is {expectedState.ToString().ToLower()}", () => allMascotsIn(expectedState));
        }

        private void applyNewResult(JudgementResult judgementResult)
        {
            scoreProcessor.ApplyResult(judgementResult);

            foreach (var playfield in playfields)
            {
                var hit = new DrawableTestHit(new Hit(), judgementResult.Type);
                Add(hit);

                playfield.OnNewResult(hit, judgementResult);
            }

            foreach (var mascot in mascots)
            {
                mascot.LastResult.Value = judgementResult;
            }
        }

        private bool allMascotsIn(TaikoMascotAnimationState state) => mascots.All(d => d.State.Value == state);
        private bool someMascotsIn(TaikoMascotAnimationState state) => mascots.Any(d => d.State.Value == state);
    }
}
