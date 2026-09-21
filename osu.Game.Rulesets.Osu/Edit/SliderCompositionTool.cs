// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation.Osu;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class SliderCompositionTool : CompositionTool<OsuAction>
    {
        public SliderCompositionTool()
            : base(OsuEditorStrings.SliderTool)
        {
            TooltipText = """
                Left click for new point.
                Left click twice or S key for new segment.
                Tab, Shift-Tab, or Alt-1~4 to change current segment type.
                Right click to finish.
                Click and drag for drawing mode.
                """;
            Action = OsuAction.EditorSliderTool;
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = OsuIcon.EditorSlider };

        public override HitObjectPlacementBlueprint CreatePlacementBlueprint() => new SliderPlacementBlueprint();
    }
}
