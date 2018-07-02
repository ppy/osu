// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

namespace osu.Game.Configuration
{
    public class BindableVisualSettings : Component
    {
        public Bindable<double> DimLevel { get; private set; }
        public Bindable<double> BlurLevel { get; private set; }
        public Bindable<bool> ShowStoryboard { get; private set; }
        public Bindable<bool> BeatmapSkins { get; private set; }
        public Bindable<bool> BeatmapHitsounds { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            BlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            BeatmapSkins = config.GetBindable<bool>(OsuSetting.BeatmapSkins);
            BeatmapHitsounds = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);
        }
    }
}
