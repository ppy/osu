// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class SliderCompositionTool : CompositionTool
    {
        public SliderCompositionTool()
            : base(nameof(Slider))
        {
            TooltipText = """
                Left click for new point.
                Left click twice or S key for new segment.
                Tab, Shift-Tab, or Alt-1~4 to change current segment type.
                Right click to finish.
                Click and drag for drawing mode.
                """;
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders);

        public override HitObjectPlacementBlueprint CreatePlacementBlueprint() => new SliderPlacementBlueprint();
    }
}
