// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class AudioSettings : PlayerSettingsGroup
    {
        private readonly PlayerCheckbox beatmapHitsoundsToggle;

        public AudioSettings()
            : base("Audio Settings")
        {
            Children = new Drawable[]
            {
                beatmapHitsoundsToggle = new PlayerCheckbox { LabelText = "Beatmap hitsounds" }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            beatmapHitsoundsToggle.Current = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);
        }
    }
}
