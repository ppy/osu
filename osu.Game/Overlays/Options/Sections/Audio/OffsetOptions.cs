// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Audio
{
    public class OffsetOptions : OptionsSubsection
    {
        protected override string Header => "Offset Adjustment";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionSlider<double>
                {
                    LabelText = "Audio Offset",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.AudioOffset)
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
