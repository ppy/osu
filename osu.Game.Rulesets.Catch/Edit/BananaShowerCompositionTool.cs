// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;

namespace osu.Game.Rulesets.Catch.Edit
{
    public class BananaShowerCompositionTool : CompositionTool
    {
        public BananaShowerCompositionTool()
            : base("Banana shower")
        {
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.EditorBananaShower };

        public override HitObjectPlacementBlueprint CreatePlacementBlueprint() => new BananaShowerPlacementBlueprint();
    }
}
