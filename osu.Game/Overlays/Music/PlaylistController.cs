// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistController : OverlayContainer
    {
        private const float transition_duration = 600;

        private const float playlist_height = 510;

        private readonly Box bg;
        private readonly FilterControl filter;
        private readonly Playlist list;

        public BeatmapSetInfo[] List => list.Sets;

        public Action<BeatmapSetInfo, int> OnSelect
        {
            get { return list.OnSelect; }
            set { list.OnSelect = value; }
        }

        public BeatmapSetInfo Current
        {
            get { return list.Current; }
            set { list.Current = value; }
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
                        list = new Playlist
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 95, Bottom = 10, Right = 10 },
                        },
                        filter = new FilterControl
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(10),
                        },
                    },
                },
            };

            filter.Search.Exit = Hide;
            //todo: play the first displayed song on commit when searching is implemented
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapDatabase beatmaps)
        {
            bg.Colour = colours.Gray3;
            list.Sets = beatmaps.GetAllWithChildren<BeatmapSetInfo>().ToArray();
        }

        protected override void PopIn()
        {
            filter.Search.HoldFocus = true;
            filter.Search.TriggerFocus();


            ResizeTo(new Vector2(1, playlist_height), transition_duration, EasingTypes.OutQuint);
            FadeIn(transition_duration, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            filter.Search.HoldFocus = false;
            filter.Search.TriggerFocusLost();

            ResizeTo(new Vector2(1, 0), transition_duration, EasingTypes.OutQuint);
            FadeOut(transition_duration);
        }

        private class FilterControl : Container
        {
            public readonly FilterTextBox Search;

            public FilterControl()
            {
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0f, 10f),
                        Children = new Drawable[]
                        {
                            Search = new FilterTextBox
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
                };
            }

            public class FilterTextBox : SearchTextBox
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

        private class Playlist : Container
        {
            private readonly FillFlowContainer<PlaylistItem> items;

            private BeatmapSetInfo[] sets = { };
            public BeatmapSetInfo[] Sets
            {
                get
                {
                    return sets;
                }
                set
                {
                    if (value == sets) return;
                    sets = value;

                    List<PlaylistItem> newItems = new List<PlaylistItem>();

                    for (int i = 0; i < value.Length; i++)
                    {
                        newItems.Add(new PlaylistItem(value[i], i)
                        {
                            OnSelect = itemSelected,
                        });
                    }

                    items.Children = newItems;
                }
            }

            private void itemSelected(BeatmapSetInfo arg1, int arg2) => OnSelect?.Invoke(arg1, arg2);

            public Action<BeatmapSetInfo, int> OnSelect;

            private BeatmapSetInfo current;
            public BeatmapSetInfo Current
            {
                get { return current; }
                set
                {
                    if (value == current) return;
                    current = value;

                    foreach (PlaylistItem s in items.Children)
                        s.Current = s.RepresentedSet.ID == value.ID;
                }
            }

            public Playlist()
            {
                Children = new Drawable[]
                {
                    new ScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            items = new FillFlowContainer<PlaylistItem>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                            },
                        },
                    },
                };
            }

            private class PlaylistItem : Container
            {
                private const float fade_duration = 100;
                private Color4 currentColour;

                private readonly TextAwesome icon;
                private readonly IEnumerable<OsuSpriteText> title, artist;

                public readonly int Index;
                public readonly BeatmapSetInfo RepresentedSet;
                public Action<BeatmapSetInfo, int> OnSelect;

                private bool current;
                public bool Current
                {
                    get { return current; }
                    set
                    {
                        if (value == current) return;
                        current = value;

                        Flush(true);
                        foreach (OsuSpriteText t in title)
                            t.FadeColour(Current ? currentColour : Color4.White, fade_duration);
                    }
                }

                public PlaylistItem(BeatmapSetInfo set, int index)
                {
                    Index = index;
                    RepresentedSet = set;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                    Padding = new MarginPadding { Top = 3, Bottom = 3 };

                    FillFlowContainer<OsuSpriteText> textContainer = new FillFlowContainer<OsuSpriteText>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Left = 20 },
                        Spacing = new Vector2(5f, 0f),
                    };

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
                        textContainer,
                    };

                    textContainer.Add(title = splitText(RepresentedSet.Metadata.Title, 16, @"Exo2.0-Regular", new MarginPadding(0)));
                    textContainer.Add(artist = splitText(RepresentedSet.Metadata.Artist, 14, @"Exo2.0-Bold", new MarginPadding { Top = 1 }));
                }

                private IEnumerable<OsuSpriteText> splitText(string text, int textSize, string font, MarginPadding padding)
                {
                    List<OsuSpriteText> sprites = new List<OsuSpriteText>();

                    foreach (string w in text.Split(' '))
                    {
                        sprites.Add(new OsuSpriteText
                        {
                            TextSize = textSize,
                            Font = font,
                            Text = w,
                            Padding = padding,
                        });
                    }

                    return sprites;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    foreach (OsuSpriteText t in artist)
                        t.Colour = colours.Gray9;

                    icon.Colour = colours.Gray5;
                    currentColour = colours.Yellow;
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
                    OnSelect?.Invoke(RepresentedSet, Index);
                    return true;
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
