// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking
{
    public partial class SoloResultsScreen : ResultsScreen
    {
        public readonly IBindableList<ScoreInfo> Scores = new BindableList<ScoreInfo>();

        private bool hasLoaded;

        public SoloResultsScreen(ScoreInfo score)
            : base(score)
        {
        }

        protected override void PopulateScorePanelList()
        {
            Scores.BindCollectionChanged((_, _) => Scheduler.AddOnce(addScores), true);
        }

        private void addScores()
        {
            if (!Scores.Any() || hasLoaded)
                return;

            foreach (var s in Scores)
            {
                if (Score == null || s.ID != Score.ID)
                    AddScore(s);
            }

            hasLoaded = true;
        }
    }
}
