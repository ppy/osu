using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Mvis.Objects.Helpers
{
    public class HoverCheckContainer : Container
    {
        public readonly Bindable<bool> ScreenHovered = new Bindable<bool>();

        protected override bool OnHover(Framework.Input.Events.HoverEvent e)
        {
            this.ScreenHovered.Value = true;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(Framework.Input.Events.HoverLostEvent e)
        {
            this.ScreenHovered.Value = false;
            base.OnHoverLost(e);
        }
    }
}