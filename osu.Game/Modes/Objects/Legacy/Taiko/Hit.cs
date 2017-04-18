// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Objects.Legacy.Taiko
{
    /// <summary>
    /// Legacy osu!taiko Hit-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class Hit : HitObject, IHasCombo
    {
        public bool NewCombo { get; set; }
    }
}
