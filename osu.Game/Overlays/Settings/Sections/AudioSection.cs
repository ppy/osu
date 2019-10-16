// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections.Audio;

namespace osu.Game.Overlays.Settings.Sections
{
    public class AudioSection : SettingsSection
    {
        public override string Header => "Audio";
        public override IconUsage Icon => FontAwesome.Solid.VolumeUp;

        public AudioSection()
        {
            Children = new Drawable[]
            {
                new AudioDevicesSettings(),
                new VolumeSettings(),
                new OffsetSettings(),
                new MainMenuSettings()
            };
        }
    }
}
