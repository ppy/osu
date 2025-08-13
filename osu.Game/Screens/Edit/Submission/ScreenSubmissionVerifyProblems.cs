// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Edit.Submission
{
    [LocalisableDescription(typeof(BeatmapSubmissionStrings), nameof(BeatmapSubmissionStrings.VerifyProblems))]
    public partial class ScreenSubmissionVerifyProblems : WizardScreen
    {
        private SubmissionIssueTable table = null!;

        [Resolved]
        private BindableList<Issue> submissionProblemIssues { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Content.AddRange(new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE, weight: FontWeight.Bold))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Colour = colours.Orange1,
                    Text = BeatmapSubmissionStrings.VerifyProblemsDisclaimer,
                },
                table = new SubmissionIssueTable { RelativeSizeAxes = Axes.X, Height = 300 },
            });
            table.SetIssues(submissionProblemIssues);
        }
    }
}
