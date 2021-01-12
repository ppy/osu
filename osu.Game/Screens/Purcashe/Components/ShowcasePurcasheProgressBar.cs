using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Purcashe.Components
{
    public class ShowcasePurcasheProgressBar : ProgressBar
    {
        protected override void UpdateValue(float value)
        {
            fill.ResizeWidthTo(value * UsableWidth, 300, Easing.OutQuint);
        }
    }
}
