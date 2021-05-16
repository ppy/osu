// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Overlays
{
    public class RestoreDefaultValueButton<T> : Container, IHasTooltip, IHasCurrentValue<T>
    {
        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set
            {
                current.Current = value;
                UpdateState();
            }
        }

        private Color4 buttonColour;

        private bool hovering;

        public RestoreDefaultValueButton()
        {
            RelativeSizeAxes = Axes.Y;
            Width = SettingsPanel.CONTENT_MARGINS;
            Padding = new MarginPadding { Vertical = 1.5f };
            Alpha = 0f;

            Current.ValueChanged += _ => UpdateState();
            Current.DisabledChanged += _ => UpdateState();
            Current.DefaultChanged += _ => UpdateState();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            buttonColour = colour.Yellow;

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 3,
                Masking = true,
                Colour = buttonColour,
                EdgeEffect = new EdgeEffectParameters
                {
                    Colour = buttonColour.Opacity(0.1f),
                    Type = EdgeEffectType.Glow,
                    Radius = 2,
                },
                Size = new Vector2(0.33f, 0.8f),
                Child = new Box { RelativeSizeAxes = Axes.Both },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UpdateState();
        }

        public string TooltipText => "revert to default";

        protected override bool OnClick(ClickEvent e)
        {
            if (current != null && !current.Disabled)
                current.SetDefault();
            return true;
        }

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