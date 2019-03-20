// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylist : CompositeDrawable
    {
        private const float spacing = 10;

        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();
        private readonly FillFlowContainer<BeatmapPlaylistItem> playlistFlowContainer;

        public BeatmapPlaylist()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new ScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarOverlapsContent = false,
                Padding = new MarginPadding(spacing / 2),
                Child = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = playlistFlowContainer = new FillFlowContainer<BeatmapPlaylistItem>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(1),
                    },
                }
            };

            playlist.ItemsAdded += itemsAdded;
        }

        private void itemsAdded(IEnumerable<PlaylistItem> items)
        {
            foreach (var item in items)
            {
                var drawable = new BeatmapPlaylistItem(item);
                drawable.RemovalTriggered += handleRemoval;
                playlistFlowContainer.Add(drawable);
            }
        }

        public void AddItem(PlaylistItem item)
        {
            playlist.Add(item);
        }

        private void handleRemoval(BeatmapPlaylistItem item)
        {
            playlist.Remove(item.PlaylistItem.Value);
            playlistFlowContainer.Remove(item);
        }

        public class BeatmapPlaylistItem : Container
        {
            public readonly Bindable<PlaylistItem> PlaylistItem = new Bindable<PlaylistItem>();
            private readonly UpdateableBeatmapBackgroundSprite cover;
            private readonly Container removeButton;

            public event Action<BeatmapPlaylistItem> RemovalTriggered;

            public BeatmapPlaylistItem(PlaylistItem item)
            {
                Height = 50;
                RelativeSizeAxes = Axes.X;
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
                                        cover = new UpdateableBeatmapBackgroundSprite(BeatmapSetCoverType.List)
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
                            Padding = new MarginPadding(5),
                            Children = new Drawable[]
                            {
                                new DifficultyIcon(item.Beatmap)
                                {
                                    Scale = new Vector2(1.5f)
                                },
                                new FillFlowContainer
                                {
                                    Padding = new MarginPadding
                                    {
                                        Left = spacing
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
                                                    Text = item.Beatmap?.BeatmapSet?.Metadata?.Artist ?? "????????",
                                                    Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold)
                                                },
                                                new OsuSpriteText
                                                {
                                                    Text = "-",
                                                    Font = OsuFont.GetFont()
                                                },
                                                new OsuSpriteText
                                                {
                                                    Text = item.Beatmap?.BeatmapSet?.Metadata?.Title ?? "????????",
                                                    Font = OsuFont.GetFont()
                                                },
                                            }
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Spacing = new Vector2(spacing),
                                            Children = new Drawable[]
                                            {
                                                new OsuSpriteText
                                                {
                                                    Text = item.Beatmap?.Version ?? "??",
                                                    Font = OsuFont.GetFont(size: 12)
                                                },
                                                new OsuSpriteText
                                                {
                                                    Text = $"mapped by {item.Beatmap?.BeatmapSet?.Metadata?.Author.Username}",
                                                    Font = OsuFont.GetFont(size: 12, italics: true),
                                                    Colour = Color4.Violet,
                                                }
                                            }
                                        },
                                    }
                                },
                            }
                        },
                        removeButton = new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Alpha = 0,
                            Margin = new MarginPadding
                            {
                                Right = 10,
                            },
                            Child = new IconButton
                            {
                                Colour = Color4.White,
                                Icon = FontAwesome.fa_minus_square,
                                ButtonSize = new Vector2(14),
                                IconScale = new Vector2(0.75f),
                                Action = () => RemovalTriggered?.Invoke(this)
                            }
                        },
                    },
                };

                PlaylistItem.ValueChanged += itemChanged;
            }

            protected override bool OnHover(HoverEvent e)
            {
                removeButton.FadeTo(1f, 60);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                removeButton.FadeTo(0f, 60);
            }

            private void itemChanged(ValueChangedEvent<PlaylistItem> item)
            {
                cover.Beatmap.Value = item.NewValue.Beatmap;
            }
        }
    }
}
