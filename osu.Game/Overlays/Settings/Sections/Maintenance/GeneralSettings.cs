// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
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

        private SystemFileImportComponent systemFileImport = null!;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, GameHost host, IPerformFromScreenRunner? performer)
        {
            Add(systemFileImport = new SystemFileImportComponent(game, host));

            AddRange(new Drawable[]
            {
                new SettingsButton
                {
                    Text = DebugSettingsStrings.ImportFiles,
                    Action = () =>
                    {
                        if (systemFileImport.PresentIfAvailable())
                            return;

                        performer?.PerformFromScreen(menu => menu.Push(new FileImportScreen()));
                    },
                },
                new SettingsButton
                {
                    Text = DebugSettingsStrings.RunLatencyCertifier,
                    Action = () => performer?.PerformFromScreen(menu => menu.Push(new LatencyCertifierScreen()))
                }
            });
        }
    }
}
