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
    public partial class TeamScoreDisplay : CompositeDrawable
    {
        private readonly TeamColour teamColour;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();
        private readonly Bindable<TournamentTeam?> currentTeam = new Bindable<TournamentTeam?>();
        private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();

        private TeamDisplay? teamDisplay;

        public bool ShowScore
        {
            get => teamDisplay?.ShowScore ?? false;
            set
            {
                if (teamDisplay != null)
                {
                    teamDisplay.ShowScore = value;
                }
            }
        }

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
            currentMatch.BindValueChanged(matchChanged);

            currentTeam.BindValueChanged(teamChanged);

            updateMatch();
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            currentTeamScore.UnbindBindings();
            currentTeam.UnbindBindings();

            Scheduler.AddOnce(updateMatch);
        }

        private void updateMatch()
        {
            var match = currentMatch.Value;

            if (match != null)
            {
                match.StartMatch();

                currentTeamScore.BindTo(teamColour == TeamColour.Red ? match.Team1Score : match.Team2Score);
                currentTeam.BindTo(teamColour == TeamColour.Red ? match.Team1 : match.Team2);
            }

            // team may change to same team, which means score is not in a good state.
            // thus we handle this manually.
            currentTeam.TriggerChange();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    if (currentTeamScore.Value < currentMatch.Value?.PointsToWin)
                        currentTeamScore.Value++;
                    return true;

                case MouseButton.Right:
                    if (currentTeamScore.Value > 0)
                        currentTeamScore.Value--;
                    return true;
            }

            return base.OnMouseDown(e);
        }

        private void teamChanged(ValueChangedEvent<TournamentTeam?> team)
        {
            bool wasShowingScores = teamDisplay?.ShowScore ?? false;

            InternalChildren = new Drawable[]
            {
                teamDisplay = new TeamDisplay(team.NewValue, teamColour, currentTeamScore, currentMatch.Value?.PointsToWin ?? 0),
            };

            teamDisplay.ShowScore = wasShowingScores;
        }
    }
}
