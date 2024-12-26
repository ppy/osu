// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackSamplePointVolumeControl<T> : Container, IHasCurrentValue<int>, IHasTooltip
    {
        private readonly BindableNumberWithCurrent<int> current = new BindableNumberWithCurrent<int>();

        private bool inProgress = false;
        private readonly BindableNumberWithCurrent<int> inProgressValue = new BindableNumberWithCurrent<int>();

        public Action<int>? OnVolumeChange;

        public Bindable<int> Current
        {
            get => current;
            set
            {
                ArgumentNullException.ThrowIfNull(value, "value");
                current.Current = value;
            }
        }

        public LocalisableString TooltipText => Current.Value.ToString();

        private Box background;
        private Container fill;

        public HitSoundTrackSamplePointVolumeControl()
        {
            Masking = true;

            Children = new Drawable[]
            {
                 background = new Box
                 {
                     RelativeSizeAxes = Axes.Both,
                     Alpha = 0.3f,
                 },
                 fill = new Container
                 {
                     RelativeSizeAxes = Axes.X,
                     Anchor = Anchor.BottomLeft,
                     Origin = Anchor.BottomLeft,
                     Child = new Box { RelativeSizeAxes = Axes.Both },
                 },
            };

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Colour4.White,
                Radius = 10f,
                Hollow = false,
                Type = EdgeEffectType.Glow,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Colour.TryExtractSingleColour(out var edgeGlowColour);
            FadeEdgeEffectTo(edgeGlowColour);

            current.ValueChanged += v => inProgressValue.Value = inProgress == true ? inProgressValue.Value : v.NewValue;
            current.MaxValueChanged += v => inProgressValue.MaxValue = v;
            current.MinValueChanged += v => inProgressValue.MinValue = v;

            inProgressValue.MinValue = current.MinValue;
            inProgressValue.MaxValue = current.MaxValue;
            inProgressValue.Value = current.Value;

            inProgressValue.BindValueChanged(v =>
            {
                updateFill();
            }, true);
        }

        private void updateFill()
        {
            fill.ResizeHeightTo(inProgressValue.NormalizedValue * background.DrawHeight, 100, Easing.InOutCubic);
            FadeEdgeEffectTo(0.2f * inProgressValue.NormalizedValue, 100);
        }

        private void applyChanges()
        {
            OnVolumeChange?.Invoke(inProgressValue.Value);
        }

        protected override bool OnHover(HoverEvent e)
        {
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
        }

        #region drag & click handling

        private void updateCurrentFromMouseEvent(MouseButtonEvent e) => inProgressValue.SetProportional((background.DrawHeight - e.MousePosition.Y) / background.DrawHeight);

        protected override bool OnClick(ClickEvent e)
        {
            updateCurrentFromMouseEvent(e);
            applyChanges();
            base.OnClick(e);
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            updateCurrentFromMouseEvent(e);
            base.OnDrag(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            updateCurrentFromMouseEvent(e);
            base.OnDragStart(e);
            return true;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            applyChanges();
            base.OnDragEnd(e);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (e.AltPressed)
                return false;

            float delta;
            float sign = e.ScrollDelta.X + e.ScrollDelta.Y;

            if (e.ShiftPressed)
                delta = 10 * sign;
            else
                delta = 5 * sign;

            current.Value += (int)Math.Round(delta);

            applyChanges();

            base.OnScroll(e);
            return true;
        }

        #endregion
    }
}
