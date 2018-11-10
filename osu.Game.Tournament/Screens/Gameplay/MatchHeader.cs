// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public class MatchHeader : Container
    {
        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, TextureStore textures)
        {
            RelativeSizeAxes = Axes.X;
            Height = 100;
            Children = new Drawable[]
            {
                new Sprite
                {
                    Y = 5,
                    Texture = textures.Get("game-screen-logo"),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One
                },
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

            private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();
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

            private void matchChanged(MatchPairing match)
            {
                currentTeamScore.UnbindBindings();
                currentTeamScore.BindTo(teamColour == TeamColour.Red ? match.Team1Score : match.Team2Score);

                currentTeam.UnbindBindings();
                currentTeam.BindTo(teamColour == TeamColour.Red ? match.Team1 : match.Team2);

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
                InternalChildren = new Drawable[]
                {
                    new TeamDisplay(team, teamColour == TeamColour.Red ? red : blue, teamColour != TeamColour.Red),
                    new ScoreDisplay(currentTeamScore, teamColour != TeamColour.Red, currentMatch.Value.PointsToWin)
                };
            }
        }

        private class ScoreDisplay : CompositeDrawable
        {
            private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();
            private readonly StarCounter counter;

            public ScoreDisplay(Bindable<int?> score, bool flip, int count)
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
                    Colour = new Color4(95, 41, 60, 255),
                };

                currentTeamScore.BindValueChanged(scoreChanged);
                currentTeamScore.BindTo(score);
            }

            private void scoreChanged(int? score) => counter.CountStars = score ?? 0;
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
                            Text = team?.FullName.ToUpper() ?? "???",
                            X = (flip ? -1 : 1) * 90,
                            Y = -10,
                            TextSize = 20,
                            Colour = colour,
                            Font = "Aquatico-Regular",
                            Origin = anchor,
                            Anchor = anchor,
                        },
                    }
                };
            }
        }

        private class RoundDisplay : CompositeDrawable
        {
            private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();

            public RoundDisplay()
            {
                CornerRadius = 10;
                Masking = true;
                Width = 200;
                Height = 20;
            }

            [BackgroundDependencyLoader]
            private void load(LadderInfo ladder)
            {
                currentMatch.BindValueChanged(matchChanged);
                currentMatch.BindTo(ladder.CurrentMatch);
            }

            private void matchChanged(MatchPairing match)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = new Color4(95, 41, 60, 255),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.White,
                        Text = match.Grouping.Value?.Name.Value ?? "Unknown Grouping",
                        Font = "Aquatico-Regular",
                        TextSize = 18,
                    },
                };
            }
        }
    }
}
