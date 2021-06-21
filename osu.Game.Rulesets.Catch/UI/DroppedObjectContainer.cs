// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class DroppedObjectContainer : CompositeDrawable
    {
        private CaughtObjectPool caughtObjectPool;

        private readonly Container<CaughtObject> container;

        public DroppedObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;
            AddInternal(container = new Container<CaughtObject> { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Create a pool if not provided. It is convenient for testing.
            if (!Dependencies.TryGet(out caughtObjectPool))
                AddInternal(caughtObjectPool = new CaughtObjectPool());
        }

        /// <summary>
        /// Create a dropped object from <paramref name="source"/> and drop it immediately.
        /// </summary>
        public void Add(CaughtObject source, DropAnimation animation)
        {
            var droppedObject = caughtObjectPool.Get(source.HitObject);

            droppedObject.CopyStateFrom(source);
            droppedObject.Anchor = Anchor.TopLeft;
            droppedObject.Position = source.Parent.ToSpaceOfOtherDrawable(source.DrawPosition, container);

            container.Add(droppedObject);

            Vector2 targetPosition = source.Parent.ToSpaceOfOtherDrawable(source.DrawPosition * new Vector2(7f, 1), container);
            applyDropAnimation(droppedObject, animation, targetPosition);
        }

        public void OnRevertResult(DrawableCatchHitObject drawableHitObject)
        {
            container.RemoveAll(d => d.HitObject == drawableHitObject.HitObject);
        }

        private void applyDropAnimation(Drawable d, DropAnimation animation, Vector2 targetPosition)
        {
            switch (animation)
            {
                case DropAnimation.Drop:
                    d.MoveToY(d.Y + 75, 750, Easing.InSine);
                    d.FadeOut(750);
                    break;

                case DropAnimation.Explode:
                    d.MoveToY(d.Y - 50, 250, Easing.OutSine).Then().MoveToY(d.Y + 50, 500, Easing.InSine);
                    d.MoveToX(targetPosition.X, 1000);
                    d.FadeOut(750);
                    break;
            }

            d.Expire();
        }
    }
}
