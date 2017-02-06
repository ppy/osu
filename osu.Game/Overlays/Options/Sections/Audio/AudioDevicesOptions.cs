//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Overlays.Options.Sections.Audio
{
    public class AudioDevicesOptions : OptionsSubsection
    {
        protected override string Header => "Devices";

        private AudioManager audio;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            this.audio = audio;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var deviceItems = new List<KeyValuePair<string, string>>();
            deviceItems.Add(new KeyValuePair<string, string>("Default", string.Empty));
            deviceItems.AddRange(audio.GetDeviceNames().Select(d => new KeyValuePair<string, string>(d, d)));
            Children = new Drawable[]
            {
                new OptionDropDown<string>()
                {
                    Items = deviceItems,
                    Bindable = audio.AudioDevice
                },
            };
        }
    }
}