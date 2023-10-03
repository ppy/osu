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
        private const float spin_error = MathF.PI / 8;

        private readonly SpinFramesGenerator? last;
        private readonly float startAngle;
        private readonly float endAngle;
        private readonly double duration;

        private SpinFramesGenerator(float startAngle)
            : this(null, startAngle, 0)
        {
        }

        private SpinFramesGenerator(SpinFramesGenerator? last, float endAngle, double duration)
        {
            this.last = last;
            startAngle = last?.endAngle ?? endAngle;
            this.endAngle = endAngle;
            this.duration = duration;
        }

        public List<ReplayFrame> Build(double spinnerStartTime, float spinOffset)
        {
            List<ReplayFrame> frames = new List<ReplayFrame>();

            List<SpinFramesGenerator> allGenerators = new List<SpinFramesGenerator>();

            SpinFramesGenerator? l = this;

            while (l != null)
            {
                allGenerators.Add(l);
                l = l.last;
            }

            allGenerators.Reverse();

            double currentTime = spinnerStartTime;

            foreach (var gen in allGenerators)
            {
                double startTime = currentTime;
                double endTime = currentTime + gen.duration;

                for (; currentTime < endTime; currentTime += 10)
                    frames.Add(new OsuReplayFrame(currentTime, calcOffsetAt(gen, spinOffset, (currentTime - startTime) / (endTime - startTime)), OsuAction.LeftButton));

                frames.Add(new OsuReplayFrame(currentTime, calcOffsetAt(gen, spinOffset, 1), OsuAction.LeftButton));
            }

            frames.Add(new OsuReplayFrame(currentTime, calcOffsetAt(this, spinOffset, 1)));

            return frames;
        }

        private static Vector2 calcOffsetAt(SpinFramesGenerator generator, float spinOffset, double p)
        {
            float angle = generator.startAngle + (generator.endAngle - generator.startAngle) * (float)p;
            return new Vector2(256, 192) + spinOffset * new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        public static SpinFramesGenerator From(float startAngle) => new SpinFramesGenerator(startAngle - MathF.PI / 2f);

        public SpinFramesGenerator Spin(float amount, double duration) => new SpinFramesGenerator(this, endAngle + amount * 2 * MathF.PI + spin_error, duration);
    }
}
