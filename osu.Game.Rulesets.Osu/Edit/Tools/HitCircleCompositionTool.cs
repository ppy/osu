// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Tools
{
    public class HitCircleCompositionTool : HitObjectCompositionTool
    {
        private readonly IBeatmap beatmap;

        public HitCircleCompositionTool(IBeatmap beatmap)
            : base(nameof(HitCircle))
        {
            this.beatmap = beatmap;
        }

        public override PlacementVisualiser CreatePlacementVisualiser() => new HitCirclePlacementVisualiser(beatmap);
    }
}
