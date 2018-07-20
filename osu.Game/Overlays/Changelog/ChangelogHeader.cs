// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Changelog.Header;
using System;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogHeader : Container
    {
        protected Color4 Purple = new Color4(191, 4, 255, 255);
        private readonly Sprite coverImage;
        private readonly Sprite headerBadge;
        private readonly OsuSpriteText titleStream;
        private readonly TextBadgePairListing listing;
        private readonly TextBadgePairRelease releaseStream;

        public Action OnListingActivated;

        public APIChangelog ChangelogEntry;

        private const float cover_height = 310;
        private const float title_height = 50;
        private const float icon_size = 50;
        private const float icon_margin = 20;
        private const float version_height = 40;

        public ChangelogHeader()
        {
            SpriteIcon chevron; // AppVeyor told me this should be a local variable..?
            RelativeSizeAxes = Axes.X;
            Height = cover_height;
            Children = new Drawable[]
            {
                coverImage = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
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
                            BorderColour = Purple,
                            BorderThickness = 3,
                            MaskingSmoothness = 1,
                            Size = new Vector2(50),
                            Children = new Drawable[]
                            {
                                headerBadge = new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
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
                                    Colour = Purple,
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
                                    Font = @"Exo2.0-Light",
                                    TextSize = 38, // web: 30
                                },
                                titleStream = new OsuSpriteText
                                {
                                    Text = "Listing",
                                    TextSize = 38, // web: 30
                                    Font = @"Exo2.0-Light",
                                    Colour = Purple,
                                },
                            }
                        }
                    }
                },
                new FillFlowContainer // Listing > Lazer 2018.713.1
                {
                    X = 2 * icon_margin + icon_size - 8,
                    Height = version_height,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        listing = new TextBadgePairListing(Purple),
                        new Container // without a container, moving the chevron wont work
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding
                            {
                                Top = 10,
                                Left = 7,
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
                                    Colour = Purple,
                                    Icon = FontAwesome.fa_chevron_right,
                                    Alpha = 0,
                                    X = -200,
                                },
                            },
                        },
                        releaseStream = new TextBadgePairRelease(Purple, "Lazer")
                    },
                },
                new Box
                {
                    Colour = Purple,
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.CentreLeft,
                },
            };

            // is this a bad way to do this?
            OnLoadComplete = d =>
            {
                releaseStream.OnActivation = () =>
                {
                    listing.Deactivate();
                    chevron.MoveToX(0, 100).FadeIn(100);
                };
                listing.OnActivation = () =>
                {
                    releaseStream.Deactivate();
                    chevron.MoveToX(-20, 100).FadeOut(100);
                    changeHeaderText("Listing");
                    OnListingActivated?.Invoke();
                };
            };
        }

        public void ShowReleaseStream()
        {
            releaseStream.Activate(String.Join(" ",
                ChangelogEntry.UpdateStream.DisplayName, ChangelogEntry.DisplayVersion));
            changeHeaderText(ChangelogEntry.UpdateStream.DisplayName);
        }

        private void changeHeaderText(string headerText)
        {
            titleStream.Text = headerText;
            titleStream.FlashColour(Color4.White, 500, Easing.OutQuad);
        }

        public void ActivateListing() => listing.Activate();

        public bool IsListingActivated() => listing.IsActivated;

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
