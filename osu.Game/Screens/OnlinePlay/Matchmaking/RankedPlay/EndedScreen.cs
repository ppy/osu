// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class EndedScreen : RankedPlaySubScreen
    {
        /// <summary>
        /// Invoked when the user requests to exit this screen.
        /// </summary>
        public Action<bool>? ExitRequested { get; init; }

        protected override LocalisableString StageHeading => "Results";
        protected override LocalisableString StageCaption => string.Empty;

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        private OsuSpriteText titleText = null!;
        private Drawable titleSeparator = null!;
        private OsuTextFlowContainer localRatingText = null!;
        private OsuTextFlowContainer opponentRatingText = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            CenterColumn.Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(20),
                Children = new[]
                {
                    titleText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = "VICTORY",
                        Font = OsuFont.Torus.With(size: 100, weight: FontWeight.SemiBold),
                        UseFullGlyphHeight = false,
                        Colour = colours.Green1,
                    },
                    titleSeparator = new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Colour = colours.Green1
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(2),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = OsuGame.SHEAR,
                                Masking = true,
                                CornerRadius = 8,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black,
                                        Alpha = 0.5f
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding(10),
                                        Shear = -OsuGame.SHEAR,
                                        Children = new Drawable[]
                                        {
                                            localRatingText = new OsuTextFlowContainer(s => s.Font = OsuFont.Style.Heading1)
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y
                                            }
                                        }
                                    }
                                }
                            },
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = OsuGame.SHEAR,
                                Masking = true,
                                CornerRadius = 8,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black,
                                        Alpha = 0.5f
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding(10),
                                        Shear = -OsuGame.SHEAR,
                                        Children = new Drawable[]
                                        {
                                            opponentRatingText = new OsuTextFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            new ShearedButton
                            {
                                Width = 100,
                                Text = "Quit",
                                Action = () => ExitRequested?.Invoke(false),
                                DarkerColour = colours.Red3,
                                LighterColour = colours.Red4,
                            },
                            new ShearedButton
                            {
                                Width = 200,
                                Text = "Play Again",
                                Action = () => ExitRequested?.Invoke(true),
                                DarkerColour = colours.Green3,
                                LighterColour = colours.Green4,
                            },
                        }
                    }
                }
            };

            RankedPlayUserInfo localUser = matchInfo.RoomState.Users[Client.LocalUser!.UserID];
            RankedPlayUserInfo otherUser = matchInfo.RoomState.Users.Values.Single(u => u != localUser);

            if (matchInfo.RoomState.WinningUserId == null)
            {
                titleText.Text = "DRAW";
                titleText.Colour = titleSeparator.Colour = colours.Orange1;
            }
            else if (matchInfo.RoomState.WinningUserId == Client.LocalUser!.UserID)
            {
                titleText.Text = "VICTORY";
                titleText.Colour = titleSeparator.Colour = colours.Green1;
            }
            else
            {
                titleText.Text = "DEFEAT";
                titleText.Colour = titleSeparator.Colour = colours.Red1;
            }

            localRatingText.AddText("Your Rating: ", s => s.Font = OsuFont.Style.Heading1.With(weight: FontWeight.Regular));
            localRatingText.AddText(localUser.RatingAfter.ToString("N0"), s => s.Font = OsuFont.Style.Heading1);
            localRatingText.AddText($" ({localUser.RatingAfter - localUser.Rating:+0;-0;+0})", s =>
            {
                s.Font = OsuFont.Style.Caption1;
                s.Colour = localUser.RatingAfter >= localUser.Rating ? colours.GreenDark : colours.RedDark;
            });

            opponentRatingText.AddText("Opponent Rating: ", s => s.Font = OsuFont.Style.Heading1.With(weight: FontWeight.Regular));
            opponentRatingText.AddText(otherUser.RatingAfter.ToString("N0"), s => s.Font = OsuFont.Style.Heading1);
            opponentRatingText.AddText($" ({otherUser.RatingAfter - otherUser.Rating:+0;-0;+0})", s =>
            {
                s.Font = OsuFont.Style.Caption1;
                s.Colour = otherUser.RatingAfter >= otherUser.Rating ? colours.GreenDark : colours.RedDark;
            });
        }
    }
}
