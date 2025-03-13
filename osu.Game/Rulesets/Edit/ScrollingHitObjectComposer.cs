// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    public abstract partial class ScrollingHitObjectComposer<TObject> : HitObjectComposer<TObject>
        where TObject : HitObject
    {
        [Resolved]
        private Editor? editor { get; set; }

        private readonly Bindable<TernaryState> showSpeedChanges = new Bindable<TernaryState>();
        private Bindable<bool> configShowSpeedChanges = null!;

        private BeatSnapGrid? beatSnapGrid;

        /// <summary>
        /// Construct an optional beat snap grid.
        /// </summary>
        protected virtual BeatSnapGrid? CreateBeatSnapGrid() => null;

        protected ScrollingHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            if (DrawableRuleset is ISupportConstantAlgorithmToggle toggleRuleset)
            {
                LeftToolbox.Add(new EditorToolboxGroup("playfield")
                {
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new[]
                        {
                            new DrawableTernaryButton
                            {
                                Current = showSpeedChanges,
                                Description = "Show speed changes",
                                CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.TachometerAlt },
                            }
                        }
                    },
                });

                configShowSpeedChanges = config.GetBindable<bool>(OsuSetting.EditorShowSpeedChanges);
                configShowSpeedChanges.BindValueChanged(enabled => showSpeedChanges.Value = enabled.NewValue ? TernaryState.True : TernaryState.False, true);

                showSpeedChanges.BindValueChanged(state =>
                {
                    bool enabled = state.NewValue == TernaryState.True;

                    toggleRuleset.ShowSpeedChanges.Value = enabled;
                    configShowSpeedChanges.Value = enabled;
                }, true);
            }

            beatSnapGrid = CreateBeatSnapGrid();

            if (beatSnapGrid != null)
                AddInternal(beatSnapGrid);

            EditorBeatmap.ControlPointInfo.ControlPointsChanged += expireComposeScreenOnControlPointChange;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            updateBeatSnapGrid();
        }

        private void updateBeatSnapGrid()
        {
            if (beatSnapGrid == null)
                return;

            if (BlueprintContainer.CurrentTool is SelectTool)
            {
                if (EditorBeatmap.SelectedHitObjects.Any())
                {
                    beatSnapGrid.SelectionTimeRange = (EditorBeatmap.SelectedHitObjects.Min(h => h.StartTime), EditorBeatmap.SelectedHitObjects.Max(h => h.GetEndTime()));
                }
                else
                    beatSnapGrid.SelectionTimeRange = null;
            }
            else
            {
                var result = FindSnappedPositionAndTime(InputManager.CurrentState.Mouse.Position);
                if (result.Time is double time)
                    beatSnapGrid.SelectionTimeRange = (time, time);
                else
                    beatSnapGrid.SelectionTimeRange = null;
            }
        }

        public SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition)
        {
            var scrollingPlayfield = PlayfieldAtScreenSpacePosition(screenSpacePosition) as ScrollingPlayfield;
            if (scrollingPlayfield == null)
                return new SnapResult(screenSpacePosition, null);

            double? targetTime = scrollingPlayfield.TimeAtScreenSpacePosition(screenSpacePosition);

            // apply beat snapping
            targetTime = BeatSnapProvider.SnapTime(targetTime.Value);

            // convert back to screen space
            screenSpacePosition = scrollingPlayfield.ScreenSpacePositionAtTime(targetTime.Value);

            return new SnapResult(screenSpacePosition, targetTime, scrollingPlayfield);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (EditorBeatmap.IsNotNull())
                EditorBeatmap.ControlPointInfo.ControlPointsChanged -= expireComposeScreenOnControlPointChange;
        }

        private void expireComposeScreenOnControlPointChange() => editor?.ReloadComposeScreen();
    }
}
