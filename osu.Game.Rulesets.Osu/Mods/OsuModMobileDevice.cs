// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
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

        public override Type[] IncompatibleMods => new Type[] { typeof(OsuModDifficultyAdjust) };

        public override IconUsage? Icon => FontAwesome.Solid.Mobile;

        public Platform[] AllowedPlatforms => new Platform[] { Platform.iOS, Platform.Android };

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.CircleSize -= small_touch_device_difficulty_decrease;
            difficulty.OverallDifficulty -= small_touch_device_difficulty_decrease;
        }
    }
}
