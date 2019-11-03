// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy
{
    internal class EndTimeObjectPatternGenerator : PatternGenerator
    {
        private readonly double endTime;

        public EndTimeObjectPatternGenerator(FastRandom random, HitObject hitObject, ManiaBeatmap beatmap, IBeatmap originalBeatmap)
            : base(random, hitObject, beatmap, new Pattern(), originalBeatmap)
        {
            endTime = (HitObject as IHasEndTime)?.EndTime ?? 0;
        }

        public override IEnumerable<Pattern> Generate()
        {
            yield return generate();
        }

        private Pattern generate()
        {
            var pattern = new Pattern();

            bool generateHold = endTime - HitObject.StartTime >= 100;

            switch (TotalColumns)
            {
                case 8 when HitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH) && endTime - HitObject.StartTime < 1000:
                    addToPattern(pattern, 0, generateHold);
                    break;

                case 8:
                    addToPattern(pattern, FindAvailableColumn(GetRandomColumn(), PreviousPattern), generateHold);
                    break;

                default:
                    if (TotalColumns > 0)
                        addToPattern(pattern, GetRandomColumn(), generateHold);
                    break;
            }

            return pattern;
        }

        /// <summary>
        /// Constructs and adds a note to a pattern.
        /// </summary>
        /// <param name="pattern">The pattern to add to.</param>
        /// <param name="column">The column to add the note to.</param>
        /// <param name="holdNote">Whether to add a hold note.</param>
        private void addToPattern(Pattern pattern, int column, bool holdNote)
        {
            ManiaHitObject newObject;

            if (holdNote)
            {
                var hold = new HoldNote
                {
                    StartTime = HitObject.StartTime,
                    Column = column,
                    Duration = endTime - HitObject.StartTime
                };

                if (hold.Head.Samples == null)
                    hold.Head.Samples = new List<HitSampleInfo>();

                hold.Head.Samples.Add(new HitSampleInfo { Name = HitSampleInfo.HIT_NORMAL });

                hold.Tail.Samples = HitObject.Samples;

                newObject = hold;
            }
            else
            {
                newObject = new Note
                {
                    StartTime = HitObject.StartTime,
                    Samples = HitObject.Samples,
                    Column = column
                };
            }

            pattern.Add(newObject);
        }
    }
}
