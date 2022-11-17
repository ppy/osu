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

        private Container<DrawableStreamHitCircle> hitCircleContainer = null!;

        public DrawableStream()
            : this(null)
        {
        }

        public DrawableStream(Stream? s = null)
            : base(s)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(hitCircleContainer = new Container<DrawableStreamHitCircle> { Anchor = Anchor.TopLeft });

            PositionBindable.BindValueChanged(_ => UpdatePosition());
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableStreamHitCircle hitCircle:
                    hitCircle.Depth = hitCircleContainer.Count;
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
                case StreamHitCircle hitCircle:
                    return new DrawableStreamHitCircle(hitCircle);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected virtual void UpdatePosition()
        {
            Position = HitObject.Position;
        }
    }
}
