using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.PurePlayer.Components
{
    /// <summary>
    /// Based on <see cref="HoldToConfirmContainer"/>
    /// </summary>
    public class MusicPanelHoldToConfirmButton : MusicPanelButton
    {
        public Action ConfirmAction;
        private BindableFloat ExitProgress = new BindableFloat();
        private BindableFloat HoldingTime = new BindableFloat();
        private Box box;
        private bool pressing = false;

        [BackgroundDependencyLoader]
        private void load()
        {
            this.Add(box = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Height = 0,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Colour = Colour4.White.Opacity(0.5f)
            });
            ExitProgress.BindValueChanged( p => box.Height=p.NewValue );
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            pressing = true;
            WaitBeforeConfirm();
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            pressing = false;
            AbortConfirm();
            base.OnMouseUp(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (pressing)
            {
                WaitBeforeConfirm();
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            AbortConfirm();
        }

        protected void WaitBeforeConfirm()
        {
            this.TransformBindableTo(HoldingTime, 1, 250, Easing.Out).OnComplete(_ => BeginConfirm());
        }

        protected void BeginConfirm()
        {
            this.TransformBindableTo(HoldingTime, 0);
            this.TransformBindableTo(ExitProgress, 1, 1000, Easing.Out).OnComplete(_ => Confirm());
        }

        protected virtual void Confirm()
        {
            ConfirmAction?.Invoke();
            this.Action = null;
            this.ConfirmAction = null;
        }

        protected void AbortConfirm()
        {
            this.TransformBindableTo(HoldingTime, 0);
            this.TransformBindableTo(ExitProgress, 0, 500, Easing.Out);
        }
    }
}