// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Objects.Legacy
{
    /// <summary>
    /// Legacy Hold-type, used for parsing "specials" in beatmaps.
    /// </summary>
    public sealed class LegacyHold : HitObject, IHasPosition, IHasCombo, IHasHold
    {
        public Vector2 Position { get; set; }

        public bool NewCombo { get; set; }
    }
}
