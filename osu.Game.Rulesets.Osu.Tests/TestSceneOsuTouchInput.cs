// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneOsuTouchInput : OsuManualInputManagerTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private DefaultKeyCounter leftKeyCounter = null!;

        private DefaultKeyCounter rightKeyCounter = null!;

        private OsuInputManager osuInputManager = null!;

        private Container mainContent = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            releaseAllTouches();

            AddStep("Create tests", () =>
            {
                InputTrigger triggerLeft;
                InputTrigger triggerRight;

                Children = new Drawable[]
                {
                    osuInputManager = new OsuInputManager(new OsuRuleset().RulesetInfo)
                    {
                        Child = mainContent = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new OsuCursorContainer
                                {
                                    Depth = float.MinValue,
                                },
                                triggerLeft = new TestActionKeyCounterTrigger(OsuAction.LeftButton)
                                {
                                    Depth = float.MinValue
                                },
                                triggerRight = new TestActionKeyCounterTrigger(OsuAction.RightButton)
                                {
                                    Depth = float.MinValue
                                }
                            },
                        },
                    },
                    new TouchVisualiser(),
                };

                mainContent.AddRange(new[]
                {
                    leftKeyCounter = new DefaultKeyCounter(triggerLeft)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreRight,
                        X = -100,
                    },
                    rightKeyCounter = new DefaultKeyCounter(triggerRight)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft,
                        X = 100,
                    },
                });
            });
        }

        [Test]
        public void TestStreamInputVisual()
        {
            addHitCircleAt(TouchSource.Touch1);
            addHitCircleAt(TouchSource.Touch2);

            beginTouch(TouchSource.Touch1);
            beginTouch(TouchSource.Touch2);

            endTouch(TouchSource.Touch1);

            int i = 0;

            AddRepeatStep("Alternate", () =>
            {
                TouchSource down = i % 2 == 0 ? TouchSource.Touch3 : TouchSource.Touch4;
                TouchSource up = i % 2 == 0 ? TouchSource.Touch4 : TouchSource.Touch3;

                // sometimes the user will end the previous touch before touching again, sometimes not.
                if (RNG.NextBool())
                {
                    InputManager.BeginTouch(new Touch(down, getSanePositionForSource(down)));
                    InputManager.EndTouch(new Touch(up, getSanePositionForSource(up)));
                }
                else
                {
                    InputManager.EndTouch(new Touch(up, getSanePositionForSource(up)));
                    InputManager.BeginTouch(new Touch(down, getSanePositionForSource(down)));
                }

                i++;
            }, 100);
        }

        [Test]
        public void TestSimpleInput()
        {
            beginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            beginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            // Subsequent touches should be ignored (except position).
            beginTouch(TouchSource.Touch3);
            checkPosition(TouchSource.Touch3);

            beginTouch(TouchSource.Touch4);
            checkPosition(TouchSource.Touch4);

            assertKeyCounter(1, 1);

            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            assertKeyCounter(1, 1);
        }

        [Test]
        public void TestPositionalTrackingAfterLongDistanceTravelled()
        {
            // When a single touch has already travelled enough distance on screen, it should remain as the positional
            // tracking touch until released (unless a direct touch occurs).

            beginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            // cover some distance
            beginTouch(TouchSource.Touch1, new Vector2(0));
            beginTouch(TouchSource.Touch1, new Vector2(9999));
            beginTouch(TouchSource.Touch1, new Vector2(0));
            beginTouch(TouchSource.Touch1, new Vector2(9999));
            beginTouch(TouchSource.Touch1);

            beginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkNotPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // in this case, touch 2 should not become the positional tracking touch.
            checkPosition(TouchSource.Touch1);

            // even if the second touch moves on the screen, the original tracking touch is retained.
            beginTouch(TouchSource.Touch2, new Vector2(0));
            beginTouch(TouchSource.Touch2, new Vector2(9999));
            beginTouch(TouchSource.Touch2, new Vector2(0));
            beginTouch(TouchSource.Touch2, new Vector2(9999));

            checkPosition(TouchSource.Touch1);
        }

        [Test]
        public void TestPositionalInputUpdatesOnlyFromMostRecentTouch()
        {
            beginTouch(TouchSource.Touch1);
            checkPosition(TouchSource.Touch1);

            beginTouch(TouchSource.Touch2);
            checkPosition(TouchSource.Touch2);

            beginTouch(TouchSource.Touch1, Vector2.One);
            checkPosition(TouchSource.Touch2);

            endTouch(TouchSource.Touch2);
            checkPosition(TouchSource.Touch2);

            // note that touch1 was never ended, but is no longer valid for touch input due to touch 2 occurring.
            beginTouch(TouchSource.Touch1);
            checkPosition(TouchSource.Touch2);
        }

        [Test]
        public void TestStreamInput()
        {
            // In this scenario, the user is tapping on the first object in a stream,
            // then using one or two fingers in empty space to continue the stream.

            addHitCircleAt(TouchSource.Touch1);
            beginTouch(TouchSource.Touch1);

            // The first touch is handled as normal.
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            // The second touch should release the first, and also act as a right button.
            beginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            // Importantly, this is different from the simple case because an object was interacted with in the first touch, but not the second touch.
            // left button is automatically released.
            checkNotPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Also importantly, the positional part of the second touch is ignored.
            checkPosition(TouchSource.Touch1);

            // In this scenario, a third touch should be allowed, and handled similarly to the second.
            beginTouch(TouchSource.Touch3);

            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPosition(TouchSource.Touch1);

            endTouch(TouchSource.Touch2);

            checkPressed(OsuAction.LeftButton);
            checkNotPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPosition(TouchSource.Touch1);

            // User continues streaming
            beginTouch(TouchSource.Touch2);

            assertKeyCounter(2, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPosition(TouchSource.Touch1);

            // In this mode a maximum of three touches should be supported.
            // A fourth touch should result in no changes anywhere.
            beginTouch(TouchSource.Touch4);
            assertKeyCounter(2, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch1);
            endTouch(TouchSource.Touch4);
        }

        [Test]
        public void TestStreamInputWithInitialTouchDownLeft()
        {
            // In this scenario, the user is wanting to use stream input but we start with one finger still on the screen.
            // That finger is mapped to a left action.

            addHitCircleAt(TouchSource.Touch2);

            beginTouch(TouchSource.Touch1);
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            // hits circle as right action
            beginTouch(TouchSource.Touch2);
            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            endTouch(TouchSource.Touch1);
            checkNotPressed(OsuAction.LeftButton);

            // stream using other two fingers while touch2 tracks
            beginTouch(TouchSource.Touch1);
            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            // right button is automatically released
            checkNotPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            beginTouch(TouchSource.Touch3);
            assertKeyCounter(2, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            endTouch(TouchSource.Touch1);
            checkNotPressed(OsuAction.LeftButton);

            beginTouch(TouchSource.Touch1);
            assertKeyCounter(3, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);
        }

        [Test]
        public void TestStreamInputWithInitialTouchDownRight()
        {
            // In this scenario, the user is wanting to use stream input but we start with one finger still on the screen.
            // That finger is mapped to a right action.

            beginTouch(TouchSource.Touch1);
            beginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            endTouch(TouchSource.Touch1);

            addHitCircleAt(TouchSource.Touch1);

            // hits circle as left action
            beginTouch(TouchSource.Touch1);
            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch1);

            endTouch(TouchSource.Touch2);

            // stream using other two fingers while touch1 tracks
            beginTouch(TouchSource.Touch2);
            assertKeyCounter(2, 2);
            checkPressed(OsuAction.RightButton);
            // left button is automatically released
            checkNotPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            beginTouch(TouchSource.Touch3);
            assertKeyCounter(3, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch1);

            endTouch(TouchSource.Touch2);
            checkNotPressed(OsuAction.RightButton);

            beginTouch(TouchSource.Touch2);
            assertKeyCounter(3, 3);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch1);
        }

        [Test]
        public void TestNonStreamOverlappingDirectTouchesWithRelease()
        {
            // In this scenario, the user is tapping on three circles directly while correctly releasing the first touch.
            // All three should be recognised.

            addHitCircleAt(TouchSource.Touch1);
            addHitCircleAt(TouchSource.Touch2);
            addHitCircleAt(TouchSource.Touch3);

            beginTouch(TouchSource.Touch1);
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            beginTouch(TouchSource.Touch2);
            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            endTouch(TouchSource.Touch1);

            beginTouch(TouchSource.Touch3);
            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch3);
        }

        [Test]
        public void TestNonStreamOverlappingDirectTouchesWithoutRelease()
        {
            // In this scenario, the user is tapping on three circles directly without releasing any touches.
            // The first two should be recognised, but a third should not (as the user already has two fingers down).

            addHitCircleAt(TouchSource.Touch1);
            addHitCircleAt(TouchSource.Touch2);
            addHitCircleAt(TouchSource.Touch3);

            beginTouch(TouchSource.Touch1);
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            beginTouch(TouchSource.Touch2);
            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);

            beginTouch(TouchSource.Touch3);
            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch3);
        }

        [Test]
        public void TestMovementWhileDisallowed()
        {
            // aka "autopilot" mod

            AddStep("Disallow gameplay cursor movement", () => osuInputManager.AllowUserCursorMovement = false);

            Vector2? positionBefore = null;

            AddStep("Store cursor position", () => positionBefore = osuInputManager.CurrentState.Mouse.Position);
            beginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            AddAssert("Cursor position unchanged", () => osuInputManager.CurrentState.Mouse.Position, () => Is.EqualTo(positionBefore));
        }

        [Test]
        public void TestActionWhileDisallowed()
        {
            // aka "relax" mod

            AddStep("Disallow gameplay actions", () => osuInputManager.AllowGameplayInputs = false);

            beginTouch(TouchSource.Touch1);

            assertKeyCounter(0, 0);
            checkNotPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);
        }

        [Test]
        public void TestInputWhileMouseButtonsDisabled()
        {
            AddStep("Disable mouse buttons", () => config.SetValue(OsuSetting.MouseDisableButtons, true));

            beginTouch(TouchSource.Touch1);

            assertKeyCounter(0, 0);
            checkNotPressed(OsuAction.LeftButton);
            checkPosition(TouchSource.Touch1);

            beginTouch(TouchSource.Touch2);

            assertKeyCounter(0, 0);
            checkNotPressed(OsuAction.LeftButton);
            checkNotPressed(OsuAction.RightButton);
            checkPosition(TouchSource.Touch2);
        }

        [Test]
        public void TestAlternatingInput()
        {
            beginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);

            beginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            for (int i = 0; i < 2; i++)
            {
                endTouch(TouchSource.Touch1);

                checkPressed(OsuAction.RightButton);
                checkNotPressed(OsuAction.LeftButton);

                beginTouch(TouchSource.Touch1);

                checkPressed(OsuAction.LeftButton);
                checkPressed(OsuAction.RightButton);

                endTouch(TouchSource.Touch2);

                checkPressed(OsuAction.LeftButton);
                checkNotPressed(OsuAction.RightButton);

                beginTouch(TouchSource.Touch2);

                checkPressed(OsuAction.LeftButton);
                checkPressed(OsuAction.RightButton);
            }
        }

        [Test]
        public void TestPressReleaseOrder()
        {
            beginTouch(TouchSource.Touch1);
            beginTouch(TouchSource.Touch2);
            beginTouch(TouchSource.Touch3);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            // Touch 3 was ignored, but let's ensure that if 1 or 2 are released, 3 will be handled a second attempt.
            endTouch(TouchSource.Touch1);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.RightButton);

            endTouch(TouchSource.Touch3);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.RightButton);

            beginTouch(TouchSource.Touch3);

            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
        }

        [Test]
        public void TestWithDisallowedUserCursor()
        {
            beginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);

            beginTouch(TouchSource.Touch2);

            assertKeyCounter(1, 1);
            checkPressed(OsuAction.RightButton);

            // Subsequent touches should be ignored.
            beginTouch(TouchSource.Touch3);
            beginTouch(TouchSource.Touch4);

            assertKeyCounter(1, 1);

            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);

            assertKeyCounter(1, 1);
        }

        private void addHitCircleAt(TouchSource source)
        {
            AddStep($"Add circle at {source}", () =>
            {
                var hitCircle = new HitCircle();

                hitCircle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                mainContent.Add(new DrawableHitCircle(hitCircle)
                {
                    Clock = new FramedClock(new ManualClock()),
                    Position = mainContent.ToLocalSpace(getSanePositionForSource(source)),
                });
            });
        }

        private void beginTouch(TouchSource source, Vector2? screenSpacePosition = null) =>
            AddStep($"Begin touch for {source}", () => InputManager.BeginTouch(new Touch(source, screenSpacePosition ??= getSanePositionForSource(source))));

        private void endTouch(TouchSource source, Vector2? screenSpacePosition = null) =>
            AddStep($"Release touch for {source}", () => InputManager.EndTouch(new Touch(source, screenSpacePosition ??= getSanePositionForSource(source))));

        private Vector2 getSanePositionForSource(TouchSource source)
        {
            return new Vector2(
                osuInputManager.ScreenSpaceDrawQuad.Centre.X + osuInputManager.ScreenSpaceDrawQuad.Width * (-1 + (int)source) / 8,
                osuInputManager.ScreenSpaceDrawQuad.Centre.Y - 100
            );
        }

        private void checkPosition(TouchSource touchSource) =>
            AddAssert("Cursor position is correct", () => osuInputManager.CurrentState.Mouse.Position, () => Is.EqualTo(getSanePositionForSource(touchSource)));

        private void assertKeyCounter(int left, int right)
        {
            AddAssert($"The left key was pressed {left} times", () => leftKeyCounter.CountPresses.Value, () => Is.EqualTo(left));
            AddAssert($"The right key was pressed {right} times", () => rightKeyCounter.CountPresses.Value, () => Is.EqualTo(right));
        }

        private void releaseAllTouches()
        {
            AddStep("Release all touches", () =>
            {
                config.SetValue(OsuSetting.MouseDisableButtons, false);
                foreach (TouchSource source in InputManager.CurrentState.Touch.ActiveSources)
                    InputManager.EndTouch(new Touch(source, osuInputManager.ScreenSpaceDrawQuad.Centre));
            });
        }

        private void checkNotPressed(OsuAction action) => AddAssert($"Not pressing {action}", () => !osuInputManager.PressedActions.Contains(action));
        private void checkPressed(OsuAction action) => AddAssert($"Is pressing {action}", () => osuInputManager.PressedActions.Contains(action));

        public partial class TestActionKeyCounterTrigger : InputTrigger, IKeyBindingHandler<OsuAction>
        {
            public OsuAction Action { get; }

            public TestActionKeyCounterTrigger(OsuAction action)
                : base(action.ToString())
            {
                Action = action;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
            {
                if (e.Action == Action)
                {
                    Activate();
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
                if (e.Action == Action)
                    Deactivate();
            }
        }

        public partial class TouchVisualiser : CompositeDrawable
        {
            private readonly Drawable?[] drawableTouches = new Drawable?[TouchState.MAX_TOUCH_COUNT];

            public TouchVisualiser()
            {
                RelativeSizeAxes = Axes.Both;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            protected override bool OnTouchDown(TouchDownEvent e)
            {
                if (IsDisposed)
                    return false;

                var circle = new Circle
                {
                    Alpha = 0.5f,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20),
                    Position = e.Touch.Position,
                    Colour = colourFor(e.Touch.Source),
                };

                AddInternal(circle);
                drawableTouches[(int)e.Touch.Source] = circle;
                return false;
            }

            protected override void OnTouchMove(TouchMoveEvent e)
            {
                if (IsDisposed)
                    return;

                var circle = drawableTouches[(int)e.Touch.Source];

                Debug.Assert(circle != null);

                AddInternal(new FadingCircle(circle));
                circle.Position = e.Touch.Position;
            }

            protected override void OnTouchUp(TouchUpEvent e)
            {
                var circle = drawableTouches[(int)e.Touch.Source];

                Debug.Assert(circle != null);

                circle.FadeOut(200, Easing.OutQuint).Expire();
                drawableTouches[(int)e.Touch.Source] = null;
            }

            private Color4 colourFor(TouchSource source)
            {
                return Color4.FromHsv(new Vector4((float)source / TouchState.MAX_TOUCH_COUNT, 1f, 1f, 1f));
            }

            private partial class FadingCircle : Circle
            {
                public FadingCircle(Drawable source)
                {
                    Origin = Anchor.Centre;
                    Size = source.Size;
                    Position = source.Position;
                    Colour = source.Colour;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    this.FadeOut(200).Expire();
                }
            }
        }
    }
}
