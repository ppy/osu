// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects.Legacy.Catch
{
    /// <summary>
    /// Legacy osu!catch Slider-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSlider : Legacy.ConvertSlider, IHasPosition
    {
        public float X => Position.X;

        public float Y => Position.Y;

        public Vector2 Position { get; set; }
    }
}
