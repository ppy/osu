// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Tau.Objects;
using osuTK;

namespace osu.Game.Rulesets.Tau.Mods
{
    public class TauModHardRock : ModHardRock, IApplicableToHitObject
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public void ApplyToHitObject(HitObject hitObject)
        {
            var tauObject = (TauHitObject)hitObject;

            tauObject.PositionToEnd = new Vector2(tauObject.PositionToEnd.X, -tauObject.PositionToEnd.Y);
        }
    }
}
