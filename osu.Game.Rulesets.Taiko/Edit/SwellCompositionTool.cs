// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Taiko.Edit.Blueprints;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class SwellCompositionTool : HitObjectCompositionTool
    {
        public SwellCompositionTool()
            : base(nameof(Swell))
        {
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners);

        public override PlacementBlueprint CreatePlacementBlueprint() => new SwellPlacementBlueprint();
    }
}
