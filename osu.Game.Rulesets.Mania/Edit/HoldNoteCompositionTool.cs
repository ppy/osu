// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Edit.Blueprints;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class HoldNoteCompositionTool : HitObjectCompositionTool
    {
        private ManiaBeatmap Beatmap;
        public HoldNoteCompositionTool(ManiaBeatmap beatmap)
            : base("Hold")
        {
            Beatmap = beatmap;
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders);

        public override PlacementBlueprint CreatePlacementBlueprint() => new HoldNotePlacementBlueprint(Beatmap);
    }
}
