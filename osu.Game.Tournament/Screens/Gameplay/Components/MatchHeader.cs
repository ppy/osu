// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Showcase;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class MatchHeader : Container
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 95;
            Children = new Drawable[]
            {
                new TournamentLogo(),
                new RoundDisplay
                {
                    Y = 10,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                },
                new TeamScoreDisplay(TeamColour.Red)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                new TeamScoreDisplay(TeamColour.Blue)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
            };
        }

        private class TeamScoreDisplay : CompositeDrawable
        {
            private readonly TeamColour teamColour;

            private readonly Color4 red = new Color4(129, 68, 65, 255);
            private readonly Color4 blue = new Color4(41, 91, 97, 255);

            private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();
            private readonly Bindable<TournamentTeam> currentTeam = new Bindable<TournamentTeam>();
            private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();

            public TeamScoreDisplay(TeamColour teamColour)
            {
                this.teamColour = teamColour;

                RelativeSizeAxes = Axes.Y;
                Width = 300;
            }

            [BackgroundDependencyLoader]
            private void load(LadderInfo ladder)
            {
                currentMatch.BindValueChanged(matchChanged);
                currentMatch.BindTo(ladder.CurrentMatch);
            }

            private void matchChanged(ValueChangedEvent<TournamentMatch> match)
            {
                currentTeamScore.UnbindBindings();
                currentTeamScore.BindTo(teamColour == TeamColour.Red ? match.NewValue.Team1Score : match.NewValue.Team2Score);

                currentTeam.UnbindBindings();
                currentTeam.BindTo(teamColour == TeamColour.Red ? match.NewValue.Team1 : match.NewValue.Team2);

                // team may change to same team, which means score is not in a good state.
                // thus we handle this manually.
                teamChanged(currentTeam.Value);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                switch (e.Button)
                {
                    case MouseButton.Left:
                        if (currentTeamScore.Value < currentMatch.Value.PointsToWin)
                            currentTeamScore.Value++;
                        return true;

                    case MouseButton.Right:
                        if (currentTeamScore.Value > 0)
                            currentTeamScore.Value--;
                        return true;
                }

                return base.OnMouseDown(e);
            }

            private void teamChanged(TournamentTeam team)
            {
                var colour = teamColour == TeamColour.Red ? red : blue;
                var flip = teamColour != TeamColour.Red;

                InternalChildren = new Drawable[]
                {
                    new TeamDisplay(team, colour, flip),
                    new TeamScore(currentTeamScore, flip, currentMatch.Value.PointsToWin)
                    {
                        Colour = colour
                    }
                };
            }
        }

        private class TeamScore : CompositeDrawable
        {
            private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();
            private readonly StarCounter counter;

            public TeamScore(Bindable<int?> score, bool flip, int count)
            {
                var anchor = flip ? Anchor.CentreRight : Anchor.CentreLeft;

                Anchor = anchor;
                Origin = anchor;

                InternalChild = counter = new StarCounter(count)
                {
                    Anchor = anchor,
                    X = (flip ? -1 : 1) * 90,
                    Y = 5,
                    Scale = flip ? new Vector2(-1, 1) : Vector2.One,
                };

                currentTeamScore.BindValueChanged(scoreChanged);
                currentTeamScore.BindTo(score);
            }

            private void scoreChanged(ValueChangedEvent<int?> score) => counter.CountStars = score.NewValue ?? 0;
        }

        private class TeamDisplay : DrawableTournamentTeam
        {
            public TeamDisplay(TournamentTeam team, Color4 colour, bool flip)
                : base(team)
            {
                RelativeSizeAxes = Axes.Both;

                var anchor = flip ? Anchor.CentreRight : Anchor.CentreLeft;

                Anchor = Origin = anchor;

                Flag.Anchor = Flag.Origin = anchor;
                Flag.RelativeSizeAxes = Axes.None;
                Flag.Size = new Vector2(60, 40);
                Flag.Margin = new MarginPadding(20);

                InternalChild = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Flag,
                        new OsuSpriteText
                        {
                            Text = team?.FullName.Value.ToUpper() ?? "???",
                            X = (flip ? -1 : 1) * 90,
                            Y = -10,
                            Colour = colour,
                            Font = TournamentFont.GetFont(typeface: TournamentTypeface.Aquatico, weight: FontWeight.Regular, size: 20),
                            Origin = anchor,
                            Anchor = anchor,
                        },
                    }
                };
            }
        }

        private class RoundDisplay : CompositeDrawable
        {
            private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();

            public RoundDisplay()
            {
                Width = 200;
                Height = 20;
            }

            [BackgroundDependencyLoader]
            private void load(LadderInfo ladder)
            {
                currentMatch.BindValueChanged(matchChanged);
                currentMatch.BindTo(ladder.CurrentMatch);
            }

            private void matchChanged(ValueChangedEvent<TournamentMatch> match)
            {
                InternalChildren = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.White,
                        Text = match.NewValue.Round.Value?.Name.Value ?? "Unknown Round",
                        Font = TournamentFont.GetFont(typeface: TournamentTypeface.Aquatico, weight: FontWeight.Regular, size: 18),
                    },
                };
            }
        }
    }
}
