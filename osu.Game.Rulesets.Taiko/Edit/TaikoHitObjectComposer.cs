// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Edit
{
    [Cached]
    public partial class TaikoHitObjectComposer : ScrollingHitObjectComposer<TaikoHitObject, TaikoAction>
    {
        protected override bool ApplyHorizontalCentering => false;

        private Bindable<bool> limitPlacementToCurrentTime = null!;

        public TaikoHitObjectComposer(TaikoRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            limitPlacementToCurrentTime = config.GetBindable<bool>(OsuSetting.EditorLimitedDistanceSnap);
        }

        public override SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition)
        {
            if (limitPlacementToCurrentTime.Value
                && BlueprintContainer.CurrentHitObjectPlacement?.PlacementActive == PlacementBlueprint.PlacementState.Waiting)
            {
                var playfield = (TaikoPlayfield)Playfield;
                double time = BeatSnapProvider.SnapTime(EditorClock.CurrentTime);
                return new SnapResult(playfield.ScreenSpacePositionAtTime(time), time, playfield);
            }

            return base.FindSnappedPositionAndTime(screenSpacePosition);
        }

        protected override IReadOnlyList<CompositionTool> CompositionTools => new CompositionTool[]
        {
            new HitCompositionTool(),
            new DrumRollCompositionTool(),
            new SwellCompositionTool()
        };

        protected override DrawableRuleset<TaikoHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawableTaikoEditorRuleset(ruleset, beatmap, mods);

        protected override ComposeBlueprintContainer CreateBlueprintContainer()
            => new TaikoBlueprintContainer(this);

        protected override BeatSnapGrid CreateBeatSnapGrid() => new TaikoBeatSnapGrid();
    }
}
