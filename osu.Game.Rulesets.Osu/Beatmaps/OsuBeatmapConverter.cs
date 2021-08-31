// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using System.Linq;
using System.Threading;
using osu.Game.Rulesets.Osu.UI;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps.Legacy;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    public class OsuBeatmapConverter : BeatmapConverter<OsuHitObject>
    {
        public OsuBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasPosition);

        protected override IEnumerable<OsuHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var positionData = original as IHasPosition;
            var comboData = original as IHasCombo;

            switch (original)
            {
                case IHasPathWithRepeats curveData:
                    return new Slider
                    {
                        StartTime = original.StartTime,
                        Samples = original.Samples,
                        Path = curveData.Path,
                        NodeSamples = curveData.NodeSamples,
                        RepeatCount = curveData.RepeatCount,
                        Position = positionData?.Position ?? Vector2.Zero,
                        NewCombo = comboData?.NewCombo ?? false,
                        ComboOffset = comboData?.ComboOffset ?? 0,
                        LegacyLastTickOffset = (original as IHasLegacyLastTickOffset)?.LegacyLastTickOffset,
                        // prior to v8, speed multipliers don't adjust for how many ticks are generated over the same distance.
                        // this results in more (or less) ticks being generated in <v8 maps for the same time duration.
                        TickDistanceMultiplier = beatmap.BeatmapInfo.BeatmapVersion < 8 ? 1f / ((LegacyControlPointInfo)beatmap.ControlPointInfo).DifficultyPointAt(original.StartTime).SliderVelocity : 1
                    }.Yield();

                case IHasDuration endTimeData:
                    return new Spinner
                    {
                        StartTime = original.StartTime,
                        Samples = original.Samples,
                        EndTime = endTimeData.EndTime,
                        Position = positionData?.Position ?? OsuPlayfield.BASE_SIZE / 2,
                        NewCombo = comboData?.NewCombo ?? false,
                        ComboOffset = comboData?.ComboOffset ?? 0,
                    }.Yield();

                default:
                    return new HitCircle
                    {
                        StartTime = original.StartTime,
                        Samples = original.Samples,
                        Position = positionData?.Position ?? Vector2.Zero,
                        NewCombo = comboData?.NewCombo ?? false,
                        ComboOffset = comboData?.ComboOffset ?? 0,
                    }.Yield();
            }
        }

        protected override Beatmap<OsuHitObject> CreateBeatmap() => new OsuBeatmap();
    }
}
