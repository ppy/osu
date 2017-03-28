// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK.Input;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableStrongRimHit : DrawableStrongHit
    {
        protected override List<Key> HitKeys { get; } = new List<Key>(new[] { Key.D, Key.K });

        public DrawableStrongRimHit(Hit hit)
            : base(hit)
        {
            Add(new RimHitCirclePiece(new StrongCirclePiece()));
        }
    }
}
