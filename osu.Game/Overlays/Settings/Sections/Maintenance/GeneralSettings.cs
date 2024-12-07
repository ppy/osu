// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Localisation;
using osu.Game.Screens;
using osu.Game.Screens.Import;
using osu.Game.Screens.Utility;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class GeneralSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.General;

        [BackgroundDependencyLoader]
        private void load(IPerformFromScreenRunner? performer)
        {
            // the screen does not look appropriate on iOS with system file selection implemented,
            // and functionality is already provided by opening said files externally.
            if (RuntimeInfo.OS != RuntimeInfo.Platform.iOS)
            {
                Add(new SettingsButton
                {
                    Text = DebugSettingsStrings.ImportFiles,
                    Action = () => performer?.PerformFromScreen(menu => menu.Push(new FileImportScreen()))
                });
            }

            Add(new SettingsButton
            {
                Text = DebugSettingsStrings.RunLatencyCertifier,
                Action = () => performer?.PerformFromScreen(menu => menu.Push(new LatencyCertifierScreen()))
            });
        }
    }
}
