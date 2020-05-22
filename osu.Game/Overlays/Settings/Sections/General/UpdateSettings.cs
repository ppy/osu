// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings.Sections.Maintenance;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class UpdateSettings : SettingsSubsection
    {
        protected override string Header => "更新";

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage, OsuConfigManager config, OsuGame game)
        {
            Add(new SettingsEnumDropdown<ReleaseStream>
            {
                LabelText = "更新频道",
                Bindable = config.GetBindable<ReleaseStream>(OsuSetting.ReleaseStream),
            });

            if (RuntimeInfo.IsDesktop)
            {
                Add(new SettingsButton
                {
                    Text = "打开osu!文件夹",
                    Action = storage.OpenInNativeExplorer,
                });

                Add(new SettingsButton
                {
                    Text = "数据迁移",
                    Action = () => game?.PerformFromScreen(menu => menu.Push(new MigrationSelectScreen()))
                });
            }
        }
    }
}