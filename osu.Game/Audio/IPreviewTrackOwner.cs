// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Audio
{
    /// <summary>
    /// Interface for objects that can own <see cref="PreviewTrack"/>s.
    /// </summary>
    /// <remarks>
    /// <see cref="IPreviewTrackOwner"/>s can cancel the currently playing <see cref="PreviewTrack"/> through the
    /// global <see cref="PreviewTrackManager"/> if they're the owner of the playing <see cref="PreviewTrack"/>.
    /// </remarks>
    public interface IPreviewTrackOwner
    {
    }
}
