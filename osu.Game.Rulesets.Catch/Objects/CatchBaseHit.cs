// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects
{
    public abstract class CatchBaseHit : HitObject, IHasXPosition, IHasCombo
    {
        public float X { get; set; }

        public Color4 ComboColour { get; set; } = Color4.Gray;
        public int ComboIndex { get; set; }

        public virtual bool NewCombo { get; set; }

        /// <summary>
        /// The next fruit starts a new combo. Used for explodey.
        /// </summary>
        public virtual bool LastInCombo { get; set; }
    }
}
