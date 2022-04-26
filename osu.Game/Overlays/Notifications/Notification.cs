// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Notifications
{
    public abstract class Notification : Container
    {
        /// <summary>
        /// User requested close.
        /// </summary>
        public event Action Closed;

        public abstract LocalisableString Text { get; set; }

        /// <summary>
        /// Whether this notification should forcefully display itself.
        /// </summary>
        public virtual bool IsImportant => true;

        /// <summary>
        /// Run on user activating the notification. Return true to close.
        /// </summary>
        public Func<bool> Activated;

        /// <summary>
        /// Should we show at the top of our section on display?
        /// </summary>
        public virtual bool DisplayOnTop => true;

        public virtual string PopInSampleName => "UI/notification-pop-in";

        protected NotificationLight Light;
        private readonly CloseButton closeButton;
        protected Container IconContent;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        protected Container NotificationContent;

        public virtual bool Read { get; set; }

        protected Notification()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddRangeInternal(new Drawable[]
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
                    AutoSizeDuration = 400,
                    AutoSizeEasing = Easing.OutQuint,
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
                            AutoSizeAxes = Axes.Y,
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

        protected override bool OnHover(HoverEvent e)
        {
            closeButton.FadeIn(75);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            closeButton.FadeOut(75);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Activated?.Invoke() ?? true)
                Close();

            return true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeInFromZero(200);
            NotificationContent.MoveToX(DrawSize.X);
            NotificationContent.MoveToX(0, 500, Easing.OutQuint);
        }

        public bool WasClosed;

        public virtual void Close()
        {
            if (WasClosed) return;

            WasClosed = true;

            Closed?.Invoke();
            this.FadeOut(100);
            Expire();
        }

        private class CloseButton : OsuClickableContainer
        {
            private Color4 hoverColour;

            public CloseButton()
            {
                Colour = OsuColour.Gray(0.2f);
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.TimesCircle,
                        Size = new Vector2(20),
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoverColour = colours.Yellow;
            }

            protected override bool OnHover(HoverEvent e)
            {
                this.FadeColour(hoverColour, 200);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.FadeColour(OsuColour.Gray(0.2f), 200);
                base.OnHoverLost(e);
            }
        }

        public class NotificationLight : Container
        {
            private bool pulsate;
            private Container pulsateLayer;

            public bool Pulsate
            {
                get => pulsate;
                set
                {
                    if (pulsate == value) return;

                    pulsate = value;

                    pulsateLayer.ClearTransforms();
                    pulsateLayer.Alpha = 1;

                    if (pulsate)
                    {
                        const float length = 1000;
                        pulsateLayer.Loop(length / 2,
                            p => p.FadeTo(0.4f, length, Easing.In).Then().FadeTo(1, length, Easing.Out)
                        );
                    }
                }
            }

            public new SRGBColour Colour
            {
                set
                {
                    base.Colour = value;
                    pulsateLayer.EdgeEffect = new EdgeEffectParameters
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
