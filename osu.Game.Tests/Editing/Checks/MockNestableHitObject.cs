// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Tests.Editing.Checks
{
    public sealed class MockNestableHitObject : HitObject, IHasDuration
    {
        private readonly IEnumerable<HitObject> toBeNested;

        public MockNestableHitObject(IEnumerable<HitObject> toBeNested, double startTime, double endTime)
        {
            this.toBeNested = toBeNested;
            StartTime = startTime;
            EndTime = endTime;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            foreach (var hitObject in toBeNested)
                AddNested(hitObject);
        }

        public double EndTime { get; }

        public double Duration
        {
            get => EndTime - StartTime;
            set => throw new System.NotImplementedException();
        }
    }
}
