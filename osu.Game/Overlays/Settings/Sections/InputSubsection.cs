// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class InputSubsection : SettingsSubsection
    {
        private readonly InputHandler handler;

        private SwitchButton switchButton = null!;

        protected override LocalisableString Header => handler.Description;

        /// <summary>
        /// Whether the input handler can be toggled on/off by the user.
        /// </summary>
        protected virtual bool IsToggleable => true;

        private readonly BindableBool handlerEnabled = new BindableBool();

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public InputSubsection(InputHandler handler)
        {
            this.handler = handler;

            FlowContent.AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HeaderContainer.Add(switchButton = new SwitchButton
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Scale = new Vector2(0.6f),
                Position = new Vector2(12, 8),
                Rotation = 90,
            });

            HeaderText.X = 20;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            switchButton.Current.ValueChanged += v => handlerEnabled.Value = v.NewValue;

            handlerEnabled.BindTo(handler.Enabled);
            handlerEnabled.BindValueChanged(v =>
            {
                switchButton.Current.Disabled = false;
                switchButton.Current.Value = v.NewValue;
                switchButton.Current.Disabled = !IsToggleable;

                updateEnabledState();
            }, true);
        }

        private void updateEnabledState()
        {
            HeaderText.Colour = handlerEnabled.Value ? colourProvider.Content1 : colourProvider.Foreground1;
            HeaderText.Text = handlerEnabled.Value ? Header : $@"{Header} (disabled)";

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
                FlowContent.AutoSizeDuration = 300;
                FlowContent.AutoSizeEasing = Easing.OutQuint;
                FlowContent.AutoSizeAxes = Axes.Y;
                ScheduleAfterChildren(() => FlowContent.AutoSizeDuration = 0);

                FlowContent.FadeIn(300, Easing.OutQuint);
            }
        }
    }
}
