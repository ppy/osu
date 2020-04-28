// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
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

        private Bindable<WorkingBeatmap> workingBeatmap;

        private readonly List<DrawableTaikoMascot> mascots = new List<DrawableTaikoMascot>();
        private readonly List<TaikoPlayfield> playfields = new List<TaikoPlayfield>();
        private readonly List<DrawableTaikoRuleset> rulesets = new List<DrawableTaikoRuleset>();

        [BackgroundDependencyLoader]
        private void load(Bindable<WorkingBeatmap> beatmap)
        {
            workingBeatmap = beatmap;
        }

        [Test]
        public void TestStateTextures()
        {
            AddStep("Set beatmap", () => setBeatmap());

            AddStep("Create mascot (idle)", () =>
            {
                mascots.Clear();

                SetContents(() =>
                {
                    var mascot = new TestDrawableTaikoMascot();
                    mascots.Add(mascot);
                    return mascot;
                });
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
                rulesets.Clear();
                SetContents(() =>
                {
                    var ruleset = new TaikoRuleset();
                    var drawableRuleset = new DrawableTaikoRuleset(ruleset, workingBeatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo));
                    rulesets.Add(drawableRuleset);
                    return drawableRuleset;
                });
            });

            AddStep("Collect playfields", collectPlayfields);
            AddStep("Collect mascots", collectMascots);

            AddStep("Create hit (great)", () => addJudgement(HitResult.Miss));
            AddUntilStep("Wait for idle state", () => checkForState(TaikoMascotAnimationState.Fail));

            AddStep("Create hit (great)", () => addJudgement(HitResult.Great));
            AddUntilStep("Wait for idle state", () => checkForState(TaikoMascotAnimationState.Idle));
        }

        [Test]
        public void TestKiai()
        {
            AddStep("Set beatmap", () => setBeatmap(true));

            AddUntilStep("Wait for beatmap to be loaded", () => workingBeatmap.Value.Track.IsLoaded);

            AddStep("Create kiai ruleset", () =>
            {
                workingBeatmap.Value.Track.Start();

                rulesets.Clear();
                SetContents(() =>
                {
                    var ruleset = new TaikoRuleset();
                    var drawableRuleset = new DrawableTaikoRuleset(ruleset, workingBeatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo));
                    rulesets.Add(drawableRuleset);
                    return drawableRuleset;
                });
            });

            AddStep("Collect playfields", collectPlayfields);
            AddStep("Collect mascots", collectMascots);

            AddUntilStep("Wait for idle state", () => checkForState(TaikoMascotAnimationState.Fail));

            AddStep("Create hit (great)", () => addJudgement(HitResult.Great));
            AddUntilStep("Wait for idle state", () => checkForState(TaikoMascotAnimationState.Kiai));
        }

        private void setBeatmap(bool kiai = false)
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint { BeatLength = 90 });

            if (kiai)
                controlPointInfo.Add(0, new EffectControlPoint { KiaiMode = true });

            workingBeatmap.Value = CreateWorkingBeatmap(new Beatmap
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

        private void collectPlayfields()
        {
            playfields.Clear();
            foreach (var ruleset in rulesets)
                playfields.Add(ruleset.ChildrenOfType<TaikoPlayfield>().Single());
        }

        private void collectMascots()
        {
            mascots.Clear();

            foreach (var playfield in playfields)
            {
                var mascot = playfield.ChildrenOfType<TestDrawableTaikoMascot>()
                                      .SingleOrDefault();

                if (mascot != null)
                    mascots.Add(mascot);
            }
        }

        private void addJudgement(HitResult result)
        {
            foreach (var playfield in playfields)
                playfield.OnNewResult(new DrawableHit(new Hit()), new JudgementResult(new HitObject(), new TaikoJudgement()) { Type = result });
        }

        private bool checkForState(TaikoMascotAnimationState state) => mascots.All(d => d.State == state);

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
