// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Edit.Tools
{
    public abstract class HitObjectCompositionTool
    {
        public readonly string Name;

        protected HitObjectCompositionTool(string name)
        {
            Name = name;
        }

        public abstract PlacementBlueprint CreatePlacementBlueprint();

        public virtual Drawable CreateIcon() => null;

        public override string ToString() => Name;
    }
}
