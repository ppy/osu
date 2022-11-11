// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Streams.Components
{
    public class StreamPiece : BlueprintPiece<Stream>
    {
        private readonly Container<RingPiece> circles;

        private Vector2 streamOffset = Vector2.Zero;
        public Vector2 PathStartLocation => streamOffset;

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
            var boundingBox = DrawableStream.GetVertexBounds(positions.Select(o => o.Item1), hitObject.Scale * OsuHitObject.OBJECT_RADIUS);
            streamOffset = positions.Count == 0 ? Vector2.Zero : positions[0].Item1 - boundingBox.TopLeft;

            circles.Size = boundingBox.Size;
            Size = boundingBox.Size;
            OriginPosition = streamOffset;

            // Update all circles
            for (int i = 0; i < positions.Count; i++)
            {
                var circle = circles[i];
                circle.Position = positions[i].Item1 + streamOffset;
                circle.Scale = new Vector2(hitObject.Scale);
            }
        }
    }
}
