// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class CountdownOverlay : Container, ISkinnableDrawable
    {
        public virtual bool IsEditable => false; // todo: the default countdown is not implemented yet

        [Resolved]
        protected GameplayState GameplayState { get; private set; }

        [Resolved]
        protected DrawableRuleset DrawableRuleset { get; private set; }

        [Resolved]
        protected Player Player { get; private set; }

        internal ISkinSource Skin;

        public CountdownOverlay()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Skin = skin;
            Clock = DrawableRuleset.FrameStableClock;
        }

        public bool UsesFixedAnchor { get; set; }

        /// <summary>
        /// Calculate the <see cref="CountdownTimings"/> of a countdown.
        /// </summary>
        public static CountdownTimings GetTimings(IBeatmap beatmap)
        {
            double firstObject = beatmap.HitObjects[0].StartTime;
            double offset = beatmap.ControlPointInfo.TimingPointAt(firstObject).Time;
            double beatLengthOriginal = beatmap.ControlPointInfo.TimingPointAt(firstObject).BeatLength;

            if (beatLengthOriginal <= 0) beatLengthOriginal = beatmap.ControlPointInfo.TimingPointAt(0).BeatLength;
            if (beatLengthOriginal <= 0) return CountdownTimings.None;

            double goTime = offset - 5;
            double beatLength = beatLengthOriginal;

            //If the bpm is too fast let's double the length just because it seems sensible.
            if (beatLength <= 333)
                beatLength *= 2;

            switch (beatmap.BeatmapInfo.Countdown)
            {
                case CountdownType.DoubleSpeed:
                    beatLength /= 2;
                    break;

                case CountdownType.HalfSpeed:
                    beatLength *= 2;
                    break;

                case CountdownType.None:
                    return CountdownTimings.None;

                case CountdownType.Normal:
                    break; // don't do anything special

                default:
                    throw new ArgumentOutOfRangeException($"Unknown countdown type {beatmap.BeatmapInfo.Countdown}");
            }

            // Push the countdown back by the mapper's countdown offset before any other adjustments.
            firstObject -= beatLength * beatmap.BeatmapInfo.CountdownOffset;

            // Skip back until we are at a good place
            if (goTime >= firstObject)
            {
                while (goTime > firstObject - beatLength)
                    goTime -= beatLength;
            }

            int divisor = 1;

            while (goTime < firstObject - beatLength / divisor)
            {
                goTime += beatLength;
                double beat = ((float)(goTime - offset) / beatLengthOriginal) % 4;
                if (beat > 1.5 && beat < 3.5)
                    divisor = 2;
                else
                    divisor = 1;
            }

            goTime -= beatLength;

            bool useCountdown = goTime - 4 * beatLength > 0;
            double skipBoundary = goTime - 6 * beatLength;

            return new CountdownTimings(goTime, beatLength, useCountdown, skipBoundary);
        }
    }

    public class CountdownTimings
    {
        /// <summary>
        /// The time at which the beatmap starts. Referred to as "goTime" in stable.
        /// </summary>
        public readonly double StartTime;

        /// <summary>
        /// The beat length used in the countdown, affected by the countdown type and other factors.
        /// </summary>
        public readonly double BeatLength;

        /// <summary>
        /// Whether or not there is enough time to display a countdown.
        /// </summary>
        public readonly bool UseCountdown;

        /// <summary>
        /// The point to skip to in a song if the user presses the skip button.
        /// This is 6 beats before the start time.
        /// </summary>
        public readonly double SkipBoundaryTime;

        public static CountdownTimings None = new CountdownTimings(0, 0, false, 0);

        public CountdownTimings(double startTime, double beatLength, bool useCountdown, double skipBoundaryTime)
        {
            StartTime = startTime;
            BeatLength = beatLength;
            UseCountdown = useCountdown;
            SkipBoundaryTime = skipBoundaryTime;
        }
    }
}
