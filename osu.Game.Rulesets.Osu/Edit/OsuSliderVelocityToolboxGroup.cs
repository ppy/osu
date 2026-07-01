// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuSliderVelocityToolboxGroup : EditorToolboxGroup
    {
        /// <summary>
        /// The slider velocity to be used for new object placements.
        /// </summary>
        public IBindable<double> SliderVelocity => sliderVelocity;

        private readonly BindableDouble sliderVelocity = new BindableDouble(1)
        {
            Precision = 0.01,
            MinValue = 0.1,
            MaxValue = 10,
        };

        private ExpandableSliderVelocityControl sliderControl = null!;
        private ExpandableButton useLastSliderButton = null!;

        private readonly BindableList<HitObject> selectedHitObjects = new BindableList<HitObject>();

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        private double lastClockPosition = double.NegativeInfinity;

        private bool syncingBindables;

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
            Spacing = new osuTK.Vector2(5);
            Children = new Drawable[]
            {
                sliderControl = new ExpandableSliderVelocityControl
                {
                    ExpandedLabelText = "Slider velocity",
                    Current = new BindableDouble(1)
                    {
                        Precision = 0.01,
                        MinValue = 0.1,
                        MaxValue = 10,
                    },
                    KeyboardStep = 0.1f,
                    TransferValueOnCommit = true,
                },
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

            sliderVelocity.BindValueChanged(_ => updateSliderControlFromVelocity(), true);
            sliderControl.Current.BindValueChanged(_ =>
            {
                updateVelocityFromSliderControl();
                updateContractedText();
            });
            updateContractedText();
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

        /// <summary>
        /// Updates the displayed value of this toolbox's <see cref="sliderControl"/> from a change to <see cref="SliderVelocity"/>
        /// (which is the source-of-truth used for new object placements).
        /// This is only relevant when <see cref="overridingSliderVelocitySource"/> is false,
        /// in which case this code is responsible for propagating the velocity from <see cref="sliderVelocitySource"/> to the slider.
        /// </summary>
        private void updateSliderControlFromVelocity()
        {
            if (syncingBindables)
                return;

            if (overridingSliderVelocitySource)
                return;

            syncingBindables = true;
            sliderControl.Current.Value = sliderVelocity.Value;
            syncingBindables = false;
        }

        /// <summary>
        /// Updates the value of <see cref="SliderVelocity"/> from a user-provoked change to the <see cref="sliderControl"/>'s state.
        /// </summary>
        private void updateVelocityFromSliderControl()
        {
            if (syncingBindables)
                return;

            syncingBindables = true;

            var selectedSliders = selectedHitObjects.OfType<Slider>().ToList();

            if (selectedSliders.Any())
            {
                Debug.Assert(!overridingSliderVelocitySource);

                editorBeatmap.BeginChange();

                foreach (var selectedSlider in selectedSliders)
                {
                    selectedSlider.SliderVelocityMultiplier = sliderControl.Current.Value;
                    editorBeatmap.Update(selectedSlider);
                }

                editorBeatmap.EndChange();

                sliderControl.IsMultipleValues = false;
            }
            else
            {
                overridingSliderVelocitySource = true;
                sliderVelocity.Value = sliderControl.Current.Value;
            }

            syncingBindables = false;
            sliderVelocitySource.Invalidate();
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

                    sliderControl.IsMultipleValues = selectedSliderVelocities.Count > 1;
                    sliderVelocity.Value = selectedSliderVelocities.Count == 1 ? selectedSliderVelocities[0] : 1;
                }
                else
                {
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
                            sliderVelocity.Value = lastSlider.SliderVelocityMultiplier;
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

        internal partial class ExpandableSliderVelocityControl : ExpandableSlider<double, SliderVelocityControl>
        {
            public bool IsMultipleValues
            {
                set => Slider.IsMultipleValues = value;
            }

            public bool TransferValueOnCommit
            {
                get => Slider.TransferValueOnCommit;
                set => Slider.TransferValueOnCommit = value;
            }
        }

        internal partial class SliderVelocityControl : FormSliderBar<double>
        {
            private bool isMultipleValues;

            /// <summary>
            /// This is a hack to allow the text box to show an indication that multiple slider velocity values are active
            /// when the selection contains multiple objects with different velocities.
            /// </summary>
            public bool IsMultipleValues
            {
                get => isMultipleValues;
                set
                {
                    if (isMultipleValues == value)
                        return;

                    isMultipleValues = value;
                    updateLabelFormat();
                }
            }

            private void updateLabelFormat()
            {
                LabelFormat = isMultipleValues
                    ? static _ => "(multiple)"
                    : v => LocalisableString.Interpolate($"{v:0.00}x");
            }

            public SliderVelocityControl()
            {
                updateLabelFormat();

                // The `IsMultipleValues` / `updateLabelFormat()` hack above to jam an indicator of multiple active values does not work for tooltip
                // because the tooltip machinery framework-side is too smart for it (the tooltip text is only regenerated on direct changes to `Current`).
                // Just disable it to hide the skeleton. It's of little use anyhow.
                TooltipFormat = _ => default;
            }
        }
    }
}
