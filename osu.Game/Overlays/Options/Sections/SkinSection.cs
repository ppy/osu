// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Overlays.Options.Sections
{
    public class SkinSection : OptionsSection
    {
        public override string Header => "Skin";
        public override FontAwesome Icon => FontAwesome.fa_paint_brush;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new OptionLabel { Text = "TODO: Skin preview textures" },
                new OptionLabel { Text = "Current skin: TODO dropdown" },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Preview gameplay",
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Open skin folder",
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Export as .osk",
                },
                new OsuCheckbox
                {
                    LabelText = "Ignore all beatmap skins",
                    Bindable = config.GetBindable<bool>(OsuConfig.IgnoreBeatmapSkins),
                    TooltipText = "Defaults game settings to never use skin elemt overrides provided by beatmaps."
                },
                new OsuCheckbox
                {
                    LabelText = "Use skin's sound samples",
                    Bindable = config.GetBindable<bool>(OsuConfig.SkinSamples),
                    TooltipText = "If this is not selected, the default osu! soundset will be used for hit samples."
                },
                new OsuCheckbox
                {
                    LabelText = "Use Taiko skin for Taiko mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.UseTaikoSkin),
                    TooltipText = "If this is selected and the Taiko skin is present, it will always be used when playing Taiko mode."
                },
                new OsuCheckbox
                {
                    LabelText = "Always use skin cursor",
                    Bindable = config.GetBindable<bool>(OsuConfig.UseSkinCursor),
                    TooltipText = "The selected skin's cursor will override any beatmap-specific cursor modifications."
                },
                new OptionSlider<double, SizeSlider>
                {
                    LabelText = "Menu cursor size",
                    Bindable = config.GetBindable<double>(OsuConfig.MenuCursorSize)
                },
                new OptionSlider<double, SizeSlider>
                {
                    LabelText = "Gameplay cursor size",
                    Bindable = config.GetBindable<double>(OsuConfig.GameplayCursorSize)
                },
                new OsuCheckbox
                {
                    LabelText = "Automatic cursor size",
                    Bindable = config.GetBindable<bool>(OsuConfig.AutomaticCursorSizing),
                    TooltipText = "Cursor size will adjust based on the CS of the current beatmap"
                },
            };
        }

        private class SizeSlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.##x");
        }
    }
}
