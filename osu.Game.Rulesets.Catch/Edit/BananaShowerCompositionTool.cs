// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;

namespace osu.Game.Rulesets.Catch.Edit
{
    public class BananaShowerCompositionTool : HitObjectCompositionTool
    {
        public BananaShowerCompositionTool()
            : base(nameof(BananaShower))
        {
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners);

        public override PlacementBlueprint CreatePlacementBlueprint() => new BananaShowerPlacementBlueprint();
    }
}
