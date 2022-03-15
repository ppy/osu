using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Components
{
    public class EditorSidebarSection : Container
    {
        protected override Container<Drawable> Content { get; }

        public EditorSidebarSection(LocalisableString sectionName)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new SectionHeader(sectionName),
                    Content = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                    },
                }
            };
        }

        public class SectionHeader : CompositeDrawable
        {
            private readonly LocalisableString text;

            public SectionHeader(LocalisableString text)
            {
                this.text = text;

                Margin = new MarginPadding { Vertical = 10, Horizontal = 5 };

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                InternalChildren = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = text,
                        Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                    },
                    new Circle
                    {
                        Y = 18,
                        Colour = colourProvider.Highlight1,
                        Size = new Vector2(28, 2),
                    }
                };
            }
        }
    }
}