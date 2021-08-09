// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Settings.Sections.BeatmapDownloader
{
    public class BeatmapDownloaderSettings : SettingsSubsection
    {

        protected override LocalisableString Header => "Downloader Settings";

        private SettingsDropdown<RulesetInfo> rulesetInfoDropdown;
        private Bindable<int> ruleset { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, RulesetStore rulesets)
        {
            ruleset = config.GetBindable<int>(OsuSetting.BeatmapDownloadRuleset);

            Add(new SettingsSlider<double, StarsSlider>
            {
                LabelText = "Download Sets that have atleast",
                Current = config.GetBindable<double>(OsuSetting.BeatmapDownloadMinimumStarRating),
                KeyboardStep = 0.1f,
            });

            Add(new SettingsEnumDropdown<BeatmapListing.SearchCategory>
            {
                LabelText = "Category",
                Current = config.GetBindable<BeatmapListing.SearchCategory>(OsuSetting.BeatmapDownloadSearchCategory),
            });

            Add(rulesetInfoDropdown = new SettingsDropdown<RulesetInfo>
            {
                LabelText = "Ruleset",
                Items = rulesets.AvailableRulesets,
            });

            rulesetInfoDropdown.SettingChanged += () =>
            {
                ruleset.Value = rulesetInfoDropdown.Current.Value.ID ?? 0;
            };

            Add(new SettingsCheckbox
            {
                LabelText = "Download at launch",
                Current = config.GetBindable<bool>(OsuSetting.BeatmapDownloadStartUp),
            });
        }
    }

    internal class StarsSlider : OsuSliderBar<double>
    {
        public override LocalisableString TooltipText => Current.Value.ToString(@"0.## stars");
    }
}
