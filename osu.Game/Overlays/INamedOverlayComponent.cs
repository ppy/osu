// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Overlays
{
    public interface INamedOverlayComponent
    {
        IconUsage Icon { get; }

        LocalisableString Title { get; }

        LocalisableString Description { get; }
    }
}
