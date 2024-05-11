// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.UI;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneCatcher : OsuTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; }

        private DroppedObjectContainer droppedObjectContainer;

        private TestCatcher catcher;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            var difficulty = new BeatmapDifficulty
            {
                CircleSize = 0,
            };

            droppedObjectContainer = new DroppedObjectContainer();

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    droppedObjectContainer,
                    catcher = new TestCatcher(droppedObjectContainer, difficulty),
                }
            };
        });

        [Test]
        public void TestCatcherHyperStateReverted()
        {
            JudgementResult result1 = null;
            JudgementResult result2 = null;
            AddStep("catch hyper fruit", () =>
            {
                result1 = attemptCatch(new Fruit { HyperDashTarget = new Fruit { X = 100 } });
            });
            AddStep("catch normal fruit", () =>
            {
                result2 = attemptCatch(new Fruit());
            });
            AddStep("revert second result", () =>
            {
                catcher.OnRevertResult(result2);
            });
            checkHyperDash(true);
            AddStep("revert first result", () =>
            {
                catcher.OnRevertResult(result1);
            });
            checkHyperDash(false);
        }

        [Test]
        public void TestCatcherAnimationStateReverted()
        {
            JudgementResult result = null;
            AddStep("catch kiai fruit", () =>
            {
                result = attemptCatch(new TestKiaiFruit());
            });
            checkState(CatcherAnimationState.Kiai);
            AddStep("revert result", () =>
            {
                catcher.OnRevertResult(result);
            });
            checkState(CatcherAnimationState.Idle);
        }

        [Test]
        public void TestCatcherCatchWidth()
        {
            float halfWidth = Catcher.CalculateCatchWidth(new BeatmapDifficulty { CircleSize = 0 }) / 2;

            AddStep("move catcher to center", () => catcher.X = CatchPlayfield.CENTER_X);

            float leftPlateBounds = CatchPlayfield.CENTER_X - halfWidth;
            float rightPlateBounds = CatchPlayfield.CENTER_X + halfWidth;

            AddStep("catch fruit", () =>
            {
                attemptCatch(new Fruit { X = leftPlateBounds + 1 });
                attemptCatch(new Fruit { X = rightPlateBounds - 1 });
            });
            checkPlate(2);

            AddStep("miss fruit", () =>
            {
                attemptCatch(new Fruit { X = leftPlateBounds - 1 });
                attemptCatch(new Fruit { X = rightPlateBounds + 1 });
            });
            checkPlate(2);
        }

        [Test]
        public void TestFruitClampedToCatchableRegion()
        {
            AddStep("catch fruit left", () => attemptCatch(new Fruit { X = -CatchPlayfield.WIDTH }));
            checkPlate(1);
            AddStep("move catcher to right", () => catcher.X = CatchPlayfield.WIDTH);
            AddStep("catch fruit right", () => attemptCatch(new Fruit { X = CatchPlayfield.WIDTH * 2 }));
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
        public void TestLastBananaShouldClearPlateOnMiss()
        {
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            checkPlate(1);
            AddStep("miss banana", () => attemptCatch(new Banana { X = 100 }));
            checkPlate(1);
            AddStep("miss last banana", () => attemptCatch(new Banana { LastInCombo = true, X = 100 }));
            checkPlate(0);
        }

        [Test]
        public void TestLastBananaShouldClearPlateOnCatch()
        {
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            checkPlate(1);
            AddStep("catch banana", () => attemptCatch(new Banana()));
            checkPlate(2);
            AddStep("catch last banana", () => attemptCatch(new Banana { LastInCombo = true }));
            checkPlate(0);
        }

        [Test]
        public void TestCatcherRandomStacking()
        {
            AddStep("catch more fruits", () => attemptCatch(() => new Fruit
            {
                X = (RNG.NextSingle() - 0.5f) * Catcher.CalculateCatchWidth(Vector2.One)
            }, 50));
        }

        [Test]
        public void TestCatcherStackingSameCaughtPosition()
        {
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            checkPlate(1);
            AddStep("catch more fruits", () => attemptCatch(() => new Fruit(), 9));
            checkPlate(10);
            AddAssert("caught objects are stacked", () =>
                catcher.CaughtObjects.All(obj => obj.Y <= 0) &&
                catcher.CaughtObjects.Any(obj => obj.Y == 0) &&
                catcher.CaughtObjects.Any(obj => obj.Y < 0));
        }

        [Test]
        public void TestCatcherExplosionAndDropping()
        {
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            AddStep("catch tiny droplet", () => attemptCatch(new TinyDroplet()));
            AddAssert("tiny droplet is exploded", () => catcher.CaughtObjects.Count() == 1 && droppedObjectContainer.Count == 1);
            AddUntilStep("wait explosion", () => !droppedObjectContainer.Any());
            AddStep("catch more fruits", () => attemptCatch(() => new Fruit(), 9));
            AddStep("explode", () => catcher.Explode());
            AddAssert("fruits are exploded", () => !catcher.CaughtObjects.Any() && droppedObjectContainer.Count == 10);
            AddUntilStep("wait explosion", () => !droppedObjectContainer.Any());
            AddStep("catch fruits", () => attemptCatch(() => new Fruit(), 10));
            AddStep("drop", () => catcher.Drop());
            AddAssert("fruits are dropped", () => !catcher.CaughtObjects.Any() && droppedObjectContainer.Count == 10);
        }

        [Test]
        public void TestHitLightingColour()
        {
            AddStep("enable hit lighting", () => config.SetValue(OsuSetting.HitLighting, true));
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            AddAssert("correct hit lighting colour", () => catcher.ChildrenOfType<HitExplosion>().First()?.Entry?.ObjectColour == this.ChildrenOfType<DrawableCatchHitObject>().First().AccentColour.Value);
        }

        [Test]
        public void TestHitLightingDisabled()
        {
            AddStep("disable hit lighting", () => config.SetValue(OsuSetting.HitLighting, false));
            AddStep("catch fruit", () => attemptCatch(new Fruit()));
            AddAssert("no hit lighting", () => !catcher.ChildrenOfType<HitExplosion>().Any());
        }

        private void checkPlate(int count) => AddAssert($"{count} objects on the plate", () => catcher.CaughtObjects.Count() == count);

        private void checkState(CatcherAnimationState state) => AddAssert($"catcher state is {state}", () => catcher.CurrentState == state);

        private void checkHyperDash(bool state) => AddAssert($"catcher is {(state ? "" : "not ")}hyper dashing", () => catcher.HyperDashing == state);

        private void attemptCatch(Func<CatchHitObject> hitObject, int count)
        {
            for (int i = 0; i < count; i++)
                attemptCatch(hitObject());
        }

        private JudgementResult attemptCatch(CatchHitObject hitObject)
        {
            hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            var drawableObject = createDrawableObject(hitObject);
            var result = createResult(hitObject);
            applyResult(drawableObject, result);
            return result;
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
            return new CatchJudgementResult(hitObject, hitObject.Judgement)
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

        public partial class TestCatcher : Catcher
        {
            public IEnumerable<CaughtObject> CaughtObjects => this.ChildrenOfType<CaughtObject>();

            public TestCatcher(DroppedObjectContainer droppedObjectTarget, IBeatmapDifficultyInfo difficulty)
                : base(droppedObjectTarget, difficulty)
            {
            }
        }

        public class TestKiaiFruit : Fruit
        {
            protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
            {
                controlPointInfo.Add(0, new EffectControlPoint { KiaiMode = true });
                base.ApplyDefaultsToSelf(controlPointInfo, difficulty);
            }
        }
    }
}
