// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Overlays.Settings.Sections
{
    public class SkinSection : SettingsSection
    {
        public override string Header => "Skin";
        public override FontAwesome Icon => FontAwesome.fa_paint_brush;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new SettingsSlider<double, SizeSlider>
                {
                    LabelText = "Menu cursor size",
                    Bindable = config.GetBindable<double>(OsuSetting.MenuCursorSize)
                },
                new SettingsSlider<double, SizeSlider>
                {
                    LabelText = "Gameplay cursor size",
                    Bindable = config.GetBindable<double>(OsuSetting.GameplayCursorSize)
                },
                new SettingsCheckbox
                {
                    LabelText = "Adjust gameplay cursor size based on current beatmap",
                    Bindable = config.GetBindable<bool>(OsuSetting.AutoCursorSize)
                },
            };
        }

        private class SizeSlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.##x");
        }
    }
}
