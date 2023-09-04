// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModBarrelRoll : ModBarrelRoll<OsuHitObject>, IApplicableToDrawableHitObject
    {
        public override Type[] IncompatibleMods => new[] { typeof(OsuModBubbles) };

        public void ApplyToDrawableHitObject(DrawableHitObject d)
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
