// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Streams.Components
{
    public partial class StreamPiece : BlueprintPiece<Stream>
    {
        private readonly Container<RingPiece> circles;

        public Vector2 PathStartLocation { get; private set; } = Vector2.Zero;

        public StreamPiece()
        {
            InternalChild = circles = new Container<RingPiece>();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        public override void UpdateFrom(Stream hitObject)
        {
            base.UpdateFrom(hitObject);

            var positions = hitObject.StreamPath.GetStreamPath();

            if (circles.Count > positions.Count)
            {
                // Remove any extra circles
                var toRemove = circles.Skip(positions.Count).ToArray();
                circles.RemoveRange(toRemove, true);
            }

            if (circles.Count < positions.Count)
            {
                // Add any missing circles
                circles.AddRange(Enumerable.Range(0, positions.Count - circles.Count).Select(_ => new RingPiece
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre,
                    Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2),
                    CornerRadius = OsuHitObject.OBJECT_RADIUS,
                    CornerExponent = 2
                }));
            }

            // Calculate the correct bounding box
            var boundingBox = getVertexBounds(positions.Select(o => o.Item1), hitObject.Scale * OsuHitObject.OBJECT_RADIUS);
            PathStartLocation = positions.Count == 0 ? Vector2.Zero : positions[0].Item1 - boundingBox.TopLeft;

            circles.Size = boundingBox.Size;
            Size = boundingBox.Size;
            OriginPosition = PathStartLocation;

            // Update all circles
            for (int i = 0; i < positions.Count; i++)
            {
                var circle = circles[i];
                circle.Position = positions[i].Item1 + PathStartLocation;
                circle.Scale = new Vector2(hitObject.Scale);
            }
        }

        private static RectangleF getVertexBounds(IEnumerable<Vector2> vertices, float radius)
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
