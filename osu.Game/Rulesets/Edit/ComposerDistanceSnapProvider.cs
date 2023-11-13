// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.TernaryButtons;

namespace osu.Game.Rulesets.Edit
{
    public abstract partial class ComposerDistanceSnapProvider : Component, IDistanceSnapProvider, IScrollBindingHandler<GlobalAction>, IToolboxAttachment
    {
        private const float adjust_step = 0.1f;

        public BindableDouble DistanceSpacingMultiplier { get; } = new BindableDouble(1.0)
        {
            MinValue = 0.1,
            MaxValue = 6.0,
            Precision = 0.01,
        };

        Bindable<double> IDistanceSnapProvider.DistanceSpacingMultiplier => DistanceSpacingMultiplier;

        private ExpandableSlider<double, SizeSlider<double>> distanceSpacingSlider = null!;
        private ExpandableButton currentDistanceSpacingButton = null!;

        [Resolved]
        private Playfield playfield { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

        [Resolved]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        public readonly Bindable<TernaryState> DistanceSnapToggle = new Bindable<TernaryState>();

        private bool distanceSnapMomentary;

        private EditorToolboxGroup? toolboxGroup;

        public void AttachToToolbox(ExpandingToolboxContainer toolboxContainer)
        {
            if (toolboxGroup != null)
                throw new InvalidOperationException($"{nameof(AttachToToolbox)} may be called only once for a single {nameof(ComposerDistanceSnapProvider)} instance.");

            toolboxContainer.Add(toolboxGroup = new EditorToolboxGroup("snapping")
            {
                Alpha = DistanceSpacingMultiplier.Disabled ? 0 : 1,
                Children = new Drawable[]
                {
                    distanceSpacingSlider = new ExpandableSlider<double, SizeSlider<double>>
                    {
                        KeyboardStep = adjust_step,
                        // Manual binding in LoadComplete to handle one-way event flow.
                        Current = DistanceSpacingMultiplier.GetUnboundCopy(),
                    },
                    currentDistanceSpacingButton = new ExpandableButton
                    {
                        Action = () =>
                        {
                            (HitObject before, HitObject after)? objects = getObjectsOnEitherSideOfCurrentTime();

                            Debug.Assert(objects != null);

                            DistanceSpacingMultiplier.Value = ReadCurrentDistanceSnap(objects.Value.before, objects.Value.after);
                            DistanceSnapToggle.Value = TernaryState.True;
                        },
                        RelativeSizeAxes = Axes.X,
                    }
                }
            });

            DistanceSpacingMultiplier.Value = editorBeatmap.BeatmapInfo.DistanceSpacing;
            DistanceSpacingMultiplier.BindValueChanged(multiplier =>
            {
                distanceSpacingSlider.ContractedLabelText = $"D. S. ({multiplier.NewValue:0.##x})";
                distanceSpacingSlider.ExpandedLabelText = $"Distance Spacing ({multiplier.NewValue:0.##x})";

                if (multiplier.NewValue != multiplier.OldValue)
                    onScreenDisplay?.Display(new DistanceSpacingToast(multiplier.NewValue.ToLocalisableString(@"0.##x"), multiplier));

                editorBeatmap.BeatmapInfo.DistanceSpacing = multiplier.NewValue;
            }, true);

            DistanceSpacingMultiplier.BindDisabledChanged(disabled => distanceSpacingSlider.Alpha = disabled ? 0 : 1, true);

            // Manual binding to handle enabling distance spacing when the slider is interacted with.
            distanceSpacingSlider.Current.BindValueChanged(spacing =>
            {
                DistanceSpacingMultiplier.Value = spacing.NewValue;
                DistanceSnapToggle.Value = TernaryState.True;
            });
            DistanceSpacingMultiplier.BindValueChanged(spacing => distanceSpacingSlider.Current.Value = spacing.NewValue);
        }

        private (HitObject before, HitObject after)? getObjectsOnEitherSideOfCurrentTime()
        {
            HitObject? lastBefore = playfield.HitObjectContainer.AliveObjects.LastOrDefault(h => h.HitObject.StartTime < editorClock.CurrentTime)?.HitObject;

            if (lastBefore == null)
                return null;

            HitObject? firstAfter = playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.HitObject.StartTime >= editorClock.CurrentTime)?.HitObject;

            if (firstAfter == null)
                return null;

            if (lastBefore == firstAfter)
                return null;

            return (lastBefore, firstAfter);
        }

        protected abstract double ReadCurrentDistanceSnap(HitObject before, HitObject after);

