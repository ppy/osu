// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModMobileFriendly : Mod, IApplicableToDifficulty, IPlatformExclusiveMod
    {
        private const int difficulty_adjust = 5;

        public override string Name => "Mobile Friendly";

        public override string Acronym => "MF";

        public override LocalisableString Description => "A mobile friendly mod with easy defaults.";

        public override double ScoreMultiplier => 1;

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new Type[] { typeof(OsuModDifficultyAdjust) };

        public override IconUsage? Icon => FontAwesome.Solid.Mobile;

        [SettingSource("Adjust circle size", "Adjust the circle size to be mobile friendly.")]
        public BindableBool AdjustCircleSize { get; } = new BindableBool(true);

        [SettingSource("Adjust overall difficulty", "Adjust the overall difficulty to be mobile friendly.")]
        public BindableBool AdjustOverallDifficulty { get; } = new BindableBool(true);

        public bool AcceptPlatform() => RuntimeInfo.IsMobile;

        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            if (AdjustCircleSize.Value)
                difficulty.CircleSize -= difficulty_adjust;

            if (AdjustOverallDifficulty.Value)
                difficulty.OverallDifficulty -= difficulty_adjust;
        }
    }
}
