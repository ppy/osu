// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class Header : Container
    {
        private const float tabs_height = 50;

        private readonly Box tabsBg;

        public Header(BeatmapSetInfo set)
        {
            RelativeSizeAxes = Axes.X;
            Height = 400;
            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.25f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = tabs_height,
                    Children = new[]
                    {
                        tabsBg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = tabs_height },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                },
                                new DelayedLoadWrapper(new BeatmapSetCover(set)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fill,
                                    OnLoadComplete = d =>
                                    {
                                        d.FadeInFromZero(400, Easing.Out);
                                    },
                                })
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    TimeBeforeLoad = 300
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.3f), Color4.Black.Opacity(0.8f)),
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 20, Bottom = 30, Horizontal = OnlineBeatmapSetOverlay.X_PADDING },
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        Name = @"beatmap picker",
                                        RelativeSizeAxes = Axes.X,
                                        Height = 113,
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = set.Metadata.Title,
                                        Font = @"Exo2.0-BoldItalic",
                                        TextSize = 37,
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = set.Metadata.Artist,
                                        Font = @"Exo2.0-SemiBoldItalic",
                                        TextSize = 25,
                                    },
                                    new Container
                                    {
                                        Name = "mapper",
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Margin = new MarginPadding { Top = 20 },
                                        Child = new AuthorInfo(set.OnlineInfo),
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 45,
                                        Spacing = new Vector2(5f),
                                        Margin = new MarginPadding { Top = 10 },
                                        Children = new Button[]
                                        {
                                            new FavouriteButton(),
                                            new DownloadButton("Download", ""),
                                            new DownloadButton("osu!direct", ""),
                                        },
                                    },
                                },
                            },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Width = OnlineBeatmapSetOverlay.RIGHT_WIDTH,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Right = OnlineBeatmapSetOverlay.X_PADDING },
                            Spacing = new Vector2(1f),
                            Children = new Drawable[]
                            {
                                new DetailBox
                                {
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 42,
                                    },
                                },
                                new DetailBox
                                {
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 35,
                                    },
                                },
                                new DetailBox
                                {
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 110,
                                    },
                                },
                                new DetailBox
                                {
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 115,
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
            tabsBg.Colour = colours.Gray3;
        }

        private class Button : OsuClickableContainer
        {
            private readonly Container content;

            protected override Container<Drawable> Content => content;

            public Button()
            {
                CornerRadius = 3;
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"094c5f"),
                    },
                    new Triangles
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColourLight = OsuColour.FromHex(@"0f7c9b"),
                        ColourDark = OsuColour.FromHex(@"094c5f"),
                        TriangleScale = 1.5f,
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }
        }

        private class DownloadButton : Button
        {
            public DownloadButton(string title, string subtitle)
            {
                Width = 120;
                RelativeSizeAxes = Axes.Y;

                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 10 },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new[]
                            {
                                new OsuSpriteText
                                {
                                    Text = title,
                                    TextSize = 13,
                                    Font = @"Exo2.0-Bold",
                                },
                                new OsuSpriteText
                                {
                                    Text = subtitle,
                                    TextSize = 11,
                                    Font = @"Exo2.0-Bold",
                                },
                            },
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Icon = FontAwesome.fa_download,
                            Size = new Vector2(16),
                            Margin = new MarginPadding { Right = 5 },
                        },
                    },
                };
            }
        }

        private class FavouriteButton : Button
        {
            public readonly Bindable<bool> Favourited = new Bindable<bool>();

            public FavouriteButton()
            {
                RelativeSizeAxes = Axes.Y;

                Container pink;
                SpriteIcon icon;
                Children = new Drawable[]
                {
                    pink = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0f,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = OsuColour.FromHex(@"9f015f"),
                            },
                            new Triangles
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColourLight = OsuColour.FromHex(@"cb2187"),
                                ColourDark = OsuColour.FromHex(@"9f015f"),
                                TriangleScale = 1.5f,
                            },
                        },
                    },
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.fa_heart_o,
                        Size = new Vector2(18),
                        Shadow = false,
                    },
                };

                Favourited.ValueChanged += value =>
                {
                    pink.FadeTo(value ? 1 : 0, 200);
                    icon.Icon = value ? FontAwesome.fa_heart : FontAwesome.fa_heart_o;
                };
                Action = () => Favourited.Value = !Favourited.Value;
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                Width = DrawHeight;
            }
        }

        private class DetailBox : Container
        {
            private Container content;
            protected override Container<Drawable> Content => content;

            public DetailBox()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = 15 },
                    },
                };
            }
        }
    }
}
