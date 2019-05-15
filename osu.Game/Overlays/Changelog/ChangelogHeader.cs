// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Changelog.Header;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogHeader : Container
    {
        private OsuSpriteText titleStream;
        private BreadcrumbListing listing;
        private SpriteIcon chevron;
        private BreadcrumbRelease releaseStream;

        public delegate void ListingSelectedEventHandler();

        public event ListingSelectedEventHandler ListingSelected;

        private const float cover_height = 150;
        private const float title_height = 50;
        private const float icon_size = 50;
        private const float icon_margin = 20;
        private const float version_height = 40;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            RelativeSizeAxes = Axes.X;
            Height = cover_height;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"https://osu.ppy.sh/images/headers/changelog.jpg"),
                            FillMode = FillMode.Fill,
                        },
                    }
                },
                new Container
                {
                    Height = title_height,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Y = -version_height,
                    Children = new Drawable[]
                    {
                        new CircularContainer
                        {
                            X = icon_margin,
                            Masking = true,
                            BorderColour = colours.Violet,
                            BorderThickness = 3,
                            MaskingSmoothness = 1,
                            Size = new Vector2(50),
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    // todo: https://osu.ppy.sh/images/icons/changelog.svg
                                    Texture = textures.Get(@"https://i.imgur.com/HQM3Vhp.png"),
                                    Size = new Vector2(0.8f),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },

                                // this box has 2 functions:
                                // - ensures the circle doesn't disappear on the X and Y edges
                                // - gets rid of the white "contamination" on the circle (due to smoothing)
                                //   (https://i.imgur.com/SMuvWBZ.png)
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                    Colour = colours.Violet,
                                }
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            X = icon_size + icon_margin * 2,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Changelog ",
                                    Font = OsuFont.GetFont(weight: FontWeight.Light, size: 38), // web: 30,
                                },
                                titleStream = new OsuSpriteText
                                {
                                    Text = "Listing",
                                    Font = OsuFont.GetFont(weight: FontWeight.Light, size: 38), // web: 30,
                                    Colour = colours.Violet,
                                },
                            }
                        }
                    }
                },
                new FillFlowContainer // Listing > Lazer 2018.713.1
                {
                    X = 2 * icon_margin + icon_size,
                    Height = version_height,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        listing = new BreadcrumbListing(colours.Violet)
                        {
                            Action = () => ListingSelected?.Invoke()
                        },
                        new Container // without a container, moving the chevron wont work
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding
                            {
                                Top = 10,
                                Left = 15,
                                Right = 18,
                                Bottom = 15,
                            },
                            Children = new Drawable[]
                            {
                                chevron = new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(7),
                                    Colour = colours.Violet,
                                    Icon = FontAwesome.Solid.ChevronRight,
                                    Alpha = 0,
                                    X = -200,
                                },
                            },
                        },
                        releaseStream = new BreadcrumbRelease(colours.Violet, "Lazer")
                        {
                            Action = () => titleStream.FlashColour(Color4.White, 500, Easing.OutQuad)
                        }
                    },
                },
                new Box
                {
                    Colour = colours.Violet,
                    RelativeSizeAxes = Axes.X,
                    Height = 2,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.CentreLeft,
                },
            };
        }

        public void ShowBuild(string displayName, string displayVersion)
        {
            listing.Deactivate();
            releaseStream.ShowBuild($"{displayName} {displayVersion}");
            titleStream.Text = displayName;
            titleStream.FlashColour(Color4.White, 500, Easing.OutQuad);
            chevron.MoveToX(0, 100).FadeIn(100);
        }

        public void ShowListing()
        {
            releaseStream.Deactivate();
            listing.Activate();
            titleStream.Text = "Listing";
            titleStream.FlashColour(Color4.White, 500, Easing.OutQuad);
            chevron.MoveToX(-20, 100).FadeOut(100);
        }
    }
}
