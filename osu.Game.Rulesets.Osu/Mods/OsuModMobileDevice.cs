// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using static osu.Framework.RuntimeInfo;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModMobileDevice : Mod, IApplicableToDifficulty, IPlatformExclusiveMod
    {
        private const int small_touch_device_difficulty_decrease = 5;

        public override string Name => "Mobile Device";

        public override string Acronym => "MD";

        public override LocalisableString Description => "A mobile friendly mod with easy defaults.";

        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new Type[] { typeof(OsuModDifficultyAdjust) };

        public override IconUsage? Icon => FontAwesome.Solid.Mobile;

        public Platform[] AllowedPlatforms => new Platform[] { Platform.iOS, Platform.Android };

        [SettingSource("Mobile friendly circles", "Adjust the circle size to be mobile friendly.")]
        public BindableBool MobileFriendlyCircleSize { get; } = new BindableBool(true);

        [SettingSource("Mobile friendly overall difficulty", "Adjust the overall difficulty to be mobile friendly.")]
        public BindableBool MobileFriendlyOverallDifficulty { get; } = new BindableBool(true);

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            if (MobileFriendlyCircleSize.Value)
                difficulty.CircleSize -= small_touch_device_difficulty_decrease;

            if (MobileFriendlyOverallDifficulty.Value)
                difficulty.OverallDifficulty -= small_touch_device_difficulty_decrease;
        }
    }
}
