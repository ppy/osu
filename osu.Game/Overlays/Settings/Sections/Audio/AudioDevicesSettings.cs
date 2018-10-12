// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class AudioDevicesSettings : SettingsSubsection
    {
        protected override string Header => "Devices";

        private const string default_audio_device = "Default";

        private AudioManager audio;
        private SettingsDropdown<AudioDeviceItem> dropdown;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            this.audio = audio;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (audio != null)
            {
                audio.OnNewDevice -= onDeviceChanged;
                audio.OnLostDevice -= onDeviceChanged;
            }
        }

        private void updateItems()
        {
            var deviceItems = new List<AudioDeviceItem> { AudioDeviceItem.DEFAULT };
            deviceItems.AddRange(audio.AudioDeviceNames.Select(name => new AudioDeviceItem(name)));

            var preferredDeviceName = audio.AudioDevice.Value;
            if (deviceItems.All(item => item.Name != preferredDeviceName))
                deviceItems.Add(new AudioDeviceItem(preferredDeviceName));

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
                dropdown = new SettingsDropdown<AudioDeviceItem>()
            };

            updateItems();

            // Todo: Create helpers to link two bindables with converters?
            dropdown.Bindable = new Bindable<AudioDeviceItem>();
            // if "Default" is selected, the AudioManager must recieve string.Empty
            dropdown.Bindable.BindValueChanged(device => audio.AudioDevice.Value = device == AudioDeviceItem.DEFAULT ? string.Empty : device.Name);
            // if the AudioManager reports null or string.Empty, select "Default" in the dropdown
            audio.AudioDevice.BindValueChanged(name => dropdown.Bindable.Value = string.IsNullOrEmpty(name)
                                                        ? AudioDeviceItem.DEFAULT
                                                        : dropdown.Items.FirstOrDefault(device => device.Name == name));

            audio.OnNewDevice += onDeviceChanged;
            audio.OnLostDevice += onDeviceChanged;
        }

        private class AudioDeviceItem
        {
            public static readonly AudioDeviceItem DEFAULT = new DefaultItem();

            public readonly string Name;

            public AudioDeviceItem(string name)
            {
                Name = name;
            }

            public override string ToString() => Name;

            private class DefaultItem : AudioDeviceItem
            {
                public DefaultItem()
                    // TODO make this localisable
                    : base("Default")
                {
                }
            }
        }
    }
}
