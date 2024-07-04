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
    public partial class AudioSettings : PlayerSettingsGroup
    {
        private Bindable<ScoreInfo> referenceScore { get; } = new Bindable<ScoreInfo>();

        private readonly PlayerCheckbox beatmapHitsoundsToggle;
        private readonly PlayerCheckbox muteHitSoundsToggle;

        public AudioSettings()
            : base("Audio Settings")
        {
            Children = new Drawable[]
            {
                beatmapHitsoundsToggle = new PlayerCheckbox { LabelText = SkinSettingsStrings.BeatmapHitsounds },
                muteHitSoundsToggle = new PlayerCheckbox { LabelText = AudioSettingsStrings.MuteHitsounds },
                new BeatmapOffsetControl
                {
                    ReferenceScore = { BindTarget = referenceScore },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics statics)
        {
            beatmapHitsoundsToggle.Current = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);
            muteHitSoundsToggle.Current = config.GetBindable<bool>(OsuSetting.MuteHitsounds);
            statics.BindWith(Static.LastLocalUserScore, referenceScore);
        }
    }
}
