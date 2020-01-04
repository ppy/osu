// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit.Masks
{
    public abstract class ManiaSelectionBlueprint : SelectionBlueprint
    {
        protected ManiaSelectionBlueprint(DrawableHitObject drawableObject)
            : base(drawableObject)
        {
            RelativeSizeAxes = Axes.None;
        }
    }
}
