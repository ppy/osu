// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class TournamentMatchScoreDisplay : MatchScoreDisplay
    {
        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc, LadderInfo ladderInfo)
        {
            // Create BindableDouble instances to hold the adjusted scores
            var team1ScoreDouble = new BindableDouble();
            var team2ScoreDouble = new BindableDouble();

            // Bind ipc scores to these BindableDouble instances with conversion
            ipc.Score1.BindValueChanged(score => team1ScoreDouble.Value = score.NewValue);
            ipc.Score2.BindValueChanged(score => team2ScoreDouble.Value = score.NewValue);

            // Bind these converted scores to the Team1Score and Team2Score
            Team1Score.BindTo(team1ScoreDouble);
            Team2Score.BindTo(team2ScoreDouble);

            // Bind multipliers
            var redMultiplier = ladderInfo.RedMultiplier;
            var blueMultiplier = ladderInfo.BlueMultiplier;

            // Bind the adjusted scores
            Team1Score.BindValueChanged(score => updateAdjustedScore(score.NewValue, redMultiplier.Value, adjustedTeam1Score));
            redMultiplier.BindValueChanged(multiplier => updateAdjustedScore(Team1Score.Value, multiplier.NewValue, adjustedTeam1Score));

            Team2Score.BindValueChanged(score => updateAdjustedScore(score.NewValue, blueMultiplier.Value, adjustedTeam2Score));
            blueMultiplier.BindValueChanged(multiplier => updateAdjustedScore(Team2Score.Value, multiplier.NewValue, adjustedTeam2Score));

            // Update the display with the adjusted scores
            adjustedTeam1Score.BindValueChanged(score => displayTeam1Score(score.NewValue), true);
            adjustedTeam2Score.BindValueChanged(score => displayTeam2Score(score.NewValue), true);
        }

        private readonly Bindable<double> adjustedTeam1Score = new Bindable<double>();
        private readonly Bindable<double> adjustedTeam2Score = new Bindable<double>();

        private void updateAdjustedScore(double rawScore, float multiplier, Bindable<double> adjustedScore)
        {
            adjustedScore.Value = rawScore * multiplier;
        }

        private void displayTeam1Score(double score)
        {
            Team1Score.Value = score;
        }

        private void displayTeam2Score(double score)
        {
            Team2Score.Value = score;
        }
    }
}
