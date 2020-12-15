// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePoolingRuleset : OsuTestScene
    {
        private const double time_between_objects = 1000;

        private TestDrawablePoolingRuleset drawableRuleset;

        [Test]
        public void TestReusedWithHitObjectsSpacedFarApart()
        {
            ManualClock clock = null;

            createTest(new Beatmap
            {
                HitObjects =
                {
                    new HitObject(),
                    new HitObject { StartTime = time_between_objects }
                }
            }, 1, () => new FramedClock(clock = new ManualClock()));

            DrawableTestHitObject firstObject = null;
            AddUntilStep("first object shown", () => this.ChildrenOfType<DrawableTestHitObject>().SingleOrDefault()?.HitObject == drawableRuleset.Beatmap.HitObjects[0]);
            AddStep("get DHO", () => firstObject = this.ChildrenOfType<DrawableTestHitObject>().Single());

            AddStep("fast forward to second object", () => clock.CurrentTime = drawableRuleset.Beatmap.HitObjects[1].StartTime);

            AddUntilStep("second object shown", () => this.ChildrenOfType<DrawableTestHitObject>().SingleOrDefault()?.HitObject == drawableRuleset.Beatmap.HitObjects[1]);
            AddAssert("DHO reused", () => this.ChildrenOfType<DrawableTestHitObject>().Single() == firstObject);
        }

        [Test]
        public void TestCustomTransformsClearedBetweenReuses()
        {
            ManualClock clock = null;

            createTest(new Beatmap
            {
                HitObjects =
                {
                    new HitObject(),
                    new HitObject { StartTime = 2000 }
                }
            }, 1, () => new FramedClock(clock = new ManualClock()));

            DrawableTestHitObject firstObject = null;
            Vector2 position = default;

            AddUntilStep("first object shown", () => this.ChildrenOfType<DrawableTestHitObject>().SingleOrDefault()?.HitObject == drawableRuleset.Beatmap.HitObjects[0]);
            AddStep("get DHO", () => firstObject = this.ChildrenOfType<DrawableTestHitObject>().Single());
            AddStep("store position", () => position = firstObject.Position);
            AddStep("add custom transform", () => firstObject.ApplyCustomUpdateState += onStateUpdate);

            AddStep("fast forward past first object", () => clock.CurrentTime = 1500);
            AddStep("unapply custom transform", () => firstObject.ApplyCustomUpdateState -= onStateUpdate);

            AddStep("fast forward to second object", () => clock.CurrentTime = drawableRuleset.Beatmap.HitObjects[1].StartTime);
            AddUntilStep("second object shown", () => this.ChildrenOfType<DrawableTestHitObject>().SingleOrDefault()?.HitObject == drawableRuleset.Beatmap.HitObjects[1]);
            AddAssert("DHO reused", () => this.ChildrenOfType<DrawableTestHitObject>().Single() == firstObject);
            AddAssert("object in new position", () => firstObject.Position != position);

            void onStateUpdate(DrawableHitObject hitObject, ArmedState state)
            {
                using (hitObject.BeginAbsoluteSequence(hitObject.StateUpdateTime))
                    hitObject.MoveToOffset(new Vector2(-100, 0));
            }
        }

        [Test]
        public void TestNotReusedWithHitObjectsSpacedClose()
        {
            ManualClock clock = null;

            createTest(new Beatmap
            {
                HitObjects =
                {
                    new HitObject(),
                    new HitObject { StartTime = 250 }
                }
            }, 2, () => new FramedClock(clock = new ManualClock()));

            AddStep("fast forward to second object", () => clock.CurrentTime = drawableRuleset.Beatmap.HitObjects[1].StartTime);

            AddUntilStep("two DHOs shown", () => this.ChildrenOfType<DrawableTestHitObject>().Count() == 2);
            AddAssert("DHOs have different hitobjects",
                () => this.ChildrenOfType<DrawableTestHitObject>().ElementAt(0).HitObject != this.ChildrenOfType<DrawableTestHitObject>().ElementAt(1).HitObject);
        }

        [Test]
        public void TestManyHitObjects()
        {
            var beatmap = new Beatmap();

            for (int i = 0; i < 500; i++)
                beatmap.HitObjects.Add(new HitObject { StartTime = i * 10 });

            createTest(beatmap, 100);

            AddUntilStep("any DHOs shown", () => this.ChildrenOfType<DrawableTestHitObject>().Any());
            AddUntilStep("no DHOs shown", () => !this.ChildrenOfType<DrawableTestHitObject>().Any());
        }

        private void createTest(IBeatmap beatmap, int poolSize, Func<IFrameBasedClock> createClock = null) => AddStep("create test", () =>
        {
            var ruleset = new TestPoolingRuleset();

            drawableRuleset = (TestDrawablePoolingRuleset)ruleset.CreateDrawableRulesetWith(CreateWorkingBeatmap(beatmap).GetPlayableBeatmap(ruleset.RulesetInfo));
            drawableRuleset.FrameStablePlayback = true;
            drawableRuleset.PoolSize = poolSize;

            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = createClock?.Invoke() ?? new FramedOffsetClock(Clock, false) { Offset = -Clock.CurrentTime },
                Child = drawableRuleset
            };
        });

        #region Ruleset

        private class TestPoolingRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => new TestDrawablePoolingRuleset(this, beatmap, mods);

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new TestBeatmapConverter(beatmap, this);

            public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => throw new NotImplementedException();

            public override string Description { get; } = string.Empty;

            public override string ShortName { get; } = string.Empty;
        }

        private class TestDrawablePoolingRuleset : DrawableRuleset<TestHitObject>
        {
            public int PoolSize;

            public TestDrawablePoolingRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
                : base(ruleset, beatmap, mods)
            {
            }

            public override DrawableHitObject<TestHitObject> CreateDrawableRepresentation(TestHitObject h) => null;

            protected override PassThroughInputManager CreateInputManager() => new PassThroughInputManager();

            protected override Playfield CreatePlayfield() => new TestPlayfield(PoolSize);
        }

        private class TestPlayfield : Playfield
        {
            private readonly int poolSize;

            public TestPlayfield(int poolSize)
            {
                this.poolSize = poolSize;
                AddInternal(HitObjectContainer);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RegisterPool<TestHitObject, DrawableTestHitObject>(poolSize);
            }

            protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new TestHitObjectLifetimeEntry(hitObject);

            protected override GameplayCursorContainer CreateCursor() => null;
        }

        private class TestHitObjectLifetimeEntry : HitObjectLifetimeEntry
        {
            public TestHitObjectLifetimeEntry(HitObject hitObject)
                : base(hitObject)
            {
            }

            protected override double InitialLifetimeOffset => 0;
        }

        private class TestBeatmapConverter : BeatmapConverter<TestHitObject>
        {
            public TestBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
                : base(beatmap, ruleset)
            {
            }

            public override bool CanConvert() => true;

            protected override IEnumerable<TestHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
            {
                yield return new TestHitObject
                {
                    StartTime = original.StartTime,
                    Duration = 250
                };
            }
        }

        #endregion

        #region HitObject

        private class TestHitObject : ConvertHitObject
        {
            public double EndTime => StartTime + Duration;

            public double Duration { get; set; }
        }

        private class DrawableTestHitObject : DrawableHitObject<TestHitObject>
        {
            public DrawableTestHitObject()
                : base(null)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Size = new Vector2(50, 50);

                Colour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1f);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddInternal(new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                });
            }

            protected override void OnApply()
            {
                base.OnApply();
                Position = new Vector2(RNG.Next(-200, 200), RNG.Next(-200, 200));
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (timeOffset > HitObject.Duration)
                    ApplyResult(r => r.Type = r.Judgement.MaxResult);
            }

            protected override void UpdateHitStateTransforms(ArmedState state)
            {
                base.UpdateHitStateTransforms(state);

                switch (state)
                {
                    case ArmedState.Hit:
                    case ArmedState.Miss:
                        this.FadeOut(250);
                        break;
                }
            }
        }

        #endregion
    }
}
