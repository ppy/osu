// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Localisation;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Submission
{
    public partial class BeatmapSubmissionOverlay : WizardOverlay
    {
        [Cached]
        private readonly BindableList<Issue> submissionProblemIssues = new BindableList<Issue>();

        public BeatmapSubmissionOverlay()
            : base(OverlayColourScheme.Aquamarine)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, BeatmapManager beatmapManager)
        {
            if (beatmap.Value.BeatmapSetInfo.OnlineID <= 0)
            {
                AddStep<ScreenContentPermissions>();
                AddStep<ScreenFrequentlyAskedQuestions>();
            }

            // Run verify checks and insert a step when there are problems
            var generalVerifier = new BeatmapVerifier();
            var rulesetVerifier = beatmap.Value.BeatmapInfo.Ruleset.CreateInstance().CreateBeatmapVerifier();

            var interpretedDifficulty = StarDifficulty.GetDifficultyRating(beatmap.Value.BeatmapInfo.StarRating);

            var context = BeatmapVerifierContext.Create(
                beatmap.Value.GetPlayableBeatmap(beatmap.Value.BeatmapInfo.Ruleset),
                beatmap.Value,
                interpretedDifficulty,
                beatmapManager
            );

            var issues = generalVerifier.Run(context);

            if (rulesetVerifier != null)
                issues = issues.Concat(rulesetVerifier.Run(context));

            submissionProblemIssues.Clear();
            submissionProblemIssues.AddRange(issues.Where(i => i.Template.Type == IssueType.Problem));

            if (submissionProblemIssues.Count > 0)
                AddStep<ScreenSubmissionVerifyProblems>();

            AddStep<ScreenSubmissionSettings>();

            Header.Title = BeatmapSubmissionStrings.BeatmapSubmissionTitle;
            Header.Description = BeatmapSubmissionStrings.BeatmapSubmissionDescription;
        }
    }
}
