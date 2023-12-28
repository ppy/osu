// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Configuration
{
    /// <summary>
    /// Tracks the local user's average hit error during the ongoing play session.
    /// </summary>
    [Cached]
    public partial class SessionAverageHitErrorTracker : Component
    {
        public IBindableList<double> AverageHitErrorHistory => averageHitErrorHistory;
        private readonly BindableList<double> averageHitErrorHistory = new BindableList<double>();

        private readonly Bindable<ScoreInfo?> latestScore = new Bindable<ScoreInfo?>();

        [BackgroundDependencyLoader]
        private void load(SessionStatics statics)
        {
            statics.BindWith(Static.LastLocalUserScore, latestScore);
            latestScore.BindValueChanged(score => calculateAverageHitError(score.NewValue), true);
        }

        private void calculateAverageHitError(ScoreInfo? newScore)
        {
            if (newScore == null)
                return;

            if (newScore.Mods.Any(m => !m.UserPlayable || m is IHasNoTimedInputs))
                return;

            if (newScore.HitEvents.Count < 10)
                return;

            if (newScore.HitEvents.CalculateAverageHitError() is not double averageError)
                return;

            // keep a sane maximum number of entries.
            if (averageHitErrorHistory.Count >= 50)
                averageHitErrorHistory.RemoveAt(0);
            averageHitErrorHistory.Add(averageError);
        }

        public void ClearHistory() => averageHitErrorHistory.Clear();
    }
}
