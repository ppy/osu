// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public abstract class StageHint : CompositeDrawable, IHasAccentColour
    {
        public abstract Color4 AccentColour { get; set; }
    }
}
