using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK;

#nullable disable

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public partial class SettingsSlider<T> : OsuSliderBar<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        private Container circle;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = 1;
            Child = circle = new Container
            {
                RelativePositionAxes = Axes.X,
                Size = new Vector2(25),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Child = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.X,
                    X = -0.5f
                }
            };

            RangePadding = 0;
        }

        protected override void UpdateValue(float value)
        {
            circle.MoveToX(value, 250, Easing.OutExpo);
            circle.ScaleTo(value + 0.2f, 250, Easing.OutBack);
        }
    }
}
