using OpenTK;
// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Catch.Objects
{
    public abstract class CatchBaseHit : HitObject, IHasPosition
    {
        public Vector2 Position { get; set; }
    }
}
