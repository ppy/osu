// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
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
        }).ToList();

        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        private IEnumerable<TestDrawableTaikoMascot> mascots => this.ChildrenOfType<TestDrawableTaikoMascot>();
        private IEnumerable<TaikoPlayfield> playfields => this.ChildrenOfType<TaikoPlayfield>();

        [Test]
        public void TestStateTextures()
        {
            AddStep("Set beatmap", () => setBeatmap());

            AddStep("Create mascot (idle)", () =>
            {
                SetContents(() => new TestDrawableTaikoMascot());
            });

            AddStep("Clear state", () => setState(TaikoMascotAnimationState.Clear));

            AddStep("Kiai state", () => setState(TaikoMascotAnimationState.Kiai));

            AddStep("Fail state", () => setState(TaikoMascotAnimationState.Fail));
        }

        [Test]
        public void TestPlayfield()
        {
            AddStep("Set beatmap", () => setBeatmap());

            AddStep("Create ruleset", () =>
            {
                SetContents(() =>
                {
                    var ruleset = new TaikoRuleset();
                    return new DrawableTaikoRuleset(ruleset, Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo));
                });
            });

            AddStep("Create hit (miss)", () =>
            {
                foreach (var playfield in playfields)
                    addJudgement(playfield, HitResult.Miss);
            });

            AddUntilStep("Wait for fail state", () => mascots.All(d => d.State == TaikoMascotAnimationState.Fail));

            AddStep("Create hit (great)", () =>
            {
                foreach (var playfield in playfields)
                    addJudgement(playfield, HitResult.Great);
            });

            AddUntilStep("Wait for idle state", () => mascots.All(d => d.State == TaikoMascotAnimationState.Idle));
        }

        [Test]
        public void TestKiai()
        {
            AddStep("Set beatmap", () => setBeatmap(true));

            AddUntilStep("Wait for beatmap to be loaded", () => Beatmap.Value.Track.IsLoaded);

            AddStep("Create kiai ruleset", () =>
            {
                Beatmap.Value.Track.Start();

                SetContents(() =>
                {
                    var ruleset = new TaikoRuleset();
                    return new DrawableTaikoRuleset(ruleset, Beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo));
                });
            });

            AddUntilStep("Wait for fail state", () => mascots.All(d => d.State == TaikoMascotAnimationState.Fail));

            AddStep("Create hit (great)", () =>
            {
                foreach (var playfield in playfields)
                    addJudgement(playfield, HitResult.Great);
            });

            AddUntilStep("Wait for kiai state", () => mascots.All(d => d.State == TaikoMascotAnimationState.Kiai));
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

        private void setState(TaikoMascotAnimationState state)
        {
            foreach (var mascot in mascots)
                mascot?.ShowState(state);
        }

        private void addJudgement(TaikoPlayfield playfield, HitResult result)
        {
            var hit = new DrawableTestHit(new Hit(), result);
            Add(hit);

            playfield.OnNewResult(hit, new JudgementResult(hit.HitObject, new TaikoJudgement()) { Type = result });
        }

        private class TestDrawableTaikoMascot : DrawableTaikoMascot
        {
            public TestDrawableTaikoMascot(TaikoMascotAnimationState startingState = TaikoMascotAnimationState.Idle)
                : base(startingState)
            {
            }

            protected override TaikoMascotAnimationState GetFinalAnimationState(EffectControlPoint effectPoint, TaikoMascotAnimationState playfieldState)
            {
                return State;
            }
        }
    }
}
