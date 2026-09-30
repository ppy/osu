// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation.Taiko;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Taiko.Edit.Blueprints;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class HitCompositionTool : CompositionTool<TaikoAction>
    {
        public HitCompositionTool()
            : base(TaikoEditorStrings.HitTool)
        {
            Action = TaikoAction.EditorHitTool;
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.EditorHit };

        public override HitObjectPlacementBlueprint CreatePlacementBlueprint() => new HitPlacementBlueprint();
    }
}
