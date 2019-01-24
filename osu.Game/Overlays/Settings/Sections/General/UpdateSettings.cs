// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class UpdateSettings : SettingsSubsection
    {
        protected override string Header => "Updates";

        [BackgroundDependencyLoader]
        private void load(Storage storage, OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<ReleaseStream>
                {
                    LabelText = "Release stream",
                    Bindable = config.GetBindable<ReleaseStream>(OsuSetting.ReleaseStream),
                },
                new SettingsButton
                {
                    Text = "Open osu! folder",
                    Action = storage.OpenInNativeExplorer,
                }
            };
        }
    }
}
