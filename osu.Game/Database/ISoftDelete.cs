﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Database
{
    /// <summary>
    /// A model that can be deleted from user's view without being instantly lost.
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Whether this model is marked for future deletion.
        /// </summary>
        bool DeletePending { get; set; }
    }
}
