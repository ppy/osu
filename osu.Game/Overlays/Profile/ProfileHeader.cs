// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile
{
    public class ProfileHeader : Container
    {
        private readonly Container coverContainer;
        private readonly OsuSpriteText coverInfoText;
        private readonly ProfileHeaderTabControl infoTabControl;

        private readonly TopHeaderContainer topHeaderContainer;
        public SupporterIcon SupporterTag => topHeaderContainer.SupporterTag;

        private readonly CenterHeaderContainer centerHeaderContainer;
        public readonly BindableBool DetailsVisible = new BindableBool();

        private readonly DetailHeaderContainer detailHeaderContainer;
        private readonly MedalHeaderContainer medalHeaderContainer;
        private readonly BottomHeaderContainer bottomHeaderContainer;

        private const float cover_height = 150;
        private const float cover_info_height = 75;

        public ProfileHeader()
        {
            Container expandedDetailContainer;
            FillFlowContainer hiddenDetailContainer, headerDetailContainer;
            SpriteIcon expandButtonIcon;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                coverContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.75f))
                        },
                    }
                },
                new Container
                {
                    Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                    Y = cover_height,
                    Height = cover_info_height,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.BottomLeft,
                    Depth = -float.MaxValue,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Player ",
                                    Font = "Exo2.0-Regular",
                                    TextSize = 30
                                },
                                coverInfoText = new OsuSpriteText
                                {
                                    Text = "Info",
                                    Font = "Exo2.0-Regular",
                                    TextSize = 30
                                }
                            }
                        },
                        infoTabControl = new ProfileHeaderTabControl
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = cover_info_height - 30,
                            Margin = new MarginPadding { Left = -UserProfileOverlay.CONTENT_X_MARGIN },
                            Padding = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN }
                        }
                    }
                },
                new FillFlowContainer
                {
                    Margin = new MarginPadding { Top = cover_height },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        topHeaderContainer = new TopHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 150,
                        },
                        centerHeaderContainer = new CenterHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 60,
                        },
                        detailHeaderContainer = new DetailHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        medalHeaderContainer = new MedalHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        bottomHeaderContainer = new BottomHeaderContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                    }
                }
            };

            infoTabControl.AddItem("Info");
            infoTabControl.AddItem("Modding");

            centerHeaderContainer.DetailsVisible.BindTo(DetailsVisible);
            DetailsVisible.ValueChanged += newValue => detailHeaderContainer.Alpha = newValue ? 0 : 1;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            coverInfoText.Colour = colours.CommunityUserGreen;

            infoTabControl.AccentColour = colours.CommunityUserGreen;
        }

        private User user;

        public User User
        {
            get => user;
            set
            {
                medalHeaderContainer.User = detailHeaderContainer.User = bottomHeaderContainer.User =
                    centerHeaderContainer.User = topHeaderContainer.User = user = value;
                updateDisplay();
            }
        }

        private void updateDisplay()
        {
            coverContainer.RemoveAll(d => d is UserCoverBackground);
            LoadComponentAsync(new UserCoverBackground(user)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
                OnLoadComplete = d => d.FadeInFromZero(200),
                Depth = float.MaxValue,
            }, coverContainer.Add);
        }

        public class HasTooltipContainer : Container, IHasTooltip
        {
            public string TooltipText { get; set; }
        }

        public class OverlinedInfoContainer : CompositeDrawable
        {
            private readonly Circle line;
            private readonly OsuSpriteText title, content;

            public string Title
            {
                set => title.Text = value;
            }

            public string Content
            {
                set => content.Text = value;
            }

            public Color4 LineColour
            {
                set => line.Colour = value;
            }

            public OverlinedInfoContainer(bool big = false, int minimumWidth = 60)
            {
                AutoSizeAxes = Axes.Both;
                InternalChild = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        line = new Circle
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 4,
                        },
                        title = new OsuSpriteText
                        {
                            Font = "Exo2.0-Bold",
                            TextSize = big ? 14 : 12,
                        },
                        content = new OsuSpriteText
                        {
                            Font = "Exo2.0-Light",
                            TextSize = big ? 40 : 18,
                        },
                        new Container //Add a minimum size to the FillFlowContainer
                        {
                            Width = minimumWidth,
                        }
                    }
                };
            }
        }
    }
}
