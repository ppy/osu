// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
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
                blueWinVideo = new TourneyVideo(storage.GetStream(@"videos/teamwin-blue.m4v"))
                {
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                redWinVideo = new TourneyVideo(storage.GetStream(@"videos/teamwin-red.m4v"))
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
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
                new TeamFlagDisplay(match.Winner)
                {
                    Size = new Vector2(300, 200),
                    Scale = new Vector2(0.5f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = -387,
                },
                new TournamentSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(78, -70),
                    Colour = OsuColour.Gray(0.33f),
                    Text = match.Round.Value?.Name.Value ?? "Unknown Round",
                    Font = OsuFont.Torus.With(size: 30, weight: FontWeight.Regular)
                },
                new TeamWithPlayers(match.Winner, redWin)
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Height = 0.6f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(78, 0),
                },
            };
        }

        private class TeamWithPlayers : CompositeDrawable
        {
            public TeamWithPlayers(TournamentTeam team, bool left = false)
            {
                FillFlowContainer players;

                var colour = left ? TournamentGame.COLOUR_RED : TournamentGame.COLOUR_BLUE;
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Text = "WINNER",
                                Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                                Colour = Color4.Black,
                            },
                            new TournamentSpriteText
                            {
                                Text = team?.FullName.Value ?? "???",
                                Font = OsuFont.Torus.With(size: 30, weight: FontWeight.SemiBold),
                                Colour = Color4.Black,
                            },
                            players = new FillFlowContainer
                            {
                                Direction = FillDirection.Vertical,
                                AutoSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Top = 10 },
                            },
                        }
                    },
                };

                if (team != null)
                {
                    foreach (var p in team.Players)
                    {
                        players.Add(new TournamentSpriteText
                        {
                            Text = p.Username,
                            Font = OsuFont.Torus.With(size: 24),
                            Colour = colour,
                            Anchor = left ? Anchor.CentreRight : Anchor.CentreLeft,
                            Origin = left ? Anchor.CentreRight : Anchor.CentreLeft,
                        });
                    }
                }
            }
        }

        private class TeamFlagDisplay : DrawableTournamentTeam
        {
            public TeamFlagDisplay(TournamentTeam team)
                : base(team)
            {
                InternalChildren = new Drawable[]
                {
                    Flag
                };
            }
        }
    }
}
