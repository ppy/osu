//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class SkinSection : OptionsSection
    {
        public override string Header => "Skin";
        public override FontAwesome Icon => FontAwesome.fa_paint_brush;
        
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            content.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Skin preview textures" },
                new SpriteText { Text = "Current skin: TODO dropdown" },
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
                new CheckBoxOption
                {
                    LabelText = "Ignore all beatmap skins",
                    Bindable = config.GetBindable<bool>(OsuConfig.IgnoreBeatmapSkins)
                },
                new CheckBoxOption
                {
                    LabelText = "Use skin's sound samples",
                    Bindable = config.GetBindable<bool>(OsuConfig.SkinSamples)
                },
                new CheckBoxOption
                {
                    LabelText = "Use Taiko skin for Taiko mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.UseTaikoSkin)
                },
                new CheckBoxOption
                {
                    LabelText = "Always use skin cursor",
                    Bindable = config.GetBindable<bool>(OsuConfig.UseSkinCursor)
                },
                new SliderOption<double>
                {
                    LabelText = "Cursor size",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.CursorSize)
                },
                new CheckBoxOption
                {
                    LabelText = "Automatic cursor size",
                    Bindable = config.GetBindable<bool>(OsuConfig.AutomaticCursorSizing)
                },
            };
        }
    }
}