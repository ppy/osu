// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy
{
    /// <summary>
    /// Legacy Hold-type, used for parsing "specials" in beatmaps.
    /// </summary>
    internal sealed class ConvertHold : HitObject, IHasPosition, IHasCombo, IHasHold
    {
        public Vector2 Position { get; set; }

        public float X => Position.X;

        public float Y => Position.Y;

        public bool NewCombo { get; set; }
    }
}
