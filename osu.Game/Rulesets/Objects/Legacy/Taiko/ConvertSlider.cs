// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Legacy.Taiko
{
    /// <summary>
    /// Legacy osu!taiko Slider-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSlider : Legacy.ConvertSlider
    {
        protected override HitObject CreateInstance() => new ConvertSlider();
    }
}
