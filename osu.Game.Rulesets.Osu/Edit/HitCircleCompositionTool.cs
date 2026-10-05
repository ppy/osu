// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation.Osu;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class HitCircleCompositionTool : CompositionTool<OsuAction>
    {
        public HitCircleCompositionTool()
            : base(OsuEditorStrings.HitCircleTool)
        {
            Action = OsuAction.EditorHitCircleTool;
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.EditorHitCircle };

        public override HitObjectPlacementBlueprint CreatePlacementBlueprint() => new HitCirclePlacementBlueprint();
    }
}
