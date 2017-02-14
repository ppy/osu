// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;

namespace osu.Game.Modes.Objects.Drawables
{
    public interface IDrawableHitObjectWithProxiedApproach
    {
        Drawable ProxiedLayer { get; }
    }
}
