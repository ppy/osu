// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public partial class NestedOutlineContainer : CompositeDrawable
    {
        private readonly List<CatchHitObject> nestedHitObjects = new List<CatchHitObject>();

        public NestedOutlineContainer()
        {
            Anchor = Anchor.BottomLeft;
        }

        public void UpdateNestedObjectsFrom(ScrollingHitObjectContainer hitObjectContainer, CatchHitObject parentHitObject)
        {
            nestedHitObjects.Clear();
            nestedHitObjects.AddRange(parentHitObject.NestedHitObjects
                                                     .OfType<CatchHitObject>()
                                                     .Where(h => !(h is TinyDroplet)));

            while (nestedHitObjects.Count < InternalChildren.Count)
                RemoveInternal(InternalChildren[^1], true);

            while (InternalChildren.Count < nestedHitObjects.Count)
                AddInternal(new FruitOutline());

            for (int i = 0; i < nestedHitObjects.Count; i++)
            {
                var hitObject = nestedHitObjects[i];
                var outline = (FruitOutline)InternalChildren[i];
                outline.Position = CatchHitObjectUtils.GetStartPosition(hitObjectContainer, hitObject) - Position;
                outline.UpdateFrom(hitObject);
                outline.Scale *= hitObject is Droplet ? 0.5f : 1;
            }
        }

        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;
    }
}
