// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Objects.Legacy.Catch
{
    /// <summary>
    /// Legacy osu!catch Slider-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class Slider : CurvedHitObject, IHasXPosition, IHasCombo
    {
        public float X { get; set; }

        public bool NewCombo { get; set; }
    }
}
