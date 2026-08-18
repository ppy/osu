// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Edit
{
    [Cached]
    public partial class TaikoHitObjectComposer : ScrollingHitObjectComposer<TaikoHitObject, TaikoAction>
    {
        public override Bindable<TernaryState>? SelectionNewComboState => null;

        public IBindable<TernaryState> SelectionRimState => selectionRimState;
        private readonly Bindable<TernaryState> selectionRimState = new Bindable<TernaryState>();

        public IBindable<TernaryState> SelectionStrongState => selectionStrongState;
        private readonly Bindable<TernaryState> selectionStrongState = new Bindable<TernaryState>();

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
            setUpStateBindables();
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

        protected override IReadOnlyList<CompositionTool<TaikoAction>> CompositionTools => new CompositionTool<TaikoAction>[]
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

        #region Selection handling

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionRimState.Value = EditorBeatmap.SelectedHitObjects.OfType<Hit>().GetTernaryState(h => h.Type == HitType.Rim);
            selectionStrongState.Value = EditorBeatmap.SelectedHitObjects.OfType<TaikoStrongableHitObject>().GetTernaryState(h => h.IsStrong);
        }

        private void setUpStateBindables()
        {
            selectionStrongState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetStrongState(false);
                        break;

                    case TernaryState.True:
                        SetStrongState(true);
                        break;
                }
            };

            selectionRimState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetRimState(false);
                        break;

                    case TernaryState.True:
                        SetRimState(true);
                        break;
                }
            };
        }

        public void SetStrongState(bool state)
        {
            if (EditorBeatmap.SelectedHitObjects.OfType<TaikoStrongableHitObject>().All(h => h.IsStrong == state))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (h is not TaikoStrongableHitObject strongable) return;

                if (strongable.IsStrong != state)
                    strongable.IsStrong = state;
            });
        }

        public void SetRimState(bool state)
        {
            if (EditorBeatmap.SelectedHitObjects.OfType<Hit>().All(h => h.Type == (state ? HitType.Rim : HitType.Centre)))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (h is Hit taikoHit)
                    taikoHit.Type = state ? HitType.Rim : HitType.Centre;
            });
        }

        #endregion
    }
}
