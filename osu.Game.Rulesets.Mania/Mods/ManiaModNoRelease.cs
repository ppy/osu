// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModNoRelease : Mod, IApplicableToDrawableHitObject
    {
        public override string Name => "No Release";

        public override string Acronym => "NR";

        public override LocalisableString Description => "No more timing the end of hold notes.";

        public override double ScoreMultiplier => 0.9;

        public override ModType Type => ModType.DifficultyReduction;

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableHoldNote hold)
            {
                hold.HitObjectApplied += dho =>
                {
                    ((DrawableHoldNote)dho).Tail.LateReleaseResult = HitResult.Perfect;
                };
            }
        }
    }
}