        protected override void Update()
        {
            base.Update();

            (HitObject before, HitObject after)? objects = getObjectsOnEitherSideOfCurrentTime();

            double currentSnap = objects == null
                ? 0
                : ReadCurrentDistanceSnap(objects.Value.before, objects.Value.after);

            if (currentSnap > DistanceSpacingMultiplier.MinValue)
            {
                currentDistanceSpacingButton.Enabled.Value = currentDistanceSpacingButton.Expanded.Value
                                                             && !DistanceSpacingMultiplier.Disabled
                                                             && !Precision.AlmostEquals(currentSnap, DistanceSpacingMultiplier.Value, DistanceSpacingMultiplier.Precision / 2);
                currentDistanceSpacingButton.ContractedLabelText = $"current {currentSnap:N2}x";
                currentDistanceSpacingButton.ExpandedLabelText = $"Use current ({currentSnap:N2}x)";
            }
            else
            {
                currentDistanceSpacingButton.Enabled.Value = false;
                currentDistanceSpacingButton.ContractedLabelText = string.Empty;
                currentDistanceSpacingButton.ExpandedLabelText = "Use current (unavailable)";
            }
        }

        public IEnumerable<TernaryButton> CreateTernaryButtons() => new[]
        {
            new TernaryButton(DistanceSnapToggle, "Distance Snap", () => new SpriteIcon { Icon = FontAwesome.Solid.Ruler })
        };

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat)
                return false;

            handleToggleViaKey(e);
            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            handleToggleViaKey(e);
            base.OnKeyUp(e);
        }

        private void handleToggleViaKey(KeyboardEvent key)
        {
            bool altPressed = key.AltPressed;

            if (altPressed != distanceSnapMomentary)
            {
                distanceSnapMomentary = altPressed;
                DistanceSnapToggle.Value = DistanceSnapToggle.Value == TernaryState.False ? TernaryState.True : TernaryState.False;
            }
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorIncreaseDistanceSpacing:
                case GlobalAction.EditorDecreaseDistanceSpacing:
                    return AdjustDistanceSpacing(e.Action, adjust_step);
            }

            return false;
        }

        public virtual void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public bool OnScroll(KeyBindingScrollEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorIncreaseDistanceSpacing:
                case GlobalAction.EditorDecreaseDistanceSpacing:
                    return AdjustDistanceSpacing(e.Action, e.ScrollAmount * adjust_step);
            }

            return false;
        }

        protected virtual bool AdjustDistanceSpacing(GlobalAction action, float amount)
        {
            if (DistanceSpacingMultiplier.Disabled)
                return false;

            if (action == GlobalAction.EditorIncreaseDistanceSpacing)
                DistanceSpacingMultiplier.Value += amount;
            else if (action == GlobalAction.EditorDecreaseDistanceSpacing)
                DistanceSpacingMultiplier.Value -= amount;

            DistanceSnapToggle.Value = TernaryState.True;
            return true;
        }

        #region IDistanceSnapProvider

        public virtual float GetBeatSnapDistanceAt(HitObject referenceObject, bool useReferenceSliderVelocity = true)
        {
            return (float)(100 * (useReferenceSliderVelocity && referenceObject is IHasSliderVelocity hasSliderVelocity ? hasSliderVelocity.SliderVelocityMultiplier : 1) * editorBeatmap.Difficulty.SliderMultiplier * 1
                           / beatSnapProvider.BeatDivisor);
        }

        public virtual float DurationToDistance(HitObject referenceObject, double duration)
        {
            double beatLength = beatSnapProvider.GetBeatLengthAtTime(referenceObject.StartTime);
            return (float)(duration / beatLength * GetBeatSnapDistanceAt(referenceObject));
        }

        public virtual double DistanceToDuration(HitObject referenceObject, float distance)
        {
            double beatLength = beatSnapProvider.GetBeatLengthAtTime(referenceObject.StartTime);
            return distance / GetBeatSnapDistanceAt(referenceObject) * beatLength;
        }

        public virtual double FindSnappedDuration(HitObject referenceObject, float distance)
            => beatSnapProvider.SnapTime(referenceObject.StartTime + DistanceToDuration(referenceObject, distance), referenceObject.StartTime) - referenceObject.StartTime;

        public virtual float FindSnappedDistance(HitObject referenceObject, float distance)
        {
            double startTime = referenceObject.StartTime;

            double actualDuration = startTime + DistanceToDuration(referenceObject, distance);

            double snappedEndTime = beatSnapProvider.SnapTime(actualDuration, startTime);

            double beatLength = beatSnapProvider.GetBeatLengthAtTime(startTime);

            // we don't want to exceed the actual duration and snap to a point in the future.
            // as we are snapping to beat length via SnapTime (which will round-to-nearest), check for snapping in the forward direction and reverse it.
            if (snappedEndTime > actualDuration + 1)
                snappedEndTime -= beatLength;

            return DurationToDistance(referenceObject, snappedEndTime - startTime);
        }

        #endregion

        private partial class DistanceSpacingToast : Toast
        {
            private readonly ValueChangedEvent<double> change;

            public DistanceSpacingToast(LocalisableString value, ValueChangedEvent<double> change)
                : base(getAction(change).GetLocalisableDescription(), value, string.Empty)
            {
                this.change = change;
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                ShortcutText.Text = config.LookupKeyBindings(getAction(change)).ToUpper();
            }

            private static GlobalAction getAction(ValueChangedEvent<double> change) => change.NewValue - change.OldValue > 0
                ? GlobalAction.EditorIncreaseDistanceSpacing
                : GlobalAction.EditorDecreaseDistanceSpacing;
        }
    }
}
