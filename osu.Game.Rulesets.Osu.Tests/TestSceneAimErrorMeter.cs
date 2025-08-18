// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Utils;
using osu.Framework.Threading;
using osu.Game.Rulesets.Osu.HUD;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneAimErrorMeter : OsuManualInputManagerTestScene
    {
        private DependencyProvidingContainer dependencyContainer = null!;
        private ScoreProcessor scoreProcessor = null!;

        private TestAimErrorMeter aimErrorMeter = null!;

        private CircularContainer gameObject = null!;

        private ScheduledDelegate? automaticAdditionDelegate;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Hit marker size", 0f, 12f, 7f, t =>
            {
                if (aimErrorMeter.IsNotNull())
                    aimErrorMeter.HitMarkerSize.Value = t;
            });
            AddSliderStep("Average position marker size", 1f, 25f, 7f, t =>
            {
                if (aimErrorMeter.IsNotNull())
                    aimErrorMeter.AverageMarkerSize.Value = t;
            });
        }

        [SetUpSteps]
        public void SetupSteps() => AddStep("Create components", () =>
        {
            automaticAdditionDelegate?.Cancel();
            automaticAdditionDelegate = null;

            var ruleset = new OsuRuleset();

            scoreProcessor = new ScoreProcessor(ruleset);
            Child = dependencyContainer = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(ScoreProcessor), scoreProcessor)
                }
            };
            dependencyContainer.Children = new Drawable[]
            {
                aimErrorMeter = new TestAimErrorMeter
                {
                    Margin = new MarginPadding
                    {
                        Top = 100
                    },
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Scale = new Vector2(2),
                },

                gameObject = new CircularContainer
                {
                    Size = new Vector2(2 * OsuHitObject.OBJECT_RADIUS),
                    Position = new Vector2(256, 192),
                    Colour = Color4.Yellow,
                    Masking = true,
                    BorderThickness = 2,
                    BorderColour = Color4.White,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        },
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(4),
                        }
                    }
                }
            };
        });

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // the division by 2 is because CS=5 applies a 0.5x (plus fudge) multiplier to `OBJECT_RADIUS`
            aimErrorMeter.AddPoint((gameObject.ToLocalSpace(e.ScreenSpaceMouseDownPosition) - new Vector2(OsuHitObject.OBJECT_RADIUS)) / 2);
            return true;
        }

        [Test]
        public void TestManyHitPointsAutomatic()
        {
            AddStep("add scheduled delegate", () =>
            {
                automaticAdditionDelegate = Scheduler.AddDelayed(() =>
                {
                    var randomPos = new Vector2(
                        RNG.NextSingle(0, 2 * OsuHitObject.OBJECT_RADIUS),
                        RNG.NextSingle(0, 2 * OsuHitObject.OBJECT_RADIUS));

                    aimErrorMeter.AddPoint(randomPos - new Vector2(OsuHitObject.OBJECT_RADIUS));
                    InputManager.MoveMouseTo(gameObject.ToScreenSpace(randomPos));
                }, 1, true);
            });
            AddWaitStep("wait for some hit points", 10);
        }

        [Test]
        public void TestDisplayStyles()
        {
            AddStep("Switch hit position marker style to +", () => aimErrorMeter.HitMarkerStyle.Value = AimErrorMeter.MarkerStyle.Plus);
            AddStep("Switch hit position marker style to x", () => aimErrorMeter.HitMarkerStyle.Value = AimErrorMeter.MarkerStyle.X);
            AddStep("Switch average position marker style to +", () => aimErrorMeter.AverageMarkerStyle.Value = AimErrorMeter.MarkerStyle.Plus);
            AddStep("Switch average position marker style to x", () => aimErrorMeter.AverageMarkerStyle.Value = AimErrorMeter.MarkerStyle.X);

            AddStep("Switch position display to absolute", () => aimErrorMeter.PositionDisplayStyle.Value = AimErrorMeter.PositionDisplay.Absolute);
            AddStep("Switch position display to relative", () => aimErrorMeter.PositionDisplayStyle.Value = AimErrorMeter.PositionDisplay.Normalised);
        }

        [Test]
        public void TestManualPlacement()
        {
            AddStep("return user input", () => InputManager.UseParentInput = true);
        }

        private partial class TestAimErrorMeter : AimErrorMeter
        {
            public void AddPoint(Vector2 position)
            {
                OnNewJudgement(new OsuHitCircleJudgementResult(new HitCircle(), new OsuJudgement())
                {
                    CursorPositionAtHit = position
                });
            }
        }
    }
}
