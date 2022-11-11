// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableStream : DrawableOsuHitObject
    {
        public new Stream HitObject => (Stream)base.HitObject;

        private Container<DrawableStreamHitCircle> hitCircleContainer = null!;

        public IBindable<int> PathVersion => pathVersion;
        private readonly Bindable<int> pathVersion = new Bindable<int>();

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
                    hitCircleContainer.Add(hitCircle);
                    vertexBoundsCache.Invalidate();
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();

            hitCircleContainer.Clear(false);
            vertexBoundsCache.Invalidate();
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

        private readonly Cached<RectangleF> vertexBoundsCache = new Cached<RectangleF>();

        protected override void Update()
        {
            base.Update();

            var boundingBox = vertexBoundsCache.IsValid
                ? vertexBoundsCache.Value
                : vertexBoundsCache.Value = GetVertexBounds(hitCircleContainer.Children.Select(o => o.Position), HitObject.Scale * OsuHitObject.OBJECT_RADIUS);

            hitCircleContainer.Size = boundingBox.Size;
            Size = boundingBox.Size;
            OriginPosition = hitCircleContainer.Count == 0 ? Vector2.Zero : hitCircleContainer.Children[0].Position - boundingBox.TopLeft;

            if (hitCircleContainer.DrawSize != Vector2.Zero)
            {
                var childAnchorPosition = Vector2.Divide(OriginPosition, hitCircleContainer.DrawSize);
                foreach (var obj in hitCircleContainer)
                    obj.RelativeAnchorPosition = childAnchorPosition;
            }
        }

        protected override void OnApply()
        {
            base.OnApply();

            // Ensure that the version will change after the upcoming BindTo().
            pathVersion.Value = int.MaxValue;
            PathVersion.BindTo(HitObject.StreamPath.Version);
        }

        protected override void OnFree()
        {
            base.OnFree();

            PathVersion.UnbindFrom(HitObject.StreamPath.Version);
        }

        protected virtual void UpdatePosition()
        {
            Position = HitObject.Position;
        }

        public static RectangleF GetVertexBounds(IEnumerable<Vector2> vertices, float radius)
        {
            float minX = 0;
            float minY = 0;
            float maxX = 0;
            float maxY = 0;

            foreach (var v in vertices)
            {
                minX = Math.Min(minX, v.X - radius);
                minY = Math.Min(minY, v.Y - radius);
                maxX = Math.Max(maxX, v.X + radius);
                maxY = Math.Max(maxY, v.Y + radius);
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
