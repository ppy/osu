// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public partial class ScrollingPath : CompositeDrawable
    {
        private readonly Path drawablePath;

        private readonly List<(double Time, float X)> vertices = new List<(double, float)>();

        public ScrollingPath()
        {
            Anchor = Anchor.BottomLeft;

            InternalChildren = new Drawable[]
            {
                drawablePath = new SmoothPath
                {
                    PathRadius = 2,
                    Alpha = 0.5f
                },
            };
        }

        public void UpdatePathFrom(ScrollingHitObjectContainer hitObjectContainer, JuiceStream hitObject)
        {
            double timeToYFactor = -hitObjectContainer.LengthAtTime(hitObject.StartTime, hitObject.StartTime + 1);

            computeTimeXs(hitObject);
            drawablePath.Vertices = vertices
                                    .Select(v => new Vector2(v.X, (float)(v.Time * timeToYFactor)))
                                    .ToArray();
            drawablePath.OriginPosition = drawablePath.PositionInBoundingBox(Vector2.Zero);
        }

        private void computeTimeXs(JuiceStream hitObject)
        {
            vertices.Clear();

            var sliderVertices = new List<Vector2>();
            hitObject.Path.GetPathToProgress(sliderVertices, 0, 1);

            if (sliderVertices.Count == 0)
                return;

            double time = 0;
            Vector2 lastPosition = Vector2.Zero;

            for (int repeat = 0; repeat < hitObject.RepeatCount + 1; repeat++)
            {
                foreach (var position in sliderVertices)
                {
                    time += Vector2.Distance(lastPosition, position) / hitObject.Velocity;
                    lastPosition = position;

                    vertices.Add((time, position.X));
                }

                sliderVertices.Reverse();
            }
        }

        // Because this has 0x0 size, the contents are otherwise masked away if the start position is outside the screen.
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;
    }
}
