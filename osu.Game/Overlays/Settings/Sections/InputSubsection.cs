// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Input.Handlers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class InputSubsection : SettingsSubsection
    {
        private readonly InputHandler handler;

        protected override LocalisableString Header => handler.Description;

        /// <summary>
        /// Whether the input handler can be toggled on/off by the user.
        /// </summary>
        protected virtual bool IsToggleable => true;

        private readonly BindableBool handlerEnabled = new BindableBool();

        public InputSubsection(InputHandler handler)
        {
            this.handler = handler;

            FlowContent.AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HeaderContainer.Child = new ToggleableHeader(Header, IsToggleable)
            {
                Current = { BindTarget = handlerEnabled },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            handlerEnabled.BindTo(handler.Enabled);
            handlerEnabled.BindValueChanged(v => updateEnabledState(), true);
        }

        private void updateEnabledState()
        {
            // set negative bottom margin to not have too much vertical gap between disabled input subsections.
            bool negativeBottomMargin = !handlerEnabled.Value || FlowContent.Count == 0;
            HeaderContainer.TransformTo(nameof(Margin), new MarginPadding { Bottom = negativeBottomMargin ? -15 : 0 }, 300, Easing.OutQuint);

            FlowContent.ClearTransforms();

            if (!handlerEnabled.Value)
            {
                FlowContent.AutoSizeAxes = Axes.None;
                FlowContent.ResizeHeightTo(0, 300, Easing.OutQuint);

                FlowContent.FadeOut(200, Easing.OutQuint);
            }
            else
            {
                // enable auto size transform momentarily for smooth pop in animation, and disable it right after the transform is added.
                // we don't want this specification to apply when a dropdown in the input settings is being open, it causes too slow animation.
                // (try removing the schedule below then watch a settings dropdown menu opening animation).
                FlowContent.AutoSizeDuration = 300;
                FlowContent.AutoSizeEasing = Easing.OutQuint;
                FlowContent.AutoSizeAxes = Axes.Y;
                ScheduleAfterChildren(() => FlowContent.AutoSizeDuration = 0);

                FlowContent.FadeIn(300, Easing.OutQuint);
            }
        }

        private partial class ToggleableHeader : CompositeDrawable
        {
            private readonly LocalisableString text;
            private readonly bool toggleable;

            public readonly BindableBool Current = new BindableBool(true);

            public ToggleableHeader(LocalisableString text, bool toggleable)
            {
                this.text = text;
                this.toggleable = toggleable;
            }

            private SwitchButton switchButton = null!;
            private OsuSpriteText headerText = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    switchButton = new SwitchButton
                    {
                        ExpandOnCurrent = false,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Width = 15,
                        Height = 22,
                    },
                    headerText = new OsuSpriteText
                    {
                        Text = InputSettingsStrings.Device(text),
                        Font = OsuFont.Style.Heading2,
                        Margin = new MarginPadding { Vertical = 12 },
                        X = 18,
                        Y = -1,
                    },
                    new HoverSounds(),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                switchButton.Current.ValueChanged += v => Current.Value = v.NewValue;

                Current.BindValueChanged(v =>
                {
                    switchButton.Current.Disabled = false;
                    switchButton.Current.Value = v.NewValue;
                    switchButton.Current.Disabled = !toggleable;

                    updateDisplay();
                }, true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateDisplay();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateDisplay();
                base.OnHoverLost(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (toggleable)
                {
                    Current.Toggle();
                    switchButton.PlaySample(Current.Value);
                }

                updateDisplay();
                return true;
            }

            private void updateDisplay()
            {
                // default, toggled on (or not toggleable)
                Color4 col = colourProvider.Content1;

                if (toggleable && !Current.Value)
                    col = IsHovered ? colourProvider.Light1 : colourProvider.Foreground1;

                headerText.FadeColour(col, 300, Easing.OutQuint);
            }
        }
    }
}
