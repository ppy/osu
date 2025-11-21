// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public partial class QuickActionSettings : SettingsSubsection
    {
        [Resolved(CanBeNull = true)]
        private FirstRunSetupOverlay? firstRunSetupOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuGame? game { get; set; }

        protected override LocalisableString Header => GeneralSettingsStrings.QuickActionsHeader;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddRange(new Drawable[]
            {
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.RunSetupWizard,
                    Keywords = new[] { @"first run", @"initial", @"getting started", @"import", @"tutorial", @"recommended beatmaps" },
                    TooltipText = FirstRunSetupOverlayStrings.FirstRunSetupDescription,
                    Action = () => firstRunSetupOverlay?.Show(),
                },
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.LearnMoreAboutLazer,
                    TooltipText = GeneralSettingsStrings.LearnMoreAboutLazerTooltip,
                    BackgroundColour = colours.YellowDark,
                    Action = () => game?.ShowWiki(@"Help_centre/Upgrading_to_lazer")
                },
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.ReportIssue,
                    TooltipText = GeneralSettingsStrings.ReportIssueTooltip,
                    BackgroundColour = colours.YellowDarker,
                    Action = () => game?.OpenUrlExternally(@"https://osu.ppy.sh/community/forums/topics/create?forum_id=5", LinkWarnMode.NeverWarn)
                },
            });
        }
    }
}
