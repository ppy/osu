using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.PurePlayer.Components
{
    public class MusicControllerPanel : Container
    {
        public MusicControllerPanel()
        {
            CornerRadius = 12.5f;
            Masking = true;
            Alpha = 0.01f;
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.FadeTo(1, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            this.FadeTo(0.01f, 500, Easing.OutQuint);
        }
    }
}