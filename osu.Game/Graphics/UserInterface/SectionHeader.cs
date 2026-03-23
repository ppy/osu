// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class SectionHeader : CompositeDrawable
    {
        public Bindable<string> Details = new Bindable<string>();

        private readonly LocalisableString text;

        private OsuTextFlowContainer textFlow = null!;
        private ITextPart? detailsPart;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public SectionHeader(LocalisableString text)
        {
            this.text = text;

            Margin = new MarginPadding { Vertical = 10, Horizontal = 5 };

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2),
                Children = new Drawable[]
                {
                    textFlow = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold))
                    {
                        Text = text,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                    new Circle
                    {
                        Colour = colourProvider.Highlight1,
                        Size = new Vector2(28, 2),
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Details.BindValueChanged(updateDetails);
        }

        private void updateDetails(ValueChangedEvent<string> details)
        {
            if (detailsPart != null)
                textFlow.RemovePart(detailsPart);

            detailsPart = textFlow.AddText($" {details.NewValue}", t => t.Colour = colourProvider.Highlight1);
        }
    }
}
