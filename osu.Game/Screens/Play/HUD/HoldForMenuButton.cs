// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.MathUtils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class HoldForMenuButton : FillFlowContainer
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public readonly Bindable<bool> IsPaused = new Bindable<bool>();

        private readonly Button button;

        public Action Action
        {
            set => button.Action = value;
        }

        private readonly OsuSpriteText text;

        public HoldForMenuButton()
        {
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(20, 0);
            Margin = new MarginPadding(10);
            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                },
                button = new Button
                {
                    HoverGained = () => text.FadeIn(500, Easing.OutQuint),
                    HoverLost = () => text.FadeOut(500, Easing.OutQuint),
                    IsPaused = { BindTarget = IsPaused }
                }
            };
            AutoSizeAxes = Axes.Both;
        }

        [Resolved]
        private OsuConfigManager config { get; set; }

        private Bindable<float> activationDelay;

        protected override void LoadComplete()
        {
            activationDelay = config.GetBindable<float>(OsuSetting.UIHoldActivationDelay);
            activationDelay.BindValueChanged(v =>
            {
                text.Text = v.NewValue > 0
                    ? "hold for menu"
                    : "press for menu";
            }, true);

            text.FadeInFromZero(500, Easing.OutQuint).Delay(1500).FadeOut(500, Easing.OutQuint);

            base.LoadComplete();
        }

        private float positionalAdjust;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            positionalAdjust = Vector2.Distance(e.ScreenSpaceMousePosition, button.ScreenSpaceDrawQuad.Centre) / 200;
            return base.OnMouseMove(e);
        }

        public bool PauseOnFocusLost
        {
            set => button.PauseOnFocusLost = value;
        }

        protected override void Update()
        {
            base.Update();

            if (text.Alpha > 0 || button.Progress.Value > 0 || button.IsHovered)
                Alpha = 1;
            else
                Alpha = Interpolation.ValueAt(
                    MathHelper.Clamp(Clock.ElapsedFrameTime, 0, 200),
                    Alpha, MathHelper.Clamp(1 - positionalAdjust, 0.04f, 1), 0, 200, Easing.OutQuint);
        }

        private class Button : HoldToConfirmContainer, IKeyBindingHandler<GlobalAction>
        {
            private SpriteIcon icon;
            private CircularProgress circularProgress;
            private Circle overlayCircle;

            public readonly Bindable<bool> IsPaused = new Bindable<bool>();

            protected override bool AllowMultipleFires => true;

            public Action HoverGained;
            public Action HoverLost;

            private readonly IBindable<bool> gameActive = new Bindable<bool>(true);

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, Framework.Game game)
            {
                Size = new Vector2(60);

                Child = new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray1,
                            Alpha = 0.5f,
                        },
                        circularProgress = new CircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            InnerRadius = 1
                        },
                        overlayCircle = new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray1,
                            Size = new Vector2(0.9f),
                        },
                        icon = new SpriteIcon
                        {
                            Shadow = false,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(15),
                            Icon = FontAwesome.Solid.Times
                        },
                    }
                };

                bind();

                gameActive.BindTo(game.IsActive);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                gameActive.BindValueChanged(_ => updateActive(), true);
            }

            private void bind()
            {
                circularProgress.Current.BindTo(Progress);
                Progress.ValueChanged += progress => icon.Scale = new Vector2(1 + (float)progress.NewValue * 0.2f);
            }

            private bool pendingAnimation;

            protected override void Confirm()
            {
                base.Confirm();

                // temporarily unbind as to not look weird if releasing during confirm animation (can see the unwind of progress).
                Progress.UnbindAll();

                // avoid starting a new confirm call until we finish animating.
                pendingAnimation = true;

                AbortConfirm();

                overlayCircle.ScaleTo(0, 100)
                             .Then().FadeOut().ScaleTo(1).FadeIn(500)
                             .OnComplete(a =>
                             {
                                 icon.ScaleTo(1, 100);
                                 circularProgress.FadeOut(100).OnComplete(_ =>
                                 {
                                     bind();

                                     circularProgress.FadeIn();
                                     pendingAnimation = false;
                                 });
                             });
            }

            protected override bool OnHover(HoverEvent e)
            {
                HoverGained?.Invoke();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                HoverLost?.Invoke();
                base.OnHoverLost(e);
            }

            private bool pauseOnFocusLost = true;

            public bool PauseOnFocusLost
            {
                set
                {
                    if (pauseOnFocusLost == value)
                        return;

                    pauseOnFocusLost = value;
                    if (IsLoaded)
                        updateActive();
                }
            }

            private void updateActive()
            {
                if (!pauseOnFocusLost || IsPaused.Value) return;

                if (gameActive.Value)
                    AbortConfirm();
                else
                    BeginConfirm();
            }

            public bool OnPressed(GlobalAction action)
            {
                switch (action)
                {
                    case GlobalAction.Back:
                        if (!pendingAnimation)
                            BeginConfirm();
                        return true;
                }

                return false;
            }

            public bool OnReleased(GlobalAction action)
            {
                switch (action)
                {
                    case GlobalAction.Back:
                        AbortConfirm();
                        return true;
                }

                return false;
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (!pendingAnimation && e.CurrentState.Mouse.Buttons.Count() == 1)
                    BeginConfirm();
                return true;
            }

            protected override bool OnMouseUp(MouseUpEvent e)
            {
                if (!e.HasAnyButtonPressed)
                    AbortConfirm();
                return true;
            }
        }
    }
}
