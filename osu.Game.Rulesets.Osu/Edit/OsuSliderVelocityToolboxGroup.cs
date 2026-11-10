// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuSliderVelocityToolboxGroup : EditorToolboxGroup
    {
        /// <summary>
        /// The slider velocity to be used for new object placements.
        /// </summary>
        public IBindable<double> SliderVelocity => sliderControl.Current;

        private ExpandableSliderVelocityAdjustmentControl sliderControl = null!;
        private ExpandableButton useLastSliderButton = null!;

        private readonly BindableList<HitObject> selectedHitObjects = new BindableList<HitObject>();

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        private double lastClockPosition = double.NegativeInfinity;

        /// <summary>
        /// This <see cref="Cached"/> is used to track whether the "source" of slider velocity is valid.
        /// That means:
        /// <list type="bullet">
        /// <item>
        /// When there are no objects with velocity selected, the presumed "source" of slider velocity is
        /// the last slider preceding the editor's current playback position (if one exists).
        /// </item>
        /// <item>When there are objects with velocity selected, they are the presumed "source" of slider velocity.</item>
        /// </list>
        /// Any event that may affect the readout of slider velocity from the "source" of slider velocity as defined above
        /// should invalidate this <see cref="Cached"/>.
        /// </summary>
        private readonly Cached sliderVelocitySource = new Cached();

        /// <summary>
        /// This flag is used to track whether the <see cref="sliderControl"/>
        /// is currently decoupled from and overriding the <see cref="sliderVelocitySource"/>.
        /// </summary>
        /// <remarks>
        /// This is only supported when there are no objects with velocity selected.
        /// In that scenario, this flag supports the behaviour of being able to select a slider velocity manually via <see cref="sliderControl"/>
        /// independent of the last slider preceding the editor's current playback position.
        /// <see cref="useLastSliderButton"/> is used to clear this flag.
        /// </remarks>
        private bool overridingSliderVelocitySource;

        public OsuSliderVelocityToolboxGroup()
            : base("velocity")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Spacing = new Vector2(5);
            Children = new Drawable[]
            {
                sliderControl = new ExpandableSliderVelocityAdjustmentControl(),
                useLastSliderButton = new ExpandableButton
                {
                    RelativeSizeAxes = Axes.X,
                    Action = () =>
                    {
                        overridingSliderVelocitySource = false;
                        sliderVelocitySource.Invalidate();
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sliderControl.Current.BindValueChanged(_ =>
            {
                updateContractedText();

                // if the slider velocity source is valid,
                // then this change to `sliderControl.Current` did not originate from the giant revalidation block in `Update()`,
                // meaning it was user-inflicted, meaning that the user is now overriding the velocity source.
                if (sliderVelocitySource.IsValid)
                {
                    overridingSliderVelocitySource = true;
                    sliderVelocitySource.Invalidate();
                }
            }, true);
            useLastSliderButton.Expanded.BindValueChanged(_ => sliderVelocitySource.Invalidate());

            editorBeatmap.HitObjectAdded += invalidateSliderVelocitySourceObject;
            editorBeatmap.HitObjectUpdated += invalidateSliderVelocitySourceObject;
            editorBeatmap.HitObjectRemoved += invalidateSliderVelocitySourceObject;
            selectedHitObjects.BindTo(editorBeatmap.SelectedHitObjects);
            selectedHitObjects.BindCollectionChanged((_, _) => sliderVelocitySource.Invalidate());
        }

        private void updateContractedText()
        {
            sliderControl.ContractedLabelText = LocalisableString.Interpolate($@"SV: {sliderControl.Current.Value.ToLocalisableString("N2")}x");
        }

        private void invalidateSliderVelocitySourceObject(HitObject _) => sliderVelocitySource.Invalidate();

        protected override void Update()
        {
            base.Update();

            if (editorClock.CurrentTime != lastClockPosition)
            {
                sliderVelocitySource.Invalidate();
                lastClockPosition = editorClock.CurrentTime;
            }

            // Four possible causes of invalidation:
            // - The user has selected some objects from the beatmap, in which case the velocity from the selected objects (if any)
            //   should take absolute precedence.
            // - The user seeked the clock, which means that the last slider to read the velocity from in "use last slider velocity" mode might have changed.
            // - Some change to the beatmap was made, which means the displayed velocity may no longer match the source objects in underlying beatmap
            //   (this applies when some objects are selected *and* when none are selected).
            // - The user is interacting with the toolbox in a way that requires a visual state update
            //   (hovered to expand it, clicked the button to use last slider's velocity, or dragged the manual velocity slider).
            //   This is a procedural one, because `sliderVelocitySource` will have been re-validated correctly already in that case,
            //   but to decrease unnecessary work being done every frame, the invalidation is explicitly re-triggered to update the toolbox state.
            if (!sliderVelocitySource.IsValid)
            {
                var selectedSliderVelocities = selectedHitObjects.OfType<Slider>().Select(s => s.SliderVelocityMultiplier).Distinct().ToList();

                if (selectedSliderVelocities.Count > 0)
                {
                    overridingSliderVelocitySource = false;

                    useLastSliderButton.Enabled.Value = false;
                    useLastSliderButton.ExpandedLabelText = "Adjusting velocity of selection";
                    useLastSliderButton.ContractedLabelText = default;

                    sliderControl.ObjectsToAdjust.Clear();
                    sliderControl.ObjectsToAdjust.AddRange(selectedHitObjects.OfType<Slider>());
                }
                else
                {
                    sliderControl.ObjectsToAdjust.Clear();

                    var lastSlider = editorBeatmap
                                     .HitObjects
                                     .OfType<Slider>()
                                     .LastOrDefault(h => h.StartTime <= editorClock.CurrentTime);

                    if (lastSlider == null)
                    {
                        useLastSliderButton.Enabled.Value = false;
                        useLastSliderButton.ExpandedLabelText = "No sliders to get velocity from";
                        useLastSliderButton.ContractedLabelText = default;
                    }
                    else
                    {
                        useLastSliderButton.Enabled.Value = useLastSliderButton.Expanded.Value && overridingSliderVelocitySource;
                        useLastSliderButton.ExpandedLabelText = overridingSliderVelocitySource
                            ? LocalisableString.Interpolate($@"Use last slider's velocity ({lastSlider.SliderVelocityMultiplier.ToLocalisableString("N2")}x)")
                            : "Using last slider's velocity";
                        useLastSliderButton.ContractedLabelText = $@"current {lastSlider.SliderVelocityMultiplier.ToLocalisableString("N2")}x";

                        if (!overridingSliderVelocitySource)
                            sliderControl.Current.Value = lastSlider.SliderVelocityMultiplier;
                    }
                }

                sliderVelocitySource.Validate();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (editorBeatmap.IsNotNull())
            {
                editorBeatmap.HitObjectAdded -= invalidateSliderVelocitySourceObject;
                editorBeatmap.HitObjectUpdated -= invalidateSliderVelocitySourceObject;
                editorBeatmap.HitObjectRemoved -= invalidateSliderVelocitySourceObject;
            }

            base.Dispose(isDisposing);
        }

        internal partial class ExpandableSliderVelocityAdjustmentControl : CompositeDrawable, IExpandable
        {
            private readonly OsuSpriteText contractedLabel;
            private readonly SliderVelocityAdjustmentControl adjustmentControl;

            /// <summary>
            /// The label text to display when this slider is in a contracted state.
            /// </summary>
            public LocalisableString ContractedLabelText
            {
                get => contractedLabel.Text;
                set => contractedLabel.Text = value;
            }

            public Bindable<double> Current => adjustmentControl.Current;
            public bool IsMultipleValues => adjustmentControl.IsMultipleValues;
            public BindableList<HitObject> ObjectsToAdjust => adjustmentControl.ObjectsToAdjust;

            public BindableBool Expanded { get; } = new BindableBool();

            public override bool HandlePositionalInput => true;

            public ExpandableSliderVelocityAdjustmentControl()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        contractedLabel = new OsuSpriteText(),
                        adjustmentControl = new SliderVelocityAdjustmentControl(),
                    }
                };
            }

            [Resolved]
            private IExpandingContainer? expandingContainer { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
                {
                    Expanded.Value = containerExpanded.NewValue;
                }, true);

                Expanded.BindValueChanged(v =>
                {
                    contractedLabel.FadeTo(v.NewValue ? 0 : 1);

                    adjustmentControl.FadeTo(v.NewValue ? Current.Disabled ? 0.3f : 1f : 0f, 500, Easing.OutQuint);
                    adjustmentControl.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
                }, true);

                Current.BindDisabledChanged(disabled =>
                {
                    adjustmentControl.Alpha = Expanded.Value ? disabled ? 0.3f : 1 : 0f;
                });
            }
        }
    }
}
