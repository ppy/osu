// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Overlays;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class RankedPlayMatchPanel : CompositeDrawable
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        private readonly RankedPlayRoomState state;

        private Drawable leftResultLight = null!;
        private Drawable rightResultLight = null!;
        private OsuSpriteText leftLifeText = null!;
        private OsuSpriteText rightLifeText = null!;

        public RankedPlayMatchPanel(RankedPlayRoomState state)
        {
            this.state = state;

            Width = 280;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = 10;
            BorderThickness = 2;
            BorderColour = colours.YellowDarker;

            (int UserId, RankedPlayUserInfo Info)[] users = state.Users.Select(kvp => (kvp.Key, kvp.Value)).ToArray();
            Task<APIUser?> leftUser = userLookupCache.GetUserAsync(users[0].UserId);
            Task<APIUser?> rightUser = userLookupCache.GetUserAsync(users[1].UserId);
            Task.WhenAll(leftUser, rightUser).WaitSafely();

            FillFlowContainer userLeft;
            FillFlowContainer userRight;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new BufferedContainer
                        {
                            Name = "Middle part",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            BackgroundColour = colourProvider.Background4.Opacity(0),
                            Children = new[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 0.5f,
                                    Masking = true,
                                    Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.7f), colourProvider.Background4.Opacity(0)),
                                    Child = new UserCoverBackground
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        User = leftUser.GetResultSafely()
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 0.5f,
                                    Masking = true,
                                    Colour = ColourInfo.GradientHorizontal(colourProvider.Background4.Opacity(0), Color4.White.Opacity(0.7f)),
                                    Child = new UserCoverBackground
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        User = rightUser.GetResultSafely()
                                    }
                                },
                                leftResultLight = new Container
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(0.4f, 3),
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = ColourInfo.GradientHorizontal(Color4.White, Color4.White.Opacity(0))
                                    }
                                },
                                rightResultLight = new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(0.4f, 3),
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0), Color4.White)
                                    },
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "vs",
                                    Font = OsuFont.GetFont(size: 50, weight: FontWeight.Bold),
                                    UseFullGlyphHeight = false,
                                    Colour = colourProvider.Light3,
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        userLeft = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Colour = Color4.White.Opacity(0.4f),
                                            Padding = new MarginPadding(5),
                                            Spacing = new Vector2(5),
                                            Children = new Drawable[]
                                            {
                                                new CircularContainer
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Size = new Vector2(25),
                                                    Masking = true,
                                                    Child = new UpdateableAvatar(leftUser.GetResultSafely())
                                                    {
                                                        RelativeSizeAxes = Axes.Both
                                                    }
                                                },
                                                new OsuSpriteText
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Text = leftUser.GetResultSafely()?.Username ?? "Unknown",
                                                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                                    UseFullGlyphHeight = false,
                                                },
                                            }
                                        },
                                        userRight = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Colour = Color4.White.Opacity(0.4f),
                                            Padding = new MarginPadding(5),
                                            Spacing = new Vector2(5),
                                            Children = new Drawable[]
                                            {
                                                new CircularContainer
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    Size = new Vector2(25),
                                                    Masking = true,
                                                    Child = new UpdateableAvatar(rightUser.GetResultSafely())
                                                    {
                                                        RelativeSizeAxes = Axes.Both
                                                    }
                                                },
                                                new OsuSpriteText
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    Text = rightUser.GetResultSafely()?.Username ?? "Unknown",
                                                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                                    UseFullGlyphHeight = false,
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            Name = "Bottom part",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background5
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(5),
                                    Child = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Children = new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Children = new Drawable[]
                                                {
                                                    new IconWithTooltip
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Size = new Vector2(12),
                                                        Icon = FontAwesome.Solid.Heart,
                                                        Colour = Color4.Red,
                                                        TooltipText = "Remaining Life"
                                                    },
                                                    leftLifeText = new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.CentreRight,
                                                        X = -15,
                                                        Colour = colourProvider.Foreground1,
                                                        Text = users[0].Info.Life.ToString("N0"),
                                                        UseFullGlyphHeight = false,
                                                    },
                                                    rightLifeText = new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.CentreLeft,
                                                        X = 15,
                                                        Colour = colourProvider.Foreground1,
                                                        Text = users[1].Info.Life.ToString("N0"),
                                                        UseFullGlyphHeight = false,
                                                    }
                                                },
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Children = new Drawable[]
                                                {
                                                    new IconWithTooltip
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Size = new Vector2(10),
                                                        Colour = colourProvider.Foreground1,
                                                        Icon = FontAwesome.Solid.Trophy,
                                                        TooltipText = "Rounds Won",
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.CentreRight,
                                                        X = -15,
                                                        Colour = colourProvider.Foreground1,
                                                        Text = users[0].Info.RoundsWon.ToString(),
                                                        UseFullGlyphHeight = false,
                                                    },
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.CentreLeft,
                                                        X = 15,
                                                        Colour = colourProvider.Foreground1,
                                                        Text = users[1].Info.RoundsWon.ToString(),
                                                        UseFullGlyphHeight = false,
                                                    }
                                                },
                                            }
                                        }
                                    }
                                },
                                new BufferedContainer(cachedFrameBuffer: true)
                                {
                                    Name = "Status pill",
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 10,
                                    CornerExponent = 6,
                                    Padding = new MarginPadding { Left = 10, Bottom = 10 },
                                    Margin = new MarginPadding { Left = -10, Bottom = -10 },
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colours.YellowDarker
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Margin = new MarginPadding { Horizontal = 8, Vertical = 5 },
                                            Colour = colourProvider.Background5,
                                            Text = "Completed",
                                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                                            UseFullGlyphHeight = false,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            bool leftWin = users[0].Info.Life > users[1].Info.Life;
            bool rightWin = users[1].Info.Life > users[0].Info.Life;
            bool isDraw = users[0].Info.Life == users[1].Info.Life;

            if (isDraw)
            {
                leftResultLight.Colour = colours.Yellow;
                rightResultLight.Colour = colours.Yellow;
            }
            else if (leftWin)
            {
                leftResultLight.Colour = colours.Green;
                rightResultLight.Colour = colours.Red;

                leftLifeText.Colour = userLeft.Colour = Color4.White;
                leftLifeText.Font = OsuFont.GetFont(weight: FontWeight.SemiBold);
            }
            else if (rightWin)
            {
                leftResultLight.Colour = colours.Red;
                rightResultLight.Colour = colours.Green;

                rightLifeText.Colour = userRight.Colour = Color4.White;
                rightLifeText.Font = OsuFont.GetFont(weight: FontWeight.SemiBold);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(750, Easing.OutQuint);
        }

        private partial class IconWithTooltip : SpriteIcon, IHasTooltip
        {
            public LocalisableString TooltipText { get; set; }
        }
    }
}
