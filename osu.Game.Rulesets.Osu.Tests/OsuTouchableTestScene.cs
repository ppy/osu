// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
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
    public abstract partial class OsuTouchableTestScene : OsuManualInputManagerTestScene
    {
        [Resolved]
        protected OsuConfigManager Config { get; set; } = null!;

        protected OsuInputManager OsuInputManager = null!;

        private Container mainContent = null!;

        protected DefaultKeyCounter LeftKeyCounter = null!;

        protected DefaultKeyCounter RightKeyCounter = null!;

        [SetUpSteps]
        public virtual void SetUpSteps()
        {
            releaseAllTouches();

            AddStep("Create tests", () =>
            {
                Children = new Drawable[]
                {
                    OsuInputManager = new OsuInputManager(new OsuRuleset().RulesetInfo)
                    {
                        Child = mainContent = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                LeftKeyCounter = new DefaultKeyCounter(new TestActionKeyCounterTrigger(OsuAction.LeftButton))
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreRight,
                                    Depth = float.MinValue,
                                    X = -100
                                },
                                RightKeyCounter = new DefaultKeyCounter(new TestActionKeyCounterTrigger(OsuAction.RightButton))
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.CentreLeft,
                                    Depth = float.MinValue,
                                    X = 100
                                },
                                new OsuCursorContainer
                                {
                                    Depth = float.MinValue,
                                }
                            },
                        }
                    },
                    new TouchVisualiser(),
                };
            });
        }

        private void releaseAllTouches()
        {
            AddStep("Release all touches", () =>
            {
                Config.SetValue(OsuSetting.MouseDisableButtons, false);
                foreach (TouchSource source in InputManager.CurrentState.Touch.ActiveSources)
                    InputManager.EndTouch(new Touch(source, OsuInputManager.ScreenSpaceDrawQuad.Centre));
            });
        }

        protected void BeginTouch(TouchSource source, Vector2? screenSpacePosition = null) =>
            AddStep($"Begin touch for {source}", () => InputManager.BeginTouch(new Touch(source, screenSpacePosition ??= GetSanePositionForSource(source))));

        protected void EndTouch(TouchSource source, Vector2? screenSpacePosition = null) =>
            AddStep($"Release touch for {source}", () => InputManager.EndTouch(new Touch(source, screenSpacePosition ??= GetSanePositionForSource(source))));

        protected Vector2 GetSanePositionForSource(TouchSource source)
        {
            return new Vector2(
                OsuInputManager.ScreenSpaceDrawQuad.Centre.X + OsuInputManager.ScreenSpaceDrawQuad.Width * (-1 + (int)source) / 8,
                OsuInputManager.ScreenSpaceDrawQuad.Centre.Y - 100
            );
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

        protected void AddHitCircleAt(TouchSource source)
        {
            var hitCircle = new HitCircle();

            hitCircle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            mainContent.Add(new DrawableHitCircle(hitCircle)
            {
                Clock = new FramedClock(new ManualClock()),
                Position = mainContent.ToLocalSpace(GetSanePositionForSource(source)),
            });
        }

        protected void AddHitCircleAtStep(TouchSource source)
        {
            AddStep($"Add circle at {source}", () => AddHitCircleAt(source));
        }

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
    }
}
