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
        public event Action? Closed;

        public abstract LocalisableString Text { get; set; }

        /// <summary>
        /// Whether this notification should forcefully display itself.
        /// </summary>
        public virtual bool IsImportant => true;

        /// <summary>
        /// Run on user activating the notification. Return true to close.
        /// </summary>
        public Func<bool>? Activated;

        /// <summary>
        /// Should we show at the top of our section on display?
        /// </summary>
        public virtual bool DisplayOnTop => true;

        public virtual string PopInSampleName => "UI/notification-pop-in";

        protected NotificationLight Light;

        protected Container IconContent;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        protected Container MainContent;

        public virtual bool Read { get; set; }

        protected virtual IconUsage CloseButtonIcon => FontAwesome.Solid.Check;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly Box initialFlash;

        private Box background = null!;

        protected Notification()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                Light = new NotificationLight
                {
                    Margin = new MarginPadding { Right = 5 },
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                },
                MainContent = new Container
                {
                    CornerRadius = 6,
                    Masking = true,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = 400,
                    AutoSizeEasing = Easing.OutQuint,
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize, minSize: 60)
                            },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    IconContent = new Container
                                    {
                                        Width = 40,
                                        RelativeSizeAxes = Axes.Y,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding(10),
                                        Children = new Drawable[]
                                        {
                                            content = new Container
                                            {
                                                Masking = true,
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                            },
                                        }
                                    },
                                    new CloseButton(CloseButtonIcon)
                                    {
                                        Action = Close,
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                    }
                                }
                            },
                        },
                        initialFlash = new Box
                        {
                            Colour = Color4.White.Opacity(0.8f),
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            MainContent.Add(background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background3,
                Depth = float.MaxValue
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(colourProvider.Background2, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(colourProvider.Background3, 200, Easing.OutQuint);
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

            MainContent.MoveToX(DrawSize.X);
            MainContent.MoveToX(0, 500, Easing.OutQuint);

            initialFlash.FadeOutFromOne(2000, Easing.OutQuart);
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
            private SpriteIcon icon = null!;
            private Box background = null!;

            private readonly IconUsage iconUsage;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public CloseButton(IconUsage iconUsage)
            {
                this.iconUsage = iconUsage;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Y;
                Width = 28;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        Colour = OsuColour.Gray(0).Opacity(0.15f),
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                    },
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = iconUsage,
                        Size = new Vector2(12),
                        Colour = colourProvider.Foreground1,
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeIn(200, Easing.OutQuint);
                icon.FadeColour(colourProvider.Content1, 200, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeOut(200, Easing.OutQuint);
                icon.FadeColour(colourProvider.Foreground1, 200, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }

        public class NotificationLight : Container
        {
            private bool pulsate;
            private Container pulsateLayer = null!;

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
