// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy
{
    /// <summary>
    /// A simple generator which, for any object, if the hitobject has an end time
    /// it becomes a <see cref="HoldNote"/> or otherwise a <see cref="Note"/>.
    /// </summary>
    internal class PassThroughPatternGenerator : LegacyPatternGenerator
    {
        public PassThroughPatternGenerator(LegacyRandom random, HitObject hitObject, IBeatmap beatmap, int totalColumns, Pattern previousPattern)
            : base(random, hitObject, beatmap, previousPattern, totalColumns)
        {
        }

        public override IEnumerable<Pattern> Generate()
        {
            var positionData = HitObject as IHasXPosition;
            int column = GetColumn(positionData?.X ?? 0);

            var pattern = new Pattern();

            if (HitObject is IHasDuration endTimeData)
            {
                pattern.Add(new HoldNote
                {
                    StartTime = HitObject.StartTime,
                    Duration = endTimeData.Duration,
                    Column = column,
                    Samples = HitObject.Samples,
                    NodeSamples = (HitObject as IHasRepeats)?.NodeSamples ?? HoldNote.CreateDefaultNodeSamples(HitObject)
                });
            }
            else
            {
                pattern.Add(new Note
                {
                    StartTime = HitObject.StartTime,
                    Samples = HitObject.Samples,
                    Column = column
                });
            }

            yield return pattern;
        }
    }
}
