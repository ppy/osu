// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckTimeDistanceEquality : ICheck
    {
        /// <summary>
        /// Two objects this many ms apart or more are skipped. (200 BPM 2/1)
        /// </summary>
        private const double pattern_lifetime = 600;

        /// <summary>
        /// Two objects this distance apart or less are skipped.
        /// </summary>
        private const double stack_leniency = 12;

        /// <summary>
        /// How long an observation is relevant for comparison. (120 BPM 8/1)
        /// </summary>
        private const double observation_lifetime = 4000;

        /// <summary>
        /// How different two delta times can be to still be compared. (240 BPM 1/16)
        /// </summary>
        private const double similar_time_leniency = 16;

        /// <summary>
        /// How many pixels are subtracted from the difference between current and expected distance.
        /// </summary>
        private const double distance_leniency_absolute_warning = 10;

        /// <summary>
        /// How much of the current distance that the difference can make out.
        /// </summary>
        private const double distance_leniency_percent_warning = 0.15;

        private const double distance_leniency_absolute_problem = 20;
        private const double distance_leniency_percent_problem = 0.3;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Spread, "Object too close or far away from previous");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateIrregularSpacingProblem(this),
            new IssueTemplateIrregularSpacingWarning(this)
        };

        /// <summary>
        /// Represents an observation of the time and distance between two objects.
        /// </summary>
        private readonly struct ObservedTimeDistance
        {
            public readonly double ObservationTime;
            public readonly double DeltaTime;
            public readonly double Distance;

            public ObservedTimeDistance(double observationTime, double deltaTime, double distance)
            {
                ObservationTime = observationTime;
                DeltaTime = deltaTime;
                Distance = distance;
            }
        }

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            if (context.InterpretedDifficulty > DifficultyRating.Normal)
                yield break;

            var prevObservedTimeDistances = new List<ObservedTimeDistance>();
            var hitObjects = context.Beatmap.HitObjects;

            for (int i = 0; i < hitObjects.Count - 1; ++i)
            {
                if (!(hitObjects[i] is OsuHitObject hitObject) || hitObject is Spinner)
                    continue;

                if (!(hitObjects[i + 1] is OsuHitObject nextHitObject) || nextHitObject is Spinner)
                    continue;

                double deltaTime = nextHitObject.StartTime - hitObject.GetEndTime();

                // Ignore objects that are far enough apart in time to not be considered the same pattern.
                if (deltaTime > pattern_lifetime)
                    continue;

                // Relying on FastInvSqrt is probably good enough here. We'll be taking the difference between distances later, hence square not being sufficient.
                float distance = (hitObject.StackedEndPosition - nextHitObject.StackedPosition).LengthFast;

                // Ignore stacks and half-stacks, as these are close enough to where they can't be confused for being time-distanced.
                if (distance < stack_leniency)
                    continue;

                var observedTimeDistance = new ObservedTimeDistance(nextHitObject.StartTime, deltaTime, distance);
                double expectedDistance = getExpectedDistance(prevObservedTimeDistances, observedTimeDistance);

                if (expectedDistance == 0)
                {
                    // There was nothing relevant to compare to.
                    prevObservedTimeDistances.Add(observedTimeDistance);
                    continue;
                }

                if ((Math.Abs(expectedDistance - distance) - distance_leniency_absolute_problem) / distance > distance_leniency_percent_problem)
                    yield return new IssueTemplateIrregularSpacingProblem(this).Create(expectedDistance, distance, hitObject, nextHitObject);
                else if ((Math.Abs(expectedDistance - distance) - distance_leniency_absolute_warning) / distance > distance_leniency_percent_warning)
                    yield return new IssueTemplateIrregularSpacingWarning(this).Create(expectedDistance, distance, hitObject, nextHitObject);
                else
                {
                    // We use `else` here to prevent issues from cascading; an object spaced too far could cause regular spacing to be considered "too short" otherwise.
                    prevObservedTimeDistances.Add(observedTimeDistance);
                }
            }
        }

        private double getExpectedDistance(IEnumerable<ObservedTimeDistance> prevObservedTimeDistances, ObservedTimeDistance observedTimeDistance)
        {
            int observations = prevObservedTimeDistances.Count();

            int count = 0;
            double sum = 0;

            // Looping this in reverse allows us to break before going through all elements, as we're only interested in the most recent ones.
            for (int i = observations - 1; i >= 0; --i)
            {
                var prevObservedTimeDistance = prevObservedTimeDistances.ElementAt(i);

                // Only consider observations within the last few seconds - this allows the map to build spacing up/down over time, but prevents it from being too sudden.
                if (observedTimeDistance.ObservationTime - prevObservedTimeDistance.ObservationTime > observation_lifetime)
                    break;

                // Only consider observations which have a similar time difference - this leniency allows handling of multi-BPM maps which speed up/down slowly.
                if (Math.Abs(observedTimeDistance.DeltaTime - prevObservedTimeDistance.DeltaTime) > similar_time_leniency)
                    break;

                count += 1;
                sum += prevObservedTimeDistance.Distance / Math.Max(prevObservedTimeDistance.DeltaTime, 1);
            }

            return sum / Math.Max(count, 1) * observedTimeDistance.DeltaTime;
        }

        public abstract class IssueTemplateIrregularSpacing : IssueTemplate
        {
            protected IssueTemplateIrregularSpacing(ICheck check, IssueType issueType)
                : base(check, issueType, "Expected {0:0} px spacing like previous objects, currently {1:0}.")
            {
            }

            public Issue Create(double expected, double actual, params HitObject[] hitObjects) => new Issue(hitObjects, this, expected, actual);
        }

        public class IssueTemplateIrregularSpacingProblem : IssueTemplateIrregularSpacing
        {
            public IssueTemplateIrregularSpacingProblem(ICheck check)
                : base(check, IssueType.Problem)
            {
            }
        }

        public class IssueTemplateIrregularSpacingWarning : IssueTemplateIrregularSpacing
        {
            public IssueTemplateIrregularSpacingWarning(ICheck check)
                : base(check, IssueType.Warning)
            {
            }
        }
    }
}
