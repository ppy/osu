// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract class Smoke : TexturedPath
    {
        protected bool IsActive { get; private set; }
        protected double SmokeTimeStart { get; private set; } = double.MinValue;
        protected double SmokeTimeEnd { get; private set; } = double.MinValue;

        protected readonly List<Vector2> SmokeVertexPositions = new List<Vector2>();
        protected readonly List<double> SmokeVertexTimes = new List<double>();

        [Resolved(CanBeNull = true)]
        private SmokeContainer? smokeContainer { get; set; }

        protected struct SmokePoint
        {
            public Vector2 Position;
            public double Time;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (smokeContainer != null)
            {
                smokeContainer.SmokeMoved += guardOnSmokeMoved;
                smokeContainer.SmokeEnded += guardOnSmokeEnded;
                IsActive = true;
            }

            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;

            SmokeTimeStart = Time.Current;
        }

        private void guardOnSmokeMoved(Vector2 position, double time)
        {
            if (IsActive)
                OnSmokeMoved(position, time);
        }

        private void guardOnSmokeEnded(double time)
        {
            if (IsActive)
                OnSmokeEnded(time);
        }

        protected virtual void OnSmokeMoved(Vector2 position, double time)
        {
            addSmokeVertex(position, time);
        }

        private void addSmokeVertex(Vector2 position, double time)
        {
            Debug.Assert(SmokeVertexTimes.Count == SmokeVertexPositions.Count);

            if (SmokeVertexTimes.Count > 0 && SmokeVertexTimes.Last() > time)
            {
                int index = ~SmokeVertexTimes.BinarySearch(time, new UpperBoundComparer());

                SmokeVertexTimes.RemoveRange(index, SmokeVertexTimes.Count - index);
                SmokeVertexPositions.RemoveRange(index, SmokeVertexPositions.Count - index);
            }

            SmokeVertexTimes.Add(time);
            SmokeVertexPositions.Add(position);
        }

        protected virtual void OnSmokeEnded(double time)
        {
            IsActive = false;
            SmokeTimeEnd = time;
        }

        protected override void Update()
        {
            base.Update();

            const double visible_duration = 8000;
            const float disappear_speed = 3;

            int index = 0;
            if (!IsActive)
            {
                double cutoffTime = SmokeTimeStart + disappear_speed * (Time.Current - (SmokeTimeEnd + visible_duration));
                index = ~SmokeVertexTimes.BinarySearch(cutoffTime, new UpperBoundComparer());
            }
            Vertices = new List<Vector2>(SmokeVertexPositions.Skip(index));

            Position = -PositionInBoundingBox(Vector2.Zero);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (smokeContainer != null)
            {
                smokeContainer.SmokeMoved -= guardOnSmokeMoved;
                smokeContainer.SmokeEnded -= guardOnSmokeEnded;
            }
        }

        private struct UpperBoundComparer : IComparer<double>
        {
            public int Compare(double x, double target)
            {
                // By returning -1 when the target value is equal to x, guarantees that the
                // element at BinarySearch's returned index will always be the first element
                // larger. Since 0 is never returned, the target is never "found", so the return
                // value will be the index's complement.

                return x > target ? 1 : -1;
            }
        }
    }
}
