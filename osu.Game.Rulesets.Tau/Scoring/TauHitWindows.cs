// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Tau.Scoring
{
    public class TauHitWindows : HitWindows
    {
        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        protected override DifficultyRange[] GetRanges() => new[]
        {
            new DifficultyRange(HitResult.Perfect, 140, 100, 60),
            new DifficultyRange(HitResult.Great, 200, 150, 100),
            new DifficultyRange(HitResult.Miss, 400, 400, 400),
        };
    }
}
