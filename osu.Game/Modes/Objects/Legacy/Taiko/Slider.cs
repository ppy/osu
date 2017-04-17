// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Objects.Legacy.Taiko
{
    /// <summary>
    /// Legacy Slider-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class Slider : CurvedHitObject, IHasCombo
    {
        public bool NewCombo { get; set; }
    }
}
