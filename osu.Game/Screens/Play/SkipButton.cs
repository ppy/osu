﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Ranking;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Audio.Sample;

namespace osu.Game.Screens.Play
{
    public class SkipButton : Container
    {
        private readonly double startTime;
        public IAdjustableClock AudioClock;

        private Button button;
        private Box remainingTimeBox;

        private FadeContainer fadeContainer;
        private double displayTime;

        public SkipButton(double startTime)
        {
            AlwaysReceiveInput = true;

            this.startTime = startTime;

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;

            Position = new Vector2(0.5f, 0.7f);
            Size = new Vector2(1, 0.14f);

            Origin = Anchor.Centre;
        }

        protected override bool OnMouseMove(InputState state)
        {
            fadeContainer.State = Visibility.Visible;
            return base.OnMouseMove(state);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            var baseClock = Clock;

            if (AudioClock != null)
                Clock = new FramedClock(AudioClock) { ProcessSourceClockFrames = false };

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

            FadeInFromZero(fade_time);
            using (BeginAbsoluteSequence(beginFadeTime))
                FadeOut(fade_time);

            button.Action = () => AudioClock?.Seek(startTime - skip_required_cutoff - fade_time);

            displayTime = Time.Current;

            Expire();
        }

        protected override void Update()
        {
            base.Update();
            remainingTimeBox.ResizeWidthTo((float)Math.Max(0, 1 - (Time.Current - displayTime) / (beginFadeTime - displayTime)), 120, EasingTypes.OutQuint);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.Space:
                    button.TriggerOnClick();
                    return true;
            }

            return base.OnKeyDown(state, args);
        }

        private class FadeContainer : Container, IStateful<Visibility>
        {
            private Visibility state;
            private ScheduledDelegate scheduledHide;

            public Visibility State
            {
                get
                {
                    return state;
                }
                set
                {
                    var lastState = state;

                    state = value;

                    scheduledHide?.Cancel();

                    switch (state)
                    {
                        case Visibility.Visible:
                            if (lastState == Visibility.Hidden)
                                FadeIn(500, EasingTypes.OutExpo);

                            if (!Hovering)
                                using (BeginDelayedSequence(1000))
                                    scheduledHide = Schedule(() => State = Visibility.Hidden);
                            break;
                        case Visibility.Hidden:
                            FadeOut(1000, EasingTypes.OutExpo);
                            break;
                    }
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                State = Visibility.Visible;
            }
        }

        private class Button : Container
        {
            public Action Action;
            private Color4 colourNormal;
            private Color4 colourHover;
            private Box box;
            private FillFlowContainer flow;
            private Box background;
            private AspectContainer aspect;
            private SampleChannel activationSound;

            public Button()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, AudioManager audio)
            {
                activationSound = audio.Sample.Get(@"Menu/menuhit");

                colourNormal = colours.Yellow;
                colourHover = colours.YellowDark;

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
                                    new TextAwesome { Icon = FontAwesome.fa_chevron_right },
                                    new TextAwesome { Icon = FontAwesome.fa_chevron_right },
                                    new TextAwesome { Icon = FontAwesome.fa_chevron_right },
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
                flow.TransformSpacingTo(new Vector2(5), 500, EasingTypes.OutQuint);
                box.FadeColour(colourHover, 500, EasingTypes.OutQuint);
                background.FadeTo(0.4f, 500, EasingTypes.OutQuint);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                flow.TransformSpacingTo(new Vector2(0), 500, EasingTypes.OutQuint);
                box.FadeColour(colourNormal, 500, EasingTypes.OutQuint);
                background.FadeTo(0.2f, 500, EasingTypes.OutQuint);
                base.OnHoverLost(state);
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                aspect.ScaleTo(0.75f, 2000, EasingTypes.OutQuint);
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                aspect.ScaleTo(1, 1000, EasingTypes.OutElastic);
                return base.OnMouseUp(state, args);
            }

            protected override bool OnClick(InputState state)
            {
                Action?.Invoke();

                activationSound.Play();

                box.FlashColour(Color4.White, 500, EasingTypes.OutQuint);
                aspect.ScaleTo(1.2f, 2000, EasingTypes.OutQuint);
                return true;
            }
        }
    }
}
