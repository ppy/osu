// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Ranking;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Play
{
    public class SkipOverlay : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        private readonly double startTime;

        public Action RequestSkip;

        private Button button;
        private ButtonContainer buttonContainer;
        private Box remainingTimeBox;

        private FadeContainer fadeContainer;
        private double displayTime;

        [Resolved]
        private GameplayClock gameplayClock { get; set; }

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
                Child = fadeContainer = new FadeContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        button = new Button
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        remainingTimeBox = new Box
                        {
                            Height = 5,
                            RelativeSizeAxes = Axes.X,
                            Colour = colours.Yellow,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                        }
                    }
                }
            };
        }

        private const double fade_time = 300;

        private double fadeOutBeginTime => startTime - GameplayClockContainer.MINIMUM_SKIP_TIME;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // skip is not required if there is no extra "empty" time to skip.
            // we may need to remove this if rewinding before the initial player load position becomes a thing.
            if (fadeOutBeginTime < gameplayClock.CurrentTime)
            {
                Expire();
                return;
            }

            button.Action = () => RequestSkip?.Invoke();
            displayTime = gameplayClock.CurrentTime;
        }

        protected override void Update()
        {
            base.Update();

            var progress = fadeOutBeginTime <= displayTime ? 1 : Math.Max(0, 1 - (gameplayClock.CurrentTime - displayTime) / (fadeOutBeginTime - displayTime));

            remainingTimeBox.Width = (float)Interpolation.Lerp(remainingTimeBox.Width, progress, Math.Clamp(Time.Elapsed / 40, 0, 1));

            button.Enabled.Value = progress > 0;
            buttonContainer.State.Value = progress > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!e.HasAnyButtonPressed)
                fadeContainer.Show();

            return base.OnMouseMove(e);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.SkipCutscene:
                    if (!button.Enabled.Value)
                        return false;

                    button.Click();
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

        private class FadeContainer : Container, IStateful<Visibility>
        {
            public event Action<Visibility> StateChanged;

            private Visibility state;
            private ScheduledDelegate scheduledHide;

            public override bool IsPresent => true;

            public Visibility State
            {
                get => state;
                set
                {
                    bool stateChanged = value != state;

                    state = value;

                    scheduledHide?.Cancel();

                    switch (state)
                    {
                        case Visibility.Visible:
                            // we may be triggered to become visible mnultiple times but we only want to transform once.
                            if (stateChanged)
                                this.FadeIn(500, Easing.OutExpo);

                            if (!IsHovered && !IsDragged)
                            {
                                using (BeginDelayedSequence(1000))
                                    scheduledHide = Schedule(Hide);
                            }

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
                scheduledHide?.Cancel();
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                Show();
            }

            public override void Hide() => State = Visibility.Hidden;

            public override void Show() => State = Visibility.Visible;
        }

        private class ButtonContainer : VisibilityContainer
        {
            protected override void PopIn() => this.FadeIn(fade_time);

            protected override void PopOut() => this.FadeOut(fade_time);
        }

        private class Button : OsuClickableContainer
        {
            private Color4 colourNormal;
            private Color4 colourHover;
            private Box box;
            private FillFlowContainer flow;
            private Box background;
            private AspectContainer aspect;

            private SampleChannel sampleConfirm;

            public Button()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, AudioManager audio)
            {
                colourNormal = colours.Yellow;
                colourHover = colours.YellowDark;

                sampleConfirm = audio.Samples.Get(@"SongSelect/confirm-selection");

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
