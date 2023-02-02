﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Overlays
{
    public partial class RestoreDefaultValueButton<T> : OsuClickableContainer, IHasCurrentValue<T>
    {
        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        // this is done to ensure a click on this button doesn't trigger focus on a parent element which contains the button.
        public override bool AcceptsFocus => true;

        // this is intentionally not using BindableWithCurrent, as it can use the wrong IsDefault implementation when passed a BindableNumber.
        // using GetBoundCopy() ensures that the received bindable is of the exact same type as the source bindable and uses the proper IsDefault implementation.
        private Bindable<T> current;

        public Bindable<T> Current
        {
            get => current;
            set
            {
                current?.UnbindAll();
                current = value.GetBoundCopy();

                current.ValueChanged += _ => UpdateState();
                current.DefaultChanged += _ => UpdateState();
                current.DisabledChanged += _ => UpdateState();

                if (IsLoaded)
                    UpdateState();
            }
        }

        [Resolved]
        private OsuColour colours { get; set; }

        private const float size = 4;

        private CircularContainer circle = null!;
        private Box background = null!;

        public RestoreDefaultValueButton()
            : base(HoverSampleSet.Button)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            // size intentionally much larger than actual drawn content, so that the button is easier to click.
            Size = new Vector2(3 * size);

            Add(circle = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(size),
                Masking = true,
                Child = background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour.Lime1
                }
            });

            Alpha = 0f;

            Action += () =>
            {
                if (!current.Disabled)
                    current.SetDefault();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
            FinishTransforms(true);
        }

        public override LocalisableString TooltipText => CommonStrings.RevertToDefault.ToLower();

        protected override bool OnHover(HoverEvent e)
        {
            UpdateState();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UpdateState();
        }

        public void UpdateState() => Scheduler.AddOnce(updateState);

        private const double fade_duration = 200;

        private void updateState()
        {
            if (current == null)
                return;

            Enabled.Value = !Current.Disabled;

            if (!Current.Disabled)
            {
                this.FadeTo(Current.IsDefault ? 0 : 1, fade_duration, Easing.OutQuint);
                background.FadeColour(IsHovered ? colours.Lime0 : colours.Lime1, fade_duration, Easing.OutQuint);
                circle.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Colour = (IsHovered ? colours.Lime1 : colours.Lime3).Opacity(0.4f),
                    Radius = IsHovered ? 8 : 4,
                    Type = EdgeEffectType.Glow
                }, fade_duration, Easing.OutQuint);
            }
            else
            {
                background.FadeColour(colours.Lime3, fade_duration, Easing.OutQuint);
                circle.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Colour = colours.Lime3.Opacity(0.1f),
                    Radius = 2,
                    Type = EdgeEffectType.Glow
                }, fade_duration, Easing.OutQuint);
            }
        }
    }
}
