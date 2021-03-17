// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays
{
    public interface INamedOverlayComponent
    {
        string IconTexture { get; }

        string Title { get; }

        string Description { get; }
    }
}
