// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
