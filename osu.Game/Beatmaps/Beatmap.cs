// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using System.Collections.Generic;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A Beatmap containing converted HitObjects.
    /// </summary>
    public class Beatmap<T>
        where T : HitObject
    {
        public BeatmapInfo BeatmapInfo;
        public TimingInfo TimingInfo = new TimingInfo();
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
        public List<T> HitObjects;

        /// <summary>
        /// Constructs a new beatmap.
        /// </summary>
        /// <param name="original">The original beatmap to use the parameters of.</param>
        public Beatmap(Beatmap original = null)
        {
            BeatmapInfo = original?.BeatmapInfo ?? BeatmapInfo;
            TimingInfo = original?.TimingInfo ?? TimingInfo;
            ComboColors = original?.ComboColors ?? ComboColors;
        }

        /// <summary>
        /// Finds the slider velocity at a time.
        /// </summary>
        /// <param name="time">The time to find the slider velocity at.</param>
        /// <returns>The slider velocity in positional length units.</returns>
        public double SliderVelocityAt(double time)
        {
            double scoringDistance = 100 * BeatmapInfo.Difficulty.SliderMultiplier;
            double beatDistance = TimingInfo.BeatDistanceAt(time);

            if (beatDistance > 0)
                return scoringDistance / beatDistance * 1000;
            return scoringDistance;
        }
    }

    /// <summary>
    /// A Beatmap containing un-converted HitObjects.
    /// </summary>
    public class Beatmap : Beatmap<HitObject>
    {
        /// <summary>
        /// Calculates the star difficulty for this Beatmap.
        /// </summary>
        /// <returns>The star difficulty.</returns>
        public double CalculateStarDifficulty() => Ruleset.GetRuleset(BeatmapInfo.Mode).CreateDifficultyCalculator(this).Calculate();
    }
}
