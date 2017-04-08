// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        private OptionDropdown<string> dropdown;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            this.audio = audio;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            audio.OnNewDevice -= onDeviceChanged;
            audio.OnLostDevice -= onDeviceChanged;
        }

        private void updateItems()
        {
            var deviceItems = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Default", string.Empty) };
            deviceItems.AddRange(audio.AudioDeviceNames.Select(d => new KeyValuePair<string, string>(d, d)));

            var preferredDeviceName = audio.AudioDevice.Value;
            if (deviceItems.All(kv => kv.Value != preferredDeviceName))
                deviceItems.Add(new KeyValuePair<string, string>(preferredDeviceName, preferredDeviceName));

            // The option dropdown for audio device selection lists all audio
            // device names. Dropdowns, however, may not have multiple identical
            // keys. Thus, we remove duplicate audio device names from
            // the dropdown. BASS does not give us a simple mechanism to select
            // specific audio devices in such a case anyways. Such
            // functionality would require involved OS-specific code.
            dropdown.Items = deviceItems.Distinct().ToList();
        }

        private void onDeviceChanged(string name) => updateItems();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Children = new Drawable[]
            {
                dropdown = new OptionDropdown<string>
                {
                    Bindable = audio.AudioDevice
                },
            };

            updateItems();

            audio.OnNewDevice += onDeviceChanged;
            audio.OnLostDevice += onDeviceChanged;
        }
    }
}