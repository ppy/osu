// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylistItem : Container
    {
        public readonly Bindable<PlaylistItem> PlaylistItem = new Bindable<PlaylistItem>();

        public event Action<BeatmapPlaylistItem> RequestRemoval;

        private const int fade_duration = 60;
        private readonly UpdateableBeatmapBackgroundSprite cover;
        private readonly DragHandle dragHandle;
        private readonly RemoveButton removeButton;
        private bool isHovered;
        private bool isDragged;

        public BeatmapPlaylistItem(PlaylistItem item)
        {
            Height = 50;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Left = 25,
                    },
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 5,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Black.Opacity(40),
                            Radius = 5,
                        },
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Colour = Color4.Black,
                                        RelativeSizeAxes = Axes.Both,
                                        Width = 0.25f,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Width = 0.75f,
                                        Children = new Drawable[]
                                        {
                                            cover = new UpdateableBeatmapBackgroundSprite
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                FillMode = FillMode.Stretch
                                            },
                                            new Box
                                            {
                                                Colour = ColourInfo.GradientHorizontal(Color4.Black, Color4.Black.Opacity(0.25f)),
                                                RelativeSizeAxes = Axes.Both,
                                            },
                                        },
                                    },
                                }
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Padding = new MarginPadding(7),
                                Children = new Drawable[]
                                {
                                    new DifficultyIcon(item.Beatmap)
                                    {
                                        Scale = new Vector2(1.5f)
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Padding = new MarginPadding
                                        {
                                            Left = 10,
                                        },
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Spacing = new Vector2(5),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Text = new LocalisedString((item.Beatmap?.BeatmapSet?.Metadata.ArtistUnicode, item.Beatmap?.BeatmapSet?.Metadata.Artist)),
                                                        Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold)
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Text = "-",
                                                        Font = OsuFont.GetFont()
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Text = new LocalisedString((item.Beatmap?.Metadata.TitleUnicode, item.Beatmap?.Metadata.Title)),
                                                        Font = OsuFont.GetFont()
                                                    },
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Spacing = new Vector2(10),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Text = item.Beatmap?.Version,
                                                        Font = OsuFont.GetFont(size: 12)
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Text = $"mapped by {item.Beatmap?.BeatmapSet?.Metadata.Author.Username}",
                                                        Font = OsuFont.GetFont(size: 12, italics: true),
                                                        Colour = Color4.Violet,
                                                    }
                                                }
                                            },
                                        }
                                    },
                                }
                            },
                        },
                    }
                },
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding
                    {
                        Right = 10,
                    },
                    Child = removeButton = new RemoveButton
                    {
                        AutoSizeAxes = Axes.Both,
                        Action = () => RequestRemoval?.Invoke(this)
                    }
                },
                dragHandle = new DragHandle()
            };

            PlaylistItem.ValueChanged += change => cover.Beatmap.Value = change.NewValue.Beatmap;
            PlaylistItem.Value = item;
        }

        private void showHoverElements(bool show)
        {
            if (show)
            {
                removeButton.Show();
                dragHandle.Show();
            }
            else
            {
                removeButton.Hide();
                dragHandle.Hide();
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (Mouse.GetState().IsButtonDown(MouseButton.Left))
            {
                if (isDragged)
                    isHovered = true;

                return true;
            }

            isHovered = true;

            showHoverElements(true);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            isHovered = false;

            if (isDragged)
                return;

            showHoverElements(false);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // We manually track dragging status as to not capture dragging events (so we don't interfere with the scrolling behaviour of our parent)
            isDragged = true;
            return false;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // This is to show the current item's buttons after having dragged a different item and landing here (i.e. the OnHover was prevented from being fired)
            if (!isHovered && !Mouse.GetState().IsButtonDown(MouseButton.Left))
                showHoverElements(true);

            return base.OnMouseMove(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            isDragged = false;

            if (!isHovered)
                showHoverElements(false);

            return base.OnMouseUp(e);
        }

        private class DragHandle : SpriteIcon
        {
            public DragHandle()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                Size = new Vector2(12);
                Icon = FontAwesome.fa_bars;
                Alpha = 0;
                Margin = new MarginPadding { Left = 5, Top = 2 };
            }

            public override void Show()
            {
                this.FadeIn(fade_duration);
            }

            public override void Hide()
            {
                this.FadeOut(fade_duration);
            }
        }

        private class RemoveButton : OsuClickableContainer
        {
            public RemoveButton()
            {
                Alpha = 0;
                Child = new SpriteIcon
                {
                    Colour = Color4.White,
                    Icon = FontAwesome.fa_minus_square,
                    Size = new Vector2(14),
                };
            }

            public override void Show()
            {
                this.FadeIn(fade_duration);
            }

            public override void Hide()
            {
                this.FadeOut(fade_duration);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                Content.ScaleTo(0.75f, 2000, Easing.OutQuint);
                return base.OnMouseDown(e);
            }

            protected override bool OnMouseUp(MouseUpEvent e)
            {
                Content.ScaleTo(1, 1000, Easing.OutElastic);
                return base.OnMouseUp(e);
            }
        }
    }
}
