// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModBarrelRoll : ModBarrelRoll<OsuHitObject>, IApplicableToDrawableHitObjects
    {
        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables)
            {
                d.OnUpdate += _ =>
                {
                    switch (d)
                    {
                        case DrawableHitCircle circle:
                            circle.CirclePiece.Rotation = -CurrentRotation;
                            break;
                    }
                };
            }
        }
    }
}
