// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.Maintenance;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public partial class InstallationSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GeneralSettingsStrings.InstallationHeader;

        [Resolved]
        private OsuGame? game { get; set; }

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            Add(new SettingsButtonV2
            {
                Text = GeneralSettingsStrings.OpenOsuFolder,
                Keywords = new[] { @"logs", @"files", @"access", "directory" },
                Action = () => storage.PresentExternally(),
            });

            Add(new DangerousSettingsButtonV2
            {
                Text = GeneralSettingsStrings.ChangeFolderLocation,
                Action = () => game?.PerformFromScreen(menu => menu.Push(new MigrationSelectScreen()))
            });

            // This is a temporary stand-in until we have a setup for `SettingsItemV2` which can handle buttons.
            Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = SettingsPanel.CONTENT_PADDING,
                Children = new Drawable[]
                {
                    new SettingsNote
                    {
                        RelativeSizeAxes = Axes.X,
                        Margin = new MarginPadding { Top = -SettingsSection.ITEM_SPACING_V2 },
                        Current =
                        {
                            Value = new SettingsNote.Data(GeneralSettingsStrings.ChangeFolderLocationTooltip, SettingsNote.Type.Informational)
                        }
                    }
                }
            });
        }
    }
}
