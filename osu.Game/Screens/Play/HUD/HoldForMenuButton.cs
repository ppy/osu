// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class HoldForMenuButton : FillFlowContainer
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public readonly Bindable<bool> IsPaused = new Bindable<bool>();

        public readonly Bindable<bool> ReplayLoaded = new Bindable<bool>();

        private HoldButton button;

        public Action Action { get; set; }

        private OsuSpriteText text;

        public HoldForMenuButton()
        {
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(20, 0);
            Margin = new MarginPadding(10);

            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader(true)]
        private void load(Player player)
        {
            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                },
                button = new HoldButton(player?.Configuration.AllowRestart == false)
                {
                    HoverGained = () => text.FadeIn(500, Easing.OutQuint),
                    HoverLost = () => text.FadeOut(500, Easing.OutQuint),
                    IsPaused = { BindTarget = IsPaused },
                    ReplayLoaded = { BindTarget = ReplayLoaded },
                    Action = () => Action(),
                }
            };

            AutoSizeAxes = Axes.Both;
        }

        [Resolved]
        private SessionStatics sessionStatics { get; set; }

        private Bindable<bool> touchActive;

        protected override void LoadComplete()
        {
            button.HoldActivationDelay.BindValueChanged(v =>
            {
                text.Text = v.NewValue > 0
                    ? "hold for menu"
                    : "press for menu";
            }, true);

            touchActive = sessionStatics.GetBindable<bool>(Static.TouchInputActive);

            if (touchActive.Value)
            {
                Alpha = 1f;
                text.FadeInFromZero(500, Easing.OutQuint)
                    .Delay(1500)
                    .FadeOut(500, Easing.OutQuint);
            }
            else
            {
                Alpha = 0;
                text.Alpha = 0f;
            }

            base.LoadComplete();
        }

        private float positionalAdjust = 1; // Start at 1 to handle the case where a user never send positional input.

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            positionalAdjust = Vector2.Distance(e.MousePosition, button.ToSpaceOfOtherDrawable(button.DrawRectangle.Centre, Parent!)) / 100;
            return base.OnMouseMove(e);
        }

        protected override void Update()
        {
            base.Update();

            if (text.Alpha > 0 || button.Progress.Value > 0 || button.IsHovered)
                Alpha = 1;
            else
            {
                float minAlpha = touchActive.Value ? .08f : 0;

                Alpha = Interpolation.ValueAt(
                    Math.Clamp(Clock.ElapsedFrameTime, 0, 200),
                    Alpha, Math.Clamp(1 - positionalAdjust, minAlpha, 1), 0, 200, Easing.OutQuint);
            }
        }

        private partial class HoldButton : HoldToConfirmContainer, IKeyBindingHandler<GlobalAction>
        {
            private SpriteIcon icon;
            private CircularProgress circularProgress;
            private Circle overlayCircle;

            public readonly Bindable<bool> IsPaused = new Bindable<bool>();

            public readonly Bindable<bool> ReplayLoaded = new Bindable<bool>();

            protected override bool AllowMultipleFires => true;

            public Action HoverGained;
            public Action HoverLost;

            private const double shake_duration = 20;

            private bool pendingAnimation;
            private ScheduledDelegate shakeOperation;

            public HoldButton(bool isDangerousAction)
                : base(isDangerousAction)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
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
            }

            private void bind()
            {
                ((IBindable<double>)circularProgress.Current).BindTo(Progress);
                Progress.ValueChanged += progress =>
                {
                    icon.Scale = new Vector2(1 + (float)progress.NewValue * 0.2f);

                    if (IsDangerousAction)
                    {
                        Colour = Interpolation.ValueAt(progress.NewValue, Color4.White, Color4.Red, 0, 1, Easing.OutQuint);

                        if (progress.NewValue > 0 && progress.NewValue < 1)
                        {
                            shakeOperation ??= Scheduler.AddDelayed(shake, shake_duration, true);
                        }
                        else
                        {
                            Child.MoveTo(Vector2.Zero, shake_duration * 2, Easing.OutQuint);
                            shakeOperation?.Cancel();
                            shakeOperation = null;
                        }
                    }
                };
            }

            private void shake()
            {
                const float shake_magnitude = 8;

                Child.MoveTo(new Vector2(
                    RNG.NextSingle(-1, 1) * (float)Progress.Value * shake_magnitude,
                    RNG.NextSingle(-1, 1) * (float)Progress.Value * shake_magnitude
                ), shake_duration);
            }

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
                             .OnComplete(_ =>
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

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (e.Repeat)
                    return false;

                switch (e.Action)
                {
                    case GlobalAction.Back:
                        if (!pendingAnimation)
                            BeginConfirm();
                        return true;

                    case GlobalAction.PauseGameplay:
                        // handled by replay player
                        if (ReplayLoaded.Value) return false;

                        if (!pendingAnimation)
                            BeginConfirm();
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
                switch (e.Action)
                {
                    case GlobalAction.Back:
                        AbortConfirm();
                        break;

                    case GlobalAction.PauseGameplay:
                        if (ReplayLoaded.Value) return;

                        AbortConfirm();
                        break;
                }
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (!pendingAnimation && e.CurrentState.Mouse.Buttons.Count() == 1)
                    BeginConfirm();
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (!e.HasAnyButtonPressed)
                    AbortConfirm();
            }
        }
    }
}
