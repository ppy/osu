// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class AudioSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.AudioSettings";

        /// <summary>
        /// "Audio"
        /// </summary>
        public static LocalisableString AudioSectionHeader => new TranslatableString(getKey(@"audio_section_header"), @"Audio");

        /// <summary>
        /// "Devices"
        /// </summary>
        public static LocalisableString AudioDevicesHeader => new TranslatableString(getKey(@"audio_devices_header"), @"Devices");

        /// <summary>
        /// "Volume"
        /// </summary>
        public static LocalisableString VolumeHeader => new TranslatableString(getKey(@"volume_header"), @"Volume");

        /// <summary>
        /// "Output device"
        /// </summary>
        public static LocalisableString OutputDevice => new TranslatableString(getKey(@"output_device"), @"Output device");

        /// <summary>
        /// "Master"
        /// </summary>
        public static LocalisableString MasterVolume => new TranslatableString(getKey(@"master_volume"), @"Master");

        /// <summary>
        /// "Master (window inactive)"
        /// </summary>
        public static LocalisableString MasterVolumeInactive => new TranslatableString(getKey(@"master_volume_inactive"), @"Master (window inactive)");

        /// <summary>
        /// "Effect"
        /// </summary>
        public static LocalisableString EffectVolume => new TranslatableString(getKey(@"effect_volume"), @"Effect");

        /// <summary>
        /// "Music"
        /// </summary>
        public static LocalisableString MusicVolume => new TranslatableString(getKey(@"music_volume"), @"Music");

        /// <summary>
        /// "Offset Adjustment"
        /// </summary>
        public static LocalisableString OffsetHeader => new TranslatableString(getKey(@"offset_header"), @"Offset Adjustment");

        /// <summary>
        /// "Audio offset"
        /// </summary>
        public static LocalisableString AudioOffset => new TranslatableString(getKey(@"audio_offset"), @"Audio offset");

        /// <summary>
        /// "Offset wizard"
        /// </summary>
        public static LocalisableString OffsetWizard => new TranslatableString(getKey(@"offset_wizard"), @"Offset wizard");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
