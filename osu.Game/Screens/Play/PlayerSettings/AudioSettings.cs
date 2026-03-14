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
            Children = new Drawable[]
            {
                new BeatmapHitsoundsControl { LabelText = SkinSettingsStrings.BeatmapHitsounds },
                new BeatmapOffsetControl
                {
                    ReferenceScore = { BindTarget = referenceScore },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics statics)
        {
            statics.BindWith(Static.LastLocalUserScore, referenceScore);
        }
    }
}
