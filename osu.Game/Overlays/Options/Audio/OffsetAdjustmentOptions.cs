//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Audio
{
    public class OffsetAdjustmentOptions : OptionsSubsection
    {
        protected override string Header => "Offset Adjustment";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SliderOption<int>
                {
                    LabelText = "Universal Offset",
                    Bindable = (BindableInt)config.GetBindable<int>(OsuConfig.Offset)
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Offset wizard"
                }
            };
        }
    }
}
