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
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogHeader : OverlayHeader
    {
        private OsuSpriteText titleStream;
        private BreadcrumbListing listing;
        private SpriteIcon chevron;
        private BreadcrumbRelease releaseStream;

        public delegate void ListingSelectedEventHandler();

        public event ListingSelectedEventHandler ListingSelected;

        private const float title_height = 50;
        private const float icon_size = 50;
        private const float icon_margin = 20;
        private const float version_height = 40;

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

        protected override Drawable CreateBackground() => new HeaderBackground();

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
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
                            //BorderColour = colours.Violet,
                            BorderThickness = 3,
                            MaskingSmoothness = 1,
                            Size = new Vector2(50),
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    //Texture = textures.Get(@"Icons/changelog"),
                                    Size = new Vector2(0.8f),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    //Colour = colours.Violet,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                },
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
                                    Font = OsuFont.GetFont(weight: FontWeight.Light, size: 30),
                                },
                                titleStream = new OsuSpriteText
                                {
                                    Text = "Listing",
                                    Font = OsuFont.GetFont(weight: FontWeight.Light, size: 30),
                                    //Colour = colours.Violet,
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
                        listing = new BreadcrumbListing( /*colours.Violet*/ Color4.WhiteSmoke)
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
                                    // Colour = colours.Violet,
                                    Icon = FontAwesome.Solid.ChevronRight,
                                    Alpha = 0,
                                    X = -200,
                                },
                            },
                        },
                        releaseStream = new BreadcrumbRelease( /*colours.Violet*/ Color4.WhiteSmoke, "Lazer")
                        {
                            Action = () => titleStream.FlashColour(Color4.White, 500, Easing.OutQuad)
                        }
                    },
                },
                new Box
                {
                    //Colour = colours.Violet,
                    RelativeSizeAxes = Axes.X,
                    Height = 2,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.CentreLeft,
                },
            }
        };

        protected override ScreenTitle CreateTitle() => new ChangelogHeaderTitle();

        public class HeaderBackground : Sprite
        {
            public HeaderBackground()
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(@"Headers/changelog");
            }
        }

        private class ChangelogHeaderTitle : ScreenTitle
        {
            public ChangelogHeaderTitle()
            {
                Title = "Changelog";
                Section = "Listing";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Seafoam;
            }
        }
    }
}
