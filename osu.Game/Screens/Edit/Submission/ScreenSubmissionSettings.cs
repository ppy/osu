// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Submission
{
    [LocalisableDescription(typeof(BeatmapSubmissionStrings), nameof(BeatmapSubmissionStrings.SubmissionSettings))]
    public partial class ScreenSubmissionSettings : WizardScreen
    {
        private readonly BindableBool notifyOnDiscussionReplies = new BindableBool();
        private readonly BindableBool loadInBrowserAfterSubmission = new BindableBool();

        public override LocalisableString? NextStepText => BeatmapSubmissionStrings.ConfirmSubmission;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager, OsuColour colours, BeatmapSubmissionSettings settings)
        {
            configManager.BindWith(OsuSetting.EditorSubmissionNotifyOnDiscussionReplies, notifyOnDiscussionReplies);
            configManager.BindWith(OsuSetting.EditorSubmissionLoadInBrowserAfterSubmission, loadInBrowserAfterSubmission);

            Content.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new FormEnumDropdown<BeatmapSubmissionTarget>
                    {
                        RelativeSizeAxes = Axes.X,
                        Caption = BeatmapSubmissionStrings.BeatmapSubmissionTargetCaption,
                        Current = settings.Target,
                    },
                    new FormCheckBox
                    {
                        Caption = BeatmapSubmissionStrings.NotifyOnDiscussionReplies,
                        Current = notifyOnDiscussionReplies,
                    },
                    new FormCheckBox
                    {
                        Caption = BeatmapSubmissionStrings.LoadInBrowserAfterSubmission,
                        Current = loadInBrowserAfterSubmission,
                    },
                    new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE, weight: FontWeight.Bold))
                    {
                        RelativeSizeAxes = Axes.X,
                        Colour = colours.Orange1,
                        Text = BeatmapSubmissionStrings.LegacyExportDisclaimer,
                        Padding = new MarginPadding { Top = 20 }
                    },
                }
            });
        }
    }
}
