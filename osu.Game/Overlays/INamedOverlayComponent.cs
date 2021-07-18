// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Overlays
{
    public interface INamedOverlayComponent
    {
        string IconTexture { get; }

        LocalisableString Title { get; }

        /// <summary>
        /// An optional name used specifically for toolbar overlay toggle buttons.
        /// If null, <see cref="Title"/> should be used directly instead.
        /// </summary>
        LocalisableString? ToolbarName => null;

        LocalisableString Description { get; }
    }
}
