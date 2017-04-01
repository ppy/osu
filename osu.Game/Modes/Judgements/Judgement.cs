﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Judgements
{
    public abstract class Judgement
    {
        /// <summary>
        /// Whether this judgement is the result of a hit or a miss.
        /// </summary>
        public HitResult Result;

        /// <summary>
        /// The offset at which this judgement occurred.
        /// </summary>
        public double TimeOffset;

        /// <summary>
        /// The combo after this judgement was processed.
        /// </summary>
        public int ComboAtHit;

        /// <summary>
        /// The string representation for the result achieved.
        /// </summary>
        public abstract string ResultString { get; }

        /// <summary>
        /// The string representation for the max result achievable.
        /// </summary>
        public abstract string MaxResultString { get; }
    }
}