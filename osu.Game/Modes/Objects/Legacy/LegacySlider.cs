// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Types;
using OpenTK;

namespace osu.Game.Modes.Objects.Legacy
{
    /// <summary>
    /// Legacy Slider-type, used for parsing Beatmaps.
    /// </summary>
    public sealed class LegacySlider : CurvedHitObject, IHasPosition, IHasCombo
    {
        public Vector2 Position { get; set; }

        public bool NewCombo { get; set; }
    }
}
