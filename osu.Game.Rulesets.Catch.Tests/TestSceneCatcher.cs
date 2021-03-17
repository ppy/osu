// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Catch.UI;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatcher : OsuTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; }

        private Container<CaughtObject> droppedObjectContainer;

        private TestCatcher catcher;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            var difficulty = new BeatmapDifficulty
            {
                CircleSize = 0,
            };

            var trailContainer = new Container();
            droppedObjectContainer = new Container<CaughtObject>();
            catcher = new TestCatcher(trailContainer, droppedObjectContainer, difficulty);

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    trailContainer,
                    droppedObjectContainer,
                    catcher
                }
            };
        });

        [Test]
        public void TestCatcherHyperStateReverted()
        {
            DrawableCatchHitObject drawableObject1 = null;
            DrawableCatchHitObject drawableObject2 = null;
            JudgementResult result1 = null;
            JudgementResult result2 = null;
            AddStep("catch hyper fruit", () =>
            {
                attemptCatch(new Fruit { HyperDashTarget = new Fruit { X = 100 } }, out drawableObject1, out result1);
            });
            AddStep("catch normal fruit", () =>
            {
                attemptCatch(new Fruit(), out drawableObject2, out result2);
            });
            AddStep("revert second result", () =>
            {
                catcher.OnRevertResult(drawableObject2, result2);
            });
            checkHyperDash(true);
            AddStep("revert first result", () =>
            {
                catcher.OnRevertResult(drawableObject1, result1);
            });
            checkHyperDash(false);
        }

        [Test]
        public void TestCatcherAnimationStateReverted()
        {
            DrawableCatchHitObject drawableObject = null;
            JudgementResult result = null;
            AddStep("catch kiai fruit", () =>
            {
                attemptCatch(new TestKiaiFruit(), out drawableObject, out result);
            });
            checkState(CatcherAnimationState.Kiai);
            AddStep("revert result", () =>
            {
                catcher.OnRevertResult(drawableObject, result);
            });
            checkState(CatcherAnimationState.Idle);
        }

        [Test]
        public void TestCatcherCatchWidth()
        {
            var halfWidth = Catcher.CalculateCatchWidth(new BeatmapDifficulty { CircleSize = 0 }) / 2;
            AddStep("catch fruit", () =>
            {
                attemptCatch(new Fruit { X = -halfWidth + 1 });
                attemptCatch(new Fruit { X = halfWidth - 1 });
            });
            checkPlate(2);
            AddStep("miss fruit", () =>
            {
                attemptCatch(new Fruit { X = -halfWidth - 1 });
                attemptCatch(new Fruit { X = halfWidth + 1 });
            });
            checkPlate(2);
        }

        [Test]
        public void TestFruitChangesCatcherState()
        {
            AddStep("miss fruit", () => attemptCatch(new Fruit { X = 100 }));
            checkState(CatcherAnimationState.Fail);
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            checkState(CatcherAnimationState.Idle);
            AddStep("catch kiai fruit", () => attemptCatch(new TestKiaiFruit()));
            checkState(CatcherAnimationState.Kiai);
        }

        [Test]
        public void TestNormalFruitResetsHyperDashState()
        {
            AddStep("catch hyper fruit", () => attemptCatch(new Fruit
            {
                HyperDashTarget = new Fruit { X = 100 }
            }));
            checkHyperDash(true);
            AddStep("catch normal fruit", () => attemptCatch(new Fruit()));
            checkHyperDash(false);
        }

        [Test]
        public void TestTinyDropletMissPreservesCatcherState()
        {
            AddStep("catch hyper kiai fruit", () => attemptCatch(new TestKiaiFruit
            {
                HyperDashTarget = new Fruit { X = 100 }
            }));
            AddStep("catch tiny droplet", () => attemptCatch(new TinyDroplet()));
            AddStep("miss tiny droplet", () => attemptCatch(new TinyDroplet { X = 100 }));
            // catcher state and hyper dash state is preserved
            checkState(CatcherAnimationState.Kiai);
            checkHyperDash(true);
        }

        [Test]
        public void TestBananaMissPreservesCatcherState()
        {
            AddStep("catch hyper kiai fruit", () => attemptCatch(new TestKiaiFruit
            {
                HyperDashTarget = new Fruit { X = 100 }
            }));
            AddStep("miss banana", () => attemptCatch(new Banana { X = 100 }));
            // catcher state is preserved but hyper dash state is reset
            checkState(CatcherAnimationState.Kiai);
            checkHyperDash(false);
        }

        [Test]
        public void TestCatcherStacking()
        {
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            checkPlate(1);
            AddStep("catch more fruits", () => attemptCatch(new Fruit(), 9));
            checkPlate(10);
            AddAssert("caught objects are stacked", () =>
                catcher.CaughtObjects.All(obj => obj.Y <= 0) &&
                catcher.CaughtObjects.Any(obj => obj.Y == 0) &&
                catcher.CaughtObjects.Any(obj => obj.Y < -20));
        }

        [Test]
        public void TestCatcherExplosionAndDropping()
        {
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            AddStep("catch tiny droplet", () => attemptCatch(new TinyDroplet()));
            AddAssert("tiny droplet is exploded", () => catcher.CaughtObjects.Count() == 1 && droppedObjectContainer.Count == 1);
            AddUntilStep("wait explosion", () => !droppedObjectContainer.Any());
            AddStep("catch more fruits", () => attemptCatch(new Fruit(), 9));
            AddStep("explode", () => catcher.Explode());
            AddAssert("fruits are exploded", () => !catcher.CaughtObjects.Any() && droppedObjectContainer.Count == 10);
            AddUntilStep("wait explosion", () => !droppedObjectContainer.Any());
            AddStep("catch fruits", () => attemptCatch(new Fruit(), 10));
            AddStep("drop", () => catcher.Drop());
            AddAssert("fruits are dropped", () => !catcher.CaughtObjects.Any() && droppedObjectContainer.Count == 10);
        }

        [Test]
        public void TestHitLightingColour()
        {
            var fruitColour = SkinConfiguration.DefaultComboColours[1];
            AddStep("enable hit lighting", () => config.Set(OsuSetting.HitLighting, true));
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            AddAssert("correct hit lighting colour", () =>
                catcher.ChildrenOfType<HitExplosion>().First()?.ObjectColour == fruitColour);
        }

        [Test]
        public void TestHitLightingDisabled()
        {
            AddStep("disable hit lighting", () => config.Set(OsuSetting.HitLighting, false));
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            AddAssert("no hit lighting", () => !catcher.ChildrenOfType<HitExplosion>().Any());
        }

        private void checkPlate(int count) => AddAssert($"{count} objects on the plate", () => catcher.CaughtObjects.Count() == count);

        private void checkState(CatcherAnimationState state) => AddAssert($"catcher state is {state}", () => catcher.CurrentState == state);

        private void checkHyperDash(bool state) => AddAssert($"catcher is {(state ? "" : "not ")}hyper dashing", () => catcher.HyperDashing == state);

        private void attemptCatch(CatchHitObject hitObject, int count = 1)
        {
            for (var i = 0; i < count; i++)
                attemptCatch(hitObject, out _, out _);
        }

        private void attemptCatch(CatchHitObject hitObject, out DrawableCatchHitObject drawableObject, out JudgementResult result)
        {
            hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            drawableObject = createDrawableObject(hitObject);
            result = createResult(hitObject);
            applyResult(drawableObject, result);
        }

        private void applyResult(DrawableCatchHitObject drawableObject, JudgementResult result)
        {
            // Load DHO to set colour of hit explosion correctly
            Add(drawableObject);
            drawableObject.OnLoadComplete += _ =>
            {
                catcher.OnNewResult(drawableObject, result);
                drawableObject.Expire();
            };
        }

        private JudgementResult createResult(CatchHitObject hitObject)
        {
            return new CatchJudgementResult(hitObject, hitObject.CreateJudgement())
            {
                Type = catcher.CanCatch(hitObject) ? HitResult.Great : HitResult.Miss
            };
        }

        private DrawableCatchHitObject createDrawableObject(CatchHitObject hitObject)
        {
            switch (hitObject)
            {
                case Banana banana:
                    return new DrawableBanana(banana);

                case Droplet droplet:
                    return new DrawableDroplet(droplet);

                case Fruit fruit:
                    return new DrawableFruit(fruit);

                default:
                    throw new ArgumentOutOfRangeException(nameof(hitObject));
            }
        }

        public class TestCatcher : Catcher
        {
            public IEnumerable<CaughtObject> CaughtObjects => this.ChildrenOfType<CaughtObject>();

            public TestCatcher(Container trailsTarget, Container<CaughtObject> droppedObjectTarget, BeatmapDifficulty difficulty)
                : base(trailsTarget, droppedObjectTarget, difficulty)
            {
            }
        }

        public class TestKiaiFruit : Fruit
        {
            protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
            {
                controlPointInfo.Add(0, new EffectControlPoint { KiaiMode = true });
                base.ApplyDefaultsToSelf(controlPointInfo, difficulty);
            }
        }
    }
}
