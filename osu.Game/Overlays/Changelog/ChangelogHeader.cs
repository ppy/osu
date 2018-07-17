// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Changelog.Header;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogHeader : Container
    {
        private readonly Container coverContainer;

        private Color4 purple = new Color4(191, 4, 255, 255);
        private readonly Sprite coverImage;
        private readonly Sprite headerBadge; //50x50, margin-right: 20
        private readonly FillFlowContainer headerTextContainer;
        private readonly OsuSpriteText title, titleStream;
        private readonly TextBadgePairListing listing;
        private readonly TextBadgePairRelease releaseStream;
        private readonly FillFlowContainer breadcrumbContainer;

        private const float cover_height = 310;
        private const float title_height = 50;
        private const float icon_size = 50;
        private const float icon_margin = 20;
        private const float version_height = 40;

        public ChangelogHeader()
        {
            RelativeSizeAxes = Axes.X;
            Height = cover_height + 5; // 5 is for the "badge" that sticks a bit out of the bottom
            Masking = true; // is masking necessary? since I see it nearly everywhere
            Children = new Drawable[]
            {
                coverContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Children = new Drawable[]
                    {
                        coverImage = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new OpenTK.Vector2(1),
                            FillMode = FillMode.Fill,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        //new Container
                        //{
                        //    RelativeSizeAxes = Axes.X,
                        //    Height = cover_height,
                        //    Children = new Drawable[]
                        //    {
                                new Container // this is the line badge-Changelog-Stream
                                {
                                    Height = title_height,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Y = -version_height,
                                    Children = new Drawable[]
                                    {
                                        new CircularContainer // a purple circle
                                        {
                                            X = icon_margin,
                                            Masking = true,
                                            BorderColour = purple,
                                            BorderThickness = 3,
                                            MaskingSmoothness = 1,
                                            Size = new OpenTK.Vector2(50),
                                            Children = new Drawable[]
                                            {
                                                headerBadge = new Sprite
                                                {
                                                    Size = new OpenTK.Vector2(0.8f),
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                                new Box // this ensures the purple circle doesn't disappear..?
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Size = new OpenTK.Vector2(1),
                                                    AlwaysPresent = true,
                                                    Colour = Color4.Transparent,
                                                }
                                            }
                                        },
                                        headerTextContainer = new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            X = icon_size + icon_margin * 2,
                                            Children = new Drawable[]
                                            {
                                                title = new OsuSpriteText
                                                {
                                                    Text = "Changelog ",
                                                    Font = @"Exo2.0-Light",
                                                    TextSize = 38, // web: 30
                                                },
                                                titleStream = new OsuSpriteText
                                                {
                                                    Text = "Listing",
                                                    TextSize = 38, // web: 30
                                                    Font = @"Exo2.0-Light",
                                                    Colour = purple,
                                                },
                                            }
                                        }
                                    }
                                },
                                breadcrumbContainer = new FillFlowContainer // Listing > Lazer 2018.713.1
                                {
                                    X = 2 * icon_margin + icon_size - 8, // for some reason off by 3px
                                    Height = version_height,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        listing = new TextBadgePairListing(purple),
                                        new SpriteIcon
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(7),
                                            Colour = OsuColour.FromHex(@"bf04ff"),
                                            Icon = FontAwesome.fa_chevron_right,
                                            Margin = new MarginPadding()
                                            {
                                                Top = 8,
                                                Left = 5,
                                                Right = 5,
                                                Bottom = 15,
                                            },
                                        },
                                        releaseStream = new TextBadgePairRelease(purple, "Lazer")
                                    },
                                },
                                new Box // purple line
                                {
                                    Colour = purple,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 3,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                        //    }
                        //}
                    }
                }
            };

            // is this a bad way to do this?
            OnLoadComplete = d =>
            {
                releaseStream.OnActivation = listing.Deactivate;
                listing.OnActivation = () =>
                {
                    releaseStream.Deactivate();
                    ChangeHeaderText("Listing");
                };
            };
        }

        public void ShowReleaseStream(string headerText, string breadcrumbText)
        {
            releaseStream.Activate(breadcrumbText);
            ChangeHeaderText(headerText);
        }

        private void ChangeHeaderText(string headerText)
        {
            titleStream.Text = headerText;
            titleStream.FlashColour(Color4.White, 500, Easing.OutQuad);
        }

        public void ActivateListing() => listing.Activate();
        
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            // should be added to osu-resources?
            // headerBadge.Texture = textures.Get(@"https://osu.ppy.sh/images/icons/changelog.svg"); // this is not working
            headerBadge.Texture = textures.Get(@"https://i.imgur.com/HQM3Vhp.png");
            coverImage.Texture = textures.Get(@"https://osu.ppy.sh/images/headers/changelog.jpg");
        }
    }
}
