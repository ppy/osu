// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Screens.Play
{
    [Cached]
    public interface ILocalUserPlayInfo
    {
        /// <summary>
        /// Whether the local user is currently playing.
        /// </summary>
        IBindable<bool> IsPlaying { get; }
    }
}
