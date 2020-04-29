// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneDrawableTaikoMascot : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(DrawableTaikoMascot),
            typeof(TaikoMascotAnimation)
        }).ToList();

        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        private IEnumerable<DrawableTaikoMascot> mascots => this.ChildrenOfType<DrawableTaikoMascot>();
        private IEnumerable<TaikoPlayfield> playfields => this.ChildrenOfType<TaikoPlayfield>();

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
        public void TestClearStateTransition()
        {
            AddStep("set beatmap", () => setBeatmap());

            // the bindables need to be independent for each content cell to prevent interference,
            // as if some of the skins don't implement the animation they'll immediately revert to the previous state from the clear state.
            var states = new List<Bindable<TaikoMascotAnimationState>>();

            AddStep("create mascot", () => SetContents(() =>
            {
                var state = new Bindable<TaikoMascotAnimationState>(TaikoMascotAnimationState.Clear);
                states.Add(state);
                return new DrawableTaikoMascot { State = { BindTarget = state } };
            }));

            AddStep("set clear state", () => states.ForEach(state => state.Value = TaikoMascotAnimationState.Clear));
            AddStep("miss", () => mascots.ForEach(mascot => mascot.OnNewResult(new JudgementResult(new Hit(), new TaikoJudgement()) { Type = HitResult.Miss })));
            AddAssert("skins with animations remain in clear state", () => mascots.Any(mascot => mascot.State.Value == TaikoMascotAnimationState.Clear));
            AddUntilStep("state reverts to fail", () => someMascotsIn(TaikoMascotAnimationState.Fail));

            AddStep("set clear state again", () => states.ForEach(state => state.Value = TaikoMascotAnimationState.Clear));
            AddAssert("skins with animations change to clear", () => someMascotsIn(TaikoMascotAnimationState.Clear));
        }

        [Test]
        public void TestPlayfield()
        {
            AddStep("set beatmap", () => setBeatmap());

            AddStep("create drawable ruleset", () =>
            {
                SetContents(() =>
                {
                    var ruleset = new TaikoRuleset();
                    return new DrawableTaikoRuleset(ruleset, Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo));
                });
            });

            AddStep("miss result for normal hit", () => addJudgement(HitResult.Miss, new TaikoJudgement()));
            AddUntilStep("state is fail", () => allMascotsIn(TaikoMascotAnimationState.Fail));

            AddStep("great result for normal hit", () => addJudgement(HitResult.Great, new TaikoJudgement()));
            AddUntilStep("state is idle", () => allMascotsIn(TaikoMascotAnimationState.Idle));

            AddStep("miss result for strong hit", () => addJudgement(HitResult.Miss, new TaikoStrongJudgement()));
            AddAssert("state remains idle", () => allMascotsIn(TaikoMascotAnimationState.Idle));
        }

        [Test]
        public void TestKiai()
        {
            AddStep("set beatmap", () => setBeatmap(true));

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

            AddUntilStep("state is fail", () => allMascotsIn(TaikoMascotAnimationState.Fail));

            AddStep("great result for normal hit", () => addJudgement(HitResult.Great, new TaikoJudgement()));
            AddUntilStep("state is kiai", () => allMascotsIn(TaikoMascotAnimationState.Kiai));
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
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        AuthorString = @"Craftplacer",
                    },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
                ControlPointInfo = controlPointInfo
            });
        }

        private void addJudgement(HitResult result, Judgement judgement)
        {
            foreach (var playfield in playfields)
            {
                var hit = new DrawableTestHit(new Hit(), result);
                Add(hit);

                playfield.OnNewResult(hit, new JudgementResult(hit.HitObject, judgement) { Type = result });
            }
        }

        private bool allMascotsIn(TaikoMascotAnimationState state) => mascots.All(d => d.State.Value == state);
        private bool someMascotsIn(TaikoMascotAnimationState state) => mascots.Any(d => d.State.Value == state);
    }
}
