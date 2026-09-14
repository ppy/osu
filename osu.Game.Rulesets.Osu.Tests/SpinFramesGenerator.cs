// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class SpinFramesGenerator
    {
        /// <summary>
        /// A small amount to spin beyond a given angle to mitigate floating-point precision errors.
        /// </summary>
        public const float SPIN_ERROR = 1;

        /// <summary>
        /// The offset from the centre of the spinner at which to spin.
        /// </summary>
        private const float centre_spin_offset = 50;

        private readonly double startTime;
        private readonly float startAngle;
        private readonly List<(float deltaAngle, double duration)> sequences = new List<(float deltaAngle, double duration)>();

        /// <summary>
        /// Creates a new <see cref="SpinFramesGenerator"/> that can be used to generate spinner spin frames.
        /// </summary>
        /// <param name="startTime">The time at which to start spinning.</param>
        /// <param name="startAngle">The angle, in radians, at which to start spinning from. Defaults to the positive-y-axis.</param>
        public SpinFramesGenerator(double startTime, float startAngle = -MathF.PI / 2f)
        {
            this.startTime = startTime;
            this.startAngle = startAngle;
        }

        /// <summary>
        /// Performs a single spin.
        /// </summary>
        /// <param name="delta">The amount of degrees to spin.</param>
        /// <param name="duration">The time to spend to perform the spin.</param>
        /// <returns>This <see cref="SpinFramesGenerator"/>.</returns>
        public SpinFramesGenerator Spin(float delta, double duration)
        {
            sequences.Add((delta / 360 * 2 * MathF.PI, duration));
            return this;
        }

        /// <summary>
        /// Constructs the replay frames.
        /// </summary>
        /// <returns>The replay frames.</returns>
        public List<ReplayFrame> Build()
        {
            List<ReplayFrame> frames = new List<ReplayFrame>();

            double lastTime = startTime;
            float lastAngle = startAngle;
            int lastDirection = 0;

            for (int i = 0; i < sequences.Count; i++)
            {
                var seq = sequences[i];

                int seqDirection = Math.Sign(seq.deltaAngle);
                float seqError = SPIN_ERROR * seqDirection;

                if (seqDirection == lastDirection)
                {
                    // Spinning in the same direction, but the error was already added in the last rotation.
                    seqError = 0;
                }
                else if (lastDirection != 0)
                {
                    // Spinning in a different direction, we need to account for the error of the start angle, so double it.
                    seqError *= 2;
                }

                double seqStartTime = lastTime;
                double seqEndTime = lastTime + seq.duration;
                float seqStartAngle = lastAngle;
                float seqEndAngle = seqStartAngle + seq.deltaAngle + seqError;

                // Intermediate spin frames.
                for (; lastTime < seqEndTime; lastTime += 10)
                    frames.Add(new OsuReplayFrame(lastTime, calcOffsetAt((lastTime - seqStartTime) / (seqEndTime - seqStartTime), seqStartAngle, seqEndAngle), OsuAction.LeftButton));

                // Final frame at the end of the current spin.
                frames.Add(new OsuReplayFrame(seqEndTime, calcOffsetAt(1, seqStartAngle, seqEndAngle), OsuAction.LeftButton));

                lastTime = seqEndTime;
                lastAngle = seqEndAngle;
                lastDirection = seqDirection;
            }

            // Key release frame.
            if (frames.Count > 0)
                frames.Add(new OsuReplayFrame(frames[^1].Time, ((OsuReplayFrame)frames[^1]).Position));

            return frames;
        }

        private static Vector2 calcOffsetAt(double p, float startAngle, float endAngle)
        {
            float angle = startAngle + (endAngle - startAngle) * (float)p;
            return new Vector2(256, 192) + centre_spin_offset * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }
    }
}
