// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModMirror : ModMirror, IApplicableToHitObject
    {
        public void ApplyToHitObject(HitObject hitObject)
        {
            var catchObject = (CatchHitObject)hitObject;

            catchObject.X = 1 - catchObject.X;
            catchObject.XOffsetReversed = true;

            foreach (var nested in catchObject.NestedHitObjects.Cast<CatchHitObject>())
            {
                nested.X = 1 - nested.X;
                nested.XOffsetReversed = true;
            }

            if (catchObject is JuiceStream juiceStream)
            {
                foreach (var point in juiceStream.Path.ControlPoints)
                    point.Position.Value = new Vector2(-point.Position.Value.X, point.Position.Value.Y);
            }
        }
    }
}