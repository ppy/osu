// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModMirror : ModMirror, IApplicableToHitObject
    {
        public override string Description => "Fruits are flipped horizontally.";
        
        public void ApplyToHitObject(HitObject hitObject)
        {
            var catchObject = (CatchHitObject)hitObject;

            if (catchObject is BananaShower)
                return;

            catchObject.OriginalX = CatchPlayfield.WIDTH - catchObject.OriginalX;

            foreach (var nested in catchObject.NestedHitObjects.Cast<CatchHitObject>())
                nested.OriginalX = CatchPlayfield.WIDTH - nested.OriginalX;

            if (!(catchObject is JuiceStream juiceStream))
                return;

            var controlPoints = juiceStream.Path.ControlPoints.Select(p => new PathControlPoint(p.Position.Value, p.Type.Value)).ToArray();
            foreach (var point in controlPoints)
                point.Position.Value = new Vector2(-point.Position.Value.X, point.Position.Value.Y);

            juiceStream.Path = new SliderPath(controlPoints, juiceStream.Path.ExpectedDistance.Value);
        }
    }
}
