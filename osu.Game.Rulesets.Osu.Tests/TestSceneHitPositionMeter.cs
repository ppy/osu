// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using System;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Framework.Threading;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneHitPositionMeter : OsuManualInputManagerTestScene
    {
        private DependencyProvidingContainer dependencyContainer = null!;
        private ScoreProcessor scoreProcessor = null!;

        private TestHitPositionMeter hitPositionMeter = null!;

        private CircularContainer gameObject = null!;

        private ScheduledDelegate? automaticAdditionDelegate;

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
                hitPositionMeter = new TestHitPositionMeter
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
                    Size = new Vector2(100),
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
            hitPositionMeter.AddPoint(gameObject.ToLocalSpace(e.ScreenSpaceMouseDownPosition) - new Vector2(50));
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
                        RNG.NextSingle(0, 100),
                        RNG.NextSingle(0, 100));

                    hitPositionMeter.AddPoint(randomPos - new Vector2(50));
                    InputManager.MoveMouseTo(gameObject.ToScreenSpace(randomPos));
                }, 1, true);
            });

            AddWaitStep("wait for some hit points", 10);
        }

        [Test]
        public void TestManualPlacement()
        {
            AddStep("return user input", () => InputManager.UseParentInput = true);
        }

        private partial class TestHitPositionMeter : HitPositionMeter
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
