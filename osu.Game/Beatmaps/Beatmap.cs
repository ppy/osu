// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO.Serialization;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A Beatmap containing converted HitObjects.
    /// </summary>
    public class Beatmap<T>
        where T : HitObject
    {
        public BeatmapInfo BeatmapInfo = new BeatmapInfo();
        public ControlPointInfo ControlPointInfo = new ControlPointInfo();
        public List<BreakPeriod> Breaks = new List<BreakPeriod>();
        public readonly List<Color4> ComboColors = new List<Color4>
        {
            new Color4(17, 136, 170, 255),
            new Color4(102, 136, 0, 255),
            new Color4(204, 102, 0, 255),
            new Color4(121, 9, 13, 255)
        };

        public BeatmapMetadata Metadata => BeatmapInfo?.Metadata ?? BeatmapInfo?.BeatmapSet?.Metadata;

        /// <summary>
        /// The HitObjects this Beatmap contains.
        /// </summary>
        public List<T> HitObjects = new List<T>();

        /// <summary>
        /// Total amount of break time in the beatmap.
        /// </summary>
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
            ComboColors = original?.ComboColors ?? ComboColors;
            HitObjects = original?.HitObjects ?? HitObjects;
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
        public Beatmap(Beatmap original = null)
            : base(original)
        {
        }
    }
}
