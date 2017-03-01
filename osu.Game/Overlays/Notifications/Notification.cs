﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public abstract class Notification : Container
    {
        /// <summary>
        /// Use requested close.
        /// </summary>
        public Action Closed;

        /// <summary>
        /// Run on user activating the notification. Return true to close.
        /// </summary>
        public Func<bool> Activated;

        /// <summary>
        /// Should we show at the top of our section on display?
        /// </summary>
        public virtual bool DisplayOnTop => true;

        protected NotificationLight Light;
        private CloseButton closeButton;
        protected Container IconContent;
        private Container content;

        protected override Container<Drawable> Content => content;

        protected Container NotificationContent;

        private bool read;

        public virtual bool Read
        {
            get { return read; }
            set
            {
                read = value;
            }
        }

        public Notification()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(new Drawable[]
            {
                Light = new NotificationLight
                {
                    Margin = new MarginPadding { Right = 5 },
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                },
                NotificationContent = new Container
                {
                    CornerRadius = 8,
                    Masking = true,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Padding = new MarginPadding(5),
                            Height = 60,
                            Children = new Drawable[]
                            {
                                IconContent = new Container
                                {
                                    Size = new Vector2(40),
                                    Masking = true,
                                    CornerRadius = 5,
                                },
                                content = new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding
                                    {
                                        Top = 5,
                                        Left = 45,
                                        Right = 30
                                    },
                                }
                            }
                        },
                        closeButton = new CloseButton
                        {
                            Alpha = 0,
                            Action = Close,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Margin = new MarginPadding
                            {
                                Right = 5
                            },
                        }
                    }
                }
            });
        }

        protected override bool OnHover(InputState state)
        {
            closeButton.FadeIn(75);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            closeButton.FadeOut(75);
            base.OnHoverLost(state);
        }

        protected override bool OnClick(InputState state)
        {
            if (Activated?.Invoke() ?? true)
                Close();

            return true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            FadeInFromZero(200);
            NotificationContent.MoveToX(DrawSize.X);
            NotificationContent.MoveToX(0, 500, EasingTypes.OutQuint);
        }

        private bool wasClosed;

        public virtual void Close()
        {
            if (wasClosed) return;
            wasClosed = true;

            Closed?.Invoke();
            FadeOut(100);
            Expire();
        }

        class CloseButton : ClickableContainer
        {
            private Color4 hoverColour;

            public CloseButton()
            {
                Colour = OsuColour.Gray(0.2f);
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    new TextAwesome
                    {
                        Anchor = Anchor.Centre,
                        Icon = FontAwesome.fa_times_circle,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoverColour = colours.Yellow;
            }

            protected override bool OnHover(InputState state)
            {
                FadeColour(hoverColour, 200);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                FadeColour(OsuColour.Gray(0.2f), 200);
                base.OnHoverLost(state);
            }
        }

        public class NotificationLight : Container
        {
            private bool pulsate;
            private Container pulsateLayer;

            public bool Pulsate
            {
                get { return pulsate; }
                set
                {
                    pulsate = value;

                    pulsateLayer.ClearTransforms();
                    pulsateLayer.Alpha = 1;

                    if (pulsate)
                    {
                        const float length = 1000;
                        pulsateLayer.Transforms.Add(new TransformAlpha
                        {
                            StartTime = Time.Current,
                            EndTime = Time.Current + length,
                            StartValue = 1,
                            EndValue = 0.4f,
                            Easing = EasingTypes.In
                        });
                        pulsateLayer.Transforms.Add(new TransformAlpha
                        {
                            StartTime = Time.Current + length,
                            EndTime = Time.Current + length * 2,
                            StartValue = 0.4f,
                            EndValue = 1,
                            Easing = EasingTypes.Out
                        });

                        //todo: figure why we can't add arbitrary delays at the end of loop.
                        pulsateLayer.Loop(length * 2);
                    }
                }
            }

            public new SRGBColour Colour
            {
                set
                {
                    base.Colour = value;
                    pulsateLayer.EdgeEffect = new EdgeEffect
                    {
                        Colour = ((Color4)value).Opacity(0.5f), //todo: avoid cast
                        Type = EdgeEffectType.Glow,
                        Radius = 12,
                        Roundness = 12,
                    };
                }
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(6, 15);

                Children = new[]
                {
                    pulsateLayer = new CircularContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    }
                };
            }
        }
    }
}