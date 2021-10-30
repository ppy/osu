using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public class SettingsSlider<T> : OsuSliderBar<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        private Circle circle;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = 1;
            Child = circle = new Circle
            {
                RelativePositionAxes = Axes.X,
                Size = new Vector2(25),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }

        protected override void UpdateValue(float value)
        {
            circle.MoveToX(value - 0.5f, 250, Easing.OutExpo);
            circle.ScaleTo(value + 0.2f, 250, Easing.OutBack);
        }
    }
}
