// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class KiaiSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.KiaiFlash;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = GraphicsSettingsStrings.KiaiFlash,
                    TransferValueOnCommit = true,
                    Current = osuConfig.GetBindable<float>(OsuSetting.KiaiFlash),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                    Keywords = new[] { "kiai", "glow", "highlight" },
                }
            };
        }
    }
}
