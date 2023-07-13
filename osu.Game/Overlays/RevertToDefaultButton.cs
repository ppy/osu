// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osuTK;

namespace osu.Game.Overlays
{
    public partial class RevertToDefaultButton<T> : OsuClickableContainer, IHasCurrentValue<T>
    {
        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        // this is done to ensure a click on this button doesn't trigger focus on a parent element which contains the button.
        public override bool AcceptsFocus => true;

        // this is intentionally not using BindableWithCurrent, as it can use the wrong IsDefault implementation when passed a BindableNumber.
        // using GetBoundCopy() ensures that the received bindable is of the exact same type as the source bindable and uses the proper IsDefault implementation.
        private Bindable<T>? current;

        private SpriteIcon icon = null!;
        private Circle circle = null!;

        [Resolved]
        private OverlayColourProvider colours { get; set; } = null!;

        public Bindable<T> Current
        {
            get => current.AsNonNull();
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

        public RevertToDefaultButton()
            : base(HoverSampleSet.Button)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // size intentionally much larger than actual drawn content, so that the button is easier to click.
            Size = new Vector2(14);

            AddRange(new Drawable[]
            {
                circle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                icon = new SpriteIcon
                {
                    Icon = FontAwesome.Solid.Undo,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(8),
                }
            });

            Action += () =>
            {
                if (current?.Disabled == false)
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

            Enabled.Value = !current.Disabled;

            this.FadeTo(current.Disabled ? 0.2f : (Current.IsDefault ? 0 : 1), fade_duration, Easing.OutQuint);

            if (IsHovered && Enabled.Value)
            {
                icon.RotateTo(-40, 500, Easing.OutQuint);

                icon.FadeColour(colours.Light1, 300, Easing.OutQuint);
                circle.FadeColour(colours.Background2, 300, Easing.OutQuint);
                this.ScaleTo(1.2f, 300, Easing.OutQuint);
            }
            else
            {
                icon.RotateTo(0, 100, Easing.OutQuint);

                icon.FadeColour(colours.Colour0, 100, Easing.OutQuint);
                circle.FadeColour(colours.Background3, 100, Easing.OutQuint);
                this.ScaleTo(1f, 100, Easing.OutQuint);
            }
        }
    }
}
