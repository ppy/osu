// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePoolingRuleset : OsuTestScene
    {
        private const double time_between_objects = 1000;

        private TestDrawablePoolingRuleset drawableRuleset;

        private TestPlayfield playfield => (TestPlayfield)drawableRuleset.Playfield;

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

        [Test]
        public void TestRevertResult()
        {
            ManualClock clock = null;
            Beatmap beatmap;

            createTest(beatmap = new Beatmap
            {
                HitObjects =
                {
                    new TestHitObject { StartTime = 0 },
                    new TestHitObject { StartTime = 500 },
                    new TestHitObject { StartTime = 1000 },
                }
            }, 10, () => new FramedClock(clock = new ManualClock()));

            AddStep("fast forward to end", () => clock.CurrentTime = beatmap.HitObjects[^1].GetEndTime() + 100);
            AddUntilStep("all judged", () => playfield.JudgedObjects.Count, () => Is.EqualTo(3));

            AddStep("rewind to middle", () => clock.CurrentTime = beatmap.HitObjects[1].StartTime - 100);
            AddUntilStep("some results reverted", () => playfield.JudgedObjects.Count, () => Is.EqualTo(1));

            AddStep("fast forward to end", () => clock.CurrentTime = beatmap.HitObjects[^1].GetEndTime() + 100);
            AddUntilStep("all judged", () => playfield.JudgedObjects.Count, () => Is.EqualTo(3));

            AddStep("disable frame stability", () => drawableRuleset.FrameStablePlayback = false);
            AddStep("instant seek to start", () => clock.CurrentTime = beatmap.HitObjects[0].StartTime - 100);
            AddAssert("all results reverted", () => playfield.JudgedObjects.Count, () => Is.EqualTo(0));
        }

        [Test]
        public void TestRevertNestedObjects()
        {
            ManualClock clock = null;

            var beatmap = new Beatmap();
            beatmap.HitObjects.Add(new TestHitObjectWithNested
            {
                Duration = 40,
                NestedObjects = new HitObject[]
                {
                    new PooledNestedHitObject { StartTime = 10 },
                    new PooledNestedHitObject { StartTime = 20 },
                    new PooledNestedHitObject { StartTime = 30 }
                }
            });

            createTest(beatmap, 10, () => new FramedClock(clock = new ManualClock()));

            AddStep("skip to middle of object", () => clock.CurrentTime = (beatmap.HitObjects[0].StartTime + beatmap.HitObjects[0].GetEndTime()) / 2);
            AddAssert("2 objects judged", () => playfield.JudgedObjects.Count, () => Is.EqualTo(2));

            AddStep("skip to before end of object", () => clock.CurrentTime = beatmap.HitObjects[0].GetEndTime() - 1);
            AddAssert("3 objects judged", () => playfield.JudgedObjects.Count, () => Is.EqualTo(3));

            DrawableHitObject drawableHitObject = null;
            HashSet<HitObject> revertedHitObjects = new HashSet<HitObject>();

            AddStep("retrieve drawable hit object", () => drawableHitObject = playfield.ChildrenOfType<DrawableTestHitObjectWithNested>().Single());
            AddStep("set up revert tracking", () =>
            {
                revertedHitObjects.Clear();
                drawableHitObject.OnRevertResult += (ho, _) => revertedHitObjects.Add(ho.HitObject);
            });
            AddStep("skip back to object start", () => clock.CurrentTime = beatmap.HitObjects[0].StartTime);
            AddAssert("3 reverts fired", () => revertedHitObjects, () => Has.Count.EqualTo(3));
            AddAssert("no objects judged", () => playfield.JudgedObjects.Count, () => Is.EqualTo(0));
        }

        [Test]
        public void TestApplyHitResultOnKilled()
        {
            ManualClock clock = null;

            var beatmap = new Beatmap();
            beatmap.HitObjects.Add(new TestKilledHitObject { Duration = 20 });

            createTest(beatmap, 10, () => new FramedClock(clock = new ManualClock()));

            AddStep("skip past object", () => clock.CurrentTime = beatmap.HitObjects[0].GetEndTime() + 1000);

            AddAssert("object judged", () => playfield.JudgedObjects.Count == 1);
        }

        [Test]
        public void TestPooledObjectWithNonPooledNesteds()
        {
            ManualClock clock = null;
            TestHitObjectWithNested hitObjectWithNested;

            var beatmap = new Beatmap();
            beatmap.HitObjects.Add(hitObjectWithNested = new TestHitObjectWithNested
            {
                Duration = 40,
                NestedObjects = new HitObject[]
                {
                    new PooledNestedHitObject { StartTime = 10 },
                    new NonPooledNestedHitObject { StartTime = 20 },
                    new NonPooledNestedHitObject { StartTime = 30 }
                }
            });

            createTest(beatmap, 10, () => new FramedClock(clock = new ManualClock()));

            AddAssert("hitobject entry has all nesteds", () => playfield.HitObjectContainer.Entries.Single().NestedEntries, () => Has.Count.EqualTo(3));

            AddStep("skip to middle of object", () => clock.CurrentTime = (hitObjectWithNested.StartTime + hitObjectWithNested.GetEndTime()) / 2);
            AddAssert("2 objects judged", () => playfield.JudgedObjects.Count, () => Is.EqualTo(2));
            AddAssert("entry not all judged", () => playfield.HitObjectContainer.Entries.Single().AllJudged, () => Is.False);

            AddStep("skip to before end of object", () => clock.CurrentTime = hitObjectWithNested.GetEndTime() - 1);
            AddAssert("3 objects judged", () => playfield.JudgedObjects.Count, () => Is.EqualTo(3));
            AddAssert("entry not all judged", () => playfield.HitObjectContainer.Entries.Single().AllJudged, () => Is.False);

            AddStep("removing object doesn't crash", () => playfield.Remove(hitObjectWithNested));
            AddStep("clear judged", () => playfield.JudgedObjects.Clear());

            AddStep("add object back", () => playfield.Add(hitObjectWithNested));
            AddAssert("entry not all judged", () => playfield.HitObjectContainer.Entries.Single().AllJudged, () => Is.False);

            AddStep("skip to long past object", () => clock.CurrentTime = 100_000);
            // the parent entry should still be linked to nested entries of pooled objects that are managed externally
            // but not contain synthetic entries that were created for the non-pooled objects.
            AddAssert("entry still has non-synthetic nested entries", () => playfield.HitObjectContainer.Entries.Single().NestedEntries, () => Has.Count.EqualTo(1));
            AddAssert("entry all judged", () => playfield.HitObjectContainer.Entries.Single().AllJudged, () => Is.True);
        }

        private void createTest(IBeatmap beatmap, int poolSize, Func<IFrameBasedClock> createClock = null)
        {
            AddStep("create test", () =>
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
        }

        #region Ruleset

        private class TestPoolingRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => new TestDrawablePoolingRuleset(this, beatmap, mods);

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new TestBeatmapConverter(beatmap, this);

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();

            public override string Description { get; } = string.Empty;

            public override string ShortName { get; } = string.Empty;
        }

        private partial class TestDrawablePoolingRuleset : DrawableRuleset<TestHitObject>
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

        private partial class TestPlayfield : Playfield
        {
            public readonly HashSet<HitObject> JudgedObjects = new HashSet<HitObject>();

            private readonly int poolSize;

            public TestPlayfield(int poolSize)
            {
                this.poolSize = poolSize;
                AddInternal(HitObjectContainer);
                NewResult += (_, r) =>
                {
                    Assert.That(JudgedObjects, Has.No.Member(r.HitObject));
                    JudgedObjects.Add(r.HitObject);
                };
                RevertResult += r =>
                {
                    Assert.That(JudgedObjects, Has.Member(r.HitObject));
                    JudgedObjects.Remove(r.HitObject);
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RegisterPool<TestHitObject, DrawableTestHitObject>(poolSize);
                RegisterPool<TestKilledHitObject, DrawableTestKilledHitObject>(poolSize);
                RegisterPool<TestHitObjectWithNested, DrawableTestHitObjectWithNested>(poolSize);
                RegisterPool<PooledNestedHitObject, DrawableNestedHitObject>(poolSize);
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
                switch (original)
                {
                    case TestKilledHitObject h:
                        yield return h;

                        break;

                    default:
                        yield return new TestHitObject
                        {
                            StartTime = original.StartTime,
                            Duration = 250
                        };

                        break;
                }
            }
        }

        #endregion

        #region HitObjects

        private class TestHitObject : ConvertHitObject, IHasDuration
        {
            public double EndTime => StartTime + Duration;

            public double Duration { get; set; }
        }

        private partial class DrawableTestHitObject : DrawableHitObject<TestHitObject>
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
                    ApplyMaxResult();
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

        private class TestKilledHitObject : TestHitObject
        {
        }

        private partial class DrawableTestKilledHitObject : DrawableHitObject<TestKilledHitObject>
        {
            public DrawableTestKilledHitObject()
                : base(null)
            {
            }

            protected override void UpdateHitStateTransforms(ArmedState state)
            {
                base.UpdateHitStateTransforms(state);
                Expire();
            }

            public override void OnKilled()
            {
                base.OnKilled();
                ApplyMinResult();
            }
        }

        private class TestHitObjectWithNested : TestHitObject
        {
            public IEnumerable<HitObject> NestedObjects { get; init; } = Array.Empty<HitObject>();

            protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
            {
                base.CreateNestedHitObjects(cancellationToken);

                foreach (var ho in NestedObjects)
                    AddNested(ho);
            }
        }

        private class PooledNestedHitObject : ConvertHitObject
        {
        }

        private class NonPooledNestedHitObject : ConvertHitObject
        {
        }

        private partial class DrawableTestHitObjectWithNested : DrawableHitObject<TestHitObjectWithNested>
        {
            private Container nestedContainer;

            public DrawableTestHitObjectWithNested()
                : base(null)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AddRangeInternal(new Drawable[]
                {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Red
                    },
                    nestedContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                });
            }

            protected override void OnApply()
            {
                base.OnApply();

                Size = new Vector2(200, 50);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            protected override void AddNestedHitObject(DrawableHitObject hitObject)
            {
                base.AddNestedHitObject(hitObject);
                nestedContainer.Add(hitObject);
            }

            protected override void ClearNestedHitObjects()
            {
                base.ClearNestedHitObjects();
                nestedContainer.Clear(false);
            }

            protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
                => hitObject is NonPooledNestedHitObject nonPooled ? new DrawableNestedHitObject(nonPooled) : null;

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                base.CheckForResult(userTriggered, timeOffset);
                if (timeOffset >= 0)
                    ApplyMaxResult();
            }
        }

        private partial class DrawableNestedHitObject : DrawableHitObject
        {
            public DrawableNestedHitObject()
            {
            }

            public DrawableNestedHitObject(PooledNestedHitObject hitObject)
                : base(hitObject)
            {
            }

            public DrawableNestedHitObject(NonPooledNestedHitObject hitObject)
                : base(hitObject)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(15);
                Colour = Colour4.White;
                RelativePositionAxes = Axes.Both;
                Origin = Anchor.Centre;

                AddInternal(new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                });
            }

            protected override void OnApply()
            {
                base.OnApply();

                X = (float)((HitObject.StartTime - ParentHitObject!.HitObject.StartTime) / (ParentHitObject.HitObject.GetEndTime() - ParentHitObject.HitObject.StartTime));
                Y = 0.5f;

                LifetimeStart = ParentHitObject.LifetimeStart;
                LifetimeEnd = ParentHitObject.LifetimeEnd;
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                base.CheckForResult(userTriggered, timeOffset);
                if (timeOffset >= 0)
                    ApplyMaxResult();
            }
        }

        #endregion
    }
}
