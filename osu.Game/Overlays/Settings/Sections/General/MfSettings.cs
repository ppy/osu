// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MfSettings : SettingsSubsection
    {
        protected override string Header => "Mf-osu";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "启用Mf自定义UI",
                    Bindable = config.GetBindable<bool>(OsuSetting.OptUI)
                },
                new SettingsCheckbox
                {
                    LabelText = "启用三角形粒子动画",
                    Bindable = config.GetBindable<bool>(OsuSetting.TrianglesEnabled)
                },
            };
        }
    }
}
