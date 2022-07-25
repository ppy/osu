// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModEasy : ModEasyWithExtraLives, IApplicableToHitObject
    {
        public override string Description => @"Larger circles, more forgiving HP drain, less accuracy required, and three lives!";

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            float currentCircleSize = difficulty.CircleSize;

            base.ApplyToDifficulty(difficulty);

            // Undo CS change.
            difficulty.CircleSize = currentCircleSize;
        }

        public void ApplyToHitObject(HitObject hitObject)
        {
            if (hitObject is OsuHitObject osuHitObject) {
                osuHitObject.Scale += 0.125f;
            }
        }
    }
}
