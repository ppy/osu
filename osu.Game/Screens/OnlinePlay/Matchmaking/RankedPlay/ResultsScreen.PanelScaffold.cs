// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class ResultsScreen
    {
        public partial class PanelScaffold : Container
        {
            private const float corner_radius = 6;
            private const float border_thickness = 2;

            protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

            public readonly ScreenBottomOrnament BottomOrnament = new ScreenBottomOrnament();

            private BufferedContainer background = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren =
                [
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Bottom = -30 },
                        Child = background = new BufferedContainer(cachedFrameBuffer: false)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Bottom = 30 },
                            BackgroundColour = Color4Extensions.FromHex("222228").Opacity(0),
                            Alpha = 0.7f,
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = corner_radius,
                                BorderThickness = border_thickness,
                                BorderColour = new ColourInfo
                                {
                                    TopLeft = RankedPlayColourScheme.BLUE.PrimaryDarkest.Opacity(0.5f),
                                    BottomLeft = RankedPlayColourScheme.BLUE.Primary.Opacity(0.75f),
                                    TopRight = RankedPlayColourScheme.RED.PrimaryDarkest.Opacity(0.5f),
                                    BottomRight = RankedPlayColourScheme.RED.Primary.Opacity(0.75f),
                                },
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex("222228"),
                                },
                            },
                        }
                    },
                    Content.With(static d =>
                    {
                        d.Masking = true;
                        d.CornerRadius = corner_radius;
                    }),
                    BottomOrnament.With(static d =>
                    {
                        d.Anchor = Anchor.BottomCentre;
                        d.Origin = Anchor.Centre;
                        d.Y -= border_thickness / 2;
                    }),
                ];

                background.Add(BottomOrnament.Background.CreateProxy());
            }
        }

        public partial class ScreenBottomOrnament : Container
        {
            protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both, };

            public Drawable Background => background;

            private readonly Container background = new Container { RelativeSizeAxes = Axes.Both };

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                InternalChildren =
                [
                    background.WithChildren([
                        new NineSliceSprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get("Online/RankedPlay/damage-display-background"),
                            TextureInsetRelativeAxes = Axes.None,
                            TextureInset = new MarginPadding { Horizontal = 30 },
                            Colour = Color4Extensions.FromHex("222228"),
                        },
                        new NineSliceSprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get("Online/RankedPlay/damage-display-border"),
                            TextureInsetRelativeAxes = Axes.None,
                            TextureInset = new MarginPadding { Horizontal = 30 },
                            Alpha = 0.25f,
                            Colour = Color4Extensions.FromHex("ddddff")
                        },
                    ]),
                    Content,
                ];
            }
        }
    }
}
