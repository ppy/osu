// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Submission
{
    public partial class BeatmapSubmissionOverlay : WizardOverlay
    {
        public BeatmapSubmissionOverlay()
            : base(OverlayColourScheme.Aquamarine)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            if (beatmap.Value.BeatmapSetInfo.OnlineID <= 0)
            {
                AddStep<ScreenContentPermissions>();
                AddStep<ScreenFrequentlyAskedQuestions>();
            }

            AddStep<ScreenSubmissionSettings>();

            Header.Title = BeatmapSubmissionStrings.BeatmapSubmissionTitle;
            Header.Description = BeatmapSubmissionStrings.BeatmapSubmissionDescription;
        }
    }
}
