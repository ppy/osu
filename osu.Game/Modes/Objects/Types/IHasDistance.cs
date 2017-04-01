﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that has a positional length.
    /// </summary>
    public interface IHasDistance : IHasEndTime
    {
        /// <summary>
        /// The positional length of the HitObject.
        /// </summary>
        double Distance { get; }
    }
}
