// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuSliderVelocityToolboxGroup : EditorToolboxGroup
    {
        /// <summary>
        /// Whether the last slider's velocity should be used (if available).
        /// </summary>
        private bool useLastSliderVelocity;

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

        private ExpandableSlider<double> slider = null!;
        private ExpandableButton useLastSliderButton = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        private bool syncingBindables;
        private double lastClockPosition = double.NegativeInfinity;
        private readonly Cached<Slider?> sliderVelocitySourceObject = new Cached<Slider?>();

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
                slider = new ExpandableSlider<double>
                {
                    ExpandedLabelText = "Slider velocity",
                    Current = new BindableDouble(1)
                    {
                        Precision = 0.01,
                        MinValue = 0.1,
                        MaxValue = 10,
                    },
                    KeyboardStep = 0.1f,
                },
                useLastSliderButton = new ExpandableButton
                {
                    RelativeSizeAxes = Axes.X,
                    Action = () =>
                    {
                        useLastSliderVelocity = true;
                        sliderVelocitySourceObject.Invalidate();
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // set unconditionally to true initially.
            // if there is no object available to get the slider velocity from, the code in `Update()` will handle that.
            useLastSliderVelocity = true;

            sliderVelocity.BindValueChanged(_ => updateSliderFromVelocity(), true);
            slider.Current.BindValueChanged(_ =>
            {
                updateVelocityFromSlider();
                updateContractedText();
            });
            updateContractedText();
            useLastSliderButton.Expanded.BindValueChanged(_ => sliderVelocitySourceObject.Invalidate());

            editorBeatmap.HitObjectAdded += invalidateSliderVelocitySourceObject;
            editorBeatmap.HitObjectUpdated += invalidateSliderVelocitySourceObject;
            editorBeatmap.HitObjectRemoved += invalidateSliderVelocitySourceObject;
        }

        private void updateContractedText()
        {
            slider.ContractedLabelText = LocalisableString.Interpolate($@"SV: {slider.Current.Value.ToLocalisableString("N2")}x");
        }

        /// <summary>
        /// Updates the displayed value of this toolbox's slider from a change to <see cref="SliderVelocity"/>
        /// (which is the source-of-truth used for new object placements).
        /// This is only relevant when <see cref="useLastSliderVelocity"/> is true,
        /// in which case this code is responsible for propagating the velocity from <see cref="sliderVelocitySourceObject"/> to the slider.
        /// </summary>
        private void updateSliderFromVelocity()
        {
            if (syncingBindables)
                return;

            if (!useLastSliderVelocity)
                return;

            syncingBindables = true;
            slider.Current.Value = sliderVelocity.Value;
            syncingBindables = false;
        }

        /// <summary>
        /// Updates the value of <see cref="SliderVelocity"/> from a change to the slider's state.
        /// This change is assumed to be user-provoked, and therefore <see cref="useLastSliderVelocity"/> is switched unconditionally off
        /// as the presumed intent is to override the velocity from <see cref="sliderVelocitySourceObject"/>.
        /// </summary>
        private void updateVelocityFromSlider()
        {
            if (syncingBindables)
                return;

            syncingBindables = true;
            useLastSliderVelocity = false;
            sliderVelocity.Value = slider.Current.Value;
            syncingBindables = false;
            sliderVelocitySourceObject.Invalidate();
        }

        private void invalidateSliderVelocitySourceObject(HitObject _) => sliderVelocitySourceObject.Invalidate();

        protected override void Update()
        {
            base.Update();

            if (editorClock.CurrentTime != lastClockPosition)
            {
                sliderVelocitySourceObject.Invalidate();
                lastClockPosition = editorClock.CurrentTime;
            }

            // Three possible causes of invalidation:
            // - The user seeked the clock, which means a different velocity source object needs to be used.
            // - Some change to the beatmap was made, which means the previously-used velocity source object may no longer be the most relevant one.
            // - The user is interacting with the toolbox in a way that requires a visual state update
            //   (hovered to expand it, clicked the button to use last slider's velocity, or dragged the manual velocity slider).
            //   This is a procedural one, because `sliderVelocitySourceObject` will have been pointing at the correct object already,
            //   but to decrease unnecessary work being done every frame, the invalidation is explicitly re-triggered to update the toolbox state.
            if (!sliderVelocitySourceObject.IsValid)
            {
                var lastSlider = getLastSlider();
                sliderVelocitySourceObject.Value = lastSlider;

                if (lastSlider == null)
                {
                    useLastSliderButton.Enabled.Value = false;
                    useLastSliderButton.ExpandedLabelText = "No sliders to get velocity from";
                    useLastSliderButton.ContractedLabelText = default;
                }
                else
                {
                    useLastSliderButton.Enabled.Value = useLastSliderButton.Expanded.Value && !useLastSliderVelocity;
                    useLastSliderButton.ExpandedLabelText = useLastSliderVelocity
                        ? "Using last slider's velocity"
                        : LocalisableString.Interpolate($@"Use last slider's velocity ({lastSlider.SliderVelocityMultiplier.ToLocalisableString("N2")}x)");
                    useLastSliderButton.ContractedLabelText = $@"current {lastSlider.SliderVelocityMultiplier.ToLocalisableString("N2")}x";
                    if (useLastSliderVelocity)
                        sliderVelocity.Value = lastSlider.SliderVelocityMultiplier;
                }
            }
        }

        private Slider? getLastSlider()
        {
            return editorBeatmap
                   .HitObjects
                   .OfType<Slider>()
                   .LastOrDefault(h => h.StartTime <= editorClock.CurrentTime);
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
    }
}
