// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Objects.Drawables
{
    public interface IDrawableHitObjectWithProxiedApproach
    {
        Drawable ProxiedLayer { get; }
    }
}
