// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Streams;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class StreamCompositionTool : HitObjectCompositionTool
    {
        public StreamCompositionTool()
            : base(@"Stream")
        {
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Streams);

        public override PlacementBlueprint CreatePlacementBlueprint() => new StreamPlacementBlueprint();
    }
}
