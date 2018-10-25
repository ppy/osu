using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using OpenTK;

namespace osu.Core.Wiki.Sections.OptionExplanations
{
    public class WikiOptionEnumSplitExplanation<T> : FillFlowContainer
        where T : struct
    {
        public WikiOptionEnumSplitExplanation(Bindable<T> bindable, Container leftSide, Container rightSide)
        {
            OsuColour osu = new OsuColour();
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Masking = true;

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<T>
                {
                    Bindable = bindable
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,

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
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.45f,

                            Child = leftSide
                        },
                        new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.45f,

                            Child = rightSide
                        }
                    }
                }

            };
        }
    }
}
