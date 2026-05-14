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

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuSliderVelocityToolboxGroup : EditorToolboxGroup
    {
        /// <summary>
        /// Whether the last slider's velocity should be used (if available).
        /// </summary>
        public bool UseLastSliderVelocity;

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
                    Action = () => UseLastSliderVelocity = true,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // set unconditionally to true initially.
            // if there is no object available to get the slider velocity from, the code in `Update()` will handle that.
            UseLastSliderVelocity = true;

            sliderVelocity.BindValueChanged(_ => updateSliderFromVelocity(), true);
            slider.Current.BindValueChanged(_ =>
            {
                updateVelocityFromSlider();
                updateContractedText();
            });
            updateContractedText();

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
        /// This is only relevant when <see cref="UseLastSliderVelocity"/> is true,
        /// in which case this code is responsible for propagating the velocity from <see cref="sliderVelocitySourceObject"/> to the slider.
        /// </summary>
        private void updateSliderFromVelocity()
        {
            if (syncingBindables)
                return;

            if (!UseLastSliderVelocity)
                return;

            syncingBindables = true;
            slider.Current.Value = sliderVelocity.Value;
            syncingBindables = false;
        }

        /// <summary>
        /// Updates the value of <see cref="SliderVelocity"/> from a change to the slider's state.
        /// This change is assumed to be user-provoked, and therefore <see cref="UseLastSliderVelocity"/> is switched unconditionally off
        /// as the presumed intent is to override the velocity from <see cref="sliderVelocitySourceObject"/>.
        /// </summary>
        private void updateVelocityFromSlider()
        {
            if (syncingBindables)
                return;

            syncingBindables = true;
            UseLastSliderVelocity = false;
            sliderVelocity.Value = slider.Current.Value;
            syncingBindables = false;
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

            // Two possible causes of invalidation:
            // - The user seeked the clock, which means we may want a different velocity source object
            // - Some change to the beatmap was made, which means the previously-used velocity source object may no longer be the most relevant one
            if (!sliderVelocitySourceObject.IsValid)
                sliderVelocitySourceObject.Value = getLastSlider();

            // This functions as a null check and allows simplified access later (instead of needing to type `sliderVelocitySourceObject.Value` multiple times)
            if (sliderVelocitySourceObject.Value is not Slider lastSlider)
            {
                useLastSliderButton.Enabled.Value = false;
                useLastSliderButton.ExpandedLabelText = "No sliders to get velocity from";
            }
            else
            {
                useLastSliderButton.Enabled.Value = !UseLastSliderVelocity;
                useLastSliderButton.ExpandedLabelText = UseLastSliderVelocity
                    ? "Using last slider's velocity"
                    : LocalisableString.Interpolate($@"Use last slider's velocity ({lastSlider.SliderVelocityMultiplier.ToLocalisableString("N2")}x)");
                if (UseLastSliderVelocity)
                    sliderVelocity.Value = lastSlider.SliderVelocityMultiplier;
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
