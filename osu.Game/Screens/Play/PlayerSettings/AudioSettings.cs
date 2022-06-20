// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class AudioSettings : PlayerSettingsGroup
    {
        public Bindable<ScoreInfo> ReferenceScore { get; } = new Bindable<ScoreInfo>();

        private readonly PlayerCheckbox beatmapHitsoundsToggle;

        public AudioSettings()
            : base("Audio Settings")
        {
            Children = new Drawable[]
            {
                beatmapHitsoundsToggle = new PlayerCheckbox { LabelText = SkinSettingsStrings.BeatmapHitsounds },
                new BeatmapOffsetControl
                {
                    ReferenceScore = { BindTarget = ReferenceScore },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            beatmapHitsoundsToggle.Current = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);
        }
    }
}
