// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MvisSettings : SettingsSubsection
    {
        protected override string Header => "Mvis播放器";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<int>
                {
                    LabelText = "屏幕粒子数",
                    TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(OsuSetting.MvisParticleAmount),
                    KeyboardStep = 1,
                },
                new SettingsCheckbox
                {
                    LabelText = "使用原版Logo效果",
                    Bindable = config.GetBindable<bool>(OsuSetting.MvisUseOsuLogoVisualisation),
                },
            };
        }
    }
}
