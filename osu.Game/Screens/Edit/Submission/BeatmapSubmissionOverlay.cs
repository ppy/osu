// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Submission
{
    public partial class BeatmapSubmissionOverlay : WizardOverlay
    {
        [Cached]
        private readonly BindableList<BeatmapVerifier.ScopedIssue> submissionProblemIssues = new BindableList<BeatmapVerifier.ScopedIssue>();

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

            submissionProblemIssues.AddRange(BeatmapVerifier.RunForBeatmapSet(beatmap.Value, beatmapManager)
                                                            .Where(i => i.Issue.Template.Type == IssueType.Problem));

            if (submissionProblemIssues.Count > 0)
                AddStep<ScreenSubmissionVerifyProblems>();

            AddStep<ScreenSubmissionSettings>();

            Header.Title = BeatmapSubmissionStrings.BeatmapSubmissionTitle;
            Header.Description = BeatmapSubmissionStrings.BeatmapSubmissionDescription;
        }
    }
}
