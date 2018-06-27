// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Audio
{
    /// <summary>
    /// Interface for objects that can own <see cref="IPreviewTrack"/>s.
    /// </summary>
    /// <remarks>
    /// <see cref="IPreviewTrackOwner"/>s can cancel the currently playing <see cref="PreviewTrack"/> through the
    /// global <see cref="PreviewTrackManager"/> if they're the owner of the playing <see cref="PreviewTrack"/>.
    /// </remarks>
    public interface IPreviewTrackOwner
    {
    }
}
