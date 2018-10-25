using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Core.Wiki.Sections.Subsection
{
    public abstract class WikiSubSection : FillFlowContainer
    {
        public abstract string Title { get; }

        private readonly FillFlowContainer content;

        protected override Container<Drawable> Content => content;

        protected WikiSubSection()
        {
            Direction = FillDirection.Vertical;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                new WikiSubSectionHeader
                {
                    Text = Title,
                },
                content = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                }
            };
        }
    }
}
