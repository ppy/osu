using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;

namespace Symcol.Rulesets.Core.Wiki
{
    public class WikiOptionEnumExplanation<T> : Container
        where T : struct
    {
        public OsuTextFlowContainer Description;

        public WikiOptionEnumExplanation(Bindable<T> bindable)
        {
            OsuColour osu = new OsuColour();
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Masking = true;

            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = osu.Yellow,
                    Masking = true,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(10, 0.98f),
                    CornerRadius = 5,

                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                },
                new Container
                {
                    Position = new Vector2(-10, 0),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.45f,

                    Child = new SettingsEnumDropdown<T>
                    {
                        Bindable = bindable
                    }
                },
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.45f,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = 100,
                    AutoSizeEasing = Easing.OutQuint,

                    Child = Description = new OsuTextFlowContainer(t => { t.TextSize = 20; })
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    }
                }
            };
        }
    }
}
