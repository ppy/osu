// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using M.DBus;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class LinuxSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Linux集成";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, DBusManager dBusManager)
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<GamemodeActivateCondition>
                {
                    LabelText = "Gamemode启用条件",
                    TooltipText = "依赖libgamemode",
                    Current = config.GetBindable<GamemodeActivateCondition>(MSetting.Gamemode)
                }
            };
        }
    }
}
