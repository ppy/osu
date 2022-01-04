// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckLowDiffOverlaps : ICheck
    {
        // For the lowest difficulties, the osu! Ranking Criteria encourages overlapping ~180 BPM 1/2, but discourages ~180 BPM 1/1.
        private const double should_overlap_threshold = 150; // 200 BPM 1/2
        private const double should_probably_overlap_threshold = 175; // 170 BPM 1/2
        private const double should_not_overlap_threshold = 250; // 120 BPM 1/2 = 240 BPM 1/1

        /// <summary>
        /// Objects need to overlap this much before being treated as an overlap, else it may just be the borders slightly touching.
        /// </summary>
        private const double overlap_leniency = 5;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Spread, "Missing or unexpected overlaps");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateShouldOverlap(this),
            new IssueTemplateShouldProbablyOverlap(this),
            new IssueTemplateShouldNotOverlap(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            // TODO: This should also apply to *lowest difficulty* Normals - they are skipped for now.
            if (context.InterpretedDifficulty > DifficultyRating.Easy)
                yield break;

            var hitObjects = context.Beatmap.HitObjects;

            for (int i = 0; i < hitObjects.Count - 1; ++i)
            {
                if (!(hitObjects[i] is OsuHitObject hitObject) || hitObject is Spinner)
                    continue;

                if (!(hitObjects[i + 1] is OsuHitObject nextHitObject) || nextHitObject is Spinner)
                    continue;

                double deltaTime = nextHitObject.StartTime - hitObject.GetEndTime();
                if (deltaTime >= hitObject.TimeFadeIn + hitObject.TimePreempt)
                    // The objects are not visible at the same time (without mods), hence skipping.
                    continue;

                float distanceSq = (hitObject.StackedEndPosition - nextHitObject.StackedPosition).LengthSquared;
                double diameter = (hitObject.Radius - overlap_leniency) * 2;
                double diameterSq = diameter * diameter;

                bool areOverlapping = distanceSq < diameterSq;

                // Slider ends do not need to be overlapped because of slider leniency.
                if (!areOverlapping && !(hitObject is Slider))
                {
                    if (deltaTime < should_overlap_threshold)
                        yield return new IssueTemplateShouldOverlap(this).Create(deltaTime, hitObject, nextHitObject);
                    else if (deltaTime < should_probably_overlap_threshold)
                        yield return new IssueTemplateShouldProbablyOverlap(this).Create(deltaTime, hitObject, nextHitObject);
                }

                if (areOverlapping && deltaTime > should_not_overlap_threshold)
                    yield return new IssueTemplateShouldNotOverlap(this).Create(deltaTime, hitObject, nextHitObject);
            }
        }

        public abstract class IssueTemplateOverlap : IssueTemplate
        {
            protected IssueTemplateOverlap(ICheck check, IssueType issueType, string unformattedMessage)
                : base(check, issueType, unformattedMessage)
            {
            }

            public Issue Create(double deltaTime, params HitObject[] hitObjects) => new Issue(hitObjects, this, deltaTime);
        }

        public class IssueTemplateShouldOverlap : IssueTemplateOverlap
        {
            public IssueTemplateShouldOverlap(ICheck check)
                : base(check, IssueType.Problem, "These are {0} ms apart and so should be overlapping.")
            {
            }
        }

        public class IssueTemplateShouldProbablyOverlap : IssueTemplateOverlap
        {
            public IssueTemplateShouldProbablyOverlap(ICheck check)
                : base(check, IssueType.Warning, "These are {0} ms apart and so should probably be overlapping.")
            {
            }
        }

        public class IssueTemplateShouldNotOverlap : IssueTemplateOverlap
        {
            public IssueTemplateShouldNotOverlap(ICheck check)
                : base(check, IssueType.Problem, "These are {0} ms apart and so should NOT be overlapping.")
            {
            }
        }
    }
}
