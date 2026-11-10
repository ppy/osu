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

        public AudioSettings()
            : base(PlayerSettingsOverlayStrings.AudioSettingsTitle)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics statics)
        {
            statics.BindWith(Static.LastLocalUserScore, referenceScore);

            Children = new Drawable[]
            {
                new PlayerCheckbox
                {
                    LabelText = SkinSettingsStrings.BeatmapHitsounds,
                    Current = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds),
                },
                new PlayerSliderBar<double>
                {
                    LabelText = SkinSettingsStrings.HitsoundVolume,
                    Current = config.GetBindable<double>(OsuSetting.HitsoundVolume),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true,
                },
                new BeatmapOffsetControl
                {
                    ReferenceScore = { BindTarget = referenceScore },
                },
            };
        }
    }
}
