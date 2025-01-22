// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.States;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneOsuTouchInputWithPen : OsuManualInputManagerTestScene
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
            releaseAllInput();

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
            addHitCircleAtPen();

            pressPen();

            int i = 0;

            AddRepeatStep("Alternate", () =>
            {
                TouchSource down = i % 2 == 0 ? TouchSource.Touch1 : TouchSource.Touch2;
                TouchSource up = i % 2 == 0 ? TouchSource.Touch2 : TouchSource.Touch1;

                // sometimes the user will end the previous touch before touching again, sometimes not.
                if (RNG.NextBool())
                {
                    InputManager.BeginTouch(new Touch(down, getSanePositionForTouch(down)));
                    InputManager.EndTouch(new Touch(up, getSanePositionForTouch(up)));
                }
                else
                {
                    InputManager.EndTouch(new Touch(up, getSanePositionForTouch(up)));
                    InputManager.BeginTouch(new Touch(down, getSanePositionForTouch(down)));
                }

                i++;
            }, 100);

            releasePen();
        }

        [Test]
        public void TestSimpleInputUsingPenOnly()
        {
            pressPen();

            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkNotPressed(OsuAction.RightButton);
            checkPositionIsAtPen();

            releasePen();

            assertKeyCounter(1, 0);
            checkNotPressed(OsuAction.LeftButton);
            checkNotPressed(OsuAction.RightButton);
            checkPositionIsAtPen();
        }

        [Test]
        public void TestStreamInputWithPenAsPointer()
        {
            addHitCircleAtPen();
            pressPen();

            // The pen press is handled as normal.
            assertKeyCounter(1, 0);
            checkPressed(OsuAction.LeftButton);
            checkPositionIsAtPen();

            // The first touch should release the action associated with the pen, and also act as a right button.
            beginTouch(TouchSource.Touch1);

            assertKeyCounter(1, 1);
            // Importantly, this is different from the simple case because an object was interacted with the pen, but not the first touch.
            // left button is automatically released.
            checkNotPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Also importantly, the positional part of the first touch is ignored.
            checkPositionIsAtPen();

            // In this scenario, a second touch should be allowed, and handled similarly to the second.
            beginTouch(TouchSource.Touch2);

            assertKeyCounter(2, 1);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPositionIsAtPen();

            endTouch(TouchSource.Touch1);

            checkPressed(OsuAction.LeftButton);
            checkNotPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPositionIsAtPen();

            // User continues streaming
            beginTouch(TouchSource.Touch1);

            assertKeyCounter(2, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            // Position is still ignored.
            checkPositionIsAtPen();

            // In this mode a maximum of two touches should be supported.
            // A third touch for tapping should result in no changes anywhere.
            beginTouch(TouchSource.Touch3);
            assertKeyCounter(2, 2);
            checkPressed(OsuAction.LeftButton);
            checkPressed(OsuAction.RightButton);
            checkPositionIsAtPen();
            endTouch(TouchSource.Touch3);

            releasePen();
        }

        private void addHitCircleAtPen()
        {
            AddStep("Add circle", () =>
            {
                var hitCircle = new HitCircle();

                hitCircle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                mainContent.Add(new DrawableHitCircle(hitCircle)
                {
                    Clock = new FramedClock(new ManualClock()),
                    Position = mainContent.ToLocalSpace(getSanePositionForPen()),
                    CheckHittable = (_, _, _) => ClickAction.Hit
                });
            });
        }

        private void pressPen()
        {
            AddStep("Press pen", () =>
            {
                InputManager.MovePenTo(getSanePositionForPen(), TabletPenDeviceType.Direct);
                InputManager.PressPen(TabletPenDeviceType.Direct);
            });
        }

        private void releasePen()
        {
            AddStep("Release pen", () => InputManager.ReleasePen(TabletPenDeviceType.Direct));
        }

        private void beginTouch(TouchSource source, Vector2? screenSpacePosition = null) =>
            AddStep($"Begin touch for {source}", () => InputManager.BeginTouch(new Touch(source, screenSpacePosition ??= getSanePositionForTouch(source))));

        private void endTouch(TouchSource source, Vector2? screenSpacePosition = null) =>
            AddStep($"Release touch for {source}", () => InputManager.EndTouch(new Touch(source, screenSpacePosition ??= getSanePositionForTouch(source))));

        private Vector2 getSanePositionForPen()
        {
            return new Vector2(
                osuInputManager.ScreenSpaceDrawQuad.Centre.X + osuInputManager.ScreenSpaceDrawQuad.Width * -1 / 8,
                osuInputManager.ScreenSpaceDrawQuad.Centre.Y - 100
            );
        }

        private Vector2 getSanePositionForTouch(TouchSource source)
            => getSanePositionForPen() + new Vector2(osuInputManager.ScreenSpaceDrawQuad.Width * (int)(1 + source) / 8, 0);

        private void checkPositionIsAtPen() =>
            AddAssert("Cursor position is correct", () => osuInputManager.CurrentState.Mouse.Position, () => Is.EqualTo(getSanePositionForPen()));

        private void assertKeyCounter(int left, int right)
        {
            AddAssert($"The left key was pressed {left} times", () => leftKeyCounter.CountPresses.Value, () => Is.EqualTo(left));
            AddAssert($"The right key was pressed {right} times", () => rightKeyCounter.CountPresses.Value, () => Is.EqualTo(right));
        }

        private void releaseAllInput()
        {
            AddStep("Release pen", () => InputManager.ReleasePen());
            AddStep("Release all touches", () =>
            {
                config.SetValue(OsuSetting.MouseDisableButtons, false);
                config.SetValue(OsuSetting.TouchDisableGameplayTaps, false);
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
