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

        void IApplicableToHitObject.ApplyToHitObject(HitObject hitObject)
        {
            const double multiplier = 1 / 1.4;

            switch (hitObject)
            {
                case Note:
                    ((ManiaHitWindows)hitObject.HitWindows).DifficultyMultiplier = multiplier;
                    break;

                case HoldNote hold:
                    ((ManiaHitWindows)hold.Head.HitWindows).DifficultyMultiplier = multiplier;
                    ((ManiaHitWindows)hold.Tail.HitWindows).DifficultyMultiplier = multiplier;
                    break;
            }
        }
    }
}
