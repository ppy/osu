// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Database;

namespace osu.Game.Overlays.Settings.Sections.DebugSettings
{
    public partial class BatchImportSettings : SettingsSubsection
    {
        protected override LocalisableString Header => @"批量导入";

        private SettingsButton importBeatmapsButton = null!;
        private SettingsButton importCollectionsButton = null!;
        private SettingsButton importScoresButton = null!;
        private SettingsButton importSkinsButton = null!;

        [BackgroundDependencyLoader]
        private void load(LegacyImportManager? legacyImportManager)
        {
            if (legacyImportManager?.SupportsImportFromStable != true)
                return;

            AddRange(new[]
            {
                importBeatmapsButton = new SettingsButton
                {
                    Text = @"从Stable中导入谱面",
                    Action = () =>
                    {
                        importBeatmapsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Beatmaps).ContinueWith(_ => Schedule(() => importBeatmapsButton.Enabled.Value = true));
                    }
                },
                importSkinsButton = new SettingsButton
                {
                    Text = @"从Stable中导入皮肤",
                    Action = () =>
                    {
                        importSkinsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Skins).ContinueWith(_ => Schedule(() => importSkinsButton.Enabled.Value = true));
                    }
                },
                importCollectionsButton = new SettingsButton
                {
                    Text = @"从Stable中导入收藏夹",
                    Action = () =>
                    {
                        importCollectionsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Collections).ContinueWith(_ => Schedule(() => importCollectionsButton.Enabled.Value = true));
                    }
                },
                importScoresButton = new SettingsButton
                {
                    Text = @"从Stable中导入成绩",
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
