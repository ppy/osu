// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// Represents a <see cref="HitObjectComposer{TObject}"/> for rulesets with the concept of distances between objects.
    /// </summary>
    /// <typeparam name="TObject">The base type of supported objects.</typeparam>
    public abstract class DistancedHitObjectComposer<TObject> : HitObjectComposer<TObject>, IDistanceSnapProvider, IScrollBindingHandler<GlobalAction>
        where TObject : HitObject
    {
        private const float adjust_step = 0.1f;

        public Bindable<double> DistanceSpacingMultiplier { get; } = new BindableDouble(1.0)
        {
            MinValue = 0.1,
            MaxValue = 6.0,
            Precision = 0.01,
        };

        IBindable<double> IDistanceSnapProvider.DistanceSpacingMultiplier => DistanceSpacingMultiplier;

        protected ExpandingToolboxContainer RightSideToolboxContainer { get; private set; }

        private ExpandableSlider<double, SizeSlider<double>> distanceSpacingSlider;

        [Resolved(canBeNull: true)]
        private OnScreenDisplay onScreenDisplay { get; set; }

        protected DistancedHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(RightSideToolboxContainer = new ExpandingToolboxContainer(130, 250)
            {
                Padding = new MarginPadding(10),
                Alpha = DistanceSpacingMultiplier.Disabled ? 0 : 1,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Child = new EditorToolboxGroup("snapping")
                {
                    Child = distanceSpacingSlider = new ExpandableSlider<double, SizeSlider<double>>
                    {
                        Current = { BindTarget = DistanceSpacingMultiplier },
                        KeyboardStep = adjust_step,
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!DistanceSpacingMultiplier.Disabled)
            {
                DistanceSpacingMultiplier.Value = EditorBeatmap.BeatmapInfo.DistanceSpacing;
                DistanceSpacingMultiplier.BindValueChanged(multiplier =>
                {
                    distanceSpacingSlider.ContractedLabelText = $"D. S. ({multiplier.NewValue:0.##x})";
                    distanceSpacingSlider.ExpandedLabelText = $"Distance Spacing ({multiplier.NewValue:0.##x})";

                    if (multiplier.NewValue != multiplier.OldValue)
                        onScreenDisplay?.Display(new DistanceSpacingToast(multiplier.NewValue.ToLocalisableString(@"0.##x"), multiplier));

                    EditorBeatmap.BeatmapInfo.DistanceSpacing = multiplier.NewValue;
                }, true);
            }
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorIncreaseDistanceSpacing:
                case GlobalAction.EditorDecreaseDistanceSpacing:
                    return adjustDistanceSpacing(e.Action, adjust_step);
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public bool OnScroll(KeyBindingScrollEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorIncreaseDistanceSpacing:
                case GlobalAction.EditorDecreaseDistanceSpacing:
                    return adjustDistanceSpacing(e.Action, e.ScrollAmount * adjust_step);
            }

            return false;
        }

        private bool adjustDistanceSpacing(GlobalAction action, float amount)
        {
            if (DistanceSpacingMultiplier.Disabled)
                return false;

            if (action == GlobalAction.EditorIncreaseDistanceSpacing)
                DistanceSpacingMultiplier.Value += amount;
            else if (action == GlobalAction.EditorDecreaseDistanceSpacing)
                DistanceSpacingMultiplier.Value -= amount;

            return true;
        }

        public virtual float GetBeatSnapDistanceAt(HitObject referenceObject)
        {
            return (float)(100 * EditorBeatmap.Difficulty.SliderMultiplier * referenceObject.DifficultyControlPoint.SliderVelocity / BeatSnapProvider.BeatDivisor);
        }

        public virtual float DurationToDistance(HitObject referenceObject, double duration)
        {
            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(referenceObject.StartTime);
            return (float)(duration / beatLength * GetBeatSnapDistanceAt(referenceObject));
        }

        public virtual double DistanceToDuration(HitObject referenceObject, float distance)
        {
            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(referenceObject.StartTime);
            return distance / GetBeatSnapDistanceAt(referenceObject) * beatLength;
        }

        public virtual double FindSnappedDuration(HitObject referenceObject, float distance)
            => BeatSnapProvider.SnapTime(referenceObject.StartTime + DistanceToDuration(referenceObject, distance), referenceObject.StartTime) - referenceObject.StartTime;

        public virtual float FindSnappedDistance(HitObject referenceObject, float distance)
        {
            double startTime = referenceObject.StartTime;

            double actualDuration = startTime + DistanceToDuration(referenceObject, distance);

            double snappedEndTime = BeatSnapProvider.SnapTime(actualDuration, startTime);

            double beatLength = BeatSnapProvider.GetBeatLengthAtTime(startTime);

            // we don't want to exceed the actual duration and snap to a point in the future.
            // as we are snapping to beat length via SnapTime (which will round-to-nearest), check for snapping in the forward direction and reverse it.
            if (snappedEndTime > actualDuration + 1)
                snappedEndTime -= beatLength;

            return DurationToDistance(referenceObject, snappedEndTime - startTime);
        }

        private class DistanceSpacingToast : Toast
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
