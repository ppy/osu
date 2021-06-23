// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public class JuiceStreamSelectionBlueprint : CatchSelectionBlueprint<JuiceStream>
    {
        public override Quad SelectionQuad => HitObjectContainer.ToScreenSpace(getBoundingBox().Offset(new Vector2(0, HitObjectContainer.DrawHeight)));

        private float minNestedX;
        private float maxNestedX;

        public JuiceStreamSelectionBlueprint(JuiceStream hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HitObject.DefaultsApplied += onDefaultsApplied;
            computeObjectBounds();
        }

        private void onDefaultsApplied(HitObject _) => computeObjectBounds();

        private void computeObjectBounds()
        {
            minNestedX = HitObject.NestedHitObjects.OfType<CatchHitObject>().Min(nested => nested.OriginalX) - HitObject.OriginalX;
            maxNestedX = HitObject.NestedHitObjects.OfType<CatchHitObject>().Max(nested => nested.OriginalX) - HitObject.OriginalX;
        }

        private RectangleF getBoundingBox()
        {
            float left = HitObject.OriginalX + minNestedX;
            float right = HitObject.OriginalX + maxNestedX;
            float top = HitObjectContainer.PositionAtTime(HitObject.EndTime);
            float bottom = HitObjectContainer.PositionAtTime(HitObject.StartTime);
            float objectRadius = CatchHitObject.OBJECT_RADIUS * HitObject.Scale;
            return new RectangleF(left, top, right - left, bottom - top).Inflate(objectRadius);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            HitObject.DefaultsApplied -= onDefaultsApplied;
        }
    }
}
