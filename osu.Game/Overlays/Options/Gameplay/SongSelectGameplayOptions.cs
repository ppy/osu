using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class SongSelectGameplayOptions : OptionsSubsection
    {
        protected override string Header => "Song Select";

        private BindableInt starMinimum, starMaximum;
        private StarCounter counterMin, counterMax;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            starMinimum = (BindableInt)config.GetBindable<int>(OsuConfig.DisplayStarsMinimum);
            starMaximum = (BindableInt)config.GetBindable<int>(OsuConfig.DisplayStarsMaximum);
            Children = new Drawable[]
            {
                new SliderOption<int> { LabelText = "Display beatmaps from", Bindable = starMinimum },
                counterMin = new StarCounter { Count = starMinimum.Value },
                new SliderOption<int> { LabelText = "up to", Bindable = starMaximum },
                counterMax = new StarCounter { Count = starMaximum.Value },
            };
            starMinimum.ValueChanged += starValueChanged;
            starMaximum.ValueChanged += starValueChanged;
        }

        private void starValueChanged(object sender, EventArgs e)
        {
            counterMin.Count = starMinimum.Value;
            counterMax.Count = starMaximum.Value;
        }
        
        protected override void Dispose(bool isDisposing)
        {
            starMinimum.ValueChanged -= starValueChanged;
            starMaximum.ValueChanged -= starValueChanged;
            base.Dispose(isDisposing);
        }
    }
}

