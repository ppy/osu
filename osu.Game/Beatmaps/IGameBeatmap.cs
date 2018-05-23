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
    }
}
