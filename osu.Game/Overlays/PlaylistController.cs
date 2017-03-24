// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;

namespace osu.Game.Overlays
{
    public class PlaylistController : FocusedOverlayContainer
    {
        private const float transition_duration = 800;

        private Box bg;
        private FilterTextBox search;
        private Playlist songList;

        public Action<BeatmapSetInfo> OnSelect
        {
            get { return songList.OnSelect; }
            set { songList.OnSelect = value; }
        }

        public BeatmapSetInfo Current
        {
            get { return songList.Current; }
            set { songList.Current = value; }
        }

        public PlaylistController()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 5,
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(40),
                        Radius = 5,
                    },
                    Children = new Drawable[]
                    {
                        bg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        songList = new Playlist
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 95, Bottom = 10, Right = 10 }, //todo: static sizes aren't good
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(10),
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(0f, 10f),
                                    Children = new Drawable[]
                                    {
                                        search = new FilterTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 40,
                                        },
                                        new CollectionsDropdown<PlaylistCollection>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Items = new[] { new KeyValuePair<string, PlaylistCollection>(@"All", PlaylistCollection.All) },
                                        }
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bg.Colour = colours.Gray3;
        }

        protected override void PopIn()
        {
            base.PopIn();

            search.HoldFocus = true;

            songList.ScrollContainer.ScrollDraggerVisible = false;
            ResizeTo(new Vector2(1f), transition_duration, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            search.HoldFocus = false;
            search.TriggerFocusLost();

            songList.ScrollContainer.ScrollDraggerVisible = false;
            ResizeTo(new Vector2(1f, 0f), transition_duration, EasingTypes.OutQuint);
        }

        private class Playlist : Container
        {
            private FillFlowContainer<PlaylistItem> songs;

            // exposed so PlaylistController can hide the scroll dragger when hidden
            // because the scroller can be seen when scrolled to the bottom and PlaylistController is closed
            public readonly ScrollContainer ScrollContainer;

            private BeatmapDatabase database;

            private Action<BeatmapSetInfo> onSelect;
            public Action<BeatmapSetInfo> OnSelect
            {
                get { return onSelect; }
                set
                {
                    onSelect = value;

                    foreach (PlaylistItem s in songs.Children)
                        s.OnSelect = value;
                }
            }

            private BeatmapSetInfo current;
            public BeatmapSetInfo Current
            {
                get { return current; }
                set
                {
                    if (value == current) return;
                    current = value;

                    foreach (PlaylistItem s in songs.Children)
                        s.Current = s.RepresentedSet.ID == value.ID;
                }
            }

            public Playlist()
            {
                Children = new Drawable[]
                {
                    ScrollContainer = new ScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            songs = new FillFlowContainer<PlaylistItem>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                            },
                        },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(BeatmapDatabase beatmaps)
            {
                database = beatmaps;

                foreach (BeatmapSetInfo b in beatmaps.GetAllWithChildren<BeatmapSetInfo>())
                {
                    songs.Add(new PlaylistItem(b)
                    {
                        OnSelect = OnSelect,
                    });
                }
            }

            private class PlaylistItem : Container
            {
                private const float fade_duration = 100;
                private Color4 current_colour;

                private TextAwesome icon;
                private OsuSpriteText title, artist;

                public readonly BeatmapSetInfo RepresentedSet;
                public Action<BeatmapSetInfo> OnSelect;

                private bool current;
                public bool Current
                {
                    get { return current; }
                    set
                    {
                        if (value == current) return;
                        current = value;

                        title.FadeColour(Current ? current_colour : Color4.White, fade_duration);
                    }
                }

                public PlaylistItem(BeatmapSetInfo set)
                {
                    RepresentedSet = set;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                    Padding = new MarginPadding { Top = 3, Bottom = 3 };

                    Children = new Drawable[]
                    {
                        icon = new TextAwesome
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            TextSize = 12,
                            Icon = FontAwesome.fa_bars,
                            Alpha = 0f,
                            Margin = new MarginPadding { Left = 5 },
                            Padding = new MarginPadding { Top = 2 },
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Left = 20 },
                            Spacing = new Vector2(10f, 0f),
                            Children = new Drawable[]
                            {
                                title = new OsuSpriteText
                                {
                                    TextSize = 16,
                                    Font = @"Exo2.0-Regular",
                                    Text = RepresentedSet.Metadata.Title,
                                },
                                artist = new OsuSpriteText
                                {
                                    TextSize = 14,
                                    Font = @"Exo2.0-Bold",
                                    Text = RepresentedSet.Metadata.Artist,
                                    Padding = new MarginPadding { Top = 1 },
                                },
                            },
                        },
                    };
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    icon.Colour = colours.Gray5;
                    artist.Colour = colours.Gray9;
                    current_colour = colours.Yellow;
                }

                protected override bool OnHover(Framework.Input.InputState state)
                {
                    icon.FadeIn(fade_duration);

                    return base.OnHover(state);
                }

                protected override void OnHoverLost(Framework.Input.InputState state)
                {
                    icon.FadeOut(fade_duration);
                }

                protected override bool OnClick(Framework.Input.InputState state)
                {
                    OnSelect?.Invoke(RepresentedSet);
                    return true;
                }
            }
        }

        private class FilterTextBox : SearchTextBox
        {
            protected override Color4 BackgroundUnfocused => OsuColour.FromHex(@"222222");
            protected override Color4 BackgroundFocused => OsuColour.FromHex(@"222222");

            public FilterTextBox()
            {
                Masking = true;
                CornerRadius = 5;
            }
        }

        private class CollectionsDropdown<T> : OsuDropdown<T>
        {
            protected override DropdownHeader CreateHeader() => new CollectionsHeader { AccentColour = AccentColour };
            protected override Menu CreateMenu() => new CollectionsMenu();

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Gray6;
            }

            private class CollectionsHeader : OsuDropdownHeader
            {
                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = colours.Gray4;
                }

                public CollectionsHeader()
                {
                    CornerRadius = 5;
                    Height = 30;
                    Icon.TextSize = 14;
                    Icon.Margin = new MarginPadding(0);
                    Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 10, Right = 10 };
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.3f),
                        Radius = 3,
                        Offset = new Vector2(0f, 1f),
                    };
                }
            }

            private class CollectionsMenu : OsuMenu
            {
                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Background.Colour = colours.Gray4;
                }

                public CollectionsMenu()
                {
                    CornerRadius = 5;
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.3f),
                        Radius = 3,
                        Offset = new Vector2(0f, 1f),
                    };
                }
            }
        }
    }

    //todo: placeholder
    public enum PlaylistCollection
    {
        All
    }
}
