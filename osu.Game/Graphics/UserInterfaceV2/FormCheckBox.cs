// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormCheckBox : CompositeDrawable, IFormControl<bool>
    {
        public Bindable<bool> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<bool> current = new BindableWithCurrent<bool>();

        private LocalisableString caption;

        /// <summary>
        /// Caption describing this check box, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption
        {
            get => caption;
            set
            {
                caption = value;

                if (IsLoaded)
                    captionText.Caption = value;
            }
        }

        private LocalisableString hintText;

        /// <summary>
        /// Hint text containing an extended description of this check box, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText
        {
            get => hintText;
            set
            {
                hintText = value;

                if (IsLoaded)
                    captionText.TooltipText = value;
            }
        }

        private FormControlBackground background = null!;
        private FormFieldCaption captionText = null!;

        private SwitchButton switchButton = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public FormCheckBox()
        {
            // IMPORTANT: bindable value change logic is in constructor intentionally to support
            // "CreateSettingsControls" being used in a context it is never loaded, but requires bindable storage.
            current.BindValueChanged(_ => ValueChanged?.Invoke());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new FormControlBackground(),
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(9),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Right = SwitchButton.WIDTH + 5 },
                            Children = new Drawable[]
                            {
                                captionText = new FormFieldCaption
                                {
                                    Caption = Caption,
                                    TooltipText = HintText,
                                },
                            },
                        },
                        switchButton = new SwitchButton
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Current = Current,
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindValueChanged(_ =>
            {
                updateState();
                background.Flash();
            });
            current.BindDisabledChanged(_ => updateState(), true);
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
            switchButton.TriggerClick();
            return true;
        }

        private void updateState()
        {
            captionText.Colour = Current.Disabled ? colourProvider.Background1 : colourProvider.Content2;

            if (IsDisabled)
                background.VisualStyle = VisualStyle.Disabled;
            else if (IsHovered)
                background.VisualStyle = VisualStyle.Hovered;
            else
                background.VisualStyle = VisualStyle.Normal;
        }

        public IEnumerable<LocalisableString> FilterTerms => Caption.Yield();

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        public float MainDrawHeight => DrawHeight;
    }
}
