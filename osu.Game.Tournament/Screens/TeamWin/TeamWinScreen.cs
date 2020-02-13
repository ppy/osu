// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Showcase;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.TeamWin
{
    public class TeamWinScreen : TournamentScreen, IProvideVideo
    {
        private Container mainContainer;

        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();
        private readonly Bindable<bool> currentCompleted = new Bindable<bool>();

        private TourneyVideo blueWinVideo;
        private TourneyVideo redWinVideo;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, Storage storage)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                blueWinVideo = new TourneyVideo(storage.GetStream(@"BG Team - Win Blue.m4v"))
                {
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                redWinVideo = new TourneyVideo(storage.GetStream(@"BG Team - Win Red.m4v"))
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                new TournamentLogo(false)
                {
                    Y = 40,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            currentCompleted.BindValueChanged(_ => update());
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            currentCompleted.UnbindBindings();
            currentCompleted.BindTo(match.NewValue.Completed);

            update();
        }

        private void update()
        {
            var match = currentMatch.Value;

            if (match.Winner == null)
            {
                mainContainer.Clear();
                return;
            }

            bool redWin = match.Winner == match.Team1.Value;
            redWinVideo.Alpha = redWin ? 1 : 0;
            blueWinVideo.Alpha = redWin ? 0 : 1;

            mainContainer.Children = new Drawable[]
            {
                new TeamWithPlayers(match.Winner, redWin)
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Height = 0.6f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                new RoundDisplay(match)
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.25f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                }
            };
        }

        private class RoundDisplay : CompositeDrawable
        {
            public RoundDisplay(TournamentMatch match)
            {
                var col = OsuColour.Gray(0.33f);

                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Colour = col,
                                Text = "WINNER",
                                Font = TournamentFont.GetFont(TournamentTypeface.Aquatico, 15, FontWeight.Regular),
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Colour = col,
                                Text = match.Round.Value?.Name.Value ?? "Unknown Round",
                                Font = TournamentFont.GetFont(TournamentTypeface.Aquatico, 50, FontWeight.Light),
                                Spacing = new Vector2(10, 0),
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Colour = col,
                                Text = match.Date.Value.ToUniversalTime().ToString("dd MMMM HH:mm UTC"),
                                Font = TournamentFont.GetFont(TournamentTypeface.Aquatico, 20, FontWeight.Light),
                            },
                        }
                    }
                };
            }
        }

        private class TeamWithPlayers : CompositeDrawable
        {
            private readonly Color4 red = new Color4(129, 68, 65, 255);
            private readonly Color4 blue = new Color4(41, 91, 97, 255);

            public TeamWithPlayers(TournamentTeam team, bool left = false)
            {
                var colour = left ? red : blue;
                InternalChildren = new Drawable[]
                {
                    new TeamDisplay(team, left ? "Team Red" : "Team Blue", colour)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(0, 5),
                        Padding = new MarginPadding(20),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.Both,
                    },
                };
            }

            private class TeamDisplay : DrawableTournamentTeam
            {
                public TeamDisplay(TournamentTeam team, string teamName, Color4 colour)
                    : base(team)
                {
                    AutoSizeAxes = Axes.Both;

                    Flag.Anchor = Flag.Origin = Anchor.TopCentre;
                    Flag.RelativeSizeAxes = Axes.None;
                    Flag.Size = new Vector2(300, 200);
                    Flag.Scale = new Vector2(0.4f);
                    Flag.Margin = new MarginPadding { Bottom = 20 };

                    InternalChild = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            Flag,
                            new OsuSpriteText
                            {
                                Text = team?.FullName.Value.ToUpper() ?? "???",
                                Font = TournamentFont.GetFont(TournamentTypeface.Aquatico, 40, FontWeight.Light),
                                Colour = Color4.Black,
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                            new OsuSpriteText
                            {
                                Text = teamName.ToUpper(),
                                Font = OsuFont.GetFont(size: 20),
                                Colour = colour,
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            }
                        }
                    };
                }
            }
        }
    }
}
