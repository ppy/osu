// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Database;

namespace osu.Game.Overlays.Settings.Sections.DebugSettings
{
    public partial class BatchImportSettings : SettingsSubsection
    {
        protected override LocalisableString Header => @"Batch Import";

        private SettingsButtonV2 importBeatmapsButton = null!;
        private SettingsButtonV2 importCollectionsButton = null!;
        private SettingsButtonV2 importScoresButton = null!;
        private SettingsButtonV2 importSkinsButton = null!;

        [BackgroundDependencyLoader]
        private void load(LegacyImportManager? legacyImportManager)
        {
            if (legacyImportManager?.SupportsImportFromStable != true)
                return;

            AddRange(new[]
            {
                importBeatmapsButton = new SettingsButtonV2
                {
                    Text = @"Import beatmaps from stable",
                    Action = () =>
                    {
                        importBeatmapsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Beatmaps).ContinueWith(_ => Schedule(() => importBeatmapsButton.Enabled.Value = true));
                    }
                },
                importSkinsButton = new SettingsButtonV2
                {
                    Text = @"Import skins from stable",
                    Action = () =>
                    {
                        importSkinsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Skins).ContinueWith(_ => Schedule(() => importSkinsButton.Enabled.Value = true));
                    }
                },
                importCollectionsButton = new SettingsButtonV2
                {
                    Text = @"Import collections from stable",
                    Action = () =>
                    {
                        importCollectionsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Collections).ContinueWith(_ => Schedule(() => importCollectionsButton.Enabled.Value = true));
                    }
                },
                importScoresButton = new SettingsButtonV2
                {
                    Text = @"Import scores from stable",
                    Action = () =>
                    {
                        importScoresButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Scores).ContinueWith(_ => Schedule(() => importScoresButton.Enabled.Value = true));
                    }
                },
            });
        }
    }
}
