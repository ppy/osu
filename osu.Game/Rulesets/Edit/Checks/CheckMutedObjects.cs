// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckMutedObjects : ICheck
    {
        /// <summary>
        /// Volume percentages lower than or equal to this are typically inaudible.
        /// </summary>
        private const int muted_threshold = 5;

        /// <summary>
        /// Volume percentages lower than or equal to this can sometimes be inaudible depending on sample used and music volume.
        /// </summary>
        private const int low_volume_threshold = 20;

        private enum EdgeType
        {
            Head,
            Repeat,
            Tail,
            None
        }

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Audio, "Low volume hitobjects");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateMutedActive(this),
            new IssueTemplateLowVolumeActive(this),
            new IssueTemplateMutedPassive(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            foreach (var hitObject in context.Beatmap.HitObjects)
            {
                // Worth keeping in mind: The samples of an object always play at its end time.
                // Objects like spinners have no sound at its start because of this, while hold notes have nested objects to accomplish this.
                foreach (var nestedHitObject in hitObject.NestedHitObjects)
                {
                    foreach (var issue in getVolumeIssues(hitObject, nestedHitObject))
                        yield return issue;
                }

                foreach (var issue in getVolumeIssues(hitObject))
                    yield return issue;
            }
        }

        private IEnumerable<Issue> getVolumeIssues(HitObject hitObject, HitObject? sampledHitObject = null)
        {
            sampledHitObject ??= hitObject;
            if (!sampledHitObject.Samples.Any())
                yield break;

            // Samples that allow themselves to be overridden by control points have a volume of 0.
            int maxVolume = sampledHitObject.Samples.Max(sample => sample.Volume);
            double samplePlayTime = sampledHitObject.GetEndTime();

            EdgeType edgeType = getEdgeAtTime(hitObject, samplePlayTime);
            // We only care about samples played on the edges of objects, not ones like spinnerspin or slidertick.
            if (edgeType == EdgeType.None)
                yield break;

            string postfix = hitObject is IHasDuration ? edgeType.ToString().ToLowerInvariant() : string.Empty;

            if (maxVolume <= muted_threshold)
            {
                if (edgeType == EdgeType.Head)
                    yield return new IssueTemplateMutedActive(this).Create(hitObject, maxVolume / 100f, sampledHitObject.GetEndTime(), postfix);
                else
                    yield return new IssueTemplateMutedPassive(this).Create(hitObject, maxVolume / 100f, sampledHitObject.GetEndTime(), postfix);
            }
            else if (maxVolume <= low_volume_threshold && edgeType == EdgeType.Head)
            {
                yield return new IssueTemplateLowVolumeActive(this).Create(hitObject, maxVolume / 100f, sampledHitObject.GetEndTime(), postfix);
            }
        }

        private EdgeType getEdgeAtTime(HitObject hitObject, double time)
        {
            if (Precision.AlmostEquals(time, hitObject.StartTime, 1f))
                return EdgeType.Head;
            if (Precision.AlmostEquals(time, hitObject.GetEndTime(), 1f))
                return EdgeType.Tail;

            if (hitObject is IHasRepeats hasRepeats)
            {
                double spanDuration = hasRepeats.Duration / hasRepeats.SpanCount();
                if (spanDuration <= 0)
                    // Prevents undefined behaviour in cases like where zero/negative-length sliders/hold notes exist.
                    return EdgeType.None;

                double spans = (time - hitObject.StartTime) / spanDuration;
                double acceptableDifference = 1 / spanDuration; // 1 ms of acceptable difference, as with head/tail above.

                if (Precision.AlmostEquals(spans, Math.Ceiling(spans), acceptableDifference) ||
                    Precision.AlmostEquals(spans, Math.Floor(spans), acceptableDifference))
                {
                    return EdgeType.Repeat;
                }
            }

            return EdgeType.None;
        }

        public abstract class IssueTemplateMuted : IssueTemplate
        {
            protected IssueTemplateMuted(ICheck check, IssueType type, string unformattedMessage)
                : base(check, type, unformattedMessage)
            {
            }

            public Issue Create(HitObject hitobject, double volume, double time, string postfix = "")
            {
                string objectName = hitobject.GetType().Name;
                if (!string.IsNullOrEmpty(postfix))
                    objectName += " " + postfix;

                return new Issue(hitobject, this, objectName, volume) { Time = time };
            }
        }

        public class IssueTemplateMutedActive : IssueTemplateMuted
        {
            public IssueTemplateMutedActive(ICheck check)
                : base(check, IssueType.Problem, "{0} has a volume of {1:0%}. Clickable objects must have clearly audible feedback.")
            {
            }
        }

        public class IssueTemplateLowVolumeActive : IssueTemplateMuted
        {
            public IssueTemplateLowVolumeActive(ICheck check)
                : base(check, IssueType.Warning, "{0} has a volume of {1:0%}, ensure this is audible.")
            {
            }
        }

        public class IssueTemplateMutedPassive : IssueTemplateMuted
        {
            public IssueTemplateMutedPassive(ICheck check)
                : base(check, IssueType.Negligible, "{0} has a volume of {1:0%}, ensure there is no distinct sound here in the song if inaudible.")
            {
            }
        }
    }
}
