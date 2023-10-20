// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHardRock : ModHardRock, IApplicableToHitObject
    {
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModMirror)).ToArray();

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            float currentCircleSize = difficulty.CircleSize;

            base.ApplyToDifficulty(difficulty);

            // Undo CS change.
            difficulty.CircleSize = currentCircleSize;
        }

        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            osuObject.Scale -= 0.125f;

            OsuHitObjectGenerationUtils.ReflectVerticallyAlongPlayfield(osuObject);
        }

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            difficulty.CircleSize = Math.Min(difficulty.CircleSize * 1.3f, 10.0f); // CS uses a custom 1.3 ratio.
            difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * ADJUST_RATIO, 10.0f);
        }
    }
}
