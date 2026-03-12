// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using Vector2 = osuTK.Vector2;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An <see cref="IExpandable"/> implementation for the UI slider bar control.
    /// </summary>
    public partial class ExpandableSlider<T, TSlider> : CompositeDrawable, IExpandable, IHasCurrentValue<T>
        where T : struct, INumber<T>, IMinMaxValue<T>
        where TSlider : FormSliderBar<T>, new()
    {
        private readonly OsuSpriteText contractedLabel;
        private readonly TSlider slider;

        /// <summary>
        /// The label text to display when this slider is in a contracted state.
        /// </summary>
        public LocalisableString ContractedLabelText
        {
            get => contractedLabel.Text;
            set => contractedLabel.Text = value;
        }

        /// <summary>
        /// The label text to display when this slider is in an expanded state.
        /// </summary>
        public LocalisableString ExpandedLabelText
        {
            get => slider.Caption;
            set => slider.Caption = value;
        }

        public Bindable<T> Current
        {
            get => slider.Current;
            set => slider.Current = value;
        }

        /// <summary>
        /// A custom step value for each key press which actuates a change on this control.
        /// </summary>
        public float KeyboardStep
        {
            get => slider.KeyboardStep;
            set => slider.KeyboardStep = value;
        }

        public BindableBool Expanded { get; } = new BindableBool();

        public override bool HandlePositionalInput => true;

        public ExpandableSlider()
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
                    slider = new TSlider
                    {
                        RelativeSizeAxes = Axes.X,
                    },
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

                slider.FadeTo(v.NewValue ? Current.Disabled ? 0.3f : 1f : 0f, 500, Easing.OutQuint);
                slider.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
            }, true);

            Current.BindDisabledChanged(disabled =>
            {
                slider.Alpha = Expanded.Value ? disabled ? 0.3f : 1 : 0f;
            });
        }
    }

    /// <summary>
    /// An <see cref="IExpandable"/> implementation for the UI slider bar control.
    /// </summary>
    public partial class ExpandableSlider<T> : ExpandableSlider<T, FormSliderBar<T>>
        where T : struct, INumber<T>, IMinMaxValue<T>
    {
    }
}
