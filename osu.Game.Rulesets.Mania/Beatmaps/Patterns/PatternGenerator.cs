// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Logging;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns
{
    /// <summary>
    /// Generator to create a pattern <see cref="Pattern"/> from a hit object.
    /// </summary>
    internal abstract class PatternGenerator
    {
        /// <summary>
        /// An arbitrary maximum amount of iterations to perform in <see cref="RunWhile"/>.
        /// The specific value is not super important - enough such that no false-positives occur.
        /// </summary>
        private const int max_rng_iterations = 20;

        /// <summary>
        /// The last pattern.
        /// </summary>
        protected readonly Pattern PreviousPattern;

        /// <summary>
        /// The hit object to create the pattern for.
        /// </summary>
        protected readonly HitObject HitObject;

        /// <summary>
        /// The beatmap which <see cref="HitObject"/> is a part of.
        /// </summary>
        protected readonly ManiaBeatmap Beatmap;

        protected readonly int TotalColumns;

        protected PatternGenerator(HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern)
        {
            if (hitObject == null) throw new ArgumentNullException(nameof(hitObject));
            if (beatmap == null) throw new ArgumentNullException(nameof(beatmap));
            if (previousPattern == null) throw new ArgumentNullException(nameof(previousPattern));

            HitObject = hitObject;
            Beatmap = beatmap;
            PreviousPattern = previousPattern;

            TotalColumns = Beatmap.TotalColumns;
        }

        protected void RunWhile([InstantHandle] Func<bool> condition, Action action)
        {
            int iterations = 0;

            while (condition())
            {
                if (iterations++ >= max_rng_iterations)
                {
                    // log an error but don't throw. we want to continue execution.
                    Logger.Error(new ExceededAllowedIterationsException(new StackTrace(0)),
                        "Conversion encountered errors. The beatmap may not be correctly converted.");
                    return;
                }

                action();
            }
        }

        /// <summary>
        /// Generates the patterns for <see cref="HitObject"/>, each filled with hit objects.
        /// </summary>
        /// <returns>The <see cref="Pattern"/>s containing the hit objects.</returns>
        public abstract IEnumerable<Pattern> Generate();

        /// <summary>
        /// Denotes when a single conversion operation is in an infinitely looping state.
        /// </summary>
        public class ExceededAllowedIterationsException : Exception
        {
            private readonly string stackTrace;

            public ExceededAllowedIterationsException(StackTrace stackTrace)
            {
                this.stackTrace = stackTrace.ToString();
            }

            public override string StackTrace => stackTrace;
            public override string ToString() => $"{GetType().Name}: {Message}\r\n{StackTrace}";
        }
    }
}
