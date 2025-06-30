// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Changes;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Changes
{
    public class ConvertJuiceStreamPathToSliderPathChange : CompositeChange
    {
        /// <summary>
        /// The height of legacy osu!standard playfield.
        /// The sliders converted by <see cref="ConvertJuiceStreamPathToSliderPathChange"/> are vertically contained in this height.
        /// </summary>
        internal const float OSU_PLAYFIELD_HEIGHT = 384;

        private readonly JuiceStreamPath path;
        private readonly SliderPath sliderPath;
        private readonly float sliderStartY;
        private readonly double velocity;

        /// <summary>
        /// Convert the path of this <see cref="JuiceStreamPath"/> to a <see cref="SliderPath"/> and write the result to <paramref name="sliderPath"/>.
        /// The resulting slider is "folded" to make it vertically contained in the playfield `(0..<see cref="OSU_PLAYFIELD_HEIGHT"/>)` assuming the slider start position is <paramref name="sliderStartY"/>.
        ///
        /// The velocity of the converted slider is assumed to be <paramref name="velocity"/>.
        /// To preserve the path, <paramref name="velocity"/> should be at least the value returned by <see cref="JuiceStreamPath.ComputeRequiredVelocity"/>.
        /// </summary>
        public ConvertJuiceStreamPathToSliderPathChange(JuiceStreamPath path, SliderPath sliderPath, float sliderStartY, double velocity)
        {
            this.path = path;
            this.sliderPath = sliderPath;
            this.sliderStartY = sliderStartY;
            this.velocity = velocity;
        }

        protected override void SubmitChanges()
        {
            const float margin = 1;

            // Note: these two variables and `sliderPath` are modified by the local functions.
            double currentTime = 0;
            Vector2 lastPosition = new Vector2(path.Vertices[0].X, 0);

            Submit(new RemoveRangePathControlPointChange(sliderPath.ControlPoints, 0, sliderPath.ControlPoints.Count));
            Submit(new InsertPathControlPointChange(sliderPath.ControlPoints, 0, new PathControlPoint(lastPosition)));

            for (int i = 1; i < path.Vertices.Count; i++)
            {
                Submit(new PathControlPointTypeChange(sliderPath.ControlPoints[^1], PathType.LINEAR));

                float deltaX = path.Vertices[i].X - lastPosition.X;
                double length = (path.Vertices[i].Time - currentTime) * velocity;

                // Should satisfy `deltaX^2 + deltaY^2 = length^2`.
                // The expression inside the `sqrt` is (almost) non-negative if the slider velocity is large enough.
                double deltaY = Math.Sqrt(Math.Max(0, length * length - (double)deltaX * deltaX));

                // When `deltaY` is small, one segment is always enough.
                // This case is handled separately to prevent divide-by-zero.
                if (deltaY <= OSU_PLAYFIELD_HEIGHT / 2 - margin)
                {
                    float nextX = path.Vertices[i].X;
                    float nextY = (float)(lastPosition.Y + getYDirection() * deltaY);
                    addControlPoint(nextX, nextY);
                    continue;
                }

                // When `deltaY` is large or when the slider velocity is fast, the segment must be partitioned to subsegments to stay in bounds.
                for (double currentProgress = 0; currentProgress < deltaY;)
                {
                    double nextProgress = Math.Min(currentProgress + getMaxDeltaY(), deltaY);
                    float nextX = (float)(path.Vertices[i - 1].X + nextProgress / deltaY * deltaX);
                    float nextY = (float)(lastPosition.Y + getYDirection() * (nextProgress - currentProgress));
                    addControlPoint(nextX, nextY);
                    currentProgress = nextProgress;
                }
            }

            int getYDirection()
            {
                float lastSliderY = sliderStartY + lastPosition.Y;
                return lastSliderY < OSU_PLAYFIELD_HEIGHT / 2 ? 1 : -1;
            }

            float getMaxDeltaY()
            {
                float lastSliderY = sliderStartY + lastPosition.Y;
                return Math.Max(lastSliderY, OSU_PLAYFIELD_HEIGHT - lastSliderY) - margin;
            }

            void addControlPoint(float nextX, float nextY)
            {
                Vector2 nextPosition = new Vector2(nextX, nextY);
                Submit(new InsertPathControlPointChange(sliderPath.ControlPoints, sliderPath.ControlPoints.Count, new PathControlPoint(nextPosition)));
                currentTime += Vector2.Distance(lastPosition, nextPosition) / velocity;
                lastPosition = nextPosition;
            }
        }
    }
}
