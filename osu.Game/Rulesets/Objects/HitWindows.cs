// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Objects
{
    public class HitWindows
    {
        private static readonly IReadOnlyDictionary<HitResult, (double max, double mid, double min)> base_ranges = new Dictionary<HitResult, (double, double, double)>
        {
            { HitResult.Perfect, (44.8, 38.8, 27.8) },
            { HitResult.Great, (128, 98, 68 ) },
            { HitResult.Good, (194, 164, 134) },
            { HitResult.Ok, (254, 224, 194) },
            { HitResult.Meh, (382, 272, 242) },
            { HitResult.Miss, (376, 346, 316) },
        };

        /// <summary>
        /// Hit window for a <see cref="HitResult.Perfect"/> hit.
        /// </summary>
        public double Perfect { get; private set; }

        /// <summary>
        /// Hit window for a <see cref="HitResult.Great"/> hit.
        /// </summary>
        public double Great { get; private set; }

        /// <summary>
        /// Hit window for a <see cref="HitResult.Good"/> hit.
        /// </summary>
        public double Good { get; private set; }

        /// <summary>
        /// Hit window for an <see cref="HitResult.OK"/> hit.
        /// </summary>
        public double Ok { get; private set; }

        /// <summary>
        /// Hit window for a <see cref="HitResult.Meh"/> hit.
        /// </summary>
        public double Meh { get; private set; }

        /// <summary>
        /// Hit window for a <see cref="HitResult.Miss"/> hit.
        /// </summary>
        public double Miss { get; private set; }

        /// <summary>
        /// Constructs hit windows by fitting a parameter to a 2-part piecewise linear function for each hit window.
        /// </summary>
        /// <param name="difficulty">The parameter.</param>
        public HitWindows(double difficulty)
        {
            Perfect = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Perfect]);
            Great = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Great]);
            Good = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Good]);
            Ok = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Ok]);
            Meh = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Meh]);
            Miss = BeatmapDifficulty.DifficultyRange(difficulty, base_ranges[HitResult.Miss]);
        }

        /// <summary>
        /// Retrieves the hit result for a time offset.
        /// </summary>
        /// <param name="timeOffset">The time offset. This should always be a positive value indicating the absolute time offset.</param>
        /// <returns>The hit result, or null if <paramref name="timeOffset"/> doesn't result in a judgement.</returns>
        public HitResult? ResultFor(double timeOffset)
        {
            timeOffset = Math.Abs(timeOffset);

            if (timeOffset <= Perfect / 2)
                return HitResult.Perfect;
            if (timeOffset <= Great / 2)
                return HitResult.Great;
            if (timeOffset <= Good / 2)
                return HitResult.Good;
            if (timeOffset <= Ok / 2)
                return HitResult.Ok;
            if (timeOffset <= Meh / 2)
                return HitResult.Meh;
            if (timeOffset <= Miss / 2)
                return HitResult.Miss;

            return null;
        }

        /// <summary>
        /// Given a time offset, whether the <see cref="HitObject"/> can ever be hit in the future.
        /// This happens if <paramref name="timeOffset"/> > <see cref="Meh"/>.
        /// </summary>
        /// <param name="timeOffset">The time offset.</param>
        /// <returns>Whether the <see cref="HitObject"/> can be hit at any point in the future from this time offset.</returns>
        public bool CanBeHit(double timeOffset) => timeOffset <= Meh / 2;

        /// <summary>
        /// Multiplies all hit windows by a value.
        /// </summary>
        /// <param name="windows">The hit windows to multiply.</param>
        /// <param name="value">The value to multiply each hit window by.</param>
        public static HitWindows operator *(HitWindows windows, double value)
        {
            windows.Perfect *= value;
            windows.Great *= value;
            windows.Good *= value;
            windows.Ok *= value;
            windows.Meh *= value;
            windows.Miss *= value;

            return windows;
        }

        /// <summary>
        /// Divides all hit windows by a value.
        /// </summary>
        /// <param name="windows">The hit windows to divide.</param>
        /// <param name="value">The value to divide each hit window by.</param>
        public static HitWindows operator /(HitWindows windows, double value)
        {
            windows.Perfect /= value;
            windows.Great /= value;
            windows.Good /= value;
            windows.Ok /= value;
            windows.Meh /= value;
            windows.Miss /= value;

            return windows;
        }
    }
}
