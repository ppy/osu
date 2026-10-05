// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Ranking;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public partial class SkipOverlay : Container, IKeyBindingHandler<GlobalAction>
    {
        /// <summary>
        /// The total number of successful skips performed by this overlay.
        /// </summary>
        public int SkipCount { get; private set; }

        private readonly double startTime;

        public Action RequestSkip;

        protected FadeContainer FadingContent { get; private set; }

        private OsuClickableContainer button;

        private ButtonContainer buttonContainer;
        protected Circle RemainingTimeBox { get; private set; }

        private double displayTime;

        /// <summary>
        /// Whether the gameplay clock is currently at the skippable period.
        /// </summary>
        private readonly BindableBool inSkipPeriod = new BindableBool();

        private bool skipQueued;

        [Resolved]
        private IGameplayClock gameplayClock { get; set; }

        internal bool IsButtonVisible => FadingContent.State == Visibility.Visible && buttonContainer.State.Value == Visibility.Visible;
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        /// <summary>
        /// Displays a skip overlay, giving the user the ability to skip forward.
        /// </summary>
        /// <param name="startTime">The time at which gameplay begins to appear.</param>
        public SkipOverlay(double startTime)
        {
            this.startTime = startTime;

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.X;

            Position = new Vector2(0.5f, 0.7f);
            Size = new Vector2(1, 100);

            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours)
        {
            InternalChild = buttonContainer = new ButtonContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = FadingContent = new FadeContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        button = CreateButton(inSkipPeriod),
                        RemainingTimeBox = new Circle
                        {
                            Height = 5,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Colour = colours.Orange3,
                            RelativeSizeAxes = Axes.X
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Creates a skip button.
        /// </summary>
        /// <param name="inSkipPeriod">Whether the gameplay clock is currently at the skippable period.</param>
        protected virtual OsuClickableContainer CreateButton(IBindable<bool> inSkipPeriod) => new Button
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Enabled = { BindTarget = inSkipPeriod },
        };

        private const double fade_time = 300;

        private double fadeOutBeginTime => startTime - MasterGameplayClockContainer.MINIMUM_SKIP_TIME;

        public override void Hide()
        {
            base.Hide();
            FadingContent.Hide();
        }

        public override void Show()
        {
            base.Show();
            FadingContent.TriggerShow();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            displayTime = gameplayClock.CurrentTime;

            // skip is not required if there is no extra "empty" time to skip.
            // we may need to remove this if rewinding before the initial player load position becomes a thing.
            if (fadeOutBeginTime <= displayTime)
            {
                Expire();
                return;
            }

            button.Action = () =>
            {
                SkipCount++;
                RequestSkip?.Invoke();
            };

            FadingContent.TriggerShow();
        }

        /// <summary>
        /// Triggers an "automated" skip to happen as soon as available.
        /// </summary>
        public void SkipWhenReady()
        {
            if (skipQueued) return;

            skipQueued = true;
            attemptNextSkip();

            void attemptNextSkip() => Scheduler.AddDelayed(() =>
            {
                if (!button.Enabled.Value)
                {
                    skipQueued = false;
                    return;
                }

                button.TriggerClick();
                attemptNextSkip();
            }, 200);
        }

        protected override void Update()
        {
            base.Update();

            // This case causes an immediate expire in `LoadComplete`, but `Update` may run once after that.
            // Avoid div-by-zero below.
            if (fadeOutBeginTime <= displayTime)
                return;

            double progress = Math.Max(0, 1 - (gameplayClock.CurrentTime - displayTime) / (fadeOutBeginTime - displayTime));

            RemainingTimeBox.Width = (float)Interpolation.DampContinuously(RemainingTimeBox.Width, progress, 40, Math.Abs(Time.Elapsed));

            inSkipPeriod.Value = progress > 0;
            buttonContainer.State.Value = inSkipPeriod.Value ? Visibility.Visible : Visibility.Hidden;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (inSkipPeriod.Value && !e.HasAnyButtonPressed)
                FadingContent.TriggerShow();

            return base.OnMouseMove(e);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.SkipCutscene:
                    if (!button.Enabled.Value)
                        return false;

                    button.TriggerClick();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public partial class FadeContainer : Container, IStateful<Visibility>
        {
            [CanBeNull]
            public event Action<Visibility> StateChanged;

            private Visibility state;
            private double? nextHideTime;

            public override bool IsPresent => true;

            public void TriggerShow()
            {
                Show();

                if (!IsHovered && !IsDragged)
                    nextHideTime = Time.Current + 1000;
                else
                    nextHideTime = null;
            }

            protected override void Update()
            {
                base.Update();

                if (nextHideTime != null && nextHideTime <= Time.Current)
                {
                    Hide();
                    nextHideTime = null;
                }
            }

            public Visibility State
            {
                get => state;
                set
                {
                    if (value == state)
                        return;

                    state = value;

                    switch (state)
                    {
                        case Visibility.Visible:
                            this.FadeIn(500, Easing.OutExpo);
                            break;

                        case Visibility.Hidden:
                            this.FadeOut(1000, Easing.OutExpo);
                            break;
                    }

                    StateChanged?.Invoke(State);
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Show();
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                Show();
                nextHideTime = null;
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                Show();
            }

            public override void Hide() => State = Visibility.Hidden;

            public override void Show() => State = Visibility.Visible;
        }

        private partial class ButtonContainer : VisibilityContainer
        {
            protected override void PopIn() => this.FadeIn(fade_time);

            protected override void PopOut() => this.FadeOut(fade_time);
        }

        private partial class Button : OsuClickableContainer
        {
            private Color4 colourNormal;
            private Color4 colourHover;
            private Box box;
            private FillFlowContainer flow;
            private Box background;
            private AspectContainer aspect;

            private Sample sampleConfirm;

            public Button()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, AudioManager audio)
            {
                colourNormal = colours.Orange3;
                colourHover = colours.Orange3.Lighten(0.2f);

                sampleConfirm = audio.Samples.Get(@"UI/submit-select");

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        Alpha = 0.2f,
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                    aspect = new AspectContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Height = 0.6f,
                        Masking = true,
                        CornerRadius = 15,
                        Children = new Drawable[]
                        {
                            box = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourNormal,
                            },
                            new TrianglesV2
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientVertical(colourNormal.Lighten(0.2f), colourNormal)
                            },
                            flow = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                RelativePositionAxes = Axes.Y,
                                Y = 0.4f,
                                AutoSizeAxes = Axes.Both,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Horizontal,
                                Children = new[]
                                {
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.Solid.ChevronRight },
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.Solid.ChevronRight },
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.Solid.ChevronRight },
                                }
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                RelativePositionAxes = Axes.Y,
                                Y = 0.7f,
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 12),
                                Origin = Anchor.Centre,
                                Text = @"SKIP",
                            },
                        }
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                flow.TransformSpacingTo(new Vector2(5), 500, Easing.OutQuint);
                box.FadeColour(colourHover, 500, Easing.OutQuint);
                background.FadeTo(0.4f, 500, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                flow.TransformSpacingTo(new Vector2(0), 500, Easing.OutQuint);
                box.FadeColour(colourNormal, 500, Easing.OutQuint);
                background.FadeTo(0.2f, 500, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                aspect.ScaleTo(0.75f, 2000, Easing.OutQuint);
                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                aspect.ScaleTo(1, 1000, Easing.OutElastic);
                base.OnMouseUp(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled.Value)
                    return false;

                sampleConfirm.Play();

                box.FlashColour(Color4.White, 500, Easing.OutQuint);
                aspect.ScaleTo(1.2f, 2000, Easing.OutQuint);

                return base.OnClick(e);
            }
        }
    }
}
