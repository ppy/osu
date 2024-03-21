// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum BackgroundSource
    {
        [LocalisableDescription(typeof(SkinSettingsStrings), nameof(SkinSettingsStrings.SkinSectionHeader))]
        Skin,

        [LocalisableDescription(typeof(GameplaySettingsStrings), nameof(GameplaySettingsStrings.BeatmapHeader))]
        Beatmap,

        [LocalisableDescription(typeof(UserInterfaceStrings), nameof(UserInterfaceStrings.BeatmapWithStoryboard))]
        BeatmapWithStoryboard,
    }
}
