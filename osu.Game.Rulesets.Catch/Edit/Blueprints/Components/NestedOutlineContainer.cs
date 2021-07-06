// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public class NestedOutlineContainer : CompositeDrawable
    {
        private readonly Container<FruitOutline> nestedOutlines;

        private readonly List<CatchHitObject> nestedHitObjects = new List<CatchHitObject>();

        public NestedOutlineContainer()
        {
            Anchor = Anchor.BottomLeft;

            InternalChild = nestedOutlines = new Container<FruitOutline>();
        }

        public void UpdatePositionFrom(ScrollingHitObjectContainer hitObjectContainer, CatchHitObject parentHitObject)
        {
            X = parentHitObject.OriginalX;
            Y = hitObjectContainer.PositionAtTime(parentHitObject.StartTime);
        }

        public void UpdateNestedObjectsFrom(ScrollingHitObjectContainer hitObjectContainer, CatchHitObject parentHitObject)
        {
            nestedHitObjects.Clear();
            nestedHitObjects.AddRange(parentHitObject.NestedHitObjects
                                                     .OfType<CatchHitObject>()
                                                     .Where(h => !(h is TinyDroplet)));

            while (nestedHitObjects.Count < nestedOutlines.Count)
                nestedOutlines.Remove(nestedOutlines[^1]);

            while (nestedOutlines.Count < nestedHitObjects.Count)
                nestedOutlines.Add(new FruitOutline());

            for (int i = 0; i < nestedHitObjects.Count; i++)
            {
                var hitObject = nestedHitObjects[i];
                nestedOutlines[i].UpdateFrom(hitObjectContainer, hitObject, parentHitObject);
                nestedOutlines[i].Scale *= hitObject is Droplet ? 0.5f : 1;
            }
        }
    }
}
