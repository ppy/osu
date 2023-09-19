// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public interface IHasCustomColours
    {
        Dictionary<string, Color4> CustomColours { get; }
    }
}
