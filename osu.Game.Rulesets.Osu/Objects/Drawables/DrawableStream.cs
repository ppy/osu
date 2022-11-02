// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableStream : DrawableOsuHitObject
    {
        public new Stream HitObject => (Stream)base.HitObject;

        private Container<DrawableHitCircle> hitCircleContainer = null!;
        private int depthIndex = 0;

        public DrawableStream(Stream s)
            : base(s)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(hitCircleContainer = new Container<DrawableHitCircle> { RelativeSizeAxes = Axes.Both });
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableHitCircle hitCircle:
                    hitCircleContainer.Add(hitCircle);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();

            hitCircleContainer.Clear(false);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case HitCircle hitCircle:
                    return new DrawableHitCircle(hitCircle) { Depth = depthIndex++ };
            }

            return base.CreateNestedHitObject(hitObject);
        }
    }
}
