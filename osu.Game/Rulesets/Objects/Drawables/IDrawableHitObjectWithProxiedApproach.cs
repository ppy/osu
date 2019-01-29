// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Objects.Drawables
{
    public interface IDrawableHitObjectWithProxiedApproach
    {
        Drawable ProxiedLayer { get; }
    }
}
