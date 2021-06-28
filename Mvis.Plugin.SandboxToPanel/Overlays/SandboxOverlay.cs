using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.RulesetPanel.Overlays
{
    public abstract class SandboxOverlay : OverlayContainer
    {
        protected SandboxOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f
                },
                new Container
                {
                    Size = new Vector2(500, 300),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 3,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White,
                                    EdgeSmoothness = Vector2.One
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(15),
                                    Child = new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        RowDimensions = new[]
                                        {
                                            new Dimension(),
                                            new Dimension(GridSizeMode.AutoSize)
                                        },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension()
                                        },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                CreateContent()
                                            },
                                            new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Padding = new MarginPadding { Top = 15 },
                                                    Child = new FillFlowContainer<SandboxOverlayButton>
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Direction = FillDirection.Horizontal,
                                                        Spacing = new Vector2(40, 0),
                                                        Children = CreateButtons()
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        protected abstract Drawable CreateContent();

        protected abstract SandboxOverlayButton[] CreateButtons();

        protected override void PopIn() => this.FadeIn(250, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(250, Easing.OutQuint);

        public class SandboxOverlayButton : CompositeDrawable
        {
            public Action ClickAction;

            private readonly Box bg;
            private readonly Container content;

            public SandboxOverlayButton(string text)
            {
                Size = new Vector2(100, 50);
                AddInternal(content = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.4f),
                        Offset = new Vector2(0, 3),
                        Radius = 7,
                        Type = EdgeEffectType.Shadow
                    },
                    Children = new Drawable[]
                    {
                        bg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = baseColour,
                            EdgeSmoothness = Vector2.One
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Color4.White,
                            Text = text,
                            Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold)
                        }
                    }
                });

                AddInternal(new HoverClickSounds(HoverSampleSet.Button));
            }

            private static Color4 baseColour => new Color4(20, 20, 20, 255);

            private static Color4 hoverColour => new Color4(60, 60, 60, 255);

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                base.OnMouseDown(e);

                content.ScaleTo(0.97f, 250, Easing.OutQuad);

                content.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Colour = Color4.Black.Opacity(0.4f),
                    Radius = 3,
                    Offset = Vector2.Zero,
                    Type = EdgeEffectType.Shadow
                }, 250, Easing.OutQuad);

                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                base.OnMouseUp(e);

                content.ScaleTo(1, 500, Easing.OutElastic);

                content.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Colour = Color4.Black.Opacity(0.4f),
                    Radius = 7,
                    Offset = new Vector2(0, 3),
                    Type = EdgeEffectType.Shadow
                }, 500, Easing.OutElastic);

                // Delayed to fix cases when confirm sound can not be played.
                Scheduler.AddDelayed(() =>
                {
                    if (IsHovered)
                        ClickAction?.Invoke();
                }, 10);
            }

            protected override bool OnHover(HoverEvent e)
            {
                bg.FadeColour(hoverColour, 250, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                bg.FadeColour(baseColour, 250, Easing.OutQuint);
            }
        }
    }
}
