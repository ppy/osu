using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Purcashe.Components
{
    public class ShowcasePurcasheProgressBar : ProgressBar
    {
        protected override void UpdateValue(float value)
        {
            fill.ResizeWidthTo(value * UsableWidth, 200, Easing.OutQuint);
        }

        public ShowcasePurcasheProgressBar(bool allowSeek = false)
            : base(allowSeek)
        {
        }
    }
}
