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

namespace osu.Game.Tournament.Screens.TeamIntro
{
    public class TeamIntroScreen : TournamentScreen, IProvideVideo
    {
        private Container mainContainer;

        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("teamintro")
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(LadderInfo.CurrentMatch);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            if (match.NewValue == null)
            {
                mainContainer.Clear();
                return;
            }

            mainContainer.Children = new Drawable[]
            {
                new TeamWithPlayers(match.NewValue.Team1.Value, true)
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Height = 0.6f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight
                },
                new TeamWithPlayers(match.NewValue.Team2.Value)
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Height = 0.6f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft
                },
                new RoundDisplay(match.NewValue)
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.25f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Y = 180,
                }
            };
        }

        private class RoundDisplay : CompositeDrawable
        {
            public RoundDisplay(TournamentMatch match)
            {
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
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Colour = OsuColour.Gray(0.33f),
                                Text = match.Round.Value?.Name.Value ?? "未知回合",
                                Font = OsuFont.Torus.With(size: 26, weight: FontWeight.Light)
                            },
                        }
                    }
                };
            }
        }

        private class TeamWithPlayers : CompositeDrawable
        {
            public TeamWithPlayers(TournamentTeam team, bool left = false)
            {
                FillFlowContainer players;
                var colour = left ? TournamentGame.COLOUR_RED : TournamentGame.COLOUR_BLUE;
                InternalChildren = new Drawable[]
                {
                    new TeamDisplay(team)
                    {
                        Anchor = left ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.Both,
                        X = (left ? -1 : 1) * 0.3145f,
                        Y = -0.077f,
                    },
                    players = new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(0, 5),
                        Padding = new MarginPadding(20),
                        Anchor = left ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = left ? Anchor.CentreRight : Anchor.CentreLeft,
                        RelativePositionAxes = Axes.Both,
                        X = (left ? -1 : 1) * 0.58f,
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

            private class TeamDisplay : DrawableTournamentTeam
            {
                public TeamDisplay(TournamentTeam team)
                    : base(team)
                {
                    AutoSizeAxes = Axes.Both;

                    Flag.Anchor = Flag.Origin = Anchor.TopCentre;
                    Flag.RelativeSizeAxes = Axes.None;
                    Flag.Size = new Vector2(300, 200);
                    Flag.Scale = new Vector2(0.32f);

                    InternalChild = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(160),
                        Children = new Drawable[]
                        {
                            Flag,
                            new TournamentSpriteText
                            {
                                Text = team?.FullName.Value ?? "???",
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Regular),
                                Colour = OsuColour.Gray(0.2f),
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                            },
                        }
                    };
                }
            }
        }
    }
}
