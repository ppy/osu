// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
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

        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            OsuHitObjectGenerationUtils.ReflectVertically(osuObject);
        }
    }
}
