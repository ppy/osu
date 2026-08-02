// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModEasy : ModEasyWithExtraLives, IApplicableToHitObject
    {
        public override LocalisableString Description => @"More forgiving HP drain, less accuracy required, and extra lives!";

        public const double HIT_WINDOW_DIFFICULTY_MULTIPLIER = 1 / 1.4;

        void IApplicableToHitObject.ApplyToHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Note:
                    ((ManiaHitWindows)hitObject.HitWindows).DifficultyMultiplier = HIT_WINDOW_DIFFICULTY_MULTIPLIER;
                    break;

                case HoldNote hold:
                    ((ManiaHitWindows)hold.Head.HitWindows).DifficultyMultiplier = HIT_WINDOW_DIFFICULTY_MULTIPLIER;
                    ((ManiaHitWindows)hold.Tail.HitWindows).DifficultyMultiplier = HIT_WINDOW_DIFFICULTY_MULTIPLIER;
                    break;
            }
        }
    }
}
