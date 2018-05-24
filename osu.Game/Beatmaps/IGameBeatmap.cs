// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Read-only interface for the <see cref="OsuGame"/> beatmap.
    /// </summary>
    public interface IGameBeatmap : IBindable<WorkingBeatmap>
    {
        /// <summary>
        /// Retrieve a new <see cref="IGameBeatmap"/> instance weakly bound to this <see cref="IGameBeatmap"/>.
        /// If you are further binding to events of the retrieved <see cref="IGameBeatmap"/>, ensure a local reference is held.
        /// </summary>
        IGameBeatmap GetBoundCopy();
    }
}
