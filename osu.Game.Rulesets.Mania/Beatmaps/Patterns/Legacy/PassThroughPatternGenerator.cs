// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
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
                // despite the beatmap originally being made for mania, if the object is parsed as a slider rather than a hold, sliding samples should still be played.
                // this is seemingly only possible to achieve by modifying the .osu file directly, but online beatmaps that do that exist
                // (see second and fourth notes of https://osu.ppy.sh/beatmapsets/73883#mania/216407)
                bool playSlidingSamples = (HitObject is IHasLegacyHitObjectType hasType && hasType.LegacyType == LegacyHitObjectType.Slider) || HitObject is IHasPath;

                pattern.Add(new HoldNote
                {
                    StartTime = HitObject.StartTime,
                    Duration = endTimeData.Duration,
                    Column = column,
                    Samples = HitObject.Samples,
                    PlaySlidingSamples = playSlidingSamples,
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
