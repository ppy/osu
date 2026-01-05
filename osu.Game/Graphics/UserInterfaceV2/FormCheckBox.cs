// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormCheckBox : CompositeDrawable, IHasCurrentValue<bool>, IFormControl
    {
        public Bindable<bool> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<bool> current = new BindableWithCurrent<bool>();

        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        private Box background = null!;
        private FormFieldCaption caption = null!;
        private OsuSpriteText text = null!;

        private Sample? sampleChecked;
        private Sample? sampleUnchecked;
        private Sample? sampleDisabled;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(9),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding { Right = SwitchButton.WIDTH + 5 },
                            Spacing = new Vector2(0f, 4f),
                            Children = new Drawable[]
                            {
                                caption = new FormFieldCaption
                                {
                                    Caption = Caption,
                                    TooltipText = HintText,
                                },
                                text = new OsuSpriteText
                                {
                                    RelativeSizeAxes = Axes.X,
                                },
                            },
                        },
                        new SwitchButton
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Current = Current,
                        },
                    },
                },
            };
            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
            sampleDisabled = audio.Samples.Get(@"UI/default-select-disabled");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindValueChanged(_ =>
            {
                updateState();
                playSamples();
                background.FlashColour(ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark2), 800, Easing.OutQuint);

                ValueChanged?.Invoke();
            });
            current.BindDisabledChanged(_ => updateState(), true);
        }

        private void playSamples()
        {
            if (Current.Value)
                sampleChecked?.Play();
            else
                sampleUnchecked?.Play();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!Current.Disabled)
                Current.Value = !Current.Value;
            else
                sampleDisabled?.Play();

            return true;
        }

        private void updateState()
        {
            caption.Colour = Current.Disabled ? colourProvider.Background1 : colourProvider.Content2;
            text.Colour = Current.Disabled ? colourProvider.Background1 : colourProvider.Content1;

            text.Text = Current.Value ? CommonStrings.Enabled : CommonStrings.Disabled;

            // use FadeColour to override any existing colour transform (i.e. FlashColour on click).
            background.FadeColour(IsHovered
                ? ColourInfo.GradientVertical(colourProvider.Background5, colourProvider.Dark4)
                : colourProvider.Background5);

            BorderThickness = IsHovered ? 2 : 0;
            BorderColour = Current.Disabled ? colourProvider.Dark1 : colourProvider.Light4;
        }

        public IEnumerable<LocalisableString> FilterTerms => Caption.Yield();

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;
    }
}
