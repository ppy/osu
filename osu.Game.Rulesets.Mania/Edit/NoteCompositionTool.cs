// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class NoteCompositionTool : HitObjectCompositionTool
    {
        public NoteCompositionTool()
            : base(nameof(Note))
        {
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles);

        public override PlacementBlueprint CreatePlacementBlueprint() => new NotePlacementBlueprint();
    }
}
