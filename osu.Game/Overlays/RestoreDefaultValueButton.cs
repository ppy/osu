// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public class RestoreDefaultValueButton<T> : OsuButton, IHasTooltip, IHasCurrentValue<T>
    {
        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        // this is done to ensure a click on this button doesn't trigger focus on a parent element which contains the button.
        public override bool AcceptsFocus => true;

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private Color4 buttonColour;

        private bool hovering;

        public RestoreDefaultValueButton()
        {
            Height = 1;

            RelativeSizeAxes = Axes.Y;
            Width = SettingsPanel.CONTENT_MARGINS;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            BackgroundColour = colour.Yellow;
            buttonColour = colour.Yellow;
            Content.Width = 0.33f;
            Content.CornerRadius = 3;
            Content.EdgeEffect = new EdgeEffectParameters
            {
                Colour = buttonColour.Opacity(0.1f),
                Type = EdgeEffectType.Glow,
                Radius = 2,
            };

            Padding = new MarginPadding { Vertical = 1.5f };
            Alpha = 0f;

            Action += () =>
            {
                if (!current.Disabled) current.SetDefault();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.ValueChanged += _ => UpdateState();
            Current.DisabledChanged += _ => UpdateState();
            Current.DefaultChanged += _ => UpdateState();

            UpdateState();
        }

        public LocalisableString TooltipText => "revert to default";

        protected override bool OnHover(HoverEvent e)
        {
            hovering = true;
            UpdateState();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hovering = false;
            UpdateState();
        }

        public void UpdateState() => Scheduler.AddOnce(updateState);

        private void updateState()
        {
            if (current == null)
                return;

            this.FadeTo(current.IsDefault ? 0f :
                hovering && !current.Disabled ? 1f : 0.65f, 200, Easing.OutQuint);
            this.FadeColour(current.Disabled ? Color4.Gray : buttonColour, 200, Easing.OutQuint);
        }
    }
}
