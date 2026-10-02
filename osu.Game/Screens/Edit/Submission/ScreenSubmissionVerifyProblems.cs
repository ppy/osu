// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Submission
{
    [LocalisableDescription(typeof(BeatmapSubmissionStrings), nameof(BeatmapSubmissionStrings.VerifyProblems))]
    public partial class ScreenSubmissionVerifyProblems : WizardScreen
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuGame? game)
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

                new RoundedButton
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 300,
                    Text = BeatmapSubmissionStrings.OpenBeatmapVerifier,
                    Action = () =>
                    {
                        game?.PerformFromScreen(s =>
                        {
                            if (s is Editor editor)
                                editor.Mode.Value = EditorScreenMode.Verify;
                        }, [typeof(Editor)]);
                    },
                },

                new SubmissionIssueTable { RelativeSizeAxes = Axes.X, Height = 350 },
            });
        }
    }
}
