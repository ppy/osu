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
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Overlays
{
    public class RestoreDefaultValueButton<T> : Container, IHasTooltip
    {
        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        private Bindable<T> bindable;

        public Bindable<T> Bindable
        {
            get => bindable;
            set
            {
                bindable = value;
                bindable.ValueChanged += _ => UpdateState();
                bindable.DisabledChanged += _ => UpdateState();
                bindable.DefaultChanged += _ => UpdateState();
                UpdateState();
            }
        }

        private Color4 buttonColour;

        private bool hovering;

        public RestoreDefaultValueButton()
        {
            RelativeSizeAxes = Axes.Y;
            Width = SettingsPanel.CONTENT_MARGINS;
            Alpha = 0f;
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
            if (bindable != null && !bindable.Disabled)
                bindable.SetDefault();
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

        public void SetButtonColour(Color4 buttonColour)
        {
            this.buttonColour = buttonColour;
            UpdateState();
        }

        public void UpdateState() => Scheduler.AddOnce(updateState);

        private void updateState()
        {
            if (bindable == null)
                return;

            this.FadeTo(bindable.IsDefault ? 0f :
                hovering && !bindable.Disabled ? 1f : 0.65f, 200, Easing.OutQuint);
            this.FadeColour(bindable.Disabled ? Color4.Gray : buttonColour, 200, Easing.OutQuint);
        }
    }
}