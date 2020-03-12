// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Tournament.Models;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class TeamScoreDisplay : CompositeDrawable
    {
        private readonly TeamColour teamColour;

        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();
        private readonly Bindable<TournamentTeam> currentTeam = new Bindable<TournamentTeam>();
        private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();

        private TeamDisplay teamDisplay;

        public bool ShowScore { set => teamDisplay.ShowScore = value; }

        public TeamScoreDisplay(TeamColour teamColour)
        {
            this.teamColour = teamColour;

            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindTo(ladder.CurrentMatch);
            currentMatch.BindValueChanged(matchChanged, true);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            currentTeamScore.UnbindBindings();
            currentTeam.UnbindBindings();

            if (match.NewValue != null)
            {
                currentTeamScore.BindTo(teamColour == TeamColour.Red ? match.NewValue.Team1Score : match.NewValue.Team2Score);
                currentTeam.BindTo(teamColour == TeamColour.Red ? match.NewValue.Team1 : match.NewValue.Team2);
            }

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
                teamDisplay = new TeamDisplay(team, teamColour, currentTeamScore, currentMatch.Value?.PointsToWin ?? 0),
            };
        }
    }
}
