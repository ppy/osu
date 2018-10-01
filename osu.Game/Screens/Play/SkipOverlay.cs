// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Ranking;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Play
{
    public class SkipOverlay : OverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        private readonly double startTime;

        public IAdjustableClock AdjustableClock;
        public IFrameBasedClock FramedClock;

        private Button button;
        private Box remainingTimeBox;

        private FadeContainer fadeContainer;
        private double displayTime;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
        protected override bool BlockPositionalInput => false;

        public SkipOverlay(double startTime)
        {
            this.startTime = startTime;

            State = Visibility.Visible;

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;

            Position = new Vector2(0.5f, 0.7f);
            Size = new Vector2(1, 0.14f);

            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            var baseClock = Clock;

            if (FramedClock != null)
            {
                Clock = FramedClock;
                ProcessCustomClock = false;
            }

            Children = new Drawable[]
            {
                fadeContainer = new FadeContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        button = new Button
                        {
                            Clock = baseClock,
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

        private const double skip_required_cutoff = 3000;
        private const double fade_time = 300;

        private double beginFadeTime => startTime - skip_required_cutoff - fade_time;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (startTime < skip_required_cutoff)
            {
                Alpha = 0;
                Expire();
                return;
            }

            this.FadeInFromZero(fade_time);
            using (BeginAbsoluteSequence(beginFadeTime))
                this.FadeOut(fade_time);

            button.Action = () => AdjustableClock?.Seek(startTime - skip_required_cutoff - fade_time);

            displayTime = Time.Current;

            Expire();
        }

        protected override void PopIn() => this.FadeIn();

        protected override void PopOut() => this.FadeOut();

        protected override void Update()
        {
            base.Update();
            remainingTimeBox.ResizeWidthTo((float)Math.Max(0, 1 - (Time.Current - displayTime) / (beginFadeTime - displayTime)), 120, Easing.OutQuint);
        }

        protected override bool OnMouseMove(InputState state)
        {
            if (!state.Mouse.HasAnyButtonPressed)
                fadeContainer.State = Visibility.Visible;
            return base.OnMouseMove(state);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.SkipCutscene:
                    button.Click();
                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;

        private class FadeContainer : Container, IStateful<Visibility>
        {
            public event Action<Visibility> StateChanged;

            private Visibility state;
            private ScheduledDelegate scheduledHide;

            public Visibility State
            {
                get { return state; }
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
                                using (BeginDelayedSequence(1000))
                                    scheduledHide = Schedule(() => State = Visibility.Hidden);
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
                State = Visibility.Visible;
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                scheduledHide?.Cancel();
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                State = Visibility.Visible;
                return base.OnMouseUp(state, args);
            }
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

                sampleConfirm = audio.Sample.Get(@"SongSelect/confirm-selection");

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
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.fa_chevron_right },
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.fa_chevron_right },
                                    new SpriteIcon { Size = new Vector2(15), Shadow = true, Icon = FontAwesome.fa_chevron_right },
                                }
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                RelativePositionAxes = Axes.Y,
                                Y = 0.7f,
                                TextSize = 12,
                                Font = @"Exo2.0-Bold",
                                Origin = Anchor.Centre,
                                Text = @"SKIP",
                            },
                        }
                    }
                };
            }

            protected override bool OnHover(InputState state)
            {
                flow.TransformSpacingTo(new Vector2(5), 500, Easing.OutQuint);
                box.FadeColour(colourHover, 500, Easing.OutQuint);
                background.FadeTo(0.4f, 500, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                flow.TransformSpacingTo(new Vector2(0), 500, Easing.OutQuint);
                box.FadeColour(colourNormal, 500, Easing.OutQuint);
                background.FadeTo(0.2f, 500, Easing.OutQuint);
                base.OnHoverLost(state);
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                aspect.ScaleTo(0.75f, 2000, Easing.OutQuint);
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                aspect.ScaleTo(1, 1000, Easing.OutElastic);
                return base.OnMouseUp(state, args);
            }

            protected override bool OnClick(InputState state)
            {
                if (!Enabled)
                    return false;

                sampleConfirm.Play();

                box.FlashColour(Color4.White, 500, Easing.OutQuint);
                aspect.ScaleTo(1.2f, 2000, Easing.OutQuint);

                bool result = base.OnClick(state);

                // for now, let's disable the skip button after the first press.
                // this will likely need to be contextual in the future (bound from external components).
                Enabled.Value = false;

                return result;
            }
        }
    }
}
