// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO.Serialization;
using Newtonsoft.Json;
using osu.Game.IO.Serialization.Converters;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A Beatmap containing converted HitObjects.
    /// </summary>
    public class Beatmap<T> : IJsonSerializable
        where T : HitObject
    {
        public BeatmapInfo BeatmapInfo = new BeatmapInfo();
        public ControlPointInfo ControlPointInfo = new ControlPointInfo();
        public List<BreakPeriod> Breaks = new List<BreakPeriod>();

        [JsonIgnore]
        public BeatmapMetadata Metadata => BeatmapInfo?.Metadata ?? BeatmapInfo?.BeatmapSet?.Metadata;

        /// <summary>
        /// The HitObjects this Beatmap contains.
        /// </summary>
        [JsonConverter(typeof(TypedListConverter<HitObject>))]
        public List<T> HitObjects = new List<T>();

        /// <summary>
        /// Total amount of break time in the beatmap.
        /// </summary>
        [JsonIgnore]
        public double TotalBreakTime => Breaks.Sum(b => b.Duration);

        /// <summary>
        /// Constructs a new beatmap.
        /// </summary>
        /// <param name="original">The original beatmap to use the parameters of.</param>
        public Beatmap(Beatmap<T> original = null)
        {
            BeatmapInfo = original?.BeatmapInfo.DeepClone() ?? BeatmapInfo;
            ControlPointInfo = original?.ControlPointInfo ?? ControlPointInfo;
            Breaks = original?.Breaks ?? Breaks;
            HitObjects = original?.HitObjects ?? HitObjects;

            if (original == null && Metadata == null)
            {
                // we may have no metadata in cases we weren't sourced from the database.
                // let's fill it (and other related fields) so we don't need to null-check it in future usages.
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Unknown",
                        AuthorString = @"Unknown Creator",
                    },
                    Version = @"Normal",
                    BaseDifficulty = new BeatmapDifficulty()
                };
            }
        }
    }

    /// <summary>
    /// A Beatmap containing un-converted HitObjects.
    /// </summary>
    public class Beatmap : Beatmap<HitObject>
    {
        /// <summary>
        /// Constructs a new beatmap.
        /// </summary>
        /// <param name="original">The original beatmap to use the parameters of.</param>
        public Beatmap(Beatmap original)
            : base(original)
        {
        }

        public Beatmap()
        {
        }
    }
}
