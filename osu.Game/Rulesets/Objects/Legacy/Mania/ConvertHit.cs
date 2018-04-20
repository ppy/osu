// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    /// <summary>
    /// Legacy osu!mania Hit-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertHit : HitObject, IHasXPosition, IHasCombo
    {
        public float X { get; set; }

        public bool NewCombo { get; set; }
    }
}
