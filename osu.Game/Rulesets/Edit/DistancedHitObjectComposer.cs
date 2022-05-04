// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// Represents a <see cref="HitObjectComposer{TObject}"/> for rulesets with the concept of distances between objects.
    /// </summary>
    /// <typeparam name="TObject">The base type of supported objects.</typeparam>
    [Cached(typeof(IDistanceSnapProvider))]
    public abstract class DistancedHitObjectComposer<TObject> : HitObjectComposer<TObject>, IDistanceSnapProvider, IScrollBindingHandler<GlobalAction>
        where TObject : HitObject
    {
        protected Bindable<double> DistanceSpacingMultiplier { get; } = new BindableDouble(1.0)
        {
            MinValue = 0.1,
            MaxValue = 6.0,
            Precision = 0.01,
        };

        IBindable<double> IDistanceSnapProvider.DistanceSpacingMultiplier => DistanceSpacingMultiplier;

        protected ExpandingToolboxContainer RightSideToolboxContainer { get; private set; }

        private ExpandableSlider<double, SizeSlider<double>> distanceSpacingSlider;

        protected DistancedHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(RightSideToolboxContainer = new ExpandingToolboxContainer
            {
                Alpha = DistanceSpacingMultiplier.Disabled ? 0 : 1,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Child = new EditorToolboxGroup("snapping")
                {
                    Child = distanceSpacingSlider = new ExpandableSlider<double, SizeSlider<double>>
                    {
                        Current = { BindTarget = DistanceSpacingMultiplier },
                        KeyboardStep = 0.1f,
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
                DistanceSpacingMultiplier.BindValueChanged(v =>
                {
                    distanceSpacingSlider.ContractedLabelText = $"D. S. ({v.NewValue:0.##x})";
                    distanceSpacingSlider.ExpandedLabelText = $"Distance Spacing ({v.NewValue:0.##x})";
                    EditorBeatmap.BeatmapInfo.DistanceSpacing = v.NewValue;
                }, true);
            }
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorIncreaseDistanceSpacing:
                case GlobalAction.EditorDecreaseDistanceSpacing:
                    return adjustDistanceSpacing(e.Action, 0.1f);
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
                    return adjustDistanceSpacing(e.Action, e.ScrollAmount * (e.IsPrecise ? 0.01f : 0.1f));
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

        public virtual double GetSnappedDurationFromDistance(HitObject referenceObject, float distance)
            => BeatSnapProvider.SnapTime(referenceObject.StartTime + DistanceToDuration(referenceObject, distance), referenceObject.StartTime) - referenceObject.StartTime;

        public virtual float GetSnappedDistanceFromDistance(HitObject referenceObject, float distance)
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

        protected class ExpandingToolboxContainer : ExpandingContainer
        {
            protected override double HoverExpansionDelay => 250;

            public ExpandingToolboxContainer()
                : base(130, 250)
            {
                RelativeSizeAxes = Axes.Y;
                Padding = new MarginPadding { Left = 10 };

                FillFlow.Spacing = new Vector2(10);
            }
        }
    }
}
