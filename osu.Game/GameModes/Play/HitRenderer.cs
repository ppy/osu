//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Objects;

namespace osu.Game.GameModes.Play
{
    public abstract class HitRenderer : LargeContainer
    {
        public abstract List<BaseHit> Objects { get; set; }
    }
}
