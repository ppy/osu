// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Submission
{
    [LocalisableDescription(typeof(BeatmapSubmissionStrings), nameof(BeatmapSubmissionStrings.ContentPermissions))]
    public partial class ScreenContentPermissions : WizardScreen
    {
        [BackgroundDependencyLoader]
        private void load(OsuGame? game)
        {
            Content.AddRange(new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = BeatmapSubmissionStrings.ContentPermissionsDisclaimer,
                },
                new RoundedButton
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 450,
                    Text = BeatmapSubmissionStrings.CheckContentUsageGuidelines,
                    Action = () => game?.ShowWiki(@"Rules/Content_usage_permissions"),
                },
            });
        }

        public override LocalisableString? NextStepText => BeatmapSubmissionStrings.ContentPermissionsAcknowledgement;
    }
}
