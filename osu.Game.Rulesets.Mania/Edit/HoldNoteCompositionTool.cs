// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Edit.Blueprints;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class HoldNoteCompositionTool : CompositionTool
    {
        public HoldNoteCompositionTool()
            : base("Hold")
        {
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.EditorHoldNote };

        public override HitObjectPlacementBlueprint CreatePlacementBlueprint() => new HoldNotePlacementBlueprint();
    }
}
